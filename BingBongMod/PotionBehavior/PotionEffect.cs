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

    internal class HealthRegenEffect : Effect
    {
        public int HealthIncreaseAmount { get; set; }
        private Coroutine healthCoroutine = null;
        public HealthRegenEffect()
        {
            Name = "HealthRegen";
        }
        public override void TriggerBehavior()
        {
            base.TriggerBehavior();
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            healthCoroutine = player.StartCoroutine(HealthCoroutine(player));
        }

        public override void DisableBehavior()
        {
            base.DisableBehavior();
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            player.StopCoroutine(healthCoroutine);
            healthCoroutine = null;
        }

        private System.Collections.IEnumerator HealthCoroutine(PlayerControllerB player)
        {
            while (true)
            {
                if (player.health < 100)
                {
                    BingBongModBase.MLS.LogInfo("Updating player health from " + player.health);
                    player.health = Mathf.Clamp(player.health + HealthIncreaseAmount, 0, 100);
                    HUDManager.Instance.UpdateHealthUI(health: player.health, hurtPlayer: false);
                    BingBongModBase.MLS.LogInfo("Player health is now " + player.health);
                }
                yield return new WaitForSeconds(1f); // run update every second
            }
        }
    }

    // NOTE: should be good for now, but you may want to have an ondestroy method which
    // calls the disable behavior (for things like invisible players) but you could also
    // handle it in the network class (only need case I see this for is if players have
    // a mod that lets them join after the game has started)
    internal class PotionEffect : MonoBehaviour
    {
        private Coroutine currentEffectCoroutine = null;
        private bool effectWasTriggered = false;
        public Effect currentEffect = null;

        private static List<Effect> effects = new List<Effect>
        {
            new AgilityEffect { Probability = 0, MaxDuration = 20, MinDuration = 20 },
            new InvisibilityEffect { Probability = 0, MaxDuration = 45, MinDuration = 45 },
            new HealthRegenEffect { Probability = 3, MaxDuration = 45, MinDuration = 45, HealthIncreaseAmount = 1 },
        };

        public void ChoosePotionEffect()
        {
            currentEffect = RandomizeEffect();
            if (currentEffect != null)
            {
                BingBongModBase.MLS.LogInfo("Setting effect for " + currentEffect.Name);
                SetEffect();
            }
            else
            {
                BingBongModBase.MLS.LogError("No effect was chosen in triggerPotionEffect");
            }
        }

        private Effect RandomizeEffect()
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

        private void SetEffect()
        {
            int duration = UnityEngine.Random.Range(currentEffect.MinDuration, currentEffect.MaxDuration + 1); // int within the range, including min and max
            currentEffectCoroutine = StartCoroutine(EffectCoroutine(duration));
        }

        public void CancelCoroutineIfRunning()
        {
            if(currentEffectCoroutine != null)
            {
                if(effectWasTriggered)
                {
                    currentEffect.DisableBehavior();
                    effectWasTriggered = false;
                    currentEffect = null;
                }
                StopCoroutine(currentEffectCoroutine);
                currentEffectCoroutine = null;
            }
        }

        private System.Collections.IEnumerator EffectCoroutine(int time)
        {
            yield return new WaitForSeconds(4f); // wait for 4 seconds before enabling effect
            effectWasTriggered = true;
            currentEffect.TriggerBehavior();
            yield return new WaitForSeconds(time);
            currentEffect.DisableBehavior();
            effectWasTriggered = false;
            currentEffect = null;
            currentEffectCoroutine = null; // after effect is disabled, we can allow another to be applied
        }
    }
}
