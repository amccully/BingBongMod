using GameNetcodeStuff;
using LethalLib.Extras;
using LethalNetworkAPI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;

namespace BingBongMod.PotionBehavior
{
    internal class PotionNetwork : NetworkBehaviour
    {
        //public static LethalNetworkVariable<HashSet<int>> invisiblePlayers = new LethalNetworkVariable<HashSet<int>>(identifier: "invisiblePlayers");
        public static HashSet<int> invisiblePlayers = new HashSet<int>();

        public static LethalServerMessage<NetworkObject> drankPotionServerMessage = new LethalServerMessage<NetworkObject>(identifier: "drankPotionId");
        public static LethalClientMessage<NetworkObject> drankPotionClientMessage = new LethalClientMessage<NetworkObject>(identifier: "drankPotionId");

        public static LethalServerMessage<(int, bool)> invisibilityServerMessage = new LethalServerMessage<(int, bool)>(identifier: "invisibilityId");
        public static LethalClientMessage<(int, bool)> invisibilityClientMessage = new LethalClientMessage<(int, bool)>(identifier: "invisibilityId");

        public static void Init()
        {
            BingBongModBase.MLS.LogInfo("POTION NETWORK INIT CALLED, SUBSCRIBING TO EVENTS");
            drankPotionClientMessage.OnReceived += ReceiveFromServerDrankPotion;
            drankPotionServerMessage.OnReceived += ReceiveFromClientDrankPotion;
            invisibilityClientMessage.OnReceived += ReceiveFromServerInvisibility;
            invisibilityServerMessage.OnReceived += ReceiveFromClientInvisibility;
        }

        // client subscription for INVISIBILITY
        public static void ReceiveFromServerInvisibility((int playerId, bool enable) data)
        {
            BingBongModBase.MLS.LogInfo("Received request from server for " + invisibilityClientMessage.ToString());
            
            // player script
            PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[data.playerId];
            // player object
            GameObject playerObj = playerScript.gameObject;

            // player object, what to set mesh renderer to, whether we want to disable the arms of the local player
            playerScript.DisablePlayerModel(playerObj, enable: data.enable, disableLocalArms: data.enable);

            // for the client who needs their local arms enabled (the player under the effect)
            // this needs to be done over the network to ensure it is executed after the DisablePlayerModel method
            if(data.enable && playerScript.IsOwner)
            {
                playerScript.thisPlayerModelArms.enabled = true;
            }

            // the following section is for updating visbility on things like cosmetics
            Transform scavengerModel = playerObj.transform.Find("ScavengerModel");
            Transform metarig = scavengerModel?.Find("metarig");
            Transform spine = metarig?.Find("spine");

            if (spine != null)
            {
                // Get all Renderer components in children
                Renderer[] renderers = spine.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = data.enable; // Change visibility
                }
                BingBongModBase.MLS.LogInfo($"Player body visuals have been {(data.enable ? "enabled" : "disabled")}.");
            }
            else
            {
                BingBongModBase.MLS.LogError("Spine object not found in the hierarchy.");
            }
        }

        // server subscription for INVISIBILITY
        public static void ReceiveFromClientInvisibility((int playerId, bool enable) data, ulong clientId)
        {
            BingBongModBase.MLS.LogInfo("Received request from client for " + invisibilityServerMessage.ToString() + ". CURRENTLY RUNNING ON SERVER"); // testing toString
            // updating who is invisible:
            if (!data.enable)
            {
                invisiblePlayers.Add(data.playerId);
            }
            else
            {
                invisiblePlayers.Remove(data.playerId);
            }
            foreach (int num in invisiblePlayers) {
                BingBongModBase.MLS.LogInfo("PLAYER " + num + " is invis!");
            }
            invisibilityServerMessage.SendAllClients(data);
        }

        //-------------------------------------------------

        // client subscription for DRANK POTION
        public static void ReceiveFromServerDrankPotion(NetworkObject data)
        {
            BingBongModBase.MLS.LogInfo("Received request from server for drank potion");

            GrabbableObject potion = data.GetComponent<GrabbableObject>();
            if (!potion)
            {
                BingBongModBase.MLS.LogError("Expected GrabbableObject component not found for potion");
                return;
            }

            DrinkPotion drinkComp = data.GetComponent<DrinkPotion>();
            if (!drinkComp)
            {
                BingBongModBase.MLS.LogError("Expected DrinkPotion component not found for potion");
                return;
            }
            drinkComp.potionWasUsed = true;

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
