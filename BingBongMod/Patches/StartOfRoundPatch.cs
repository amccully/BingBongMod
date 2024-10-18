using UnityEngine;
using HarmonyLib;
using System;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]

    internal class StartOfRoundPatch
    {
        //[HarmonyPatch("ShipLeave")]
        //[HarmonyPostfix]
        //static void shipLeave(StartOfRound __instance)
        //{
        //    BingBongModBase.MLS.LogInfo("Ship leave called on client, send remaining data if any");
        //    CustomPlayerNotes.addEmoteTime(GameNetworkManager.Instance.localPlayerController, PlayerControllerBPatch.getEmoteTime());
        //}

        [HarmonyPatch("ShipHasLeft")]
        [HarmonyPrefix]
        static void getUnrecordedStats(StartOfRound __instance)
        {
            BingBongModBase.MLS.LogInfo("Ship has left called! SHOULD ONLY CALL ONCE. Send remaining game stats data.");
            float emoteTime = PlayerControllerBPatch.GetEmoteTime();
            PlayerControllerBPatch.ResetUserVars();
            if (emoteTime > 0f)
            {
                CustomPlayerNotes.addEmoteTime(GameNetworkManager.Instance.localPlayerController, emoteTime);
            }
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
                if (__instance.gameStats.allPlayerStats[pair.Key].isActivePlayer && pair.Value > num)
                {
                    num = pair.Value;
                    num2 = pair.Key;
                }
            }
            BingBongModBase.MLS.LogDebug("Connected player count is: " + __instance.connectedPlayersAmount);
            if (__instance.connectedPlayersAmount > 0 && num2 != -1)
            {
                __instance.gameStats.allPlayerStats[num2].playerNotes.Insert(0, "Clumsiest misstepper \U0001F573");
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

        [HarmonyPatch("ResetStats")]
        [HarmonyPostfix]
        static void resetStats(StartOfRound __instance)
        {
            PlayerControllerBPatch.ResetUserVars();
            CustomPlayerNotes.ResetStats();
        }
    }
}