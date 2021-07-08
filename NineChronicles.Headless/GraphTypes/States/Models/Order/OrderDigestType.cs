using GraphQL.Types;
using Lib9c.Model.Order;
using Libplanet.Explorer.GraphTypes;

namespace NineChronicles.Headless.GraphTypes.States.Models.Order
{
    public class OrderDigestType : ObjectGraphType<OrderDigest>
    {
        public OrderDigestType()
        {
            Field<NonNullGraphType<GuidGraphType>>(
                nameof(OrderDigest.OrderId),
                resolve: context => context.Source.OrderId);
            Field<NonNullGraphType<GuidGraphType>>(
                nameof(OrderDigest.TradableId),
                resolve: context => context.Source.TradableId);
            Field<NonNullGraphType<AddressType>>(
                nameof(OrderDigest.SellerAgentAddress),
                resolve: context => context.Source.SellerAgentAddress);
            Field<NonNullGraphType<FungibleAssetValueType>>(
                nameof(OrderDigest.Price),
                resolve: context => context.Source.Price);
            Field<NonNullGraphType<LongGraphType>>(nameof(OrderDigest.StartedBlockIndex));
            Field<NonNullGraphType<LongGraphType>>(nameof(OrderDigest.ExpiredBlockIndex));
            Field<NonNullGraphType<IntGraphType>>(
                nameof(OrderDigest.ItemCount),
                resolve: context => context.Source.ItemCount);
            Field<NonNullGraphType<IntGraphType>>(
                nameof(OrderDigest.CombatPoint),
                resolve: context => context.Source.CombatPoint);
            Field<NonNullGraphType<IntGraphType>>(
                nameof(OrderDigest.Level),
                resolve: context => context.Source.Level);
            Field<NonNullGraphType<IntGraphType>>(
                nameof(OrderDigest.ItemId),
                resolve: context => context.Source.ItemId);
        }
    }
}
