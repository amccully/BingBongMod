using BingBongMod.Patches;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BingBongMod.PotionBehavior
{
    internal class Effect
    {
        // reference to the player, FOR NOW NOT USING THIS
        // public static PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

        // whether to notify the player of the effect applied, MAKE READ ONLY LATER
        public static bool ShowHUDTips = false;
        public string Name { get; set; }
        public int Probability { get; set; }

        // measured in seconds
        public int MaxDuration { get; set; }
        public int MinDuration { get; set; }

        public void SetEffect()
        {
            TriggerBehavior();
            int duration = UnityEngine.Random.Range(MinDuration, MaxDuration + 1); // int within the range, including min and max
            GameNetworkManager.Instance.localPlayerController.StartCoroutine(DisableEffectAfterTime(duration));
        }
        private System.Collections.IEnumerator DisableEffectAfterTime(int time)
        {
            yield return new WaitForSeconds(time);
            DisableBehavior();
            PotionEffect.potionEffectActive = false; // after effect is disabled, we can allow another to be applied
        }
        public virtual void TriggerBehavior() 
        {
            BingBongModBase.MLS.LogInfo("trigger for " + Name);
        }

        public virtual void DisableBehavior() 
        {
            BingBongModBase.MLS.LogInfo("disable for " + Name);
        }
    }

    internal class AgilityEffect : Effect
    {
        public AgilityEffect()
        {
            Name = "Agility";
        }
        public override void TriggerBehavior()
        {
            base.TriggerBehavior();
            GameNetworkManager.Instance.localPlayerController.movementSpeed *= 2;
            GameNetworkManager.Instance.localPlayerController.jumpForce *= 2;
        }

        public override void DisableBehavior() 
        { 
            base.DisableBehavior();
            GameNetworkManager.Instance.localPlayerController.movementSpeed /= 2;
            GameNetworkManager.Instance.localPlayerController.jumpForce /= 2;
        }
    }

    internal class InvisibilityEffect : Effect
    {
        public InvisibilityEffect()
        {
            Name = "Invisibility";
        }
        public override void TriggerBehavior()
        {
            base.TriggerBehavior();
            PotionNetwork.invisibilityClientMessage.SendServer(((int)GameNetworkManager.Instance.localPlayerController.playerClientId, false));
        }

        public override void DisableBehavior()
        {
            base.DisableBehavior();
            PotionNetwork.invisibilityClientMessage.SendServer(((int)GameNetworkManager.Instance.localPlayerController.playerClientId, true));
        }
    }

    internal class PotionEffect
    {
        public static bool potionEffectActive = false; // one effect applied at a time, ensure no effect is present if using potion

        private static List<Effect> effects = new List<Effect>
        {
            new AgilityEffect { Probability = 0, MaxDuration = 20, MinDuration = 20 },
            new InvisibilityEffect { Probability = 3, MaxDuration = 20, MinDuration = 20 },
            //new Effect { Name = "HealthRegen", Probability = 3 },
        };

        //private static System.Random random = new System.Random();

        public static void triggerPotionEffect()
        {
            Effect chosenEffect = chooseEffect();
            if (chosenEffect != null)
            {
                BingBongModBase.MLS.LogInfo("Setting effect for " + chosenEffect.Name);
                chosenEffect.SetEffect();
            }
            else
            {
                BingBongModBase.MLS.LogError("No effect was chosen in triggerPotionEffect");
            }
        }

        public static Effect chooseEffect()
        {
            int totalWeight = 0;
            foreach (var effect in effects)
                totalWeight += effect.Probability;

            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentSum = 0;

            foreach (var effect in effects)
            {
                currentSum += effect.Probability;
                if (randomValue < currentSum)
                    return effect;
            }

            return null;
        }
    }
}
