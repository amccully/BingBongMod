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
        [HarmonyPatch("HitShovel")]
        [HarmonyPostfix]
        static void listItemsHitByShovel(List<RaycastHit> ___objectsHitByShovelList)
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
