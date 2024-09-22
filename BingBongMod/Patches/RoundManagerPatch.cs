using HarmonyLib;

namespace BingBongMod.Patches
{
    [HarmonyPatch(typeof(RoundManager))]

    internal class RoundManagerPatch
    {
        [HarmonyPatch(nameof(RoundManager.LoadNewLevel))]
        [HarmonyPostfix]

        static void NoEnemySpawningPatch(ref SelectableLevel ___currentLevel) // triple underscore?
        {
            if (___currentLevel)
            {
                ___currentLevel.maxEnemyPowerCount = 0;
                ___currentLevel.maxOutsideEnemyPowerCount = 0;
                ___currentLevel.maxDaytimeEnemyPowerCount = 0;
            }
        }
    }
}
