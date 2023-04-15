using ModernUO.Serialization;
using Server.Engines.Craft;
using Server.Menus.ItemLists; // Add this line to reference the BlacksmithMenuGumpGump
using Server;
using Server.Items;
using Server.Network;
using Server.Gumps;

namespace Server.Items;

[Flippable(0xfbb, 0xfbc)]
[SerializationGenerator(0, false)]
public partial class Tongs : BaseTool
{
    [Constructible]
    public Tongs() : base(0xFBB) => Weight = 2.0;

    [Constructible]
    public Tongs(int uses) : base(uses, 0xFBB) => Weight = 2.0;

    public override CraftSystem CraftSystem => DefBlacksmithy.CraftSystem;
	
	public override void OnDoubleClick(Mobile from)
    {
        if (from.AccessLevel >= AccessLevel.Administrator)
        {
            if (from.HasGump<MenuChoiceSmith>())
            {
                from.CloseGump<MenuChoiceSmith>();
            }
    
            from.SendGump(new MenuChoiceSmith(this));
        }
        else
        {
            base.OnDoubleClick(from);
        }
    }

}
