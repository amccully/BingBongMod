using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal class ShovelPatch
    {
        // idea: in hit shovel method you could use information about
        // which IDs got hit, see what the first is to identify hit player
        // then we could check if the centipede is clinging to that player (or what other checks we might need)
        // lastly register the damage request from that method

        [HarmonyPatch("HitShovel")]
        [HarmonyPostfix]
        static void listItemsHitByShovel(List<RaycastHit> ___objectsHitByShovelList, PlayerControllerB ___previousPlayerHeldBy, Shovel __instance)
        {
            for (int i = 0; i < ___objectsHitByShovelList.Count; i++)
            {
                // Check if the hit object has the PlayerControllerB component
                PlayerControllerB playerController = ___objectsHitByShovelList[i].transform.GetComponent<PlayerControllerB>();

                if (playerController != null)
                {
                    int playerClientId = (int)playerController.playerClientId;
                    BingBongModBase.MLS.LogInfo("Shovel Obj " + i + " was player with Client ID: " + playerClientId);
                }
                else
                {
                    BingBongModBase.MLS.LogInfo("Shovel Obj " + i + " was not a player");
                }

            }
        }
    }
}
