using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Libplanet;
using Nekoyume;
using Nekoyume.Model.State;
using NineChronicles.Headless.GraphTypes.States;
using NineChronicles.Headless.GraphTypes.States.Models;
using Xunit;
using static NineChronicles.Headless.Tests.GraphQLTestUtils;

namespace NineChronicles.Headless.Tests.GraphTypes.States.Models
{
    public class ShardedShopStateV2TypeTest
    {
        [Fact]
        public async Task Query()
        {
            const string query = @"{
                address
                orderDigestList {
                    sellerAgentAddress
                    price {
                        currency
                        quantity
                    }
                    tradableId
                    orderId
                }
            }";
            var shopState = new ShardedShopStateV2(default(Address));
            var queryResult = await ExecuteQueryAsync<ShardedShopStateV2Type>(query, source: shopState);
            Assert.Null(queryResult.Errors);
        }
    }
}
