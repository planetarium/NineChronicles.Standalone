using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Bencodex;
using GraphQL;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Crypto;
using Libplanet.Store;
using Libplanet.Store.Trie;
using Libplanet.Tx;
using Nekoyume.Action;
using NineChronicles.Headless.GraphTypes;
using Xunit;
using NCAction = Libplanet.Action.PolymorphicAction<Nekoyume.Action.ActionBase>;

namespace NineChronicles.Headless.Tests.GraphTypes
{
    public class TransactionHeadlessQueryTest
    {
        private readonly BlockChain<NCAction> _blockChain;

        public TransactionHeadlessQueryTest()
        {
            _blockChain = new BlockChain<NCAction>(
                new BlockPolicy<NCAction>(),
                new VolatileStagePolicy<NCAction>(),
                new DefaultStore(null),
                new TrieStateStore(new DefaultKeyValueStore(null), new DefaultKeyValueStore(null)),
                BlockChain<NCAction>.MakeGenesisBlock());
        }

        [Fact]
        public async Task NextTxNonce()
        {
            var userPrivateKey = new PrivateKey();
            var userAddress = userPrivateKey.ToAddress();
            string query = $"{{ nextTxNonce(address: \"{userAddress}\") }}";
            var queryResult = await ExecuteAsync(query);
            Assert.Equal(
                new Dictionary<string, object>
                {
                    ["nextTxNonce"] = 0L
                },
                queryResult.Data
            );

            _blockChain.MakeTransaction(userPrivateKey, new PolymorphicAction<ActionBase>[] { });
            queryResult = await ExecuteAsync(query);
            Assert.Equal(
                new Dictionary<string, object>
                {
                    ["nextTxNonce"] = 1L
                },
                queryResult.Data
            );
        }

        [Fact]
        public async Task GetTx()
        {
            var userPrivateKey = new PrivateKey();
            var queryFormat = @"query {{
                getTx(txId: ""{0}"") {{
                    id
                    nonce
                    signer
                    signature
                    timestamp
                    updatedAddresses
                    actions {{
                        inspection
                    }}
                }}
            }}";
            var queryResult = await ExecuteAsync(string.Format(queryFormat, new TxId()));
            Assert.Equal(
                new Dictionary<string, object?>
                {
                    ["getTx"] = null
                },
                queryResult.Data
            );

            var action = new CreateAvatar2
            {
                index = 0,
                hair = 1,
                lens = 2,
                ear = 3,
                tail = 4,
                name = "action",
            };
            var transaction = _blockChain.MakeTransaction(userPrivateKey, new PolymorphicAction<ActionBase>[] { action });
            _blockChain.StageTransaction(transaction);
            await _blockChain.MineBlock(new Address());
            queryResult = await ExecuteAsync(string.Format(queryFormat, transaction.Id));
            var tx = queryResult.Data
                .As<Dictionary<string, object>>()["getTx"]
                .As<Dictionary<string, object>>();

            Assert.Equal(tx["id"], transaction.Id.ToString());
            Assert.Equal(tx["nonce"], transaction.Nonce);
            Assert.Equal(tx["signer"], transaction.Signer.ToString());
            Assert.Equal(tx["signature"], ByteUtil.Hex(transaction.Signature));
            Assert.Equal(tx["timestamp"], transaction.Timestamp.ToString());
            Assert.Equal(tx["updatedAddresses"], transaction.UpdatedAddresses.Select(a => a.ToString()));

            var plainValue = tx["actions"]
                .As<List<object>>()
                .First()
                .As<Dictionary<string, object>>()["inspection"];
            Assert.Equal(transaction.Actions.First().PlainValue.Inspection, plainValue);
        }
        
        [Fact]
        public async Task CreateUnsignedTx()
        {
            var privateKey = new PrivateKey();
            PublicKey publicKey = privateKey.PublicKey;
            Address signer = publicKey.ToAddress();
            long nonce = _blockChain.GetNextTxNonce(signer);
            NCAction action = new CreateAvatar2
            {
                index = 0,
                hair = 1,
                lens = 2,
                ear = 3,
                tail = 4,
                name = "action",
            };

            var codec = new Codec();
            var queryFormat = @"query {{
                createUnsignedTx(publicKey: ""{0}"", plainValue: ""{1}"")
            }}";
            var queryResult = await ExecuteAsync(string.Format(
                queryFormat,
                Convert.ToBase64String(publicKey.Format(false)),
                Convert.ToBase64String(codec.Encode(action.PlainValue))));
            var base64EncodedUnsignedTx = (string)queryResult.Data
                .As<Dictionary<string, object>>()["createUnsignedTx"];
            Transaction<NCAction> unsignedTx =
                Transaction<NCAction>.Deserialize(Convert.FromBase64String(base64EncodedUnsignedTx), validate: false);
            Assert.Empty(unsignedTx.Signature);
            Assert.Equal(publicKey, unsignedTx.PublicKey);
            Assert.Equal(signer, unsignedTx.Signer);
            Assert.Equal(nonce, unsignedTx.Nonce);
            Assert.Contains(signer, unsignedTx.UpdatedAddresses);
        }
        
        [Theory]
        [InlineData("", "")]  // Empty String
        [InlineData(
            "026d06672c8d5c63c1a81c89fec5ab7faab523fd98895492d33ac32aa94dbd7c6c",
            "6475373a747970655f69647532323a636f6d62696e6174696f6e5f65717569706d656e743575363a76616c756573647531333a61" +
            "76617461724164647265737332303a467740d542f9ad3ac49ca3cb02cc9e3c761d22b975323a696431363a497d2e326a102a499a" +
            "b6d5ad1c56b37875383a726563697065496475313a3175393a736c6f74496e64657875313a307531313a73756252656369706549" +
            "646e6565"
        )]  // Not encoded by base64
        public async Task CreateUnsignedTxThrowsErrorsIfIncorrectArgumentPassed(string publicKey, string plainValue)
        {
            var queryFormat = @"query {{
                createUnsignedTx(publicKey: ""{0}"", plainValue: ""{1}"")
            }}";
            var result = await ExecuteAsync(string.Format(queryFormat, publicKey, plainValue));
            Assert.NotNull(result.Errors);
        }
        
        [Fact]
        public async Task AttachSignature()
        {
            var privateKey = new PrivateKey();
            PublicKey publicKey = privateKey.PublicKey;
            Address signer = publicKey.ToAddress();
            long nonce = _blockChain.GetNextTxNonce(signer);
            Transaction<NCAction> unsignedTx = Transaction<NCAction>.CreateUnsigned(
                nonce,
                publicKey,
                _blockChain.Genesis.Hash,
                ImmutableArray<NCAction>.Empty);
            byte[] serializedUnsignedTx = unsignedTx.Serialize(false);
            // ignore timestamp's millisecond over 6 digits.
            unsignedTx = Transaction<NCAction>.Deserialize(serializedUnsignedTx, false);
            byte[] signature = privateKey.Sign(serializedUnsignedTx);

            var queryFormat = @"query {{
                attachSignature(unsignedTransaction: ""{0}"", signature: ""{1}"")
            }}";
            var result = await ExecuteAsync(string.Format(
                queryFormat,
                Convert.ToBase64String(serializedUnsignedTx),
                Convert.ToBase64String(signature)));
            Assert.Null(result.Errors);
            var base64EncodedSignedTx = (string)result.Data
                .As<Dictionary<string, object>>()["attachSignature"];
            Transaction<NCAction> signedTx =
                Transaction<NCAction>.Deserialize(Convert.FromBase64String(base64EncodedSignedTx));
            Assert.Equal(signature, signedTx.Signature);
            Assert.Equal(unsignedTx.PublicKey, signedTx.PublicKey);
            Assert.Equal(unsignedTx.Signer, signedTx.Signer);
            Assert.Equal(unsignedTx.Nonce, signedTx.Nonce);
            Assert.Equal(unsignedTx.UpdatedAddresses, signedTx.UpdatedAddresses);
            Assert.Equal(unsignedTx.Timestamp, signedTx.Timestamp);
            Assert.Equal(unsignedTx.Actions, signedTx.Actions);
        }

        [Theory]
        [InlineData("", "")]  // Empty String
        [InlineData(
            "64313a616c65313a6733323a1fc6e506d66c8c04283309518f06f1ef21ed9329366b60380fbf3bf35668c72a313a6e693065313a" +
            "7036353a04338573b6979f6bafcd04bcbb299d4790370ae303053a74d7f9c5251fa8074a08206660afd47acb1a9d6873d7051513" +
            "fce050082a801d67f7320c8b2a8e587a2a313a7332303a6fe1c708ace1030b857f3dbb603df831f59b6940313a747532373a3230" +
            "32312d30372d30395430363a34333a33392e3436373835355a313a756c6565",
            "30440220433e7ef8055d89b1e7954af8f650aa1e19e11be16a6c0e6e606484f58d827f6502204030c18376184a24bff7933bb3d2" +
            "60994dc2d0c1721a939ae504fde6e3d8dc71"
        )]  // Not encoded by base64
        public async Task AttachSignatureThrowsErrorsIfIncorrectArgumentPassed(string unsignedTransaction, string signature)
        {
            var query = @$"query {{
                attachSignature(unsignedTransaction: ""{unsignedTransaction}"", signature: ""{signature}"")
            }}";
            var result = await ExecuteAsync(query);
            Assert.NotNull(result.Errors);
        }

        private Task<ExecutionResult> ExecuteAsync(string query)
        {
            return GraphQLTestUtils.ExecuteQueryAsync<TransactionHeadlessQuery>(query, standaloneContext: new StandaloneContext
            {
                BlockChain = _blockChain,
            });
        }  
    }     
}
