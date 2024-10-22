using BepInEx;
using BepInEx.Logging;
using BingBongMod.Patches;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BingBongMod
{
    [BepInDependency("LethalNetworkAPI")]
    [BepInPlugin(modGUID, modName, modVer)]
    //[BepInDependency("Saradora.UnityNetworkMessages")]
    //[BepInDependency(LethalLib.Plugin.ModGUID)]
    public class BingBongModBase : BaseUnityPlugin
    {
        private const string modGUID = "GoodLucck.BingBongMod";
        private const string modName = "Bing Bong Mod";
        private const string modVer = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static BingBongModBase Instance;

        // make static so it can be used by patches
        internal static ManualLogSource MLS;

        // static var for number of inventory slots
        internal static int numSlotsToAdd = 2;

        // asset bundle for sound
        internal static List<AudioClip> AddedSounds;
        internal static AssetBundle assetBundle;

        void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }

            MLS = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            MLS.LogInfo("Initializing Bing Bong Mod");

            CustomPlayerNotes.Init();

            harmony.PatchAll(typeof(BingBongModBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            //harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(BoomboxItemPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            //harmony.PatchAll(typeof(ShovelPatch));

            AddedSounds = new List<AudioClip>();
            //string currDir = Environment.CurrentDirectory.ToString();
            //string assetLocation = System.IO.Directory.GetParent(currDir).FullName;
            //assetLocation = assetLocation.TrimEnd("BingBongMod.dll".ToCharArray());
            string currDir = Instance.Info.Location;
            string assetLocation = currDir.TrimEnd("BingBongMod.dll".ToCharArray());
            assetBundle = AssetBundle.LoadFromFile(assetLocation + "boombox_music");
            if(!assetBundle)
            {
                MLS.LogError("Asset files for this mod were not found! The assets should be in the same folder as the mod's dll!");
            }
            else
            {
                MLS.LogInfo("Loading asset bundle...");
                AddedSounds = assetBundle.LoadAllAssets<AudioClip>().ToList();
            }

        }
    }
}
