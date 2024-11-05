using LethalLib.Extras;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace BingBongMod.PotionBehavior
{
    internal class DrinkPotion : PhysicsProp
    {
        public bool potionIsActive = false;
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && !potionIsActive)
            {
                if (!playerHeldBy || !base.IsOwner)
                {
                    return;
                }
                BingBongModBase.MLS.LogInfo("Item used successfully");
                potionIsActive = true;

                NetworkObject potionNetObj = base.gameObject.GetComponent<NetworkObject>();
                if (potionNetObj) {
                    PotionNetwork.drankPotionClientMessage.SendServer(potionNetObj);
                }
            }
        }
    }
}
