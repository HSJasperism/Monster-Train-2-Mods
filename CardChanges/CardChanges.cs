using BepInEx;
using HarmonyLib;

namespace CardChanges
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess("MonsterTrain2.exe")]
    public class CardChanges : BaseUnityPlugin
    {
        public const string GUID = "com.mod.cardchanges";
        public const string NAME = "Card Changes";
        public const string VERSION = "0.0.1";

        public void Awake()
        {
            var Hook = new Harmony(GUID);
            Hook.PatchAll();
        }
    }

    [HarmonyPatch(typeof(AssetLoadingManager), "Start")]
    public class PatchCardChanges
    {
        public static void Postfix()
        {
            Mod.ValidateData();
            Patches.LunaCoven.Start();
            Patches.Underlegion.Start();
            Patches.Hellhorned.Start();
            Patches.Awoken.Start();
            Patches.StygianGuard.Start();
            Patches.Umbra.Start();
            Patches.MeltingRemnant.Start();
        }
    }
}