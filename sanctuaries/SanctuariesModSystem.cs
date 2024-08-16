using Cairo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace sanctuaries
{
    public class SanctuariesModSystem : ModSystem
    {
        ICoreAPI _api;             

        public override void Start(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass("ReinforcementSickness", typeof(CollectibleBehaviorReinforcementSickness));
                      

            api.RegisterBlockClass("BlockSanctuary", typeof(BlockSanctuary));
            api.RegisterBlockEntityClass("BESanctuary", typeof(BESanctuary));
            api.RegisterBlockBehaviorClass("BlockBehaviorBlockPlacementSickness", typeof(BlockBehaviorBlockPlacementSickness));
                        
            _api = api;
                    
            base.Start(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Event.ServerRunPhase(EnumServerRunPhase.ModsAndConfigReady, AddBlockPlacementSickness);

            api.Event.PlayerLeave += Event_PlayerLeave;
            api.Event.PlayerJoin += Event_PlayerJoin;

            base.StartServerSide(api);
        }


        private void AddBlockPlacementSickness()
        {
            foreach (Block block in _api.World.Blocks)
            {
                if (!(block.Code == null) && block.Id != 0)
                {
                    block.BlockBehaviors = block.BlockBehaviors.Append(new BlockBehaviorBlockPlacementSickness(block));
                    block.CollectibleBehaviors = block.CollectibleBehaviors.Append(new BlockBehaviorBlockPlacementSickness(block));
                }
            }
        }

        private void Event_PlayerJoin(IServerPlayer byPlayer)
        {
            if (byPlayer.GetModData<int>("BlockPlacementSickness", 0) != 0)
            {
                byPlayer.Entity.SetActivityRunning("BlockPlacementSickness", byPlayer.GetModData<int>("BlockPlacementSickness"));
            }

            if (byPlayer.GetModData<int>("ReinforcementSickness", 0) != 0)
            {
                byPlayer.Entity.SetActivityRunning("ReinforcementSickness", byPlayer.GetModData<int>("ReinforcementSickness"));
            }
        }

        private void Event_PlayerLeave(IServerPlayer byPlayer)
        {
            if (byPlayer.Entity.ActivityTimers.ContainsKey("BlockPlacementSickness"))
            {
                byPlayer.SetModData<int>("BlockPlacementSickness", byPlayer.Entity.RemainingActivityTime("BlockPlacementSickness"));

            }
            if (byPlayer.Entity.ActivityTimers.ContainsKey("ReinforcementSickness"))
            {
                byPlayer.SetModData<int>("ReinforcementSickness", byPlayer.Entity.RemainingActivityTime("ReinforcementSickness"));

            }
        }




    }
}
