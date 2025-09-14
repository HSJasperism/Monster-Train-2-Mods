using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace ThreeChoices
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess("MonsterTrain2.exe")]
    public class ThreeChoices : BaseUnityPlugin
    {
        public const string GUID = "com.mod.threechoices";
        public const string NAME = "Three Choices";
        public const string VERSION = "0.0.1";

        public static uint changeFrom = 2u;
        public static uint changeTo = 3u;

        private static readonly FieldInfo RelicTool = AccessTools.Field(typeof(RelicDraftRewardData), "draftOptionsCount");
        private static readonly FieldInfo DraftTool = AccessTools.Field(typeof(DraftRewardData), "draftOptionsCount");

        public static void UpdateHerzalHoard(RelicDraftRewardData target)
        {
            if ((uint)RelicTool.GetValue(target) == changeFrom)
            {
                RelicTool.SetValue(target, changeTo);
            }
        }

        public static void UpdateBanner(DraftRewardData target)
        {
            if ((uint)DraftTool.GetValue(target) == changeFrom)
            {
                DraftTool.SetValue(target, changeTo);
            }
        }

        public void Awake()
        {
            Harmony Hook = new Harmony(GUID);
            Hook.PatchAll();
        }
    }

    [HarmonyPatch(typeof(RelicDraftRewardData), "GatherPregeneratedRelics")]
    internal class RelicDraftRewardData_GatherPregeneratedRelics_Patch
    {
        internal static void Prefix(RelicDraftRewardData __instance) => ThreeChoices.UpdateHerzalHoard(__instance);
    }

    [HarmonyPatch(typeof(RelicDraftRewardData), "GetRelicsFromClasses")]
    internal class RelicDraftRewardData_GetRelicsFromClasses_Patch
    {
        internal static void Prefix(RelicDraftRewardData __instance) => ThreeChoices.UpdateHerzalHoard(__instance);
    }

    [HarmonyPatch(typeof(DraftRewardData), "GetCardsFromClasses")]
    internal class DraftRewardData_GetCardsFromClasses_Patch
    {
        internal static void Prefix(DraftRewardData __instance) => ThreeChoices.UpdateBanner(__instance);
    }
}