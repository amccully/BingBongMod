using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Netcode;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        static bool playerIsDancing = false;
        static float timeElapsedSinceStartedDancing = 0f;

        [HarmonyPatch("PerformEmote")]
        [HarmonyPostfix]
        static void PerformDancingEmote(PlayerControllerB __instance, int emoteID)
        {
            if(__instance.performingEmote && emoteID == 1)
            {
                playerIsDancing = true;
                BingBongModBase.MLS.LogInfo("PerformEmote called, playerIsDancing is set to true: " + playerIsDancing);
            }
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPrefix]
        static void PlayerFallDamage(PlayerControllerB __instance, bool fallDamage, int damageNumber)
        {
            if (__instance.takingFallDamage)
            {
                BingBongModBase.MLS.LogInfo("Player took fall damage, adding " + damageNumber + " to fall damage amount");
                CustomPlayerNotes.addFallDamage(__instance, damageNumber);
            }
        }

        // (inside of HarmonyPatch:) nameof(PlayerControllerB.), after . search for name of method
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void InfiniteSprint(PlayerControllerB __instance, ref float ___sprintMeter) // triple underscore?
        {
            ___sprintMeter = 1f; // max value for meter, so every frame it'll be set to max
        }

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
                if (__instance.performingEmote && playerIsDancing)
                {
                    timeElapsedSinceStartedDancing = __instance.timeSinceStartingEmote;
                    BingBongModBase.MLS.LogDebug("timeElapsedSinceStartedDancing: " + timeElapsedSinceStartedDancing);
                }
                if (!__instance.performingEmote && playerIsDancing)
                {
                    BingBongModBase.MLS.LogInfo("Player finished dancing, adding " + timeElapsedSinceStartedDancing + " to emote time amount");
                    CustomPlayerNotes.addEmoteTime(__instance, timeElapsedSinceStartedDancing);
                    timeElapsedSinceStartedDancing = 0f; // not needed but probably good to do anyways
                    playerIsDancing = false;
                }
            }
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
