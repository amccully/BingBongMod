using BepInEx;
using BepInEx.Logging;
using BingBongMod.Patches;
using BingBongMod.PotionBehavior;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BingBongMod
{
    [BepInDependency("LethalNetworkAPI")]
    [BepInPlugin(modGUID, modName, modVer)]
    //[BepInDependency("Saradora.UnityNetworkMessages")]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
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

        // potion asset
        internal static Item emptyPotionBottle;
        internal static AudioClip drinkPotionSound;

        void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            
            MLS = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            MLS.LogInfo("Initializing Bing Bong Mod");

            CustomPlayerNotes.Init();
            PotionNetwork.Init();

            harmony.PatchAll(typeof(BingBongModBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            //harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(BoomboxItemPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            //harmony.PatchAll(typeof(ShovelPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));

            // for testing
            var methods = harmony.GetPatchedMethods();
            foreach (var method in methods)
            {
                BingBongModBase.MLS.LogInfo($"Patching method: {method.Name}");
            }


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

            // grab potion asset file from directory (must be in dll directory)
            string potionAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "potionitem");
            AssetBundle potionBundle = AssetBundle.LoadFromFile(potionAssetDir);

            // SOVEREIGN POTION ITEM
            Item sovereignPotion = potionBundle.LoadAsset<Item>("Assets/Potion Items/SovereignPotion.asset");
            sovereignPotion.syncUseFunction = true; // testing for syncing bottle change across clients

            // assign custom physics prop with properties set
            DrinkPotion potionScript = sovereignPotion.spawnPrefab.AddComponent<DrinkPotion>();
            potionScript.grabbable = true;
            potionScript.grabbableToEnemies = true;
            potionScript.itemProperties = sovereignPotion;

            Utilities.FixMixerGroups(sovereignPotion.spawnPrefab);

            int rarity = 100;
            Items.RegisterScrap(sovereignPotion, rarity, Levels.LevelTypes.All);

            NetworkPrefabs.RegisterNetworkPrefab(sovereignPotion.spawnPrefab);
            //

            // You need to make sure you register the Item for an empty glass, so that it can persist when set
            // however, the object were dealing with is just a grabbable object, which refers to an Item with its
            // itemProperties var, so we can potentially just the itemProperties to be our new Item and be finished

            // EMPTY POTION BOTTLE ITEM
            emptyPotionBottle = potionBundle.LoadAsset<Item>("Assets/Potion Items/Potion Glass.asset");
            emptyPotionBottle.syncUseFunction = true;
            Utilities.FixMixerGroups(emptyPotionBottle.spawnPrefab);
            Items.RegisterScrap(emptyPotionBottle, 0, Levels.LevelTypes.None); // Should never spawn
            NetworkPrefabs.RegisterNetworkPrefab(emptyPotionBottle.spawnPrefab);
            //

            // Potion drink audio
            drinkPotionSound = potionBundle.LoadAsset<AudioClip>("Assets/Potion Items/drink_potion.mp3");
            //
        }
    }
}
