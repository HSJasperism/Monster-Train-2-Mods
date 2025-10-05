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
            CardChanges.ModDataManager ??= new DataManager();
            var PatchingTasks = new Task[]
            {
                Task.Run(() => Patches.LunaCoven()),
                Task.Run(() => Patches.Hellhorned()),
                Task.Run(() => Patches.Awoken()),
                Task.Run(() => Patches.StygianGuard()),
                Task.Run(() => Patches.Umbra()),
                Task.Run(() => Patches.MeltingRemnant())
            };
        }
    }
}