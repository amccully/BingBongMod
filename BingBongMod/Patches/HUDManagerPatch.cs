using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void AddInventorySlotsUI()
        {
            // testing based on code I found (check if server), seemingly results in slots only loading for the host
            //if (!NetworkManager.Singleton.IsServer)
            //{
            //    BingBongModBase.MLS.LogWarning("IsServer check in HUDManager Awake was true?");
            //    return;
            //}

            if (!HUDManager.Instance || HUDManager.Instance.itemSlotIconFrames.Length != 4 || HUDManager.Instance.itemSlotIcons.Length != 4)
            {
                BingBongModBase.MLS.LogWarning("Inventory UI has been tampered with already! No modification applied.");
                return;
            }

            var numSlotsToAdd = BingBongModBase.numSlotsToAdd;
            GameObject ItemSlotObj = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/Inventory/Slot3");
            var itemSlotIconFramesAsList = HUDManager.Instance.itemSlotIconFrames.ToList();
            var itemSlotIconsAsList = HUDManager.Instance.itemSlotIcons.ToList();

            BingBongModBase.MLS.LogInfo("Setting HUDManager instance now!");
            for (int i = 0; i < numSlotsToAdd; i++)
            {
                GameObject newSlot = UnityEngine.Object.Instantiate(ItemSlotObj);
                newSlot.transform.SetParent(ItemSlotObj.transform.parent);
                newSlot.name = "Slot" + (i+4);
                Vector3 baseLocalPosition = ItemSlotObj.transform.localPosition;
                newSlot.transform.SetLocalPositionAndRotation( new Vector3(baseLocalPosition.x + 50, 
                                                                           baseLocalPosition.y, 
                                                                           baseLocalPosition.z), 
                                                               ItemSlotObj.transform.rotation );

                BingBongModBase.MLS.LogInfo("Icons before modification: " + itemSlotIconsAsList.Count);
                BingBongModBase.MLS.LogInfo("Frames before modification: " + itemSlotIconFramesAsList.Count);
                itemSlotIconsAsList.Add(newSlot.transform.GetChild(0).GetComponent<Image>());
                itemSlotIconFramesAsList.Add(newSlot.GetComponent<Image>());
                BingBongModBase.MLS.LogInfo("Icons after modification: " + itemSlotIconsAsList.Count);
                BingBongModBase.MLS.LogInfo("Frames after modification: " + itemSlotIconFramesAsList.Count);

                ItemSlotObj = newSlot;
            }

            HUDManager.Instance.itemSlotIconFrames = itemSlotIconFramesAsList.ToArray();
            HUDManager.Instance.itemSlotIcons = itemSlotIconsAsList.ToArray();
        }
    }
}
