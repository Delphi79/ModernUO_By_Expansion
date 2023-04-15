using System;
using System.Text;
using Microsoft.Toolkit.HighPerformance;
using Server.Commands;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    public static class TestCenter
    {
        public static bool Enabled { get; private set; }

        public static void Configure()
        {
            Enabled = ServerConfiguration.GetOrUpdateSetting("testCenter.enable", false);
        }

        public static void Initialize()
        {
            // Register our speech handler
            if (Enabled)
            {
                EventSink.Speech += EventSink_Speech;
            }
        }

        private static void EventSink_Speech(SpeechEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            if (args.Speech.InsensitiveStartsWith("set"))
            {
                var from = args.Mobile;

                var tokenizer = args.Speech.Tokenize(' ');
                if (!tokenizer.MoveNext())
                {
                    return;
                }

                var name = tokenizer.MoveNext() ? tokenizer.Current : null;
                var valueStr = tokenizer.MoveNext() ? tokenizer.Current : null;
                if (valueStr == null)
                {
                    return;
                }

                try
                {
                    var value = double.Parse(valueStr);

                    if (name.InsensitiveEquals("str"))
                    {
                        ChangeStrength(from, (int)value);
                    }
                    else if (name.InsensitiveEquals("dex"))
                    {
                        ChangeDexterity(from, (int)value);
                    }
                    else if (name.InsensitiveEquals("int"))
                    {
                        ChangeIntelligence(from, (int)value);
                    }
                    else
                    {
                        ChangeSkill(from, name.ToString(), value);
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else if (args.Speech.InsensitiveEquals("help"))
            {
                args.Mobile.SendGump(new TCHelpGump());
                args.Handled = true;
            }
        }

        private static void ChangeStrength(Mobile from, int value)
        {
            if (value is < 10 or > 125)
            {
                from.SendLocalizedMessage(1005628); // Stats range between 10 and 125.
            }
            else
            {
                if (value + from.RawDex + from.RawInt > from.StatCap)
                {
                    from.SendLocalizedMessage(
                        1005629
                    ); // You can not exceed the stat cap.  Try setting another stat lower first.
                }
                else
                {
                    from.RawStr = value;
                    from.SendLocalizedMessage(1005630); // Your stats have been adjusted.
                }
            }
        }

        private static void ChangeDexterity(Mobile from, int value)
        {
            if (value is < 10 or > 125)
            {
                from.SendLocalizedMessage(1005628); // Stats range between 10 and 125.
            }
            else
            {
                if (from.RawStr + value + from.RawInt > from.StatCap)
                {
                    from.SendLocalizedMessage(
                        1005629
                    ); // You can not exceed the stat cap.  Try setting another stat lower first.
                }
                else
                {
                    from.RawDex = value;
                    from.SendLocalizedMessage(1005630); // Your stats have been adjusted.
                }
            }
        }

        private static void ChangeIntelligence(Mobile from, int value)
        {
            if (value is < 10 or > 125)
            {
                from.SendLocalizedMessage(1005628); // Stats range between 10 and 125.
            }
            else
            {
                if (from.RawStr + from.RawDex + value > from.StatCap)
                {
                    from.SendLocalizedMessage(
                        1005629
                    ); // You can not exceed the stat cap.  Try setting another stat lower first.
                }
                else
                {
                    from.RawInt = value;
                    from.SendLocalizedMessage(1005630); // Your stats have been adjusted.
                }
            }
        }

        private static void ChangeSkill(Mobile from, string name, double value)
        {
            if (!Enum.TryParse(name, true, out SkillName index) || !Core.SE && (int)index > 51 ||
                !Core.AOS && (int)index > 48)
            {
                from.SendLocalizedMessage(1005631); // You have specified an invalid skill to set.
                return;
            }

            var skill = from.Skills[index];

            if (skill != null)
            {
                if (value < 0 || value > skill.Cap)
                {
                    from.SendMessage($"Your skill in {skill.Info.Name} is capped at {skill.Cap:F1}.");
                }
                else
                {
                    var newFixedPoint = (int)(value * 10.0);
                    var oldFixedPoint = skill.BaseFixedPoint;

                    if (skill.Owner.Total - oldFixedPoint + newFixedPoint > skill.Owner.Cap)
                    {
                        from.SendMessage("You can not exceed the skill cap.  Try setting another skill lower first.");
                    }
                    else
                    {
                        skill.BaseFixedPoint = newFixedPoint;
                    }
                }
            }
            else
            {
                from.SendLocalizedMessage(1005631); // You have specified an invalid skill to set.
            }
        }

        private static Item MakeNewbie(Item item)
        {
            if (!Core.AOS)
            {
                item.LootType = LootType.Newbied;
            }

            return item;
        }

        private static void PlaceItemIn(Container parent, int x, int y, Item item)
        {
            parent.AddItem(item);
            item.Location = new Point3D(x, y, 0);
        }

        private static Item MakePotionKeg(PotionEffect type, int hue)
        {
            var keg = new PotionKeg
            {
                Held = 100,
                Type = type,
                Hue = hue
            };

            return MakeNewbie(keg);
        }

        public static void FillBankbox(Mobile m)
        {
			Container cont;
            BankBox bank = m.BankBox;
			Bag bag = new Bag();

            // Begin bag of tools
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Tools";
            
			PlaceItemIn( cont, 30,  35, new TinkerTools( 1000 ) );
			PlaceItemIn( cont, 90,  35, new DovetailSaw( 1000 ) );
			PlaceItemIn( cont, 30,  68, new Scissors() );
			PlaceItemIn( cont, 45,  68, new MortarPestle( 1000 ) );
			PlaceItemIn( cont, 75,  68, new ScribesPen( 1000 ) );
			PlaceItemIn( cont, 90,  68, new SmithHammer( 1000 ) );
			PlaceItemIn( cont, 30, 118, new TwoHandedAxe() );
			PlaceItemIn( cont, 60, 118, new FletcherTools( 1000 ) );
			PlaceItemIn( cont, 90, 118, new SewingKit( 1000 ) );

			PlaceItemIn( bank, 18, 132, cont );
			// End bag of tools

			// A few dye tubs
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Dye Tubs";
			
			PlaceItemIn( cont, 30,  90, new Dyes() );
			PlaceItemIn( cont, 29,  52, new DyeTub() );
			PlaceItemIn( cont, 56,  69, new BlackDyeTub() );
			PlaceItemIn( cont, 44,  60, new DyeTub { DyedHue = 0x485, Redyable = false });
			
			DyeTub darkRedTub = new DyeTub();

			darkRedTub.DyedHue = 0x485;
			darkRedTub.Redyable = false;
			PlaceItemIn( cont, 45, 68, darkRedTub );
			
			PlaceItemIn( bank, 30, 132, cont );
			
			// Beginning Bag of Food
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Food";
			
			PlaceItemIn( cont, 30,  35, new Apple( 1000 ) );
			PlaceItemIn( cont, 90,  35, new Pear( 1000 ) );
			PlaceItemIn( cont, 30,  68, new RawRibs( 1000 ) );
			PlaceItemIn( cont, 45,  68, new RawFishSteak( 1000 ) );
			PlaceItemIn( cont, 75,  68, new RawLambLeg( 1000 ) );
			PlaceItemIn( cont, 90,  68, new Carrot( 1000 ) );
			PlaceItemIn( cont, 30, 118, new RawBird( 1000 ) );
			PlaceItemIn( cont, 60, 118, new FrenchBread( 1000 ) );
			PlaceItemIn( cont, 90, 118, new Onion( 1000 ) );
			
			PlaceItemIn( bank, 42, 132, cont );
			//End of Bag of Food
			
			//Begin Bag of Resources
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Resources";
			
			// Resources
			PlaceItemIn( cont, 30,  35, new Feather( 1000 ) );
			PlaceItemIn( cont, 90,  35, new BoltOfCloth( 1000 ) );
			PlaceItemIn( cont, 30,  68, new Hides( 1000 ) );
			PlaceItemIn( cont, 45,  68, new Bandage( 1000 ) );
			PlaceItemIn( cont, 75,  68, new Bottle( 1000 ) );
			PlaceItemIn( cont, 90,  68, new Log( 1000 ) );
			PlaceItemIn( cont, 30, 118, new IronIngot( 5000 ) );
			PlaceItemIn( cont, 30, 35, new DullCopperIngot(5000));
            PlaceItemIn( cont, 37, 35, new ShadowIronIngot(5000));
            PlaceItemIn( cont, 44, 35, new CopperIngot(5000));
            PlaceItemIn( cont, 51, 35, new BronzeIngot(5000));
            PlaceItemIn( cont, 58, 35, new GoldIngot(5000));
            PlaceItemIn( cont, 65, 35, new AgapiteIngot(5000));
            PlaceItemIn( cont, 72, 35, new VeriteIngot(5000));
            PlaceItemIn( cont, 79, 35, new ValoriteIngot(5000));

			PlaceItemIn( bank, 54, 132, cont );
			//End Bag of Resources
			
			//Begin Bag of Reagents
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Reagents";
			
			PlaceItemIn( cont, 30,  35, new BlackPearl( 1000 ) );
			PlaceItemIn( cont, 90,  35, new Bloodmoss( 1000 ) );
			PlaceItemIn( cont, 30,  68, new Garlic( 1000 ) );
			PlaceItemIn( cont, 45,  68, new Ginseng( 1000 ) );
			PlaceItemIn( cont, 75,  68, new MandrakeRoot( 1000 ) );
			PlaceItemIn( cont, 90,  68, new Nightshade( 1000 ) );
			PlaceItemIn( cont, 30, 118, new SulfurousAsh( 1000 ) );
            PlaceItemIn( cont, 60, 118, new SpidersSilk( 1000 ) );
			PlaceItemIn( cont, 65, 48, new Spellbook( ulong.MaxValue ) );
			
			PlaceItemIn( bank, 66, 132, cont );
			//End Bag of Reagents
			
			// Begin bag of treasure maps
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Treasure Maps";

			PlaceItemIn( cont, 30, 35, new TreasureMap( 1, Map.Felucca ) );
			PlaceItemIn( cont, 45, 35, new TreasureMap( 2, Map.Felucca ) );
			PlaceItemIn( cont, 60, 35, new TreasureMap( 3, Map.Felucca ) );
			PlaceItemIn( cont, 75, 35, new TreasureMap( 4, Map.Felucca ) );
			PlaceItemIn( cont, 90, 35, new TreasureMap( 5, Map.Felucca ) );
			PlaceItemIn( cont, 105, 35, new TreasureMap( 6, Map.Felucca ) );
															   
			PlaceItemIn( cont, 30, 50, new TreasureMap( 1, Map.Felucca ) );
			PlaceItemIn( cont, 45, 50, new TreasureMap( 2, Map.Felucca ) );
			PlaceItemIn( cont, 60, 50, new TreasureMap( 3, Map.Felucca ) );
			PlaceItemIn( cont, 75, 50, new TreasureMap( 4, Map.Felucca ) );
			PlaceItemIn( cont, 90, 50, new TreasureMap( 5, Map.Felucca ) );
			PlaceItemIn( cont, 105, 50, new TreasureMap( 6, Map.Felucca ) );

			PlaceItemIn( cont, 55, 100, new Lockpick( 30 ) );
			PlaceItemIn( cont, 60, 100, new Pickaxe() );

			PlaceItemIn( bank, 78, 132, cont );
			// End bag of treasure maps
			
			// Begin box of money
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Gold Coin";
			
			//1 Million Gold
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 65000 ) );
			cont.DropItem( new Gold( 25000 ) );
			
			PlaceItemIn( bank, 90, 132, cont );
			// End box of money
			
			// Beginning Recall Runes
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Recall Runes";
			
			PlaceItemIn( cont, 30,  35, new RecallRune() );
			PlaceItemIn( cont, 90,  35, new RecallRune() );
			PlaceItemIn( cont, 30,  68, new RecallRune() );
			PlaceItemIn( cont, 45,  68, new RecallRune() );
			PlaceItemIn( cont, 75,  68, new RecallRune() );
		
		    PlaceItemIn( bank, 102, 132, cont );
			// End Recall Runes
			
			// Begin bag of archery ammo
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Archery Ammo";

			PlaceItemIn( cont, 48, 76, new Arrow( 5000 ) );
			PlaceItemIn( cont, 72, 76, new Bolt( 5000 ) );

			PlaceItemIn( bank, 18, 146, cont );
			// End bag of archery ammo
			
			// Begin bag of Bows
			cont = new Bag();
			cont.ItemID = 0xE76;
			cont.Hue = Utility.RandomList(Utility.RandomMinMax(1, 1058));
			cont.Name = "Bag Of Bows";

			PlaceItemIn(cont, 31, 84, new Bow());
			PlaceItemIn(cont, 53, 71, new Crossbow());
            PlaceItemIn(cont, 56, 39, new HeavyCrossbow());

			PlaceItemIn( bank, 30, 146, cont );
			// End bag of archery ammo
        }

        private static void AddPowerScrolls(BankBox bank)
        {
            var bag = new Bag();

            for (var i = 0; i < PowerScroll.Skills.Length; ++i)
            {
                bag.DropItem(new PowerScroll(PowerScroll.Skills[i], 120.0));
            }

            bag.DropItem(new StatCapScroll(250));

            bank.DropItem(bag);
        }

        public class TCHelpGump : Gump
        {
            public TCHelpGump() : base(40, 40)
            {
                AddPage(0);
                AddBackground(0, 0, 160, 120, 5054);

                AddButton(10, 10, 0xFB7, 0xFB9, 1);
                AddLabel(45, 10, 0x34, "ModernUO");

                AddButton(10, 35, 0xFB7, 0xFB9, 2);
                AddLabel(45, 35, 0x34, "List of skills");

                AddButton(10, 60, 0xFB7, 0xFB9, 3);
                AddLabel(45, 60, 0x34, "Command list");

                AddButton(10, 85, 0xFB1, 0xFB3, 0);
                AddLabel(45, 85, 0x34, "Close");
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                switch (info.ButtonID)
                {
                    case 1:
                        {
                            sender.LaunchBrowser("https://www.modernuo.com");
                            break;
                        }
                    case 2: // List of skills
                        {
                            var strings = Enum.GetNames(typeof(SkillName));

                            Array.Sort(strings);

                            var sb = new StringBuilder();

                            if (strings.Length > 0)
                            {
                                sb.Append(strings[0]);
                            }

                            for (var i = 1; i < strings.Length; ++i)
                            {
                                var v = strings[i];

                                if (sb.Length + 1 + v.Length >= 256)
                                {
                                    sender.SendMessage(
                                        Serial.MinusOne,
                                        -1,
                                        MessageType.Label,
                                        0x35,
                                        3,
                                        true,
                                        null,
                                        "System",
                                        sb.ToString()
                                    );

                                    sb = new StringBuilder();
                                    sb.Append(v);
                                }
                                else
                                {
                                    sb.Append(' ');
                                    sb.Append(v);
                                }
                            }

                            if (sb.Length > 0)
                            {
                                sender.SendMessage(
                                    Serial.MinusOne,
                                    -1,
                                    MessageType.Label,
                                    0x35,
                                    3,
                                    true,
                                    null,
                                    "System",
                                    sb.ToString()
                                );
                            }

                            break;
                        }
                    case 3: // Command list
                        {
                            sender.Mobile.SendAsciiMessage(0x482, $"The command prefix is \"{CommandSystem.Prefix}\"");
                            CommandHandlers.Help_OnCommand(new CommandEventArgs(sender.Mobile, "help", "", Array.Empty<string>()));

                            break;
                        }
                }
            }
        }
    }
}
