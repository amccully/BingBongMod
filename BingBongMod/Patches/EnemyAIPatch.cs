using BingBongMod.PotionBehavior;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static UnityEngine.UI.Image;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatch
    {
        private static PlayerControllerB[] oldPlayerScripts;
        static IEnumerable<MethodBase> TargetMethods()
        {
            // List of methods to patch
            yield return AccessTools.Method(typeof(EnemyAI), "CheckLineOfSightForPlayer");
            yield return AccessTools.Method(typeof(EnemyAI), "CheckLineOfSightForClosestPlayer");
            yield return AccessTools.Method(typeof(EnemyAI), "GetAllPlayersInLineOfSight");
        }

        [HarmonyPrefix]
        static void PreCheckLineOfSightForPlayer()
        {
            oldPlayerScripts = StartOfRound.Instance.allPlayerScripts;
            StartOfRound.Instance.allPlayerScripts = StartOfRound.Instance.allPlayerScripts
                .Where(player => player == null || !player.isPlayerControlled || !PotionNetwork.invisiblePlayers.Contains((int)player.playerClientId))
                .ToArray();
            if(!StartOfRound.Instance.allPlayerScripts.SequenceEqual(oldPlayerScripts))
            {
                BingBongModBase.MLS.LogInfo("Detected difference, players is excluded");
            }
        }

        [HarmonyPostfix]
        static void PostCheckLineOfSightForPlayer()
        {
            StartOfRound.Instance.allPlayerScripts = oldPlayerScripts;
        }

        //[HarmonyPatch("PlayerIsTargetable")]
        //[HarmonyPrefix]
        //static bool PlayerIsTargetablePatch(PlayerControllerB playerScript)
        //{
        //    if (PotionNetwork.invisiblePlayers.Contains((int)playerScript.playerClientId)) {
        //        BingBongModBase.MLS.LogInfo("Player script " + (int)playerScript.playerClientId + " not targetable!");
        //        return false;
        //    }
        //    return true;
        //}

    }
}
