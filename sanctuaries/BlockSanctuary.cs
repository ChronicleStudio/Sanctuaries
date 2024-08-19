using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace sanctuaries
{
    internal class BlockSanctuary : Block
    {
        WorldInteraction[] interactions;

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BESanctuary sanctuary = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BESanctuary;
            return sanctuary?.OnInteract(byPlayer) == true;
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "sanctuariesBlockInteractions", () =>
            {
                return new WorldInteraction[]{
                    new WorldInteraction() {
                        RequireFreeHand = true,
                        ActionLangCode = "blockhelp-sanctuaries-activate",
                        HotKeyCode = "shift",
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) => {
                            BESanctuary bes = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BESanctuary;
                            if (bes == null) return false;
                            return bes?.activated == false;
                        }
                    },
                    new WorldInteraction()
                    {
                        RequireFreeHand = true,
                        ActionLangCode = "blockhelp-sanctuaries-name",
                        HotKeyCodes = new string[] { "shift", "ctrl" },
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) =>
                        {
                            BESanctuary bes = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BESanctuary;
                            if (bes == null) return false;
                            return bes?.sanctuaryName != null;
                        }
                    }
                };


            });
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {

            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }


}
