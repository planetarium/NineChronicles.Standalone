using System;
using System.Collections.Generic;
using Bencodex.Types;
using GraphQL;
using GraphQL.Types;
using Lib9c.Model.Order;
using Libplanet;
using Libplanet.Action;
using Libplanet.Explorer.GraphTypes;
using Nekoyume;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using NineChronicles.Headless.GraphTypes.States;
using NineChronicles.Headless.GraphTypes.States.Models;
using NineChronicles.Headless.GraphTypes.States.Models.Item.Enum;
using NineChronicles.Headless.GraphTypes.States.Models.Order;
using NineChronicles.Headless.GraphTypes.States.Models.Table;

namespace NineChronicles.Headless.GraphTypes
{
    public class StateQuery : ObjectGraphType<(AccountStateGetter accountStateGetter, AccountBalanceGetter accountBalanceGetter)>
    {
        public StateQuery()
        {
            Name = "StateQuery";
            Field<AvatarStateType>(
                name: "avatar",
                description: "State for avatar.",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<AddressType>>
                {
                    Name = "avatarAddress",
                    Description = "Address of avatar."
                }),
                resolve: context =>
                {
                    var address = context.GetArgument<Address>("avatarAddress");
                    if (!(context.Source.accountStateGetter(address) is { } state))
                    {
                        throw new InvalidOperationException($"The state {address} doesn't exists");
                    }
                    return new AvatarState((Dictionary)state);
                });
            Field<RankingMapStateType>(
                name: "rankingMap",
                description: "State for avatar EXP record.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "index",
                        Description = "RankingMapState index. 0 ~ 99"
                    }),
                resolve: context =>
                {
                    var index = context.GetArgument<int>("index");
                    if (context.Source.accountStateGetter(RankingState.Derive(index)) is { } state)
                    {
                        return new RankingMapState((Dictionary) state);
                    }

                    return null;
                });


            Field<ShardedShopStateV2Type>(
                "shop",
                arguments: new QueryArguments(
                    new QueryArgument<AddressType>
                    {
                        Name = "shopAddress",
                        Description = "shop address"
                    }
                ),
                resolve: context =>
                {
                    var shopAddress = context.GetArgument<Address>("shopAddress");
                    if (context.Source.accountStateGetter(shopAddress) is { } value)
                    {
                        return new ShardedShopStateV2((Dictionary) value);
                    }

                    return null;
                }
            );

            Field<WeeklyArenaStateType>(
                name: "weeklyArena",
                description: "State for weekly arena.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "index",
                        Description = "WeeklyArenaState index. It increases every 56,000 blocks."
                    }),
                resolve: context =>
                {
                    var index = context.GetArgument<int>("index");
                    if (context.Source.accountStateGetter(WeeklyArenaState.DeriveAddress(index)) is { } state)
                    {
                        return new WeeklyArenaState((Dictionary) state);
                    }

                    return null;
                });
            Field<AgentStateType>(
                name: "agent",
                description: "State for agent.",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<AddressType>>
                {
                    Name = "address",
                    Description = "Address of agent."
                }),
                resolve: context =>
                {
                    var address = context.GetArgument<Address>("address");
                    if (context.Source.accountStateGetter(address) is { } state)
                    {
                        return (new AgentState((Dictionary) state), context.Source.accountStateGetter, context.Source.accountBalanceGetter);
                    }

                    return null;
                }
            );

            Field<MonsterCollectionStateType>(
                nameof(MonsterCollectionState),
                description: "State for monster collection.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "agentAddress",
                        Description = "Address of agent."
                    }
                ),
                resolve: context =>
                {
                    var agentAddress = context.GetArgument<Address>("agentAddress");
                    if (!(context.Source.accountStateGetter(agentAddress) is { } value))
                    {
                        return null;
                    }
                    var agentState = new AgentState((Dictionary) value);
                    var deriveAddress = MonsterCollectionState.DeriveAddress(agentAddress, agentState.MonsterCollectionRound);
                    if (context.Source.accountStateGetter(deriveAddress) is { } state)
                    {
                        return new MonsterCollectionState((Dictionary) state);
                    }

                    return null;
                }
            );

            Field<MonsterCollectionSheetType>(
                nameof(MonsterCollectionSheet),
                resolve: context =>
                {
                    var sheetAddress = Addresses.GetSheetAddress<MonsterCollectionSheet>();
                    var rewardSheetAddress = Addresses.GetSheetAddress<MonsterCollectionRewardSheet>();
                    if (context.Source.accountStateGetter(sheetAddress) is { } ss &&
                        context.Source.accountStateGetter(rewardSheetAddress) is { } srs)
                    {
                        var monsterCollectionSheet = new MonsterCollectionSheet();
                        monsterCollectionSheet.Set((Text) ss);
                        var monsterCollectionRewardSheet = new MonsterCollectionRewardSheet();
                        monsterCollectionRewardSheet.Set((Text) srs);
                        return (monsterCollectionSheet, monsterCollectionRewardSheet);
                    }

                    return null;
                }
            );

            Field<OrderType>(
                nameof(Order),
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<GuidGraphType>>
                    {
                        Name = "orderId",
                        Description = "Order Guid."
                    }
                ),
                resolve: context =>
                {
                    var orderId = context.GetArgument<Guid>("orderId");
                    var orderAddress = Order.DeriveAddress(orderId);
                    if (context.Source.accountStateGetter(orderAddress) is { } value)
                    {
                        var order = OrderFactory.Deserialize((Dictionary) value);
                        return order;
                    }

                    return null;
                }
            );

            Field<ListGraphType<OrderDigestType>>(
                nameof(OrderDigestListState),
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "avatarAddress",
                        Description = "avatar address"
                    }
                ),
                resolve: context =>
                {
                    var avatarAddress = context.GetArgument<Address>("avatarAddress");
                    var digestListAddress = OrderDigestListState.DeriveAddress(avatarAddress);
                    if (context.Source.accountStateGetter(digestListAddress) is { } value)
                    {
                        var digestList = new OrderDigestListState((Dictionary) value);
                        return digestList.OrderDigestList;
                    }

                    return null;
                }
            );

            Field<NonNullGraphType<AddressType>>(
                "DeriveShopAddress",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ItemSubTypeEnumType>>
                    {
                        Name = "itemSubType",
                        Description = "Item type"
                    },
                    new QueryArgument<NonNullGraphType<GuidGraphType>>
                    {
                        Name = "orderId",
                        Description = "Order Guid."
                    }
                ),
                resolve: context =>
                {
                    var itemSubType = context.GetArgument<ItemSubType>("itemSubType");
                    var orderId = context.GetArgument<Guid>("orderId");
                    return ShardedShopStateV2.DeriveAddress(itemSubType, orderId);
                }
            );

        }
    }
}
