using System.Threading.Tasks;
using Lib9c.Model.Order;
using Libplanet.Assets;
using Nekoyume.Model.Item;
using NineChronicles.Headless.GraphTypes.States.Models.Order;
using Xunit;
using static NineChronicles.Headless.Tests.GraphQLTestUtils;


namespace NineChronicles.Headless.Tests.GraphTypes.States.Models
{
    public class OrderTypeTest
    {
        private readonly FungibleAssetValue _price;

        public OrderTypeTest()
        {
            _price = new FungibleAssetValue(new Currency("NCG", 2, minter: null), 10, 0);
        }

        [Theory]
        [InlineData(Order.OrderType.Fungible)]
        [InlineData(Order.OrderType.NonFungible)]
        public async Task FungibleTypeQuery(Order.OrderType type)
        {
            var query = @"
            {
                orderId
                tradableId
                sellerAgentAddress
                sellerAvatarAddress
                startedBlockIndex
                expiredBlockIndex
                price {
                    quantity
                    currency
                }
                itemCount
            }";

            Order order;
            if (type == Order.OrderType.Fungible)
            {
                order = OrderFactory.CreateFungibleOrder(
                    default,
                    default,
                    default,
                    _price,
                    default,
                    0,
                    2,
                    ItemSubType.Hourglass
                );
            }
            else
            {
                order = OrderFactory.CreateNonFungibleOrder(
                    default,
                    default,
                    default,
                    _price,
                    default,
                    0,
                    ItemSubType.Armor
                );
            }

            var queryResult = await ExecuteQueryAsync<OrderType>(query, source: order);
            Assert.Null(queryResult.Errors);
        }
    }
}
