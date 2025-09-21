using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CardChanges
{
    public static class Patches
    {
        public static void HellhornedReworks()
        {
            //
            // Champions
            //

            var HornbreakerPrince = ModData.ModCard(Cards.HornbreakerPrince);
            HornbreakerPrince.Monster.SetDamage(10);
            HornbreakerPrince.Monster.SetHP(10);

            var Brawler1 = ModData.ModUpgrade("6057fccd-90dc-4c97-b64b-cca5a02f4766");
            var Brawler2 = ModData.ModUpgrade("b1476ffd-d425-46d6-89d6-975921a8f58c");
            var Brawler3 = ModData.ModUpgrade("142535b6-ad20-4fa3-8a6b-4fff4ad45784");

            Brawler1.GetStatusEffectUpgrade(StatusEffect.Armor).count = 10;
            Brawler2.GetStatusEffectUpgrade(StatusEffect.Armor).count = 20;
            Brawler3.GetStatusEffectUpgrade(StatusEffect.Armor).count = 40;

            var Reaper1 = ModData.ModUpgrade("96f93f28-0d3c-4b62-aabc-f7de6b655995");
            var Reaper2 = ModData.ModUpgrade("86d92229-18f2-4644-8d01-08e195de9253");
            var Reaper3 = ModData.ModUpgrade("0bc77389-97b7-4073-b00e-111293e706a4");

            Reaper1.SetBonusHP(0);
            Reaper1.SetBonusDamage(20);
            Reaper2.SetBonusHP(10);
            Reaper2.SetBonusDamage(50);
            Reaper3.SetBonusHP(30);
            Reaper3.SetBonusDamage(110);

            var Wrathful1 = ModData.ModUpgrade("bbedbbd7-cf93-4b27-920a-36815e404a9a");
            var Wrathful2 = ModData.ModUpgrade("71ca2e13-c5de-426d-997a-45d0f9e21a34");
            var Wrathful3 = ModData.ModUpgrade("93e65477-e418-4da8-b456-e3e3d9d269fa");

            Wrathful1.SetBonusDamage(20);
            Wrathful2.SetBonusDamage(30);
            Wrathful3.SetBonusDamage(40);
            Wrathful1.SetBonusHP(40);
            Wrathful2.SetBonusHP(50);
            Wrathful3.SetBonusHP(60);

            //
            // Units
            //

            var Steelworker = ModData.ModCard(Cards.Steelworker);
            Steelworker.Monster.SetHP(10);
            Steelworker.Monster.ReplaceStartingStatusEffects(StatusEffect.Armor, 30);
        }

        public static void AwokenReworks()
        {
            //
            // Units
            //

            ModData.ModCard(Cards.ShardChanneler).Monster.SetHP(5);
            ModData.ModCard(Cards.WildwoodCustodian).Monster.SetHP(5);
        }

        public static void StygianReworks()
        {
            //
            // Units
            //

            var NamelessSiren = ModData.ModCard(Cards.NamelessSiren);
            NamelessSiren.Monster.SetDamage(12);
            NamelessSiren.Monster.SetHP(12);

            var SirenoftheSea = ModData.ModCard(Cards.SirenoftheSea);
            SirenoftheSea.Monster.SetDamage(12);
            SirenoftheSea.Monster.SetHP(12);

            var Coldcaelia = ModData.ModCard(Cards.Coldcaelia);
            Coldcaelia.Monster.SetDamage(5);
            Coldcaelia.Monster.SetHP(20);

            var IcyCilophyte = ModData.ModCard(Cards.IcyCilophyte);
            IcyCilophyte.Monster.SetDamage(5);
            IcyCilophyte.Monster.SetHP(20);
        }

        public static void UmbraReworks()
        {
            //
            // Units
            //

            var Morselmaker = ModData.ModCard(Cards.Morselmaker);
            Morselmaker.Monster.SetDamage(10);
            Morselmaker.Monster.SetHP(10);
            Morselmaker.SetCost(1);

            ModData.ModCard(Cards.Overgorger).Monster.SetHP(50);

            var Shadowsiege = ModData.ModCard(Cards.Shadowsiege);
            Shadowsiege.SetCost(0);
            Shadowsiege.Monster.SetHP(200);
            Shadowsiege.Monster.ReplaceStartingStatusEffects(StatusEffect.Emberdrain, 3);
        }

        public static void MeltingRemnantReworks()
        {
            //
            // Spells
            //

            ModData.ModCard(Cards.AFatalMelting).SetCost(1);
        }
    }
}
