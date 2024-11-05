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
        // reference to the player
        public static PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

        // whether to notify the player of the effect applied
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
            player.StartCoroutine(DisableEffectAfterTime(duration));
        }
        private System.Collections.IEnumerator DisableEffectAfterTime(int time)
        {
            yield return new WaitForSeconds(time);
            DisableBehavior();
            PotionEffect.potionEffectActive = false; // after effect is disabled, we can allow another to be applied
        }
        public virtual void TriggerBehavior() 
        {
            BingBongModBase.MLS.LogInfo("Effect triggered behavior");
        }

        public virtual void DisableBehavior() 
        {
            BingBongModBase.MLS.LogInfo("Effect disabled behavior");
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
            BingBongModBase.MLS.LogInfo("trigger for agility");
            /*
            player.jumpForce = 2.5f;
            player.movementSpeed = 0.25f;
            */
        }

        public override void DisableBehavior() 
        { 
            base.DisableBehavior();
            BingBongModBase.MLS.LogInfo("disable for agility");
            /*
            player.jumpForce = 5f;
            player.movementSpeed = 0.5f;
            */
        }
    }

    internal class PotionEffect
    {
        public static bool potionEffectActive = false; // one effect applied at a time, ensure no effect is present if using potion

        private static List<Effect> effects = new List<Effect>
        {
            new AgilityEffect { Probability = 3, MaxDuration = 20, MinDuration = 20 },
            //new Effect { Name = "HealthRegen", Probability = 3 },
            //new Effect { Name = "Invisibility", Probability = 3 }
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
