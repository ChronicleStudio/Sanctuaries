using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace sanctuaries
{



    internal class BESanctuary : BlockEntity
    {
        GuiDialogSanctuaryName sancDialog;

        public int radius, vertRange, sicknessDuration, tickRate, foodContainerRange, maxFoodContainers, consumeFoodRate;

        public bool activated = false;
        public bool underSiege = false;

        public bool running => currentSaturation > 0;
        public float currentSaturation;
        public float maxSaturation;
        public float saturationConsumptionPerPlayer;

        public string currentOwnerUID = "";
        public string currentOwnerName = "";

        public string sanctuaryName;

        public string DialogTitle => Lang.Get("Sanctuary");

        public long? consumeFoodListenerID;
        public long? BPSListenerID;
        public long? foodContainerListenerID;

        public long lastLocatePing = 0;
        public long lastSiegePing = 0;

        public List<BlockPos> foodContainers;


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);



            if (Block.Attributes["radius"].Exists)
            {
                radius = Block.Attributes["radius"].AsInt();
            }
            if (Block.Attributes["vertRange"].Exists)
            {
                vertRange = Block.Attributes["vertRange"].AsInt();
            }

            if (Block.Attributes["sicknessDuration"].Exists)
            {
                sicknessDuration = Block.Attributes["sicknessDuration"].AsInt();
            }
            if (Block.Attributes["tickRate"].Exists)
            {
                tickRate = Block.Attributes["tickRate"].AsInt();
            }

            if (Block.Attributes["foodContainerRange"].Exists)
            {
                foodContainerRange = Block.Attributes["foodContainerRange"].AsInt();
            }
            if (Block.Attributes["maxFoodContainers"].Exists)
            {
                maxFoodContainers = Block.Attributes["maxFoodContainers"].AsInt();
            }
            if (Block.Attributes["consumeFoodRate"].Exists)
            {
                consumeFoodRate = Block.Attributes["consumeFoodRate"].AsInt();
            }
            if (Block.Attributes["maxSaturation"].Exists)
            {
                maxSaturation = Block.Attributes["maxSaturation"].AsInt();
            }
            if (Block.Attributes["saturationConsumptionPerPlayer"].Exists)
            {
                saturationConsumptionPerPlayer = Block.Attributes["saturationConsumptionPerPlayer"].AsInt();
            }


            consumeFoodListenerID = null;
            BPSListenerID = null;
            foodContainerListenerID = null;




            foodContainers = new List<BlockPos>();

            if (activated)
            {
                TryActivate(true);
            }

        }


        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetBool("activated", activated);
            tree.SetBool("underSiege", underSiege);
            tree.SetFloat("currentSaturation", currentSaturation);
            tree.SetString("currentOwnerUID", currentOwnerUID);
            tree.SetString("currentOwnerName", currentOwnerName);
            tree.SetString("sanctuaryName", sanctuaryName);


            int counter = 0;
            foreach (BlockPos pos in foodContainers)
            {
                tree.SetBlockPos("foodContainer" + counter, pos);
                counter++;
            }

            tree.SetInt("numberOfContainers", counter);


        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            activated = tree.GetBool("activated");
            underSiege = tree.GetBool("underSiege");
            currentSaturation = tree.GetFloat("currentSaturation");
            currentOwnerUID = tree.GetString("currentOwnerUID");
            currentOwnerName = tree.GetString("currentOwnerName");

            sanctuaryName = tree.GetString("sanctuaryName");


            foodContainers = new List<BlockPos>();
            int containers = tree.GetInt("numberOfContainers");

            if (containers > 0)
            {
                for (int i = 0; i < containers; i++)
                {
                    foodContainers.Add(tree.GetBlockPos("foodContainer" + i));
                }
            }
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            base.OnReceivedClientPacket(fromPlayer, packetid, data);

            if (packetid == 7000)
            {

                sanctuaryName = System.Text.Encoding.Default.GetString(data, 2, data.Length - 2);

            }
        }


        public bool OnInteract(IPlayer byPlayer)
        {
            if (Api.Side == EnumAppSide.Server && byPlayer.Entity.Controls.ShiftKey)
            {
                if (TryActivate(false))
                {
                    currentOwnerUID = byPlayer.PlayerUID;
                    currentOwnerName = byPlayer.PlayerName;
                    MarkDirty();
                    return true;
                };


            }

            if (Api.Side == EnumAppSide.Client && byPlayer.Entity.Controls.ShiftKey)
            {

                if (!activated)
                {
                    return true;
                }

                if (byPlayer.Entity.Controls.ShiftKey && byPlayer.Entity.Controls.CtrlKey && sanctuaryName == "" && (sancDialog == null || !sancDialog.IsOpened()))
                {
                    sancDialog = new GuiDialogSanctuaryName(DialogTitle, null, Pos, Api as ICoreClientAPI);
                    sancDialog.TryOpen();
                    sancDialog.OnClosed += () => sancDialog = null;

                }

            }

            return false;

        }




        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (sanctuaryName != null)
            {
                dsc.AppendLine("The Sanctuary of " + sanctuaryName);
            }
            if (activated)
            {
                dsc.AppendLine("Activated by: " + currentOwnerName);
            }
            else
            {
                dsc.AppendLine("Not Activated! RMB to activate.");
            }
            //if (IsRunning())
            //{
            //    dsc.AppendLine("Sanctuary currently running! Preventing Block Placement of all enemies!");

            //}

            //if (underSiege)
            //{
            //    dsc.AppendLine("Sanctuary currently being sieged! Man your posts!");                
            //}

            dsc.AppendLine("Range: " + radius + " / " + "Vertical Range:" + vertRange);
            dsc.AppendLine("Current Saturation: " + currentSaturation + " / " + maxSaturation);
            dsc.AppendLine("Food Container Range: " + foodContainerRange);

            //if (foodContainers != null && foodContainers.Count > 0)
            //{
            //    dsc.AppendLine("Containers currently feeding Sanctuary: " + foodContainers.Count);
            //}


            base.GetBlockInfo(forPlayer, dsc);
        }

        public bool IsRunning()
        {
            return running;
        }


        private void OnBlockPlacementSickness(float obj)
        {
            if (!IsRunning()) return;
            bool enemiesLocated = false;

            IPlayer[] players = Api.World.GetPlayersAround(Pos.ToVec3d(), radius * radius, vertRange);
            if (players == null) { return; }
            foreach (IPlayer p in players)
            {
                if (underSiege)
                {
                    if (Api.Side == EnumAppSide.Server && Api.World.Calendar.ElapsedSeconds - lastSiegePing > 1000)
                    {
                        (Api as ICoreServerAPI).SendIngameDiscovery((Api.World.PlayerByUid(currentOwnerUID) as IServerPlayer), "sanctuary-undersiege",
                            (sanctuaryName == null ? "Sanctuary is under SIEGE!" : "The Sanctuary of " + sanctuaryName + " is under attack!"));
                        lastSiegePing = Api.World.Calendar.ElapsedSeconds;

                        ModSystemBlockReinforcement reinforceMod = Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();
                        if (reinforceMod.IsReinforced(Pos))
                        {
                            foreach (IPlayer player in (Api as ICoreServerAPI).Groups.PlayerGroupsById.GetValueOrDefault(reinforceMod.GetReinforcment(Pos).GroupUid).OnlinePlayers)
                            {
                                (Api as ICoreServerAPI).SendIngameDiscovery((Api.World.PlayerByUid(player.PlayerUID) as IServerPlayer), "sanctuary-undersiege",
                                    (sanctuaryName == null ? "Sanctuary is under SIEGE!" : "The Sanctuary of " + sanctuaryName + " is under attack!"));
                            }
                        }
                    }

                    GiveReinforcementSickness(p, "sanctuary-reinforcement-sickness-undersiege");

                    if (!HasPermission(p))
                    {
                        enemiesLocated = true;
                        currentSaturation -= saturationConsumptionPerPlayer;

                        GiveBlockPlacementSickness(p, "sanctuary-blockplacement-sickness");
                    }
                }

                if (!underSiege && !HasPermission(p))
                {
                    GiveBlockPlacementSickness(p, "sanctuary-blockplacement-sickness");

                    GiveReinforcementSickness(p, "sanctuary-reinforcement-sickness");

                    currentSaturation -= saturationConsumptionPerPlayer;
                    enemiesLocated = true;

                }
            }
            if (!enemiesLocated && underSiege)
            {
                underSiege = false;
            }
            if (enemiesLocated && !underSiege)
            {

                underSiege = true;
                if (Api.Side == EnumAppSide.Server)
                {
                    (Api as ICoreServerAPI).SendIngameDiscovery((Api.World.PlayerByUid(currentOwnerUID) as IServerPlayer), "sanctuary-undersiege", "Sanctuary is under SIEGE!");
                    lastSiegePing = Api.World.Calendar.ElapsedSeconds;

                    ModSystemBlockReinforcement reinforceMod = Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();
                    if (reinforceMod.IsReinforced(Pos))
                    {
                        foreach (IPlayer player in (Api as ICoreServerAPI).Groups.PlayerGroupsById.GetValueOrDefault(reinforceMod.GetReinforcment(Pos).GroupUid).OnlinePlayers)
                        {
                            (Api as ICoreServerAPI).SendIngameDiscovery((Api.World.PlayerByUid(player.PlayerUID) as IServerPlayer), "sanctuary-undersiege", "Sanctuary is under SIEGE!");
                        }
                    }

                }



            }

            MarkDirty();
        }

        private void GiveBlockPlacementSickness(IPlayer player, string cause)
        {
            player.Entity.SetActivityRunning("BlockPlacementSickness", sicknessDuration);
            player.Entity.WatchedAttributes.SetString("BlockPlacementSicknessCause", cause);

            if (player.Entity.Controls.Gliding)
            {
                var inv = player.InventoryManager.GetOwnInventory(GlobalConstants.backpackInvClassName);
                foreach (var slot in inv)
                {
                    if (!(slot is ItemSlotBackpack)) continue;
                    if (slot.Itemstack?.Collectible is ItemGlider)
                    {
                        ItemStack glider = slot.TakeOut(1);
                        player.Entity.Api.World.SpawnItemEntity(glider, new Vec3d(player.Entity.SidedPos));
                    }
                }
            }
        }

        private void GiveReinforcementSickness(IPlayer player, string cause)
        {
            player.Entity.SetActivityRunning("ReinforcementSickness", sicknessDuration);
            player.Entity.WatchedAttributes.SetString("ReinforcementSicknessCause", cause);
        }


        private bool HasPermission(IPlayer player)
        {
            if (player == null) return false;
            if (player.PlayerName == currentOwnerName) return true;
            if (player.PlayerUID == currentOwnerUID) return true;
            ModSystemBlockReinforcement reinforceMod = Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();
            if (reinforceMod.IsReinforced(Pos))
            {
                if (player.GetGroup(reinforceMod.GetReinforcment(Pos).GroupUid) != null)
                {
                    return true;
                }
            }


            return false;
        }

        private bool TryActivate(bool fromLoad)
        {
            if (!fromLoad && activated) return false;
            activated = true;


            if (consumeFoodListenerID == null)
            {
                consumeFoodListenerID = RegisterGameTickListener(ConsumeNearbyFood, consumeFoodRate * 1000, 5000);
            }
            if (BPSListenerID == null)
            {
                BPSListenerID = RegisterGameTickListener(OnBlockPlacementSickness, tickRate, 3000);
            }
            if (foodContainerListenerID == null)
            {
                LocateContainers(0.0f);
                foodContainerListenerID = RegisterGameTickListener(LocateContainers, 30000);
            }


            MarkDirty();
            return true;

        }


        private void ConsumeNearbyFood(float obj)
        {

            if (currentSaturation > maxSaturation - 100) { return; }
            if (foodContainers.Count <= 1 && (Api.World.Calendar.ElapsedSeconds - lastLocatePing > 15)) LocateContainers(0.0f);

            List<BlockPos> posToRemove = new List<BlockPos>();



            foreach (BlockPos pos in foodContainers)
            {
                bool foundFood = false;
                Block containerBlock = Api.World.GetBlockAccessor(false, false, false).GetBlock(pos);
                BlockEntityContainer container = containerBlock?.GetBlockEntity<BlockEntityContainer>(pos);
                if (container == null)
                {
                    posToRemove.Add(pos);
                    continue;
                }

                foreach (ItemSlot slot in container.Inventory)
                {

                    if (!slot.Empty && slot.Itemstack.Collectible.NutritionProps != null)
                    {
                        foundFood = true;
                        currentSaturation += slot.Itemstack.Collectible.NutritionProps.Satiety;
                        if (currentSaturation > maxSaturation) currentSaturation = maxSaturation;

                        slot.TakeOut(1);
                        slot.MarkDirty();
                        break;
                    }
                }
                if (!foundFood)
                {
                    posToRemove.Add(pos);
                }
                else
                {
                    break;
                }

            }

            foreach (BlockPos blPos in posToRemove)
            {
                foodContainers.Remove(blPos);
            }

            MarkDirty();

        }

        private void LocateContainers(float obj)
        {
            int counter = foodContainers.Count;
            if (counter >= maxFoodContainers) return;

            List<BlockPos> list = new List<BlockPos>();

            Api.World.GetBlockAccessor(false, false, false).SearchBlocks(Pos.DownCopy(foodContainerRange).NorthCopy(foodContainerRange).WestCopy(foodContainerRange), Pos.UpCopy(foodContainerRange).SouthCopy(foodContainerRange).EastCopy(foodContainerRange), (tempBlock, tempPos) =>
            {
                if (tempBlock == null) return true;
                if (tempBlock is BlockGenericTypedContainer)
                {
                    BlockEntityContainer container = tempBlock.GetBlockEntity<BlockEntityContainer>(tempPos);
                    if (container.Inventory.Empty) return true;
                    bool containsFood = false;
                    foreach (ItemSlot slot in container.Inventory)
                    {
                        if (slot.Empty) continue;
                        if (slot.Itemstack.Collectible.NutritionProps != null)
                        {
                            containsFood = true;
                            break;
                        }
                    }
                    if (containsFood)
                    {
                        list.Add(tempPos.Copy());
                        counter++;
                    }
                }
                if (counter >= maxFoodContainers)
                {
                    return false;
                }

                return true;
            });

            foodContainers = list;
            lastLocatePing = Api.World.Calendar.ElapsedSeconds;


            MarkDirty();
        }


    }
}
