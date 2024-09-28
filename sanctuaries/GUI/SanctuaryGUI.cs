using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Sanctuaries.GUI
{
	public class SanctuaryGUI : GuiDialogBlockEntity
	{
		public ElementBounds backgroundBounds = ElementBounds.FixedSize(700, 500).WithFixedPadding(GuiStyle.ElementToDialogPadding);
		public ElementBounds dialogBounds = ElementBounds.FixedSize(700+40, 500+40).WithAlignment(EnumDialogArea.CenterMiddle).WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, 0);
		public ElementBounds tabBounds = ElementBounds.Fixed(0, -25, 500, 25);


		int curTab = 0;


		private GuiTab[] tabs = new GuiTab[]
		{
			new GuiTab() { Name = Lang.Get("sanctuaries-tab-player"), DataInt = 0},
			new GuiTab() { Name = Lang.Get("sanctuaries-tab-groups"), DataInt = 1}
		};
		


		public SanctuaryGUI(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
		{
			if (IsDuplicate) return;

			SetupDialog();
					
		}

		protected void SetupDialog()
		{
			ClearComposers();



			SingleComposer = capi.Gui
				.CreateCompo("block-entity-sanctuary-" + BlockEntityPosition, dialogBounds)
				.AddShadedDialogBG(backgroundBounds, true)
				.AddHorizontalTabs(tabs, tabBounds, onTabClicked, CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold), CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold), "tabs");
				
				
			if (curTab == 0)
			{
				SingleComposer.AddDialogTitleBar(curTab == 0 ? Lang.Get("Player Permissions") : (Lang.Get("Group Permissions")), OnTitleBarClose)
				.BeginChildElements(backgroundBounds);

				ElementBounds leftColumnBounds;
				ElementBounds adminTextBounds;
				ElementBounds lineEntryTextBounds;
				ElementBounds addOrRemoveTextBounds;
				ElementBounds buttonAdminToggleBounds;
				ElementBounds inputLineEntryBounds;
				ElementBounds buttonAddOrRemoveBounds;
			}

			if (curTab == 1)
			{
				SingleComposer.AddDialogTitleBar(curTab == 0 ? Lang.Get("Player Permissions") : (Lang.Get("Group Permissions")), OnTitleBarClose)
				.BeginChildElements(backgroundBounds);
			}
			


			var tabElem = SingleComposer.GetHorizontalTabs("tabs");
			tabElem.unscaledTabSpacing = 20;
			tabElem.unscaledTabPadding = 10;
			tabElem.activeElement = curTab;

			SingleComposer.Compose();

		}

		private void OnTitleBarClose()
		{
			SetupDialog();
		}

		private void onTabClicked(int tabid)
		{
			curTab = tabid;
			SetupDialog();
		}
	}
}
