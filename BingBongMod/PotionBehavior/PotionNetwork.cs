using GameNetcodeStuff;
using LethalLib.Extras;
using LethalNetworkAPI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace BingBongMod.PotionBehavior
{
    internal class PotionNetwork : NetworkBehaviour
    {
        public static LethalServerMessage<NetworkObject> drankPotionServerMessage = new LethalServerMessage<NetworkObject>(identifier: "drankPotionId");
        public static LethalClientMessage<NetworkObject> drankPotionClientMessage = new LethalClientMessage<NetworkObject>(identifier: "drankPotionId");

        public static void Init()
        {
            BingBongModBase.MLS.LogInfo("POTION NETWORK INIT CALLED, SUBSCRIBING TO EVENTS");
            drankPotionClientMessage.OnReceived += ReceiveFromServerDrankPotion;
            drankPotionServerMessage.OnReceived += ReceiveFromClientDrankPotion;
        }

        // client subscription for DRANK POTION
        public static void ReceiveFromServerDrankPotion(NetworkObject data)
        {
            BingBongModBase.MLS.LogInfo("Received request from server for drank potion");

            GrabbableObject potion = data.GetComponent<GrabbableObject>();

            // play potion drink sound
            AudioSource component = potion.gameObject.GetComponent<AudioSource>();
            component.PlayOneShot(BingBongModBase.drinkPotionSound);
            WalkieTalkie.TransmitOneShotAudio(component, BingBongModBase.drinkPotionSound);
            //

            // make a new instance of the itemProperties and change whats needed for the empty potion version
            // using lethallib clone extension
            /*
            Item emptyPotionProp = potion.itemProperties.Clone<Item>();
            emptyPotionProp.itemName = "Empty Potion Bottle";
            emptyPotionProp.weight = 1.07f;
            emptyPotionProp.toolTips = null;
            potion.itemProperties = emptyPotionProp;
            potion.SetScrapValue(20);
            */
            //
            potion.itemProperties = BingBongModBase.emptyPotionBottle;

            // for player holding the item only:
            if (data.IsOwner)
            {
                BingBongModBase.MLS.LogDebug("Setting Control Tips for client that drank potion");
                HUDManager.Instance.ClearControlTips();
                potion.SetControlTipsForItem();
                BingBongModBase.MLS.LogDebug("Setting carry weight for client that drank potion");
                // subtract the weight difference between unused and used potion
                float playerWeight = GameNetworkManager.Instance.localPlayerController.carryWeight;
                GameNetworkManager.Instance.localPlayerController.carryWeight = Mathf.Clamp(playerWeight - (1.09f - 1.07f), 1f, 10f);
            }
            //

            // scannode properties:
            potion.GetComponentInChildren<ScanNodeProperties>().headerText = "Empty Potion Bottle";
            potion.SetScrapValue(20);
            //

            // change potion model to be empty
            Transform potionTransform = potion.transform; // this is 'sovereign_potion'
            Transform child = potionTransform.Find("GPVFX_POTION D");

            if (child != null)
            {
                // Access and destroy 'GPVFX_Bottle_D_Fill'
                Transform fill = child.Find("GPVFX_Bottle_D_Fill");
                if (fill != null)
                {
                    Destroy(fill.gameObject); // Destroys the entire 'GPVFX_Bottle_D_Fill' GameObject
                }

                // Access and destroy 'GPVFX_CORK'
                Transform cork = child.Find("GPVFX_CORK");
                if (cork != null)
                {
                    Destroy(cork.gameObject); // Destroys the entire 'GPVFX_CORK' GameObject
                }
            }
            else
            {
                BingBongModBase.MLS.LogError("Child 'GPVFX_POTION D' not found under base transform");
            }
            //
        }

        // server subscription for DRANK POTION
        public static void ReceiveFromClientDrankPotion(NetworkObject data, ulong clientId)
        {
            BingBongModBase.MLS.LogInfo("Received request from client for drank potion. CURRENTLY RUNNING ON SERVER");
            drankPotionServerMessage.SendAllClients(data);
        }
    }
}
