using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardChanges
{
    public class DataManager : IClient
    {
        private readonly IDictionary<Type, (bool, IProvider)> ProviderDictionary = new Dictionary<Type, (bool, IProvider)>();

        public SaveManager GameData => TryGetProvider<SaveManager>();

        public CombatManager CombatManager => TryGetProvider<CombatManager>();

        public CardStatistics CardStatistics => TryGetProvider<CardStatistics>();

        public T TryGetProvider<T>() where T : IProvider => (T)ProviderDictionary[typeof(T)].Item2;

        public void NewProviderAvailable(IProvider Provider)
        {
            Type ProviderType = Provider.GetType();
            if (ProviderDictionary.ContainsKey(ProviderType)) ProviderDictionary[ProviderType] = (false, Provider);
            else ProviderDictionary.Add(ProviderType, (false, Provider));
        }

        public void NewProviderFullyInstalled(IProvider Provider)
        {
            Type ProviderType = Provider.GetType();
            if (ProviderDictionary.ContainsKey(ProviderType)) ProviderDictionary[ProviderType] = (true, Provider);
            else ProviderDictionary.Add(ProviderType, (true, Provider));
        }

        public void ProviderRemoved(IProvider Provider) => ProviderDictionary.Remove(Provider.GetType());
    }

    public static class Mod
    {
        private static readonly AllGameData _Data;
        public static AllGameData Data => _Data;

        static Mod() => _Data = CardChanges.ModDataManager.GameData.GetAllGameData();

        public static ModCardData Card(Cards card)
        {
            string CardID = card.ID();
            return new ModCardData(CardID, Data.FindCardData(CardID));
        }

        public static ModCardUpgradeData Upgrade(string upgradeID)
            => new ModCardUpgradeData(upgradeID, Data.FindCardUpgradeData(upgradeID));

        public static List<CardTraitData> TraitList(params CardTraitData[] traits)
        {
            var TraitList = new List<CardTraitData>(traits.Length);
            foreach (CardTraitData trait in traits) TraitList.Add(Trait(trait.traitStateName));
            return TraitList;
        }

        public static CardTraitData Trait(string traitname) => new CardTraitData { traitStateName = traitname };

        public static StatusEffectStackData[] StatusArray(params (StatusEffect, int)[] statuses)
        {
            var StatusArray = new StatusEffectStackData[statuses.Length];
            for (int i = 0; i < statuses.Length; i++) StatusArray[i] = Status(statuses[i].Item1, statuses[i].Item2);
            return StatusArray;
        }

        public static StatusEffectStackData[] StatusArray(StatusEffect status, int stacks = 1)
            => StatusArray((status, stacks));

        public static List<StatusEffectStackData> StatusList(params (StatusEffect, int)[] statuses)
        {
            var StatusList = new List<StatusEffectStackData>(statuses.Length);
            foreach ((StatusEffect, int) status in statuses) StatusList.Add(Status(status.Item1, status.Item2));
            return StatusList;
        }

        public static List<StatusEffectStackData> StatusList(StatusEffect status, int stacks = 1)
            => StatusList((status, stacks));

        public static StatusEffectStackData Status(StatusEffect status, int stacks = 1)
            => new StatusEffectStackData { statusId = status.ID(), count = stacks };
    }

    public abstract class ModGameData<Type> where Type : GameData
    {
        private readonly string _ID;
        public string ID => _ID;

        private readonly Type _Data;
        public Type Data => _Data;

        private Traverse _Traversing;
        public Traverse Traversing
        {
            get
            {
                _Traversing ??= Traverse.Create(Data);
                return _Traversing;
            }
        }

        protected ModGameData(string objectID, Type objectData)
        {
            _ID = objectID;
            _Data = objectData;
        }

        public virtual void SetField<fieldtype>(string fieldname, fieldtype value)
        {
            var field = Traversing.Field(fieldname);
            if (field.GetValueType() == typeof(fieldtype)) field.SetValue(value);
            else Logging.LogWarning($"Attempted SetField on {fieldname} but mismatched type: {field.GetValueType()} vs. {typeof(fieldtype)}.");
        }
    }

    public class ModCardData : ModGameData<CardData>
    {
        private readonly CardType _Type;
        public CardType Type => _Type;

        private readonly ModCharacterData _Monster;
        public ModCharacterData Monster => _Monster;

        public ModCardData(string cardID, CardData cardData) : base(cardID, cardData)
        {
            if (!(Data is null))
            {
                _Type = Data.GetCardType();
                if (Type == CardType.Monster) _Monster = new ModCharacterData(Data.GetSpawnCharacterData());
            }
            else
            {
                Logging.LogError($"Couldn't find card: {ID} - This will cause crashes.");
            }
        }

        public void SetCost(int newCost)
        {
            if (newCost < 0) newCost = 0;
            SetField("cost", newCost);
        }

        public void SetDescription(string en, string fr = "", string de = "", string ru = "", string pt = "", string zh = "")
        {
            string DescriptionKey = $"mod_card_description_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            SetField("overrideDescriptionKey", DescriptionKey);
        }
    }

    public class ModCharacterData : ModGameData<CharacterData>
    {
        public ModCharacterData(CharacterData monsterData) : base(monsterData.GetID(), monsterData)
        {
            if (Data is null) Logging.LogError($"Couldn't find monster: {ID} - This will cause crashes.");
        }

        public void SetDamage(int newDamage) => SetField("attackDamage", newDamage);

        public void SetHP(int newHP) => SetField("health", newHP);

        public void ReplaceStartingStatusEffects(StatusEffectStackData[] data) => SetField("startingStatusEffects", data);

        public void ReplaceStartingStatusEffects(StatusEffect status, int stacks = 1) => ReplaceStartingStatusEffects(Mod.StatusArray(status, stacks));

        public void SetTriggerDescription(CharacterTriggerData.Trigger trigger, string en, string fr = "", string de = "", string ru = "", string pt = "", string zh = "")
        {
            string DescriptionKey = $"mod_unit_trigger_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Traverse.Create(Data.GetTriggers().FirstOrDefault(t => t.GetTrigger() == trigger)).Field("descriptionKey").SetValue(DescriptionKey);
        }
    }

    public class ModCardUpgradeData : ModGameData<CardUpgradeData>
    {
        public ModCardUpgradeData(string upgradeID, CardUpgradeData cardUpgradeData) : base(upgradeID, cardUpgradeData)
        {
            if (Data is null) Logging.LogWarning($"Couldn't find upgrade: {ID} - This will cause crashes.");
        }

        public void SetBonusDamage(int value) => SetField("bonusDamage", value);

        public void SetBonusHP(int value) => SetField("bonusHP", value);

        public StatusEffectStackData GetStatusEffectUpgrade(StatusEffect status = StatusEffect.None)
            => Data.GetStatusEffectUpgrades().FirstOrDefault(t => status == StatusEffect.None || t.statusId == status.ID());

        public void ReplaceStatusEffectUpgrades(List<StatusEffectStackData> data) => SetField("statusEffectUpgrades", data);

        public void SetUpgradeDescription(string en, string fr = "", string de = "", string ru = "", string pt = "", string zh = "")
        {
            string DescriptionKey = $"mod_upgrade_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            SetField("upgradeDescriptionKey", DescriptionKey);
        }

        public void SetTriggerDescription(CharacterTriggerData.Trigger trigger, string en, string fr = "", string de = "", string ru = "", string pt = "", string zh = "")
        {
            string DescriptionKey = $"mod_upgrade_trigger_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Traverse.Create(Data.GetCharacterTriggerUpgrades().FirstOrDefault(t => t.GetTrigger() == trigger)).Field("descriptionKey").SetValue(DescriptionKey);
        }
    }
}