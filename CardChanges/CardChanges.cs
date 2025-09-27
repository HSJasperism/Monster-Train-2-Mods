using BepInEx;
using HarmonyLib;
using System;
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
            var stopwatch = Stopwatch.StartNew();

            CardChanges.ModDataManager ??= new DataManager();

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
            Logging.LogInfo($"Completed data patching for {CardChanges.GUID} in {stopwatch.ElapsedMilliseconds}ms.");
        }
    }
}