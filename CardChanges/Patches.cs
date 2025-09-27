using System.Linq;

namespace CardChanges
{
    public static class Patches
    {
        public static void HellhornedReworks()
        {
            //
            // Champions
            //

            var HornbreakerPrince = Mod.Card(Cards.HornbreakerPrince);
            HornbreakerPrince.Monster.SetDamage(10);
            HornbreakerPrince.Monster.SetHP(10);

            var Brawler1 = Mod.Upgrade(Upgrades.Brawler1);
            Brawler1.GetStatusEffectUpgrade(StatusEffect.Armor).count = 10;

            var Brawler2 = Mod.Upgrade(Upgrades.Brawler2);
            Brawler2.GetStatusEffectUpgrade(StatusEffect.Armor).count = 20;

            var Brawler3 = Mod.Upgrade(Upgrades.Brawler3);
            Brawler3.GetStatusEffectUpgrade(StatusEffect.Armor).count = 40;

            var Reaper1 = Mod.Upgrade(Upgrades.Reaper1);
            Reaper1.SetBonusDamage(20);
            Reaper1.SetBonusHP(0);

            var Reaper2 = Mod.Upgrade(Upgrades.Reaper2);
            Reaper2.SetBonusDamage(50);
            Reaper2.SetBonusHP(10);

            var Reaper3 = Mod.Upgrade(Upgrades.Reaper3);
            Reaper3.SetBonusDamage(110);
            Reaper3.SetBonusHP(30);

            var Wrathful1 = Mod.Upgrade(Upgrades.Wrathful1);
            Wrathful1.SetBonusDamage(20);
            Wrathful1.SetBonusHP(40);

            var Wrathful2 = Mod.Upgrade(Upgrades.Wrathful2);
            Wrathful2.SetBonusDamage(30);
            Wrathful2.SetBonusHP(50);

            var Wrathful3 = Mod.Upgrade(Upgrades.Wrathful3);
            Wrathful3.SetBonusDamage(40);
            Wrathful3.SetBonusHP(60);

            //
            // Units
            //

            var Steelworker = Mod.Card(Cards.Steelworker);
            Steelworker.Monster.SetHP(10);
            Steelworker.Monster.AddStartingStatusEffects(Mod.Status(StatusEffect.Armor, 30));

            //
            // Spells
            //

            Mod.Card(Cards.Torch).AddTraits(Mod.Trait(CardTrait.Piercing));
            Mod.Card(Cards.Vent).AddTraits(Mod.Trait(CardTrait.Piercing));
        }

        public static void AwokenReworks()
        {
            //
            // Units
            //

            Mod.Card(Cards.ShardChanneler).Monster.SetHP(5);
            Mod.Card(Cards.WildwoodCustodian).Monster.SetHP(5);
        }

        public static void StygianReworks()
        {
            //
            // Units
            //

            var NamelessSiren = Mod.Card(Cards.NamelessSiren);
            NamelessSiren.Monster.SetDamage(12);
            NamelessSiren.Monster.SetHP(12);

            var SirenoftheSea = Mod.Card(Cards.SirenoftheSea);
            SirenoftheSea.Monster.SetDamage(12);
            SirenoftheSea.Monster.SetHP(12);

            var Coldcaelia = Mod.Card(Cards.Coldcaelia);
            Coldcaelia.Monster.SetDamage(5);
            Coldcaelia.Monster.SetHP(20);

            var IcyCilophyte = Mod.Card(Cards.IcyCilophyte);
            IcyCilophyte.Monster.SetDamage(5);
            IcyCilophyte.Monster.SetHP(20);

            var GuardoftheUnnamed = Mod.Card(Cards.GuardoftheUnnamed);
            GuardoftheUnnamed.Monster.SetDamage(5);
            GuardoftheUnnamed.Monster.SetHP(20);
            GuardoftheUnnamed.Monster.AddStartingStatusEffects(Mod.Status(StatusEffect.Armor, 20));

            var TitanSentry = Mod.Card(Cards.TitanSentry);
            TitanSentry.Monster.SetDamage(5);
            TitanSentry.Monster.SetHP(20);
            TitanSentry.Monster.AddStartingStatusEffects(Mod.Status(StatusEffect.Armor, 20));
            TitanSentry.Monster.Data.GetTriggers()
                                    .FirstOrDefault()
                                    .GetEffects()
                                    .FirstOrDefault()
                                    .GetParamStatusEffects()
                                    .FirstOrDefault(t => t.statusId == StatusEffect.Frostbite.GetID())
                                    .count = 9;
        }

        public static void UmbraReworks()
        {
            //
            // Units
            //

            var Morselmaker = Mod.Card(Cards.Morselmaker);
            Morselmaker.SetCost(1);
            Morselmaker.Monster.SetDamage(9);
            Morselmaker.Monster.SetHP(9);

            var Morselmaster = Mod.Card(Cards.Morselmaster);
            Morselmaster.SetCost(1);
            Morselmaster.Monster.SetDamage(9);
            Morselmaster.Monster.SetHP(9);

            var MorselMade = Mod.Card(Cards.MorselMade);
            MorselMade.Monster.SetDamage(9);
            MorselMade.Monster.SetHP(9);
            var MorselMadeUpgrade = new ModCardUpgradeData(MorselMade.Monster.Data.GetTriggers()
                                                                                  .FirstOrDefault(t => t.GetTrigger() == CharacterTriggerData.Trigger.OnFeed)
                                                                                  .GetEffects()
                                                                                  .FirstOrDefault(t => !(t.GetParamCardUpgradeData() is null))
                                                                                  .GetParamCardUpgradeData());
            MorselMadeUpgrade.SetBonusDamage(9);
            MorselMadeUpgrade.SetBonusHP(9);

            Mod.Card(Cards.Overgorger).Monster.SetHP(50);

            var Shadowsiege = Mod.Card(Cards.Shadowsiege);
            Shadowsiege.SetCost(0);
            Shadowsiege.Monster.SetDamage(180);
            Shadowsiege.Monster.SetHP(180);
            Shadowsiege.Monster.AddStartingStatusEffects(Mod.Status(StatusEffect.Emberdrain, 3));
        }

        public static void MeltingRemnantReworks()
        {
            //
            // Spells
            //

            Mod.Card(Cards.AFatalMelting).SetCost(1);
        }
    }
}
