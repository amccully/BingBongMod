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
        // whether to notify the player of the effect applied, MAKE READ ONLY LATER
        public static bool ShowHUDTips = false;
        public string Name { get; set; }
        public int Probability { get; set; }

        // measured in seconds
        public int MaxDuration { get; set; }
        public int MinDuration { get; set; }

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

    internal class PotionEffect : MonoBehaviour
    {
        public Coroutine currentEffectCoroutine = null;

        private static List<Effect> effects = new List<Effect>
        {
            new AgilityEffect { Probability = 0, MaxDuration = 20, MinDuration = 20 },
            new InvisibilityEffect { Probability = 3, MaxDuration = 45, MinDuration = 45 },
            //new Effect { Name = "HealthRegen", Probability = 3 },
        };

        public void ChoosePotionEffect()
        {
            Effect chosenEffect = ChooseEffect();
            if (chosenEffect != null)
            {
                BingBongModBase.MLS.LogInfo("Setting effect for " + chosenEffect.Name);
                SetEffect(chosenEffect);
            }
            else
            {
                BingBongModBase.MLS.LogError("No effect was chosen in triggerPotionEffect");
            }
        }

        public Effect ChooseEffect()
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

        public void SetEffect(Effect chosenEffect)
        {
            chosenEffect.TriggerBehavior();
            int duration = UnityEngine.Random.Range(chosenEffect.MinDuration, chosenEffect.MaxDuration + 1); // int within the range, including min and max
            currentEffectCoroutine = GameNetworkManager.Instance.localPlayerController.StartCoroutine(DisableEffectAfterTime(duration, chosenEffect));
        }

        private System.Collections.IEnumerator DisableEffectAfterTime(int time, Effect chosenEffect)
        {
            yield return new WaitForSeconds(time);
            chosenEffect.DisableBehavior();
            currentEffectCoroutine = null; // after effect is disabled, we can allow another to be applied
        }
    }
}
