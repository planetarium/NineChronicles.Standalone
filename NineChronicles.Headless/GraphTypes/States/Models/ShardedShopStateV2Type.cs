using GraphQL.Types;
using Libplanet.Explorer.GraphTypes;
using Nekoyume.Model.State;
using NineChronicles.Headless.GraphTypes.States.Models.Order;

namespace NineChronicles.Headless.GraphTypes.States.Models
{
    public class ShardedShopStateV2Type : ObjectGraphType<ShardedShopStateV2>
    {
        public ShardedShopStateV2Type()
        {
            Field<NonNullGraphType<AddressType>>(
                nameof(ShardedShopStateV2.address),
                resolve: context => context.Source.address);

            Field<ListGraphType<OrderDigestType>>(
                nameof(ShardedShopStateV2.OrderDigestList),
                resolve: context => context.Source.OrderDigestList);
        }
    }
}
