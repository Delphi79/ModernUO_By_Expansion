using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using ModernUO.Serialization;
using Server.Engines.Craft;
using Server.Menus.ItemLists; // Add this line to reference the BlacksmithMenuGumpGump
using Server.Items;

namespace Server.Gumps
{
    public class MenuChoiceSmith : Gump
    {
        private readonly BaseTool m_Tool;

        public static void Initialize()
        {
            CommandSystem.Register("MenuChoiceSmith", AccessLevel.Administrator, new CommandEventHandler(MenuChoiceSmith_OnCommand));
        }

        [Usage("MenuChoiceSmith")]
        [Description("Makes a call to your custom gump.")]
        public static void MenuChoiceSmith_OnCommand(CommandEventArgs e)
        {
            if (e.Mobile.FindItemOnLayer(Layer.OneHanded) is BaseTool tool)
            {
                e.Mobile.SendGump(new MenuChoiceSmith(tool));
            }
            else
            {
                e.Mobile.SendMessage("You must have a smithing tool equipped to access the menu.");
            }
        }

        public MenuChoiceSmith(BaseTool tool) : base(0, 0)
        {
            m_Tool = tool;
        
            this.Closable = true;
            this.Disposable = true;
            this.Draggable = true;
            this.Resizable = false;
        
            AddPage(0);
            AddBackground(284, 203, 241, 149, 2620);
            AddAlphaRegion(290, 212, 229, 131);
            AddLabel(343, 316, 1150, @"Classic");
            AddLabel(457, 316, 1150, @"Modern");
            AddButton(312, 306, 2151, 2154, 0, GumpButtonType.Reply, 0);
            AddButton(427, 306, 2151, 2154, 1, GumpButtonType.Reply, 0);
            AddLabel(318, 221, 1150, @"Please choose a menu style:");
            AddItem(499, 331, 4017);
            AddImage(373, 249, 5555);
            AddItem(264, 192, 7147);
            AddItem(263, 194, 4022);
            AddItem(489, 327, 4021);
            AddItem(507, 341, 4024);
			
			
        }
		
		/* public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 0: // Classic button clicked
                {
                    from.SendGump(new BlacksmithMenu(from, m_Tool));
                    break;
                }
                case 1: // Modern button clicked
                {
                    CraftSystem craftSystem = DefBlacksmithy.CraftSystem;
                    from.SendGump(new CraftGump(from, craftSystem, m_Tool, null));
                    break;
                }
            }
        } */
    }
}
