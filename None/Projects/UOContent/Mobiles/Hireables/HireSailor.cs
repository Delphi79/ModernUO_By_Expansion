using Server.Items;

namespace Server.Mobiles
{
    public class HireSailor : BaseHire
    {
        [Constructible]
        public HireSailor()
        {
            SpeechHue = Utility.RandomDyedHue();
            Hue = Race.Human.RandomSkinHue();

            if (Female = Utility.RandomBool())
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
				EquipItem(new ShortPants(Utility.RandomNeutralHue()));
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
				EquipItem(new ShortPants(Utility.RandomNeutralHue()));
            }
            Title = "the sailor";
            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();
            Race.RandomFacialHair(this);

            SetStr(86);
            SetDex(66);
            SetInt(41);

            SetDamage(10, 23);

            SetSkill(SkillName.Stealing, 66.0, 97.5);
            SetSkill(SkillName.Peacemaking, 65.0, 87.5);
            SetSkill(SkillName.MagicResist, 25.0, 47.5);
            SetSkill(SkillName.Healing, 65.0, 87.5);
            SetSkill(SkillName.Tactics, 65.0, 87.5);
            SetSkill(SkillName.Fencing, 65.0, 87.5);
            SetSkill(SkillName.Parry, 45.0, 60.5);
            SetSkill(SkillName.Lockpicking, 65, 87);
            SetSkill(SkillName.Hiding, 65, 87);
            SetSkill(SkillName.Snooping, 65, 87);
            Fame = 100;
            Karma = 0;

			EquipItem(new Shoes(Utility.RandomNeutralHue()));
			EquipItem(new Cutlass());

            switch (Utility.Random(2))
            {
                case 0:
					EquipItem(new Doublet(Utility.RandomDyedHue()));
                    break;
                case 1:
					EquipItem(new Shirt(Utility.RandomDyedHue()));
                    break;
            }
        }

        public HireSailor(Serial serial)
            : base(serial)
        {
        }

        public override bool ClickTitle => false;
        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);// version 
        }

        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
