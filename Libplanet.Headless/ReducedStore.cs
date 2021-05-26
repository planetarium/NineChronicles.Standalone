using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blocks;
using Libplanet.Store;
using Libplanet.Tx;

namespace Libplanet.Headless
{
    /// <summary>
    /// A <see cref="IStore"> decorator that reduce space consumption by omitting input calls which
    /// are unused by Nine Chronicles.
    /// <para>Calls on this will be forwarded to its <see cref="InternalStore"/>, except for:</para>
    /// <list type="bullet">
    /// <item><description><see cref="PutTxExecution(TxSuccess)"/></description></item>
    /// </list>
    /// </summary>
    public sealed class ReducedStore : IStore
    {
        public ReducedStore(IStore internalStore)
        {
            InternalStore = internalStore;
        }

        public IStore InternalStore { get; }

        public long AppendIndex(Guid chainId, BlockHash hash) =>
            InternalStore.AppendIndex(chainId, hash);

        public bool ContainsBlock(BlockHash blockHash) =>
            InternalStore.ContainsBlock(blockHash);

        public bool ContainsTransaction(TxId txId) =>
            InternalStore.ContainsTransaction(txId);

        public long CountBlocks() =>
            InternalStore.CountBlocks();

        public long CountIndex(Guid chainId) =>
            InternalStore.CountIndex(chainId);

        public long CountTransactions() =>
            InternalStore.CountTransactions();

        public bool DeleteBlock(BlockHash blockHash) =>
            InternalStore.DeleteBlock(blockHash);

        public void DeleteChainId(Guid chainId) =>
            InternalStore.DeleteChainId(chainId);

        public bool DeleteTransaction(TxId txid) =>
            InternalStore.DeleteTransaction(txid);

        public void ForkBlockIndexes(
            Guid sourceChainId,
            Guid destinationChainId,
            BlockHash branchpoint
        ) =>
            InternalStore.ForkBlockIndexes(sourceChainId, destinationChainId, branchpoint);

        public void ForkTxNonces(Guid sourceChainId, Guid destinationChainId) =>
            InternalStore.ForkTxNonces(sourceChainId, destinationChainId);

        public Block<T> GetBlock<T>(BlockHash blockHash)
            where T : IAction, new() =>
            InternalStore.GetBlock<T>(blockHash);

        public BlockDigest? GetBlockDigest(BlockHash blockHash) =>
            InternalStore.GetBlockDigest(blockHash);

        public long? GetBlockIndex(BlockHash blockHash) =>
            InternalStore.GetBlockIndex(blockHash);

        public DateTimeOffset? GetBlockPerceivedTime(BlockHash blockHash) =>
            InternalStore.GetBlockPerceivedTime(blockHash);

        public Guid? GetCanonicalChainId() =>
            InternalStore.GetCanonicalChainId();

        public Transaction<T> GetTransaction<T>(TxId txid) where T : IAction, new() =>
            InternalStore.GetTransaction<T>(txid);

        public TxExecution GetTxExecution(BlockHash blockHash, TxId txid) =>
            InternalStore.GetTxExecution(blockHash, txid);

        public long GetTxNonce(Guid chainId, Address address) =>
            InternalStore.GetTxNonce(chainId, address);

        public void IncreaseTxNonce(Guid chainId, Address signer, long delta = 1) =>
            InternalStore.IncreaseTxNonce(chainId, signer, delta);

        public BlockHash? IndexBlockHash(Guid chainId, long index) =>
            InternalStore.IndexBlockHash(chainId, index);

        public IEnumerable<BlockHash> IterateBlockHashes() =>
            InternalStore.IterateBlockHashes();

        public IEnumerable<BlockHash> IterateIndexes(
            Guid chainId,
            int offset = 0,
            int? limit = null
        ) =>
            InternalStore.IterateIndexes(chainId, offset, limit);

        public IEnumerable<TxId> IterateStagedTransactionIds() =>
            InternalStore.IterateStagedTransactionIds();

        public IEnumerable<TxId> IterateTransactionIds() =>
            InternalStore.IterateTransactionIds();

        public IEnumerable<Guid> ListChainIds() =>
            InternalStore.ListChainIds();

        public IEnumerable<KeyValuePair<Address, long>> ListTxNonces(Guid chainId) =>
            InternalStore.ListTxNonces(chainId);

        public void PutBlock<T>(Block<T> block) where T : IAction, new() =>
            InternalStore.PutBlock(block);

        public void PutTransaction<T>(Transaction<T> tx) where T : IAction, new() =>
            InternalStore.PutTransaction(tx);

        public void PutTxExecution(TxSuccess txSuccess)
        {
            // Omit TxSuccess.UpdatedStates as it is unused by Nine Chronicles and too big.
            TxSuccess reducedTxSuccess = new TxSuccess(
                txSuccess.BlockHash,
                txSuccess.TxId,
                updatedStates: ImmutableDictionary<Address, IValue>.Empty,
                fungibleAssetsDelta: txSuccess.FungibleAssetsDelta,
                updatedFungibleAssets: txSuccess.UpdatedFungibleAssets
            );
            InternalStore.PutTxExecution(reducedTxSuccess);
        }

        public void PutTxExecution(TxFailure txFailure) =>
            InternalStore.PutTxExecution(txFailure);

        public void SetBlockPerceivedTime(BlockHash blockHash, DateTimeOffset perceivedTime) =>
            InternalStore.SetBlockPerceivedTime(blockHash, perceivedTime);

        public void SetCanonicalChainId(Guid chainId) =>
            InternalStore.SetCanonicalChainId(chainId);

        public void StageTransactionIds(IImmutableSet<TxId> txids) =>
            InternalStore.StageTransactionIds(txids);

        public void UnstageTransactionIds(ISet<TxId> txids) =>
            InternalStore.UnstageTransactionIds(txids);
    }
}
