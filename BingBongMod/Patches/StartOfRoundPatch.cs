using UnityEngine;
using HarmonyLib;
using System;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]

    internal class StartOfRoundPatch
    {
        [HarmonyPatch("WritePlayerNotes")]
        [HarmonyPostfix]
        static void fallDamagePlayerNotes(StartOfRound __instance)
        {
            BingBongModBase.MLS.LogInfo("Writing custom note now! (fall damage)");
            int num = 0;
            int num2 = -1;
            foreach (var pair in CustomPlayerNotes.fallDamageDict)
            {
                BingBongModBase.MLS.LogDebug("Logging for damage pair key: " + pair);
                if (__instance.gameStats.allPlayerStats[pair.Key].isActivePlayer && pair.Value > num)
                {
                    num = pair.Value;
                    num2 = pair.Key;
                }
            }
            BingBongModBase.MLS.LogDebug("Connected player count is: " + __instance.connectedPlayersAmount);
            if (__instance.connectedPlayersAmount > 0 && num2 != -1)
            {
                __instance.gameStats.allPlayerStats[num2].playerNotes.Add("Clumsiest misstepper \U0001F573");
                //__instance.gameStats.allPlayerStats[num2].playerNotes.Add("Clumsiest misstepper. Adding a bunch more text here to see how the game handles long notes!");
                BingBongModBase.MLS.LogDebug("Fall damage custom note added to player " + num2);
            }
        }

        [HarmonyPatch("WritePlayerNotes")]
        [HarmonyPostfix]
        static void emoteTimePlayerNotes(StartOfRound __instance)
        {
            BingBongModBase.MLS.LogInfo("Writing custom note now! (emote time)");
            float num = 0;
            int num2 = -1;
            foreach (var pair in CustomPlayerNotes.emoteTimeDict)
            {
                BingBongModBase.MLS.LogDebug("Logging for emote pair key: " + pair);
                if (__instance.gameStats.allPlayerStats[pair.Key].isActivePlayer && pair.Value > num)
                {
                    num = pair.Value;
                    num2 = pair.Key;
                }
            }
            BingBongModBase.MLS.LogDebug("Connected player count is: " + __instance.connectedPlayersAmount);
            if (__instance.connectedPlayersAmount > 0 && num2 != -1)
            {
                __instance.gameStats.allPlayerStats[num2].playerNotes.Add("Groovy Goober \U0001F60E"); // testing with emoticon text!
                BingBongModBase.MLS.LogDebug("Emote time custom note added to player " + num2);
            }
        }

        [HarmonyPatch("ResetStats")]
        [HarmonyPostfix]
        static void resetFallDamageStats(StartOfRound __instance)
        {
            CustomPlayerNotes.ResetStats();
        }
    }
}