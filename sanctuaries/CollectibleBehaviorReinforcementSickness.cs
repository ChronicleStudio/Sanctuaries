using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace sanctuaries
{
    internal class CollectibleBehaviorReinforcementSickness : CollectibleBehavior
    {
        public CollectibleBehaviorReinforcementSickness(CollectibleObject collObj) : base(collObj)
        {

        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            //base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
            if (byEntity.IsActivityRunning("ReinforcementSickness"))
            {
                if (byEntity is EntityPlayer)
                {

                    if (byEntity.Api.Side == EnumAppSide.Client)
                    {
                        (byEntity.Api as ICoreClientAPI).TriggerIngameError((byEntity as EntityPlayer).Player, "could not reinforce", byEntity.WatchedAttributes.GetString("ReinforcementSicknessCause"));
                    }
                }

                handHandling = EnumHandHandling.PreventDefault;

            }

        }
    }
}
