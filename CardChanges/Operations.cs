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

        public T TryGetProvider<T>() where T : IProvider => (T)_ProviderDictionary[typeof(T)];

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
        private static AllGameData _Data;
        public static AllGameData Data
        {
            get
            {
                _Data ??= CardChanges.ModDataManager.GameData.GetAllGameData();
                return _Data;
            }
        }

        public static ModCardData Card(Cards card)
            => new ModCardData(Data.FindCardData(card.GetID()));

        public static ModCardUpgradeData Upgrade(Upgrades upgrade)
            => new ModCardUpgradeData(Data.FindCardUpgradeData(upgrade.GetID()));

        public static ModCardUpgradeData ToModGameData(this CardUpgradeData upgrade)
            => new ModCardUpgradeData(upgrade);

        public static Traverse Field(this CharacterTriggerData characterTriggerData, string field)
            => Traverse.Create(characterTriggerData).Field(field);

        public static Traverse Field(this CardEffectData cardEffectData, string field)
            => Traverse.Create(cardEffectData).Field(field);

        public static Traverse Field<T>(this T gameData, string field) where T : GameData
            => Traverse.Create(gameData).Field(field);

        public static CardTraitData Instance(this CardTrait trait,
                                             float paramfloat = 0,
                                             int paramint = 0,
                                             string paramstr = "",
                                             string paramsubtype = "SubtypesData_None",
                                             string paramdesc = "",
                                             Team.Type paramteamtype = Team.Type.None,
                                             StatusEffectStackData[] paramstatuseffects = null)
        {
            var Trait = new CardTraitData { traitStateName = trait.GetID() };
            var TraverseTrait = Traverse.Create(Trait);
            TraverseTrait.Field("paramFloat").SetValue(paramfloat);
            TraverseTrait.Field("paramInt").SetValue(paramint);
            TraverseTrait.Field("paramStr").SetValue(paramstr);
            TraverseTrait.Field("paramDescription").SetValue(paramdesc);
            TraverseTrait.Field("paramTeamType").SetValue(paramteamtype);
            TraverseTrait.Field("paramSubtype").SetValue(paramsubtype);
            paramstatuseffects ??= new StatusEffectStackData[0];
            TraverseTrait.Field("paramStatusEffects").SetValue(paramstatuseffects);
            return Trait;
        }

        public static StatusEffectStackData Stack(this StatusEffect status, int stackCount = 1)
            => new StatusEffectStackData { statusId = status.GetID(), count = stackCount };
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

        public void SetCost(int newCost)
        {
            if (newCost < 0) newCost = 0;
            SetField("cost", newCost);
        }

        public void AddTraits(params CardTraitData[] cardTraits)
        {
            List<CardTraitData> currentTraits = Data.GetTraits();
            if (currentTraits is null) SetField("traits", cardTraits.ToList());
            else currentTraits.AddRange(cardTraits);
        }

        public void SetDamage(int damage)
        {
            if (Type == CardType.Spell)
            {
                Data.GetEffects().FirstOrDefault(t => t.GetEffectStateName() == "CardEffectDamage").Field("paramInt").SetValue(damage);
            }
        }

        public void SetDescription(string en,
                                   string fr = "",
                                   string de = "",
                                   string ru = "",
                                   string pt = "",
                                   string zh = "")
        {
            string DescriptionKey = $"mod_card_description_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            SetField("overrideDescriptionKey", DescriptionKey);
        }
    }

    public class ModCharacterData : ModGameData<CharacterData>
    {
        public ModCharacterData(CharacterData monsterData) : base(monsterData)
        {
            if (Data is null) Logging.LogError("Couldn't find monster - This will cause crashes.");
        }

        public void SetDamage(int newDamage) => SetField("attackDamage", newDamage);

        public void SetHP(int newHP) => SetField("health", newHP);

        public void AddStartingStatusEffects(params StatusEffectStackData[] data)
        {
            StatusEffectStackData[] StartingStatuses = Data.GetStartingStatusEffects();
            if (StartingStatuses.IsNullOrEmpty()) SetField("startingStatusEffects", data);
            else
            {
                var newArray = new StatusEffectStackData[StartingStatuses.Length + data.Length];
                StartingStatuses.CopyTo(newArray, 0);
                data.CopyTo(newArray, StartingStatuses.Length);
                SetField("startingStatusEffects", newArray);
            }
        }

        public void SetTriggerDescription(CharacterTriggerData.Trigger trigger,
                                          string en,
                                          string fr = "",
                                          string de = "",
                                          string ru = "",
                                          string pt = "",
                                          string zh = "")
        {
            string DescriptionKey = $"mod_unit_trigger_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Data.GetTriggers().FirstOrDefault(t => t.GetTrigger() == trigger).Field("descriptionKey").SetValue(DescriptionKey);
        }
    }

    public class ModCardUpgradeData : ModGameData<CardUpgradeData>
    {
        public ModCardUpgradeData(CardUpgradeData upgradeData) : base(upgradeData)
        {
            if (Data is null) Logging.LogWarning("Couldn't find upgrade - This will cause crashes.");
        }

        public void SetBonusDamage(int value) => SetField("bonusDamage", value);

        public void SetBonusHP(int value) => SetField("bonusHP", value);

        public StatusEffectStackData GetStatusEffectUpgrade(StatusEffect status)
            => Data.GetStatusEffectUpgrades().FirstOrDefault(t => t.statusId == status.GetID());

        public void AddStatusEffectUpgrades(params StatusEffectStackData[] data)
        {
            var currentStatusEffects = GetField<List<StatusEffectStackData>>("statusEffectUpgrades");
            if (currentStatusEffects is null) SetField("statusEffectUpgrades", data.ToList());
            else currentStatusEffects.AddRange(data);
        }

        public void SetUpgradeDescription(string en,
                                          string fr = "",
                                          string de = "",
                                          string ru = "",
                                          string pt = "",
                                          string zh = "")
        {
            string DescriptionKey = $"mod_upgrade_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            SetField("upgradeDescriptionKey", DescriptionKey);
        }

        public void SetTriggerDescription(CharacterTriggerData.Trigger trigger,
                                          string en,
                                          string fr = "",
                                          string de = "",
                                          string ru = "",
                                          string pt = "",
                                          string zh = "")
        {
            string DescriptionKey = $"mod_upgrade_trigger_{ID}";
            ModLocalization.AddLocalization(key: DescriptionKey, en_us: en, fr_fr: fr, de_de: de, ru_ru: ru, pt_br: pt, zh_cn: zh);
            Data.GetCharacterTriggerUpgrades().FirstOrDefault(t => t.GetTrigger() == trigger).Field("descriptionKey").SetValue(DescriptionKey);
        }
    }
}