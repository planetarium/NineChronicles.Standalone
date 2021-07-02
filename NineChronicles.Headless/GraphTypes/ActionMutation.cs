using Bencodex.Types;
using GraphQL;
using GraphQL.Types;
using Libplanet;
using Libplanet.Assets;
using Libplanet.Blockchain;
using Libplanet.Explorer.GraphTypes;
using Libplanet.Tx;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Lib9c.Model.Order;
using Libplanet.Action;
using Nekoyume.Model.Item;
using NineChronicles.Headless.GraphTypes.States.Models;
using NineChronicles.Headless.GraphTypes.States.Models.Item.Enum;
using NCAction = Libplanet.Action.PolymorphicAction<Nekoyume.Action.ActionBase>;

namespace NineChronicles.Headless.GraphTypes
{
    public class ActionMutation : ObjectGraphType
    {
        public ActionMutation(NineChroniclesNodeService service)
        {
            Field<NonNullGraphType<TxIdType>>("createAvatar",
                description: "Create new avatar.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>
                    {
                        Name = "avatarName",
                        Description = "Avatar name."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "avatarIndex",
                        Description = "The index of character slot. 0 ~ 2"
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "hairIndex",
                        Description = "The index of character hair color. 0 ~ 8"
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "lensIndex",
                        Description = "The index of character eye color. 0 ~ 8"
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "earIndex",
                        Description = "The index of character ear color. 0 ~ 8"
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "tailIndex",
                        Description = "The index of character tail color. 0 ~ 8"
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        if (!(service.MinerPrivateKey is { } privateKey))
                        {
                            throw new InvalidOperationException($"{nameof(service.MinerPrivateKey)} is null.");
                        }

                        if (!(service.Swarm?.BlockChain is { } blockChain))
                        {
                            throw new InvalidOperationException($"{nameof(service.Swarm.BlockChain)} is null.");
                        }

                        var avatarName = context.GetArgument<string>("avatarName");
                        var avatarIndex = context.GetArgument<int>("avatarIndex");
                        var hairIndex = context.GetArgument<int>("hairIndex");
                        var lensIndex = context.GetArgument<int>("lensIndex");
                        var earIndex = context.GetArgument<int>("earIndex");
                        var tailIndex = context.GetArgument<int>("tailIndex");
                        var action = new CreateAvatar
                        {
                            index = avatarIndex,
                            hair = hairIndex,
                            lens = lensIndex,
                            ear = earIndex,
                            tail = tailIndex,
                            name = avatarName,
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(privateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        Log.Error(msg, e);
                        throw;
                    }
                });

            Field<NonNullGraphType<TxIdType>>("hackAndSlash",
                description: "Start stage to get material.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "avatarAddress",
                        Description = "Avatar address."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "worldId",
                        Description = "World ID containing the stage ID."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "stageId",
                        Description = "Stage ID."
                    },
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "weeklyArenaAddress",
                        Description = "Address of this WeeklyArenaState"
                    },
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "rankingArenaAddress",
                        Description = "Address of RankingMapState containing the avatar address."
                    },
                    new QueryArgument<ListGraphType<GuidGraphType>>
                    {
                        Name = "costumeIds",
                        Description = "List of costume id for equip."
                    },
                    new QueryArgument<ListGraphType<GuidGraphType>>
                    {
                        Name = "equipmentIds",
                        Description = "List of equipment id for equip."
                    },
                    new QueryArgument<ListGraphType<GuidGraphType>>
                    {
                        Name = "consumableIds",
                        Description = "List of consumable id for use."
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        BlockChain<NCAction>? blockChain = service.Swarm.BlockChain;
                        if (blockChain is null)
                        {
                            throw new InvalidOperationException($"{nameof(blockChain)} is null.");
                        }

                        Address weeklyArenaAddress = context.GetArgument<Address>("weeklyArenaAddress");
                        Address rankingArenaAddress = context.GetArgument<Address>("rankingArenaAddress");
                        Address avatarAddress = context.GetArgument<Address>("avatarAddress");
                        int worldId = context.GetArgument<int>("worldId");
                        int stageId = context.GetArgument<int>("stageId");
                        List<Guid> costumeIds = context.GetArgument<List<Guid>>("costumeIds") ?? new List<Guid>();
                        List<Guid> equipmentIds = context.GetArgument<List<Guid>>("equipmentIds") ?? new List<Guid>();
                        List<Guid> consumableIds = context.GetArgument<List<Guid>>("consumableIds") ?? new List<Guid>();

                        var action = new HackAndSlash
                        {
                            avatarAddress = avatarAddress,
                            worldId = worldId,
                            stageId = stageId,
                            WeeklyArenaAddress = weeklyArenaAddress,
                            RankingMapAddress = rankingArenaAddress,
                            costumes = costumeIds,
                            equipments = equipmentIds,
                            foods = consumableIds,
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(service.MinerPrivateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        Log.Error(msg, e);
                        throw;
                    }
                });

            Field<NonNullGraphType<TxIdType>>("combinationEquipment",
                description: "Combine new equipment.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "avatarAddress",
                        Description = "Avatar address to create equipment."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "recipeId",
                        Description = "EquipmentRecipe ID from EquipmentRecipeSheet."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "slotIndex",
                        Description =  "The empty combination slot index to combine equipment. 0 ~ 3"
                    },
                    new QueryArgument<IntGraphType>
                    {
                        Name = "subRecipeId",
                        Description = "EquipmentSubRecipe ID from EquipmentSubRecipeSheet."
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        BlockChain<NCAction>? blockChain = service.BlockChain;
                        if (blockChain is null)
                        {
                            throw new InvalidOperationException($"{nameof(blockChain)} is null.");
                        }

                        int recipeId = context.GetArgument<int>("recipeId");
                        int slotIndex = context.GetArgument<int>("slotIndex");
                        int? subRecipeId = context.GetArgument<int?>("subRecipeId");
                        Address avatarAddress = context.GetArgument<Address>("avatarAddress");

                        var action = new CombinationEquipment
                        {
                            AvatarAddress = avatarAddress,
                            RecipeId = recipeId,
                            SlotIndex = slotIndex,
                            SubRecipeId = subRecipeId
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(service.MinerPrivateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        Log.Error(msg, e);
                        throw;
                    }
                });

            Field<NonNullGraphType<TxIdType>>("itemEnhancement",
                description: "Upgrade equipment.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "avatarAddress",
                        Description = "Avatar address to upgrade equipment."
                    },
                    new QueryArgument<NonNullGraphType<GuidGraphType>>
                    {
                        Name = "itemId",
                        Description = "Equipment Guid for upgrade."
                    },
                    new QueryArgument<NonNullGraphType<GuidGraphType>>
                    {
                        Name = "materialId",
                        Description = "Material Guid for equipment upgrade."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "slotIndex",
                        Description =  "The empty combination slot index to upgrade equipment. 0 ~ 3"
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        if (!(service.MinerPrivateKey is { } privatekey))
                        {
                            throw new InvalidOperationException($"{nameof(service.MinerPrivateKey)} is null.");
                        }

                        if (!(service.Swarm?.BlockChain is { } blockChain))
                        {
                            throw new InvalidOperationException($"{nameof(service.Swarm.BlockChain)} is null.");
                        }

                        Guid itemId = context.GetArgument<Guid>("itemId");
                        Guid materialId = context.GetArgument<Guid>("materialId");
                        Address avatarAddress = context.GetArgument<Address>("avatarAddress");
                        int slotIndex = context.GetArgument<int>("slotIndex");

                        var action = new ItemEnhancement
                        {
                            avatarAddress = avatarAddress,
                            slotIndex = slotIndex,
                            itemId = itemId,
                            materialId = materialId,
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(privatekey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        Log.Error(msg, e);
                        throw;
                    }
                });

            Field<NonNullGraphType<TxIdType>>("buy",
                description: "Buy registered shop item.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "buyerAvatarAddress",
                        Description = "Avatar address."
                    },
                    new QueryArgument<NonNullGraphType<ListGraphType<GuidGraphType>>>
                    {
                        Name = "orderIds",
                        Description = "Order Guid list from registered item."
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        if (!(service.MinerPrivateKey is { } privateKey))
                        {
                            throw new InvalidOperationException($"{nameof(service.MinerPrivateKey)} is null.");
                        }

                        if (!(service.Swarm?.BlockChain is { } blockChain))
                        {
                            throw new InvalidOperationException($"{nameof(service.Swarm.BlockChain)} is null.");
                        }

                        Address buyerAvatarAddress = context.GetArgument<Address>("buyerAvatarAddress");
                        List<Guid> orderIds = context.GetArgument<List<Guid>>("orderIds");

                        var purchaseInfos = new List<PurchaseInfo>();
                        foreach (var orderId in orderIds)
                        {
                            var rawOrder = blockChain.GetState(Order.DeriveAddress(orderId));
                            if (rawOrder is Dictionary dict)
                            {
                                var order = OrderFactory.Deserialize(dict);
                                var purchaseInfo = new PurchaseInfo(orderId, order.TradableId, order.SellerAgentAddress,
                                    order.SellerAvatarAddress, order.ItemSubType, order.Price);
                                purchaseInfos.Add(purchaseInfo);
                            }
                            else
                            {
                                throw new OrderIdDoesNotExistException($"Can't find {orderId}");
                            }
                        }

                        var action = new Buy
                        {
                            buyerAvatarAddress = buyerAvatarAddress,
                            purchaseInfos = purchaseInfos,
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(privateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        Log.Error(msg, e);
                        throw;
                    }
                });
            Field<NonNullGraphType<TxIdType>>("sell",
                description: "Register item to the shop.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "sellerAvatarAddress",
                        Description = "Avatar address to register shop item."
                    },
                    new QueryArgument<NonNullGraphType<GuidGraphType>>
                    {
                        Name = "tradableId",
                        Description = "Item Guid to register on shop."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "price",
                        Description = "Item selling price."
                    },
                    new QueryArgument<NonNullGraphType<ItemSubTypeEnumType>>
                    {
                        Name = "itemSubType",
                        Description = "Item type."
                    },
                    new QueryArgument<IntGraphType>
                    {
                        Name = "count",
                        Description = "Item selling count. default 1."
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        if (!(service.MinerPrivateKey is { } privateKey))
                        {
                            throw new InvalidOperationException($"{nameof(service.MinerPrivateKey)} is null.");
                        }

                        if (!(service.Swarm?.BlockChain is { } blockChain))
                        {
                            throw new InvalidOperationException($"{nameof(service.Swarm.BlockChain)} is null.");
                        }

                        Address sellerAvatarAddress = context.GetArgument<Address>("sellerAvatarAddress");
                        Guid tradableId = context.GetArgument<Guid>("tradableId");
                        var currency = new GoldCurrencyState(
                            (Dictionary)blockChain.GetState(GoldCurrencyState.Address)
                        ).Currency;
                        FungibleAssetValue price = currency * context.GetArgument<int>("price");
                        var itemSubtype = context.GetArgument<ItemSubType>("itemSubType");
                        var count = context.GetArgument("count", 1);

                        switch (itemSubtype)
                        {
                            case ItemSubType.Food:
                            case ItemSubType.FullCostume:
                            case ItemSubType.HairCostume:
                            case ItemSubType.EarCostume:
                            case ItemSubType.EyeCostume:
                            case ItemSubType.TailCostume:
                            case ItemSubType.Weapon:
                            case ItemSubType.Armor:
                            case ItemSubType.Belt:
                            case ItemSubType.Necklace:
                            case ItemSubType.Ring:
                            case ItemSubType.Title:
                            case ItemSubType.EquipmentMaterial:
                                if (count != 1)
                                {
                                    throw new InvalidItemCountException();
                                }
                                break;
                            case ItemSubType.Hourglass:
                            case ItemSubType.ApStone:
                                break;
                            default:
                                throw new InvalidItemTypeException($"{itemSubtype} does not support yet.");
                        }

                        var action = new Sell
                        {
                            sellerAvatarAddress = sellerAvatarAddress,
                            tradableId = tradableId,
                            price = price,
                            count = count,
                            itemSubType = itemSubtype,
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(privateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        throw;
                    }
                });

            Field<NonNullGraphType<TxIdType>>("dailyReward",
                description: "Get daily reward.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "avatarAddress",
                        Description = "Avatar address to receive reward."
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        if (!(service.MinerPrivateKey is { } privateKey))
                        {
                            throw new InvalidOperationException($"{nameof(service.MinerPrivateKey)} is null.");
                        }

                        if (!(service.BlockChain is { } blockChain))
                        {
                            throw new InvalidOperationException($"{nameof(service.Swarm.BlockChain)} is null.");
                        }

                        Address avatarAddress = context.GetArgument<Address>("avatarAddress");

                        var action = new DailyReward
                        {
                            avatarAddress = avatarAddress
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(privateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        Log.Error(msg, e);
                        throw;
                    }
                });

            Field<NonNullGraphType<TxIdType>>("combinationConsumable",
                description: "Combine new Consumable.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "avatarAddress",
                        Description = "Avatar address to combine consumable."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "recipeId",
                        Description = "ConsumableRecipe ID from ConsumableRecipeSheet."
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "slotIndex",
                        Description =  "The empty combination slot index to combine consumable. 0 ~ 3"
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        BlockChain<NCAction>? blockChain = service.BlockChain;
                        if (blockChain is null)
                        {
                            throw new InvalidOperationException($"{nameof(blockChain)} is null.");
                        }

                        int recipeId = context.GetArgument<int>("recipeId");
                        int slotIndex = context.GetArgument<int>("slotIndex");
                        Address avatarAddress = context.GetArgument<Address>("avatarAddress");

                        var action = new CombinationConsumable
                        {
                            AvatarAddress = avatarAddress,
                            recipeId = recipeId,
                            slotIndex = slotIndex,
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(service.MinerPrivateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        throw;
                    }
                }
            );

            Field<NonNullGraphType<TxIdType>>(nameof(MonsterCollect),
                description: "Start monster collect.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "level",
                        Description = "The monster collection level.(1 ~ 7)"
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        BlockChain<NCAction>? blockChain = service.BlockChain;
                        if (blockChain is null)
                        {
                            throw new InvalidOperationException($"{nameof(blockChain)} is null.");
                        }

                        if (service.MinerPrivateKey is null)
                        {
                            throw new InvalidOperationException($"{nameof(service.MinerPrivateKey)} is null.");
                        }

                        int level = context.GetArgument<int>("level");
                        Address agentAddress = service.MinerPrivateKey.ToAddress();
                        AgentState agentState = new AgentState((Dictionary) service.BlockChain.GetState(agentAddress));

                        Address collectionAddress = MonsterCollectionState.DeriveAddress(agentAddress, agentState.MonsterCollectionRound);
                        if (service.BlockChain.GetState(collectionAddress) is { })
                        {
                            throw new InvalidOperationException("MonsterCollectionState already exists.");
                        }
                        var action = new MonsterCollect
                        {
                            level = level,
                        };

                        var actions = new NCAction[] { action };
                        Transaction<PolymorphicAction<ActionBase>> tx = blockChain.MakeTransaction(service.MinerPrivateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        throw;
                    }
                }
            );

            Field<NonNullGraphType<TxIdType>>(nameof(ClaimMonsterCollectionReward),
                description: "Get monster collection reward.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AddressType>>
                    {
                        Name = "avatarAddress",
                        Description = "Address of avatar for get reward."
                    }
                ),
                resolve: context =>
                {
                    try
                    {
                        BlockChain<NCAction>? blockChain = service.BlockChain;
                        if (blockChain is null)
                        {
                            throw new InvalidOperationException($"{nameof(blockChain)} is null.");
                        }


                        if (service.MinerPrivateKey is null)
                        {
                            throw new InvalidOperationException($"{nameof(service.MinerPrivateKey)} is null.");
                        }

                        Address avatarAddress = context.GetArgument<Address>("avatarAddress");
                        Address agentAddress = service.MinerPrivateKey.ToAddress();
                        AgentState agentState = new AgentState((Dictionary) service.BlockChain.GetState(agentAddress));

                        var action = new ClaimMonsterCollectionReward
                        {
                            avatarAddress = avatarAddress,
                        };

                        var actions = new NCAction[] { action };
                        Transaction<NCAction> tx = blockChain.MakeTransaction(service.MinerPrivateKey, actions);
                        return tx.Id;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
                        context.Errors.Add(new ExecutionError(msg, e));
                        throw;
                    }
                }
            );

            // Field<NonNullGraphType<TxIdType>>(nameof(CancelMonsterCollect),
            //     description: "Downgrade monster collection level.",
            //     arguments: new QueryArguments(
            //         new QueryArgument<NonNullGraphType<IntGraphType>>
            //         {
            //             Name = "level",
            //             Description = "The monster collection level.(1 ~ 6)"
            //         }
            //     ),
            //     resolve: context =>
            //     {
            //         try
            //         {
            //             BlockChain<NCAction>? blockChain = service.BlockChain;
            //             if (blockChain is null)
            //             {
            //                 throw new InvalidOperationException($"{nameof(blockChain)} is null.");
            //             }
            //
            //             if (service.MinerPrivateKey is null)
            //             {
            //                 throw new InvalidOperationException($"{nameof(service.MinerPrivateKey)} is null.");
            //             }
            //
            //             int level = context.GetArgument<int>("level");
            //             Address agentAddress = service.MinerPrivateKey.ToAddress();
            //             AgentState agentState = new AgentState((Dictionary) service.BlockChain.GetState(agentAddress));
            //
            //             var action = new CancelMonsterCollect
            //             {
            //                 level = level,
            //                 collectRound = agentState.MonsterCollectionRound,
            //             };
            //
            //             var actions = new PolymorphicAction<ActionBase>[] { action };
            //             Transaction<PolymorphicAction<ActionBase>> tx = blockChain.MakeTransaction(service.MinerPrivateKey, actions);
            //             return tx.Id;
            //         }
            //         catch (Exception e)
            //         {
            //             var msg = $"Unexpected exception occurred during {typeof(ActionMutation)}: {e}";
            //             context.Errors.Add(new ExecutionError(msg, e));
            //             throw;
            //         }
            //     }
            // );
        }
    }
}
