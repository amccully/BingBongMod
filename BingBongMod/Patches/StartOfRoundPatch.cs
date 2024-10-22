using UnityEngine;
using HarmonyLib;
using System;
using GameNetcodeStuff;
using System.Linq;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]

    internal class StartOfRoundPatch
    {

        [HarmonyPatch("ShipHasLeft")]
        [HarmonyPrefix]
        static void getUnrecordedStats(StartOfRound __instance)
        {
            BingBongModBase.MLS.LogInfo("Ship has left called! SHOULD ONLY CALL ONCE. Check for remaining stats.");
            PlayerControllerBPatch.CheckRecordEmoteTime();
        }

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
                if(pair.Key < 0 || pair.Key > __instance.gameStats.allPlayerStats.Length-1)
                {
                    BingBongModBase.MLS.LogError("Expected index in bounds but it wasn't: " + pair.Key);
                    continue;
                }
                if (__instance.gameStats.allPlayerStats[pair.Key].isActivePlayer && pair.Value > num)
                {
                    num = pair.Value;
                    num2 = pair.Key;
                }
            }
            BingBongModBase.MLS.LogDebug("Connected player count is: " + __instance.connectedPlayersAmount);
            if (__instance.connectedPlayersAmount > 0 && num2 != -1)
            {
                __instance.gameStats.allPlayerStats[num2].playerNotes.Insert(0, "Clumsiest misstepper");
                //__instance.gameStats.allPlayerStats[num2].playerNotes.Add("Clumsiest misstepper. Adding a bunch more text here to see how the game handles long notes!");
                BingBongModBase.MLS.LogDebug("Fall damage custom note added to player " + num2);
            }
        }

        [HarmonyPatch("WritePlayerNotes")]
        [HarmonyPostfix]
        static void emoteTimePlayerNotes(StartOfRound __instance)
        {
            BingBongModBase.MLS.LogInfo("Writing custom note now! (emote time)");
            float num = 15f;
            int num2 = -1;
            foreach (var pair in CustomPlayerNotes.emoteTimeDict)
            {
                BingBongModBase.MLS.LogDebug("Logging for emote pair key: " + pair);
                if (pair.Key < 0 || pair.Key > __instance.gameStats.allPlayerStats.Length - 1)
                {
                    BingBongModBase.MLS.LogError("Expected index in bounds but it wasn't: " + pair.Key);
                    continue;
                }
                if (__instance.gameStats.allPlayerStats[pair.Key].isActivePlayer && pair.Value > num)
                {
                    num = pair.Value;
                    num2 = pair.Key;
                }
            }
            BingBongModBase.MLS.LogDebug("Connected player count is: " + __instance.connectedPlayersAmount);
            if (__instance.connectedPlayersAmount > 0 && num2 != -1)
            {
                __instance.gameStats.allPlayerStats[num2].playerNotes.Insert(0, "Groovy Goober \U0001F60E"); // testing with emoticon text!
                BingBongModBase.MLS.LogDebug("Emote time custom note added to player " + num2);
            }
        }

        [HarmonyPatch("WritePlayerNotes")]
        [HarmonyPostfix]
        static void bullyHitsPlayerNotes(StartOfRound __instance)
        {
            BingBongModBase.MLS.LogInfo("Writing custom note now! (biggest bully)");
            int num = 2;
            int num2 = -1;
            foreach (var pair in CustomPlayerNotes.bullyHits)
            {
                BingBongModBase.MLS.LogDebug("Logging for bully hit pair key: " + pair);
                if (pair.Key < 0 || pair.Key > __instance.gameStats.allPlayerStats.Length - 1)
                {
                    BingBongModBase.MLS.LogError("Expected index in bounds but it wasn't: " + pair.Key);
                    continue;
                }
                if (__instance.gameStats.allPlayerStats[pair.Key].isActivePlayer && pair.Value > num)
                {
                    num = pair.Value;
                    num2 = pair.Key;
                }
            }
            BingBongModBase.MLS.LogDebug("Connected player count is: " + __instance.connectedPlayersAmount);
            if (__instance.connectedPlayersAmount > 0 && num2 != -1)
            {
                __instance.gameStats.allPlayerStats[num2].playerNotes.Insert(0, "Biggest Bully"); // testing with emoticon text!
                BingBongModBase.MLS.LogDebug("Biggest bully custom note added to player " + num2);
            }
        }

        [HarmonyPatch("WritePlayerNotes")]
        [HarmonyPostfix]
        static void bullyRelationPlayerNotes(StartOfRound __instance)
        {
            BingBongModBase.MLS.LogInfo("Writing custom note now! (bullied player)");
            int num = 1;
            var playerPair = (-1,-1);
            BingBongModBase.MLS.LogInfo("Length of gameStats: " + __instance.gameStats.allPlayerStats.Length);
            foreach (var pair in CustomPlayerNotes.bullyRelation)
            {
                BingBongModBase.MLS.LogDebug("Logging for bully relation pair key: " + pair.Key);
                if (pair.Key.Item1 < 0 || pair.Key.Item1 > __instance.gameStats.allPlayerStats.Length - 1 ||
                    pair.Key.Item2 < 0 || pair.Key.Item2 > __instance.gameStats.allPlayerStats.Length - 1)
                {
                    BingBongModBase.MLS.LogError("Expected index in bounds but it wasn't: " + pair.Key);
                    continue;
                }
                if (__instance.gameStats.allPlayerStats[pair.Key.Item1].isActivePlayer &&
                    __instance.gameStats.allPlayerStats[pair.Key.Item2].isActivePlayer && pair.Value > num)
                {
                    num = pair.Value;
                    playerPair = pair.Key;
                }
            }
            BingBongModBase.MLS.LogDebug("Connected player count is: " + __instance.connectedPlayersAmount);
            if (__instance.connectedPlayersAmount > 0 && playerPair != (-1,-1))
            {
                string theBully = RoundManager.Instance.playersManager.allPlayerScripts[playerPair.Item1].playerUsername;
                __instance.gameStats.allPlayerStats[playerPair.Item2].playerNotes.Insert(0, theBully + "'s Bitch");
                BingBongModBase.MLS.LogDebug("Bullied player custom note added to player " + playerPair);
            }
        }

        [HarmonyPatch("ResetStats")]
        [HarmonyPostfix]
        static void resetStats(StartOfRound __instance)
        {
            PlayerControllerBPatch.ResetUserVars();
            CustomPlayerNotes.ResetStats();
        }
    }
}