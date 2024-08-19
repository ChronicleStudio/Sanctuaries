using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace sanctuaries
{
    internal class GuiDialogSanctuaryName : GuiDialogBlockEntity
    {
        long lastRedrawMs;

        string textInput;

        protected override double FloatyDialogPosition => 0.75;

        public GuiDialogSanctuaryName(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
            if (IsDuplicate) return;

            SetupDialog();
        }

        void SetupDialog()
        {

            ElementBounds sancBounds = ElementBounds.Fixed(0, 0, 400, 90);

            ElementBounds textInputBounds = ElementStdBounds.Sign(0, 20, 300, 40);

            ElementBounds buttonBounds = ElementBounds.Fixed(320, 20, 70, 20);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(sancBounds);

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle).WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);

            ClearComposers();

            SingleComposer = capi.Gui
                .CreateCompo("blockentitysanctuary" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle)
                .BeginChildElements(bgBounds)
                    .AddTextInput(textInputBounds, OnEnterName, null, "sanctuaryName")
                    .AddButton("Submit Name", OnTitleBarClose, buttonBounds)
                    .EndChildElements()
                .Compose();

            lastRedrawMs = capi.ElapsedMilliseconds;


        }

        private void OnEnterName(string obj)
        {
            textInput = obj;

        }

        private bool OnTitleBarClose()
        {
            capi.Network.SendBlockEntityPacket<string>(BlockEntityPosition, 7000, textInput);


            return TryClose();

        }

    }
}
