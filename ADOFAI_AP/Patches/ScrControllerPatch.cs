using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOFAI_AP.Patches
{
    [HarmonyPatch(typeof(scrController))]
    internal class ScrControllerPatch
    {   
        [HarmonyPatch("PortalTravelAction")]
        [HarmonyPrefix]
        static bool PatchPortalTravelAction(ref Portal ___portalDestination, ref String ___portalArguments)
        {   
            ADOFAI_AP.Instance.mls.LogInfo($"PortalTravelAction called: {___portalDestination}\nargs: {___portalArguments}");
            if (___portalDestination == Portal.EndOfLevel)
            {   
                ADOFAI_AP.Instance.mls.LogInfo($"LevelComplete: {scrController.currentLevel} !");
                ADOFAI_AP.Instance.CollectLocation(scrController.currentLevel);
                scrController.instance.QuitToMainMenu();

                return false;
            }
            else if (___portalDestination != Portal.GoToWorldBossIfReached)
            {
                // To prevent travel from lobby portals
                scrController.instance.QuitToMainMenu();
                return false;
            }
            bool isUnlocked;
            try
            {   
                var portalKey = $"Key_Level_{___portalDestination}";
                isUnlocked = Data_AP.ItemsReceived[___portalArguments];
            }
            catch (KeyNotFoundException)
            {
                isUnlocked = true;
            }

            if (isUnlocked)
            {
                // We have acces to the portal destination, so we can proceed with the action.
                ADOFAI_AP.Instance.mls.LogInfo($"PortalTravelAction called: {___portalDestination}\nargs: {___portalArguments} (unlocked)");
                return true;
            }
            else
            {
                // We don't have access to the portal destination, so we restart the scene to not crash the game.
                ADOFAI_AP.Instance.mls.LogInfo($"PortalTravelAction called: {___portalDestination}\nargs: {___portalArguments} (not unlocked)");
                scnGame.RestartScene();
                return false;
            }

        }

        [HarmonyPatch("Restart")]
        [HarmonyPrefix]
        static bool PatchRestart()
        {
            ADOFAI_AP.Instance.mls.LogInfo($"Restart called: {scrController.currentLevel} !");
            // We restart the scene.
            return true;
        }

    }
}
