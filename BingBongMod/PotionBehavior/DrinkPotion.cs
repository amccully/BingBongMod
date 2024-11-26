using GameNetcodeStuff;
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
            PotionEffect potionEffect = playerHeldBy.GetComponent<PotionEffect>();
            if (potionEffect == null) {
                BingBongModBase.MLS.LogError("Expected player holding potion to have PotionEffect component, but they didn't!");
                return;
            }
            if (buttonDown && !potionWasUsed && potionEffect.currentEffectCoroutine == null)
            {
                if (!playerHeldBy || !base.IsOwner)
                {
                    BingBongModBase.MLS.LogWarning("playerHeldBy: " + playerHeldBy + " and base.IsOwner: " + base.IsOwner + " respectively.");
                    return;
                }
                BingBongModBase.MLS.LogInfo("Item used successfully");

                NetworkObject potionNetObj = base.gameObject.GetComponent<NetworkObject>();
                if (potionNetObj) {
                    PotionNetwork.drankPotionClientMessage.SendServer(potionNetObj);
                }

                potionEffect.ChoosePotionEffect();
            }
            else
            {
                BingBongModBase.MLS.LogWarning("potionWasUsed: " + potionWasUsed + " and currentEffectCo: " + potionEffect.currentEffectCoroutine.ToString() + " respectively.");
            }
        }
    }
}
