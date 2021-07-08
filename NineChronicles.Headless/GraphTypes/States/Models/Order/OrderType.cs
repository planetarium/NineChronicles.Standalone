using GraphQL.Types;
using Lib9c.Model.Order;
using Libplanet.Explorer.GraphTypes;

namespace NineChronicles.Headless.GraphTypes.States.Models.Order
{
    public class OrderType : ObjectGraphType<Lib9c.Model.Order.Order>
    {
        public OrderType()
        {
            Field<NonNullGraphType<GuidGraphType>>(
                nameof(Lib9c.Model.Order.Order.OrderId),
                resolve: context => context.Source.OrderId);
            Field<NonNullGraphType<GuidGraphType>>(
                nameof(Lib9c.Model.Order.Order.TradableId),
                resolve: context => context.Source.TradableId);
            Field<NonNullGraphType<AddressType>>(nameof(Lib9c.Model.Order.Order.SellerAgentAddress));
            Field<NonNullGraphType<AddressType>>(nameof(Lib9c.Model.Order.Order.SellerAvatarAddress));
            Field<NonNullGraphType<FungibleAssetValueType>>(nameof(Lib9c.Model.Order.Order.Price));
            Field<NonNullGraphType<LongGraphType>>(nameof(Lib9c.Model.Order.Order.StartedBlockIndex));
            Field<NonNullGraphType<LongGraphType>>(nameof(Lib9c.Model.Order.Order.ExpiredBlockIndex));
            Field<NonNullGraphType<IntGraphType>>(
                nameof(FungibleOrder.ItemCount),
                resolve: context =>
                {
                    if (context.Source is FungibleOrder fungibleOrder)
                    {
                        return fungibleOrder.ItemCount;
                    }

                    return 1;
                });
        }
    }
}
