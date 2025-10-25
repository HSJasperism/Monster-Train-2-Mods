using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardChanges
{
    public class DataManager : IClient
    {
        private readonly IDictionary<Type, IProvider> _ProviderDictionary;
        public DataManager()
        {
            _ProviderDictionary = new Dictionary<Type, IProvider>(DepInjector.ProviderCount);
            DepInjector.AddClient(this);
        }

        public SaveManager GameData => TryGetProvider<SaveManager>();

        public CombatManager CombatManager => TryGetProvider<CombatManager>();

        public CardStatistics CardStatistics => TryGetProvider<CardStatistics>();

        private T TryGetProvider<T>() where T : IProvider => (T)_ProviderDictionary[typeof(T)];

        public void NewProviderAvailable(IProvider Provider)
        {
            Type ProviderType = Provider.GetType();
            if (_ProviderDictionary.ContainsKey(ProviderType)) Logging.LogWarning($"Duplicate Provider found: {Provider.GetType()}");
            else _ProviderDictionary.Add(ProviderType, Provider);
        }

        public void NewProviderFullyInstalled(IProvider Provider)
        {
            Type ProviderType = Provider.GetType();
            if (_ProviderDictionary.ContainsKey(ProviderType)) Logging.LogInfo($"Provider active: {Provider.GetType()}");
            else _ProviderDictionary.Add(ProviderType, Provider);
        }

        public void ProviderRemoved(IProvider Provider) => _ProviderDictionary.Remove(Provider.GetType());
    }

    public static class Mod
    {
        public static readonly DataManager DataManager;
        public static readonly AllGameData Data;

        static Mod()
        {
            DataManager = new DataManager();
            Data = DataManager.GameData.GetAllGameData();
        }

        public static void ValidateData()
        {
            if (Data is null) throw new Exception("ModData not properly initialized");
        }

        public static ModCardData Card(Cards card) => new ModCardData(Data.FindCardData(card.GetID()));

        public static ModCardUpgradeData Upgrade(Upgrades upgrade)
        {
            string ID = upgrade.GetID();
            switch (ID.Length)
            {
                case 36:
                    try
                    {
                        return new ModCardUpgradeData(Data.GetAllCardUpgradeData().First(t => t.GetID() == ID));
                    }
                    catch
                    {
                        goto default;
                    }
                default:
                    return new ModCardUpgradeData(Data.GetAllCardUpgradeData().First(t => t.name == ID));
            }
        }

        public static ModCardUpgradeData ToModGameData(this CardUpgradeData upgrade) => new ModCardUpgradeData(upgrade);

        public static Traverse Field(this CharacterTriggerData characterTriggerData, string field) => Traverse.Create(characterTriggerData).Field(field);

        public static Traverse Field(this CardEffectData cardEffectData, string field) => Traverse.Create(cardEffectData).Field(field);

        private static readonly Dictionary<CardTrait, CardTraitData> TraitCache = new Dictionary<CardTrait, CardTraitData>(3);

        public static bool HasTrait(this CardData Card, CardTrait Trait)
        {
            if (Card.GetTraits() is null) return false;
            if (Card.GetTraits().FirstOrDefault(t => t.traitStateName == Trait.GetID()) is null) return false;
            return true;
        }

        public static CardTraitData Instance(this CardTrait trait, int paramint = 0)
        {
            lock (TraitCache)
            {
                if (TraitCache.ContainsKey(trait)) return TraitCache[trait].Copy();

                var ReferenceCards = from card in Data.GetAllCardData()
                                     where card.HasTrait(trait)
                                     select card;

                var CardTrait = ReferenceCards.First().GetTraits().First(t => t.traitStateName == trait.GetID()).Copy();
                TraitCache[trait] = CardTrait.Copy();
                if (paramint != 0) CardTrait.SetParamInt(paramint);

                return CardTrait;
            }
        }

        public static StatusEffectStackData Stack(this StatusEffect status, int stackCount = 1)
        {
            return new StatusEffectStackData
            {
                statusId = status.GetID(),
                count = stackCount
            };
        }
    }

    public abstract class ModGameData<Type> where Type : GameData
    {
        public readonly string name;
        public readonly string ID;
        public readonly Type Data;

        protected ModGameData(Type objectData)
        {
            name = objectData.name;
            ID = objectData.GetID();
            Data = objectData;
        }

        private Traverse _DataReflection;
        public Traverse DataReflection
        {
            get
            {
                _DataReflection ??= Traverse.Create(Data);
                return _DataReflection;
            }
        }

        public virtual T GetField<T>(string fieldname)
        {
            var thisField = DataReflection.Field(fieldname);
            if (thisField.GetValueType() == typeof(T)) return DataReflection.Field(fieldname).GetValue<T>();
            else
            {
                Logging.LogWarning($"Attempted GetField on {fieldname} but mismatched type: {thisField.GetValueType()} vs. {typeof(T)}.");
                return default;
            }
        }

        public virtual void SetField<T>(string fieldname, T value)
        {
            var thisField = DataReflection.Field(fieldname);
            if (thisField.GetValueType() == typeof(T)) thisField.SetValue(value);
            else Logging.LogWarning($"Attempted SetField on {fieldname} but mismatched type: {thisField.GetValueType()} vs. {typeof(T)}.");
        }
    }

    public class ModCardData : ModGameData<CardData>
    {
        public readonly CardType Type;
        public readonly ModCharacterData Monster;

        public ModCardData(CardData cardData) : base(cardData)
        {
            if (Data is null) Logging.LogError("Couldn't find card - This will cause crashes.");
            else
            {
                Type = Data.GetCardType();
                if (Type == CardType.Monster) Monster = new ModCharacterData(Data.GetSpawnCharacterData());
            }
        }

        public CardEffectData GetEffect(Func<CardEffectData, bool> condition) => Data.GetEffects().Single(condition);

        public ModCardData SetCost(int newCost)
        {
            if (newCost < 0) newCost = 0;
            SetField("cost", newCost);
            return this;
        }

        public ModCardData AddTraits(params CardTraitData[] cardTraits)
        {
            List<CardTraitData> currentTraits = Data.GetTraits();
            if (currentTraits is null) SetField("traits", cardTraits.ToList());
            else currentTraits.AddRange(cardTraits);
            return this;
        }

        public ModCardData SetDamage(int damage)
        {
            if (Type == CardType.Spell)
            {
                foreach (var effect in Data.GetEffects().Where(t => t.GetEffectStateName() == typeof(CardEffectDamage).Name))
                {
                    effect.Field("paramInt").SetValue(damage);
                }
            }
            return this;
        }

        public ModCardData SetDescription(string en,
                                          string fr = "",
                                          string de = "",
                                          string ru = "",
                                          string pt = "",
                                          string zh = "")
        {
            string DescriptionKey = $"mod_card_description_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            SetField("overrideDescriptionKey", DescriptionKey);
            return this;
        }
    }

    public class ModCharacterData : ModGameData<CharacterData>
    {
        public ModCharacterData(CharacterData monsterData) : base(monsterData)
        {
            if (Data is null) Logging.LogError("Couldn't find monster - This will cause crashes.");
        }

        public ModCharacterData SetDamage(int newDamage)
        {
            SetField("attackDamage", newDamage);
            return this;
        }

        public ModCharacterData SetHP(int newHP)
        {
            SetField("health", newHP);
            return this;
        }

        public CharacterTriggerData GetTrigger(Func<CharacterTriggerData, bool> condition) => Data.GetTriggers().Single(condition);

        public CharacterTriggerData GetTrigger(CharacterTriggerData.Trigger trigger) => GetTrigger(t => t.GetTrigger() == trigger);

        public ModCharacterData AddStartingStatusEffects(params StatusEffectStackData[] AddedStatuses)
        {
            StatusEffectStackData[] StartingStatuses = Data.GetStartingStatusEffects();
            if (StartingStatuses.IsNullOrEmpty()) SetField("startingStatusEffects", AddedStatuses);
            else
            {
                var StatusList = new List<StatusEffectStackData>(StartingStatuses.Length + AddedStatuses.Length);
                StatusList.AddRange(StartingStatuses);
                StatusList.AddRange(AddedStatuses);
                SetField("startingStatusEffects", StatusList.ToArray());
            }
            return this;
        }

        public ModCharacterData SetTriggerDescription(CharacterTriggerData.Trigger trigger,
                                                      string en,
                                                      string fr = "",
                                                      string de = "",
                                                      string ru = "",
                                                      string pt = "",
                                                      string zh = "")
        {
            string DescriptionKey = $"mod_unit_trigger_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Data.GetTriggers().Single(t => t.GetTrigger() == trigger).Field("descriptionKey").SetValue(DescriptionKey);
            return this;
        }
    }

    public class ModCardUpgradeData : ModGameData<CardUpgradeData>
    {
        public ModCardUpgradeData(CardUpgradeData upgradeData) : base(upgradeData)
        {
            if (Data is null) Logging.LogWarning("Couldn't find upgrade - This will cause crashes.");
        }

        public ModCardUpgradeData SetBonusDamage(int value)
        {
            SetField("bonusDamage", value);
            return this;
        }

        public ModCardUpgradeData SetBonusHP(int value)
        {
            SetField("bonusHP", value);
            return this;
        }

        public StatusEffectStackData GetStatusEffectUpgrade(StatusEffect status)
        {
            return Data.GetStatusEffectUpgrades().Single(t => t.statusId == status.GetID());
        }

        public ModCardUpgradeData AddStatusEffectUpgrades(params StatusEffectStackData[] data)
        {
            var currentStatusEffects = GetField<List<StatusEffectStackData>>("statusEffectUpgrades");
            if (currentStatusEffects is null) SetField("statusEffectUpgrades", data.ToList());
            else currentStatusEffects.AddRange(data);
            return this;
        }

        public ModCardUpgradeData SetUpgradeDescription(string en,
                                                        string fr = "",
                                                        string de = "",
                                                        string ru = "",
                                                        string pt = "",
                                                        string zh = "")
        {
            string DescriptionKey = $"mod_upgrade_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            SetField("upgradeDescriptionKey", DescriptionKey);
            return this;
        }

        public ModCardUpgradeData SetTriggerDescription(CharacterTriggerData.Trigger trigger,
                                                        string en,
                                                        string fr = "",
                                                        string de = "",
                                                        string ru = "",
                                                        string pt = "",
                                                        string zh = "")
        {
            string DescriptionKey = $"mod_upgrade_trigger_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Data.GetCharacterTriggerUpgrades().Single(t => t.GetTrigger() == trigger).Field("descriptionKey").SetValue(DescriptionKey);
            return this;
        }
    }
}