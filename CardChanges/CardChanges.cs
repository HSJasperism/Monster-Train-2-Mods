using BepInEx;
using HarmonyLib;
using System.Diagnostics;
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

        public static DataManager ModDataManager;

        public static void ChangeAssets()
        {
            var stopwatch = Stopwatch.StartNew();

            var PatchingTasks = new Task[]
            {
                Task.Run(() => Patches.HellhornedReworks()),
                Task.Run(() => Patches.AwokenReworks()),
                Task.Run(() => Patches.StygianReworks()),
                Task.Run(() => Patches.UmbraReworks()),
                Task.Run(() => Patches.MeltingRemnantReworks())
            };

            Task.WaitAll(PatchingTasks);

            stopwatch.Stop();
            Logging.LogInfo($"Completed data patching for {GUID} in {stopwatch.ElapsedMilliseconds}ms.");
        }

        public void Awake()
        {
            ModDataManager ??= new DataManager();
            DepInjector.AddClient(ModDataManager);
            var Hook = new Harmony(GUID);
            Hook.PatchAll();
        }
    }

    [HarmonyPatch(typeof(AssetLoadingManager), "Start")]
    public class PatchCardChanges
    {
        public static void Postfix() => CardChanges.ChangeAssets();
    }
}