using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardChanges
{
    public class DataManager : IClient
    {
        private readonly IDictionary<Type, (bool, IProvider)> ProviderDictionary = new Dictionary<Type, (bool, IProvider)>(3);

        public SaveManager GameData => TryGetProvider<SaveManager>();

        public CombatManager CombatManager => TryGetProvider<CombatManager>();

        public CardStatistics CardStatistics => TryGetProvider<CardStatistics>();

        public T TryGetProvider<T>() where T : IProvider
        {
            return (T)ProviderDictionary[typeof(T)].Item2;
        }

        public void NewProviderAvailable(IProvider Provider)
        {
            Type ProviderType = Provider.GetType();
            if (ProviderDictionary.ContainsKey(ProviderType))
            {
                ProviderDictionary[ProviderType] = (false, Provider);
            }
            else
            {
                ProviderDictionary.Add(ProviderType, (false, Provider));
            }
        }

        public void NewProviderFullyInstalled(IProvider Provider)
        {
            Type ProviderType = Provider.GetType();
            if (ProviderDictionary.ContainsKey(ProviderType)) ProviderDictionary[ProviderType] = (true, Provider);
            else ProviderDictionary.Add(ProviderType, (true, Provider));
        }

        public void ProviderRemoved(IProvider Provider) => ProviderDictionary.Remove(Provider.GetType());
    }

    public static class ModData
    {
        private static readonly AllGameData _Data = null;
        public static AllGameData Data => _Data;

        static ModData() => _Data = CardChanges.ModDataManager.GameData.GetAllGameData();

        public static ReworkCardObject ModCard(string cardID)
            => new ReworkCardObject(cardID, Data.FindCardData(cardID));

        public static ReworkCardObject ModCard(Cards card)
            => ModCard(card.ID());

        public static ReworkUpgradeObject ModUpgrade(string upgradeID)
            => new ReworkUpgradeObject(upgradeID, Data.FindCardUpgradeData(upgradeID));

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

        public static StatusEffectStackData Status(StatusEffect status, int stacks = 1)
            => new StatusEffectStackData { statusId = status.ID(), count = stacks };
    }

    public abstract class ReworkObject<Type> where Type : GameData
    {
        private readonly string _ID;
        public string ID => _ID;

        private readonly Type _Data;
        public Type Data => _Data;

        private Traverse _Traversing = null;
        public Traverse Traversing
        {
            get
            {
                _Traversing ??= Traverse.Create(Data);
                return _Traversing;
            }
        }

        protected ReworkObject(string objectID, Type objectData)
        {
            _ID = objectID;
            _Data = objectData;
        }

        public virtual void SetField<fieldtype>(string fieldname, fieldtype value)
        {
            Traverse field = Traversing.Field(fieldname);

            if (field.GetValueType() == typeof(fieldtype)) field.SetValue(value);
            else Logging.LogWarning($"Attempted SetField on {fieldname} but mismatched type: {field.GetValueType()} vs. {typeof(fieldtype)}.");
        }
    }

    public class ReworkCardObject : ReworkObject<CardData>
    {
        private readonly CardType _Type;
        public CardType Type => _Type;

        private readonly ReworkMonsterObject _Monster;
        public ReworkMonsterObject Monster => _Monster;

        public ReworkCardObject(string cardID, CardData cardData) : base(cardID, cardData)
        {
            if (!(Data is null))
            {
                _Type = Data.GetCardType();
                if (Type == CardType.Monster) _Monster = new ReworkMonsterObject(Data.GetSpawnCharacterData());
            }
            else
            {
                Logging.LogError($"Couldn't find card: {ID} - This will cause crashes.");
            }
        }

        public void SetCost(int newCost)
        {
            if (newCost >= 0) Traversing.Field("cost").SetValue(newCost);
            else Traversing.Field("cost").SetValue(0);
        }

        public void SetDescription(string en, string fr = "", string de = "", string ru = "", string pt = "", string zh = "")
        {
            ModLocalization.AddLocalization(key: $"mod_card_description_{ID}", en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Traversing.Field("overrideDescriptionKey").SetValue($"mod_card_description_{ID}");
        }
    }

    public class ReworkMonsterObject : ReworkObject<CharacterData>
    {
        public ReworkMonsterObject(CharacterData monsterData) : base(monsterData.GetID(), monsterData)
        {
            if (Data is null) Logging.LogError($"Couldn't find monster: {ID} - This will cause crashes.");
        }

        public void SetDamage(int newDamage) => Traversing.Field("attackDamage").SetValue(newDamage);

        public void SetHP(int newHP) => Traversing.Field("health").SetValue(newHP);

        public void ReplaceStartingStatusEffects(StatusEffectStackData[] data) => Traversing.Field("startingStatusEffects").SetValue(data);

        public void ReplaceStartingStatusEffects(StatusEffect status, int stacks = 1) => ReplaceStartingStatusEffects(ModData.StatusArray(status, stacks));

        public void SetTriggerDescription(CharacterTriggerData.Trigger trigger, string en, string fr = "", string de = "", string ru = "", string pt = "", string zh = "")
        {
            ModLocalization.AddLocalization(key: $"mod_unit_trigger_{ID}", en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Traverse.Create(Data.GetTriggers().FirstOrDefault(t => t.GetTrigger() == trigger)).Field("descriptionKey").SetValue($"mod_unit_trigger_{ID}");
        }
    }

    public class ReworkUpgradeObject : ReworkObject<CardUpgradeData>
    {
        public ReworkUpgradeObject(string upgradeID, CardUpgradeData cardUpgradeData) : base(upgradeID, cardUpgradeData)
        {
            if (Data is null) Logging.LogWarning($"Couldn't find upgrade: {ID} - This will cause crashes.");
        }

        public void SetBonusDamage(int value) => Traversing.Field("bonusDamage").SetValue(value);

        public void SetBonusHP(int value) => Traversing.Field("bonusHP").SetValue(value);

        public StatusEffectStackData GetStatusEffectUpgrade(StatusEffect status = StatusEffect.None)
            => Data.GetStatusEffectUpgrades().FirstOrDefault(t => status == StatusEffect.None || t.statusId == status.ID());

        public void SetUpgradeDescription(string en, string fr = "", string de = "", string ru = "", string pt = "", string zh = "")
        {
            ModLocalization.AddLocalization(key: $"mod_upgrade_{ID}", en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Traversing.Field("upgradeDescriptionKey").SetValue($"mod_upgrade_{ID}");
        }

        public void SetTriggerDescription(CharacterTriggerData.Trigger trigger, string en, string fr = "", string de = "", string ru = "", string pt = "", string zh = "")
        {
            ModLocalization.AddLocalization(key: $"mod_upgrade_trigger_{ID}", en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Traverse.Create(Data.GetCharacterTriggerUpgrades().FirstOrDefault(t => t.GetTrigger() == trigger)).Field("descriptionKey").SetValue($"mod_upgrade_trigger_{ID}");
        }
    }
}