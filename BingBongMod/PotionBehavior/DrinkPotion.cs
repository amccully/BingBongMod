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
        public bool potionWasUsed = false; // item is one-time-use, once used, this object can't be used again
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && !potionWasUsed && !PotionEffect.potionEffectActive)
            {
                if (!playerHeldBy || !base.IsOwner)
                {
                    return;
                }
                BingBongModBase.MLS.LogInfo("Item used successfully");
                potionWasUsed = true;
                PotionEffect.potionEffectActive = true;

                NetworkObject potionNetObj = base.gameObject.GetComponent<NetworkObject>();
                if (potionNetObj) {
                    PotionNetwork.drankPotionClientMessage.SendServer(potionNetObj);
                }

                PotionEffect.triggerPotionEffect();
            }
        }
    }
}
