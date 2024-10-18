using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        static float timeElapsedSinceStartedDancing = 0f;
        
        static bool hitByShovel = false;
        public static void ResetUserVars()
        {
            BingBongModBase.MLS.LogInfo("PLAYERCONTROLLER RESET VARS CALLED, SETTING VARS");
            timeElapsedSinceStartedDancing = 0f;
        }

        public static float GetEmoteTime()
        {
            return timeElapsedSinceStartedDancing;
        }

        //[HarmonyPatch("Hit")]
        //[HarmonyPostfix]
        //static void HitByShovelItem(ref bool __result, int hitID)
        //{
        //    if (!__result) { return; }

        //    if (hitID == 1) // hit ID of the shovel weapon
        //    {
        //        BingBongModBase.MLS.LogInfo("This client got hit by a shovel. hitByShovel is now true");
        //        hitByShovel = true;
        //    }
        //}

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPrefix]
        static void PlayerShovelDamage(PlayerControllerB __instance, int damageNumber, CauseOfDeath causeOfDeath)
        {
            if (!__instance.IsOwner || __instance.isPlayerDead || !__instance.AllowPlayerDeath())
            {
                return;
            }

            if (causeOfDeath == CauseOfDeath.Bludgeoning)
            {
                BingBongModBase.MLS.LogInfo("PLAYER TOOK BLUDGEONING DAMAGE: " + damageNumber);
            }
        }

        //[HarmonyPatch("DamagePlayerFromOtherClientClientRpc")]
        //[HarmonyPostfix]
        //static void DamageFromOtherPlayers(PlayerControllerB __instance, int damageAmount, int playerWhoHit)
        //{
        //    if (__instance.IsOwner && __instance.isPlayerControlled)
        //    {
        //        BingBongModBase.MLS.LogInfo("This client got damaged by player " + playerWhoHit + " for " + damageAmount);
        //    }
        //}

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void PlayerEmoteTime(PlayerControllerB __instance)
        {
            // testing emote related vars
            // takeaway seems to be that update is called 4 times, only 1 of which we care about, hence the if check at the start
            // which encapsulates all the logic
            // "Classes deriving from NetworkBehaviour and NetworkObject have built in isServer bools to easily check"
            if ((__instance.IsOwner && __instance.isPlayerControlled && (!__instance.IsServer || __instance.isHostPlayerObject)) || __instance.isTestingPlayer)
            {
                // emoteNumber for dancing is 1
                if (__instance.performingEmote && __instance.playerBodyAnimator.GetInteger("emoteNumber") == 1)
                {
                    //timeElapsedSinceStartedDancing = __instance.timeSinceStartingEmote;
                    timeElapsedSinceStartedDancing += Time.deltaTime;
                    BingBongModBase.MLS.LogInfo("Player is in dancing animation. Time since starting: " + timeElapsedSinceStartedDancing);
                }
                else if(timeElapsedSinceStartedDancing > 0f)
                {
                    BingBongModBase.MLS.LogInfo("Player finished dancing, adding " + timeElapsedSinceStartedDancing + " to emote time amount");
                    CustomPlayerNotes.addEmoteTime(__instance, timeElapsedSinceStartedDancing);
                    timeElapsedSinceStartedDancing = 0f;
                }
            }
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static void RecordEmoteTimeOnDeath(PlayerControllerB __instance)
        {
            if (__instance.IsOwner && !__instance.isPlayerDead && __instance.AllowPlayerDeath())
            {
                // player was killed, check to see if emote time needs to be recorded
                if (__instance.performingEmote && __instance.playerBodyAnimator.GetInteger("emoteNumber") == 1)
                {
                    BingBongModBase.MLS.LogInfo("Kill player prefix called and player was dancing. Record time: " + timeElapsedSinceStartedDancing);
                    CustomPlayerNotes.addEmoteTime(__instance, timeElapsedSinceStartedDancing);
                    timeElapsedSinceStartedDancing = 0f;
                }
            }
        }

        //[HarmonyPatch("Awake")]
        //[HarmonyPostfix]
        //static void Awake(PlayerControllerB __instance)
        //{
        //    BingBongModBase.MLS.LogInfo("AWAKE METHOD CALLED");
        //    timeElapsedSinceStartedDancing = 0f;
        //}

        // -----------------------------------------

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPrefix]
        static void PlayerFallDamage(PlayerControllerB __instance, bool fallDamage, int damageNumber)
        {
            if (!__instance.IsOwner || __instance.isPlayerDead || !__instance.AllowPlayerDeath())
            {
                return;
            }

            if (__instance.takingFallDamage)
            {
                BingBongModBase.MLS.LogInfo("Player took fall damage, adding " + damageNumber + " to fall damage amount");
                CustomPlayerNotes.addFallDamage(__instance, damageNumber);
            }
        }

        // (inside of HarmonyPatch:) nameof(PlayerControllerB.), after . search for name of method
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void InfiniteSprint(ref float ___sprintMeter) // triple underscore?
        {
            ___sprintMeter = 1f; // max value for meter, so every frame it'll be set to max
        }

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void AddInventorySlots(PlayerControllerB __instance)
        {
            // testing based on code I found (check if server), seemingly results in slots only loading for the host
            //if (!NetworkManager.Singleton.IsServer)
            //{
            //    BingBongModBase.MLS.LogWarning("IsServer check in PlayerControllerB Awake was true?");
            //    return;
            //}

            if (!__instance || __instance.ItemSlots.Length != 4)
            {
                BingBongModBase.MLS.LogWarning("Inventory slots have been tampered with already! No modification applied.");
                return;
            }
            var numSlotsToAdd = BingBongModBase.numSlotsToAdd;
            var itemSlotsLength = __instance.ItemSlots.Length;
            __instance.ItemSlots = new GrabbableObject[itemSlotsLength + numSlotsToAdd];
        }

        [HarmonyPatch("Jump_performed")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> InfiniteJumpPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            for(int i = 0; i < codes.Count-10; i++) // we need to remove 10 lines of code so we can't be past Count-10
            {
                if (codes[i].IsLdarg() &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 1].operand is FieldInfo fieldInfo1 &&
                    fieldInfo1.Name == "thisController" &&
                    codes[i + 5].opcode == OpCodes.Ldfld &&
                    codes[i + 5].operand is FieldInfo fieldInfo2 &&
                    fieldInfo2.Name == "isJumping" &&
                    codes[i + 8].opcode == OpCodes.Call &&
                    codes[i + 8].operand is MethodInfo methodInfo &&
                    methodInfo.Name == "IsPlayerNearGround")
                {
                    for(int j = 0; j < 10; j++) // set all 10 instructions to be nops
                    {
                        codes[i + j].opcode = OpCodes.Nop;
                    }
                    return codes.AsEnumerable();
                }

            }
            return codes.AsEnumerable();
            
        }

    }
}
