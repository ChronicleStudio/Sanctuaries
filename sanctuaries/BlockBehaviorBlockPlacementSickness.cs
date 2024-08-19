using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace sanctuaries
{
    internal class BlockBehaviorBlockPlacementSickness : BlockBehavior
    {
        public BlockBehaviorBlockPlacementSickness(Block block) : base(block)
        {
        }
        //public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        //{
        //    if (byPlayer.Entity.IsActivityRunning("BlockPlacementSickness") /* && byPlayer.WorldData.CurrentGameMode == EnumGameMode.Survival */)
        //    {
        //        handling = EnumHandling.PreventSubsequent;


        //        failureCode = Lang.Get(byPlayer.Entity.WatchedAttributes.GetString("BlockPlacementSicknessCause"));

        //        return false;
        //    }

        //    return true;
        //}


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {

            if (byPlayer.Entity.IsActivityRunning("BlockPlacementSickness") /* && byPlayer.WorldData.CurrentGameMode == EnumGameMode.Survival */)
            {
                handling = EnumHandling.PreventSubsequent;


                failureCode = Lang.Get(byPlayer.Entity.WatchedAttributes.GetString("BlockPlacementSicknessCause"));

                return false;
            }

            return true;


        }
    }
}
