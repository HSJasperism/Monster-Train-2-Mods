using System.Linq;
using System.Threading.Tasks;

namespace CardChanges
{
    public static class Patches
    {
        public static Task LunaCoven = new Task(() =>
        {
            //
            // Units
            //

            Mod.Card(Cards.AuroraWeaver).Monster.AddStartingStatusEffects(StatusEffect.Conduit.Stack(5));

            var Mooncaller = Mod.Card(Cards.Mooncaller);
            Mooncaller.Monster.SetDamage(8);
            Mooncaller.Monster.SetHP(8);

            //
            // Spells
            //

            Mod.Card(Cards.RitualTome).SetCost(1);
        });

        public static Task Underlegion = new Task(() =>
        {
            //
            // Units
            //

            var Balmabello = Mod.Card(Cards.Balmabello);
            Balmabello.SetCost(2);
            Balmabello.Monster.SetDamage(20);
            Balmabello.Monster.SetHP(20);
        });

        public static Task Hellhorned = new Task(() =>
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
            Wrathful1.SetBonusDamage(10);
            Wrathful1.SetBonusHP(10);
            Wrathful1.AddStatusEffectUpgrades(StatusEffect.Armor.Stack(10));

            var Wrathful2 = Mod.Upgrade(Upgrades.Wrathful2);
            Wrathful2.SetBonusDamage(20);
            Wrathful2.SetBonusHP(20);
            Wrathful2.AddStatusEffectUpgrades(StatusEffect.Armor.Stack(15));

            var Wrathful3 = Mod.Upgrade(Upgrades.Wrathful3);
            Wrathful3.SetBonusDamage(40);
            Wrathful3.SetBonusHP(40);
            Wrathful3.AddStatusEffectUpgrades(StatusEffect.Armor.Stack(25));

            //
            // Units
            //

            var Steelworker = Mod.Card(Cards.Steelworker);
            Steelworker.Monster.SetHP(10);
            Steelworker.Monster.AddStartingStatusEffects(StatusEffect.Armor.Stack(30));

            //
            // Spells
            //

            Mod.Card(Cards.Torch).AddTraits(CardTrait.Piercing.Instance());
            Mod.Card(Cards.Vent).AddTraits(CardTrait.Piercing.Instance());
        });

        public static Task Awoken = new Task(() =>
        {
            //
            // Units
            //

            Mod.Card(Cards.ShardChanneler).Monster.SetHP(5);
            Mod.Card(Cards.WildwoodCustodian).Monster.SetHP(5);

            //
            // Spells
            //

            Mod.Card(Cards.Sting).SetDamage(25);

            var CycleofLife = Mod.Card(Cards.CycleofLife);
            CycleofLife.Data.GetEffects()
                            .Single(t => t.GetEffectStateName() == typeof(CardEffectAddStatusEffect).Name)
                            .GetParamStatusEffects()
                            .Single(t => t.statusId == StatusEffect.Spikes.GetID())
                            .count = 12;
            CycleofLife.Data.GetEffects()
                            .Single(t => !(t.GetParamCardUpgradeData() is null))
                            .GetParamCardUpgradeData()
                            .ToModGameData()
                            .SetBonusHP(12);
        });

        public static Task StygianGuard = new Task(() =>
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
            Coldcaelia.Monster.SetDamage(4);
            Coldcaelia.Monster.SetHP(20);

            var IcyCilophyte = Mod.Card(Cards.IcyCilophyte);
            IcyCilophyte.Monster.SetDamage(4);
            IcyCilophyte.Monster.SetHP(20);

            var GuardoftheUnnamed = Mod.Card(Cards.GuardoftheUnnamed);
            GuardoftheUnnamed.Monster.SetDamage(5);
            GuardoftheUnnamed.Monster.SetHP(20);
            GuardoftheUnnamed.Monster.AddStartingStatusEffects(StatusEffect.Armor.Stack(20));
            GuardoftheUnnamed.Monster.GetTrigger(CharacterTriggerData.Trigger.CardSpellPlayed)
                                     .GetEffects()
                                     .Single(t => t.GetEffectStateName() == typeof(CardEffectAddStatusEffect).Name)
                                     .GetParamStatusEffects()
                                     .Single(t => t.statusId == StatusEffect.Armor.GetID()).count = 5;

            var TitanSentry = Mod.Card(Cards.TitanSentry);
            TitanSentry.Monster.SetDamage(5);
            TitanSentry.Monster.SetHP(20);
            TitanSentry.Monster.AddStartingStatusEffects(StatusEffect.Armor.Stack(20));
            TitanSentry.Monster.GetTrigger(CharacterTriggerData.Trigger.OnHit)
                               .GetEffects()
                               .Single(t => t.GetEffectStateName() == typeof(CardEffectAddStatusEffect).Name)
                               .GetParamStatusEffects()
                               .Single(t => t.statusId == StatusEffect.Frostbite.GetID()).count = 9;

            var GlacialSeal = Mod.Card(Cards.GlacialSeal);
            GlacialSeal.Monster.GetTrigger(CharacterTriggerData.Trigger.CardSpellPlayed)
                               .GetEffects()
                               .Single(t => t.GetEffectStateName() == typeof(CardEffectAddStatusEffect).Name)
                               .GetParamStatusEffects()
                               .Single(t => t.statusId == StatusEffect.Frostbite.GetID()).count = 3;

            var GuardianStone = Mod.Card(Cards.GuardianStone);
            GuardianStone.Monster.GetTrigger(CharacterTriggerData.Trigger.CardSpellPlayed)
                                 .GetEffects()
                                 .Single(t => t.GetEffectStateName() == typeof(CardEffectAddStatusEffect).Name)
                                 .GetParamStatusEffects()
                                 .Single(t => t.statusId == StatusEffect.Armor.GetID()).count = 3;

            var EelGorgon = Mod.Card(Cards.EelGorgon);
            EelGorgon.Monster.SetDamage(15);
            EelGorgon.Monster.SetHP(9);

            //
            // Spells
            //

            Mod.Card(Cards.FrozenLance).AddTraits(CardTrait.Attuned.Instance());
            Mod.Card(Cards.Titanstooth).AddTraits(CardTrait.Attuned.Instance());
        });

        public static Task Umbra = new Task(() =>
        {
            //
            // Units
            //

            var MorselMiner = Mod.Card(Cards.MorselMiner);
            MorselMiner.Monster.SetDamage(9);
            MorselMiner.Monster.SetHP(9);

            var Morselmaker = Mod.Card(Cards.Morselmaker);
            Morselmaker.Monster.SetDamage(9);
            Morselmaker.Monster.SetHP(9);

            var Morselmaster = Mod.Card(Cards.Morselmaster);
            Morselmaster.Monster.SetDamage(9);
            Morselmaster.Monster.SetHP(9);

            var MorselMade = Mod.Card(Cards.MorselMade);
            MorselMade.Monster.SetDamage(9);
            MorselMade.Monster.SetHP(9);
            MorselMade.Monster.GetTrigger(CharacterTriggerData.Trigger.OnFeed)
                              .GetEffects()
                              .Single(t => !(t.GetParamCardUpgradeData() is null))
                              .GetParamCardUpgradeData()
                              .ToModGameData()
                              .SetBonusDamage(9)
                              .SetBonusHP(9);

            Mod.Card(Cards.Overgorger).Monster.SetHP(50);

            var Shadowsiege = Mod.Card(Cards.Shadowsiege);
            Shadowsiege.SetCost(0);
            Shadowsiege.Monster.SetDamage(180);
            Shadowsiege.Monster.SetHP(180);
            Shadowsiege.Monster.AddStartingStatusEffects(StatusEffect.Trample.Stack(), StatusEffect.Emberdrain.Stack(3));
        });

        public static Task MeltingRemnant = new Task(() =>
        {
            //
            // Spells
            //

            Mod.Card(Cards.AFatalMelting).SetCost(1);
            Mod.Card(Cards.MementoMori).SetCost(1);
        });
    }
}
