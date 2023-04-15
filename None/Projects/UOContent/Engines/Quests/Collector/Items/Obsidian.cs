using System.Collections.Generic;
using Server.ContextMenus;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.Quests.Collector
{
    public class Obsidian : Item
    {
        private const int m_Partial = 2;
        private const int m_Completed = 10;

        private static readonly string[] m_Names =
        {
            null,
            "an aggressive cavalier",
            "a beguiling rogue",
            "a benevolent physician",
            "a brilliant artisan",
            "a capricious adventurer",
            "a clever beggar",
            "a convincing charlatan",
            "a creative inventor",
            "a creative tinker",
            "a cunning knave",
            "a dauntless explorer",
            "a despicable ruffian",
            "an earnest malcontent",
            "an exultant animal tamer",
            "a famed adventurer",
            "a fanatical crusader",
            "a fastidious clerk",
            "a fearless hunter",
            "a festive harlequin",
            "a fidgety assassin",
            "a fierce soldier",
            "a fierce warrior",
            "a frugal magnate",
            "a glib pundit",
            "a gnomic shaman",
            "a graceful noblewoman",
            "a idiotic madman",
            "a imaginative designer",
            "an inept conjurer",
            "an innovative architect",
            "an inventive blacksmith",
            "a judicious mayor",
            "a masterful chef",
            "a masterful woodworker",
            "a melancholy clown",
            "a melodic bard",
            "a merciful guard",
            "a mirthful jester",
            "a nervous surgeon",
            "a peaceful scholar",
            "a prolific gardener",
            "a quixotic knight",
            "a regal aristocrat",
            "a resourceful smith",
            "a reticent alchemist",
            "a sanctified priest",
            "a scheming patrician",
            "a shrewd mage",
            "a singing minstrel",
            "a skilled tailor",
            "a squeamish assassin",
            "a stoic swordsman",
            "a studious scribe",
            "a thought provoking writer",
            "a treacherous scoundrel",
            "a troubled poet",
            "an unflappable wizard",
            "a valiant warrior",
            "a wayward fool"
        };

        private int m_Quantity;
        private string m_StatueName;

        [Constructible]
        public Obsidian() : base(0x1EA7)
        {
            Hue = 0x497;

            m_Quantity = 1;
            m_StatueName = "";
        }

        public Obsidian(Serial serial) : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Quantity
        {
            get => m_Quantity;
            set
            {
                if (value <= 1)
                {
                    m_Quantity = 1;
                }
                else if (value >= m_Completed)
                {
                    m_Quantity = m_Completed;
                }
                else
                {
                    m_Quantity = value;
                }

                if (m_Quantity < m_Partial)
                {
                    ItemID = 0x1EA7;
                }
                else if (m_Quantity < m_Completed)
                {
                    ItemID = 0x1F13;
                }
                else
                {
                    ItemID = 0x12CB;
                }

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string StatueName
        {
            get => m_StatueName;
            set
            {
                m_StatueName = value;
                InvalidateProperties();
            }
        }

        public override bool ForceShowProperties => ObjectPropertyList.Enabled;

        public static string RandomName(Mobile from) => m_Names.RandomElement() ?? from.Name;

        public override void AddNameProperty(IPropertyList list)
        {
            if (m_Quantity < m_Partial)
            {
                list.Add(1055137); // a section of an obsidian statue
            }
            else if (m_Quantity < m_Completed)
            {
                list.Add(1055138); // a partially reconstructed obsidian statue
            }
            else
            {
                list.Add(1055139, m_StatueName); // an obsidian statue of ~1_STATUE_NAME~
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            if (m_Quantity < m_Partial)
            {
                LabelTo(from, 1055137); // a section of an obsidian statue
            }
            else if (m_Quantity < m_Completed)
            {
                LabelTo(from, 1055138); // a partially reconstructed obsidian statue
            }
            else
            {
                LabelTo(from, 1055139, m_StatueName); // an obsidian statue of ~1_STATUE_NAME~
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive && m_Quantity >= m_Partial && m_Quantity < m_Completed && IsChildOf(from.Backpack))
            {
                list.Add(new DisassembleEntry(this));
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Quantity < m_Completed)
            {
                if (!IsChildOf(from.Backpack))
                {
                    from.NetState.SendMessageLocalized(
                        Serial,
                        ItemID,
                        MessageType.Regular,
                        0x2C,
                        3,
                        500309
                    ); // Nothing Happens.
                }
                else
                {
                    from.Target = new InternalTarget(this);
                }
            }
        }

        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version

            writer.WriteEncodedInt(m_Quantity);
            writer.Write(m_StatueName);
        }

        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();

            m_Quantity = reader.ReadEncodedInt();
            m_StatueName = Utility.Intern(reader.ReadString());
        }

        private class DisassembleEntry : ContextMenuEntry
        {
            private readonly Obsidian m_Obsidian;

            public DisassembleEntry(Obsidian obsidian) : base(6142) => m_Obsidian = obsidian;

            public override void OnClick()
            {
                var from = Owner.From;
                if (!m_Obsidian.Deleted && m_Obsidian.Quantity >= m_Partial && m_Obsidian.Quantity < m_Completed &&
                    m_Obsidian.IsChildOf(from.Backpack) && from.CheckAlive())
                {
                    for (var i = 0; i < m_Obsidian.Quantity - 1; i++)
                    {
                        from.AddToBackpack(new Obsidian());
                    }

                    m_Obsidian.Quantity = 1;
                }
            }
        }

        private class InternalTarget : Target
        {
            private readonly Obsidian m_Obsidian;

            public InternalTarget(Obsidian obsidian) : base(-1, false, TargetFlags.None) => m_Obsidian = obsidian;

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Obsidian.Deleted || m_Obsidian.Quantity >= m_Completed || targeted is not Item targ)
                {
                    return;
                }

                if (m_Obsidian.IsChildOf(from.Backpack) && targ.IsChildOf(from.Backpack) && targ is Obsidian targObsidian &&
                    targ != m_Obsidian)
                {
                    if (targObsidian.Quantity < m_Completed)
                    {
                        if (targObsidian.Quantity + m_Obsidian.Quantity <= m_Completed)
                        {
                            targObsidian.Quantity += m_Obsidian.Quantity;
                            m_Obsidian.Delete();
                        }
                        else
                        {
                            var delta = m_Completed - targObsidian.Quantity;
                            targObsidian.Quantity += delta;
                            m_Obsidian.Quantity -= delta;
                        }

                        if (targObsidian.Quantity >= m_Completed)
                        {
                            targObsidian.StatueName = RandomName(from);
                        }

                        from.NetState.SendMessage(
                            targObsidian.Serial,
                            targObsidian.ItemID,
                            MessageType.Regular,
                            0x59,
                            3,
                            true,
                            null,
                            m_Obsidian.Name,
                            "Something Happened."
                        );

                        return;
                    }
                }

                from.NetState.SendMessageLocalized(
                    m_Obsidian.Serial,
                    m_Obsidian.ItemID,
                    MessageType.Regular,
                    0x2C,
                    3,
                    500309,
                    m_Obsidian.Name
                ); // Nothing Happens.
            }
        }
    }
}
