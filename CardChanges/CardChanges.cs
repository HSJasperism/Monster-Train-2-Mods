using BepInEx;
using HarmonyLib;
using System.Threading.Tasks;

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
        public static async void Postfix()
        {
            Mod.ValidateData();
            var Tasks = new[]
            {
                Patches.LunaCoven,
                Patches.Underlegion,
                Patches.Hellhorned,
                Patches.Awoken,
                Patches.StygianGuard,
                Patches.Umbra,
                Patches.MeltingRemnant
            };
            foreach (var task in Tasks) task.Start();
            await Task.WhenAll(Tasks);
        }
    }
}