using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        // (inside of HarmonyPatch:) nameof(PlayerControllerB.), after . search for name of method
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void InfiniteSprintPatch(ref float ___sprintMeter) // triple underscore?
        {
            ___sprintMeter = 1f; // max value for meter, so every frame it'll be set to max
        }

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void AddInventorySlots(PlayerControllerB __instance)
        {
            if(!__instance || __instance.ItemSlots.Length != 4)
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
