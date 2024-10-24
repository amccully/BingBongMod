﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(BoomboxItem))]
    internal class BoomboxItemPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void AddBoomboxSongs(BoomboxItem __instance)
        {
            if(!__instance)
            {
                BingBongModBase.MLS.LogWarning("Boombox instance is null, that's odd.");
                return;
            }
            var musicAudiosAsList = __instance.musicAudios.ToList();
            musicAudiosAsList.Add(BingBongModBase.AddedSounds[0]);
            musicAudiosAsList.Add(BingBongModBase.AddedSounds[1]);
            __instance.musicAudios = musicAudiosAsList.ToArray();
        }
    }
}
