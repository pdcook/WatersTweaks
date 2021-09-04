using BepInEx; // requires BepInEx.dll and BepInEx.Harmony.dll
using BepInEx.Configuration;
using UnboundLib; // requires UnboundLib.dll
using UnboundLib.Cards; // " "
using UnboundLib.Networking;
using UnboundLib.GameModes;
using UnityEngine; // requires UnityEngine.dll, UnityEngine.CoreModule.dll, and UnityEngine.AssetBundleModule.dll
using HarmonyLib; // requires 0Harmony.dll
using System.Collections;
using Jotunn.Utils;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnboundLib.Utils.UI;
using TMPro;
using Photon.Pun;

// requires Assembly-CSharp.dll
// requires MMHOOK-Assembly-CSharp.dll

namespace WaterMod
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)] // necessary for most modding stuff here
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class WaterMod : BaseUnityPlugin
    {

        private const string ModId = "pykess.rounds.plugins.waterstweaks";

        private const string ModName = "Water's Tweaks";
        private const string CompatibilityModName = "WatersTweaks";
        private const string Version = "0.0.1";
        internal static AssetBundle ArtAssets;

        private static ConfigEntry<bool> QuickShotConfig;
        private static ConfigEntry<bool> GrowConfig;
        private static ConfigEntry<bool> GlassCannonConfig;

        internal static bool QuickShot;
        internal static bool Grow;
        internal static bool GlassCannon;

        private void Awake()
        {
            // bind configs
            QuickShotConfig = Config.Bind(CompatibilityModName, "QuickShot", true, "Rebalance Quick Shot to add attack speed and increase reload time");
            GrowConfig = Config.Bind(CompatibilityModName, "Grow", true, "Rebalance Grow to increase its rarity");
            GlassCannonConfig = Config.Bind(CompatibilityModName, "GlassCannon", true, "Rebalance Glass Cannon to remove reload penalty and reduce health penalty");


            new Harmony(ModId).PatchAll();
        }
        private void Start()
        {
            // read config in order to not orphan them
            QuickShot = QuickShotConfig.Value;
            Grow = GrowConfig.Value;
            GlassCannon = GlassCannonConfig.Value;

            // register credits with unbound
            Unbound.RegisterCredits(ModName, new string[] { "Pykess", "Commissioned by Water" }, new string[] { "github", "Commission your own mod" }, new string[] { "https://github.com/pdcook/WatersTweaks", "https://www.buymeacoffee.com/Pykess" });

            WaterMod.ArtAssets = AssetUtils.LoadAssetBundleFromResources("watermodassetbundle", typeof(WaterMod).Assembly);
            if (WaterMod.ArtAssets == null)
            {
                UnityEngine.Debug.Log("Failed to load WaterMod art asset bundle");
            }

            // build all cards
            CustomCard.BuildCard<ExtraAmmoCard>(ExtraAmmoCard.callback);

            // add GUI to modoptions menu
            Unbound.RegisterMenu(ModName, () => { }, this.NewGUI, null, false);

            // handshake to sync settings
            Unbound.RegisterHandshake(ModId, OnHandShakeCompleted);

            // tweak cards
            TweakCards.Tweak();

            // add init hook to tweak cards
            GameModeManager.AddHook(GameModeHooks.HookInitStart, TweakCards.TweakEnum);


        }
        private void NewGUI(GameObject menu)
        {

            MenuHandler.CreateText(ModName + " Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void QuickShotCheckbox(bool flag)
            {
                QuickShotConfig.Value = flag;
                QuickShot = flag;
                TweakCards.Tweak();
            }
            MenuHandler.CreateToggle(QuickShotConfig.Value, "Tweak Quick Shot", menu, QuickShotCheckbox, 30);
            MenuHandler.CreateText("Remove projectile speed stat, add +100% attack speed, increase reload time by 0.5s", menu, out TextMeshProUGUI _, 30);
            void GrowCheckbox(bool flag)
            {
                GrowConfig.Value = flag;
                Grow = flag;
                TweakCards.Tweak();
            }
            MenuHandler.CreateToggle(GrowConfig.Value, "Tweak Grow", menu, GrowCheckbox, 30);
            MenuHandler.CreateText("Change from uncommon to rare", menu, out TextMeshProUGUI _, 30);
            void GlassCannonCheckbox(bool flag)
            {
                GlassCannonConfig.Value = flag;
                GlassCannon = flag;
                TweakCards.Tweak();
            }
            MenuHandler.CreateToggle(GlassCannonConfig.Value, "Tweak Glass Cannon", menu, GlassCannonCheckbox, 30);
            MenuHandler.CreateText("Reduce health by -50% instead of -100%, remove reload penalty", menu, out TextMeshProUGUI _, 30);

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);

        }
        private static void OnHandShakeCompleted()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC_Others(typeof(WaterMod), nameof(SyncSettings), new object[] { WaterMod.QuickShot, WaterMod.Grow, WaterMod.GlassCannon });
            }
        }
        [UnboundRPC]
        private static void SyncSettings(bool quickshot, bool grow, bool glasscannon)
        {
            WaterMod.QuickShot = quickshot;
            WaterMod.Grow = grow;
            WaterMod.GlassCannon = glasscannon;

            TweakCards.Tweak();
        }
    }
}
