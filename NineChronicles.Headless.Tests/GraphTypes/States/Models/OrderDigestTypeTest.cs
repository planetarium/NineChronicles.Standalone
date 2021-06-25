using System.Threading.Tasks;
using Lib9c.Model.Order;
using Libplanet.Assets;
using Nekoyume.Model.Item;
using NineChronicles.Headless.GraphTypes.States.Models.Order;
using Xunit;
using static NineChronicles.Headless.Tests.GraphQLTestUtils;


namespace NineChronicles.Headless.Tests.GraphTypes.States.Models
{
    public class OrderDigestTypeTest
    {
        private readonly FungibleAssetValue _price;

        public OrderDigestTypeTest()
        {
            _price = new FungibleAssetValue(new Currency("NCG", 2, minter: null), 10, 0);
        }

        [Fact]
        public async Task Query()
        {
            var query = @"
            {
                orderId
                tradableId
                sellerAgentAddress
                startedBlockIndex
                expiredBlockIndex
                price {
                    quantity
                    currency
                }
                itemCount
                combatPoint
                level
                itemId
            }";

            OrderDigest orderDigest = new OrderDigest(
                    default,
                    1,
                    2,
                    default,
                    default,
                    _price,
                    100,
                    1,
                    101000,
                    1
            );

            var queryResult = await ExecuteQueryAsync<OrderDigestType>(query, source: orderDigest);
            Assert.Null(queryResult.Errors);
        }
    }
}
