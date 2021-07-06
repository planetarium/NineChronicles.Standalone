using GraphQL;
using GraphQL.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blocks;
using Libplanet.Explorer.GraphTypes;
using Libplanet.Tx;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NCAction = Libplanet.Action.PolymorphicAction<Nekoyume.Action.ActionBase>;

namespace NineChronicles.Headless.GraphTypes
{
    public class NodeStatusType : ObjectGraphType<NodeStatusType>
    {
        public bool BootstrapEnded { get; set; }

        public bool PreloadEnded { get; set; }
        
        public bool IsMining { get; set; }

        public NodeStatusType(StandaloneContext context)
        {
            Field<NonNullGraphType<BooleanGraphType>>(
                name: "bootstrapEnded",
                description: "Whether the current libplanet node has ended bootstrapping.",
                resolve: _ => context.BootstrapEnded
            );
            Field<NonNullGraphType<BooleanGraphType>>(
                name: "preloadEnded",
                description: "Whether the current libplanet node has ended preloading.",
                resolve: _ => context.PreloadEnded
            );
            Field<NonNullGraphType<BlockHeaderType>>(
                name: "tip",
                description: "Block header of the tip block from the current canonical chain.",
                resolve: _ => context.BlockChain is { } blockChain
                    ? BlockHeaderType.FromBlock(blockChain.Tip)
                    : null
            );
            Field<NonNullGraphType<ListGraphType<BlockHeaderType>>>(
                name: "topmostBlocks",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "limit",
                        Description = "The number of blocks to get."
                    },
                    new QueryArgument<IntGraphType>
                    {
                        Name = "offset",
                        Description = "The number of blocks to skip from tip.",
                        DefaultValue = 0,
                    },
                    new QueryArgument<AddressType>
                    {
                        Name = "miner",
                        Description = "List only blocks mined by the given address.  " +
                            "(List everything if omitted.)",
                        DefaultValue = null,
                    }
                ),
                description: "The topmost blocks from the current node.",
                resolve: fieldContext =>
                {
                    if (context.BlockChain is null)
                    {
                        throw new InvalidOperationException($"{nameof(context.BlockChain)} is null.");
                    }

                    IEnumerable<Block<NCAction>> blocks =
                        GetTopmostBlocks(context.BlockChain, fieldContext.GetArgument<int>("offset"));
                    if (fieldContext.GetArgument<Address?>("miner") is { } miner)
                    {
                        blocks = blocks.Where(b => b.Miner.Equals(miner));
                    }

                    return blocks
                        .Take(fieldContext.GetArgument<int>("limit"))
                        .Select(BlockHeaderType.FromBlock);
                });
            Field<ListGraphType<TxIdType>>(
                name: "stagedTxIds",
                arguments: new QueryArguments(
                    new QueryArgument<AddressType>
                    {
                        Name = "address",
                        Description = "Target address to query"
                    }
                ),
                description: "Ids of staged transactions from the current node.",
                resolve: fieldContext =>
                {
                    if (context.BlockChain is null)
                    {
                        throw new InvalidOperationException($"{nameof(context.BlockChain)} is null.");
                    }

                    if (!fieldContext.HasArgument("address"))
                    {
                        return context.BlockChain.GetStagedTransactionIds();
                    }
                    else
                    {
                        Address address = fieldContext.GetArgument<Address>("address");
                        IImmutableSet<TxId> stagedTransactionIds = context.BlockChain.GetStagedTransactionIds();

                        return stagedTransactionIds.Where(txId =>
                        context.BlockChain.GetTransaction(txId).Signer.Equals(address));
                    }
                }
            );
            Field<NonNullGraphType<BlockHeaderType>>(
                name: "genesis",
                description: "Block header of the genesis block from the current chain.",
                resolve: fieldContext =>
                    context.BlockChain is { } blockChain
                        ? BlockHeaderType.FromBlock(blockChain.Genesis)
                        : null
            );
            Field<NonNullGraphType<BooleanGraphType>>(
                name: "isMining",
                description: "Whether the current node is mining.",
                resolve: _ => context.IsMining
            );
        }

        private IEnumerable<Block<T>> GetTopmostBlocks<T>(BlockChain<T> blockChain, int offset)
            where T : IAction, new()
        {
            Block<T> block = blockChain.Tip;

            while (offset > 0)
            {
                offset--;
                if (block.PreviousHash is { } prev)
                {
                    block = blockChain[prev];
                }
            }

            while (true)
            {
                yield return block;
                if (block.PreviousHash is { } prev)
                {
                    block = blockChain[prev];
                }
                else
                {
                    break;
                }
            }
        }
    }
}
