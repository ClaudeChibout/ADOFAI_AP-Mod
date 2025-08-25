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
            if (ADOFAI_AP.Instance.client.session == null)
            {
                return true;
            }
                ADOFAI_AP.Instance.mls.LogInfo($"PortalTravelAction called: {___portalDestination}\nargs: {___portalArguments}");
            if (___portalDestination == Portal.EndOfLevel)
            {   
                ADOFAI_AP.Instance.mls.LogInfo($"LevelComplete: {scrController.currentLevel} !");
                ADOFAI_AP.Instance.CollectLocation(scrController.currentLevel);
                scrController.instance.QuitToMainMenu();

                return false;
            }
            else if (___portalDestination == Portal.GoToWorldBossIfReached)
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
                global::scnGame.RestartScene();
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

        [HarmonyPatch("EnterLevel")]
        [HarmonyPrefix]
        static bool EnterLevel(string worldAndLevel)
        {
            if (ADOFAI_AP.Instance.client.session == null)
            {
                return true;
            }

            if (ADOFAI_AP.Instance.Menu.fromDebugMenu) 
            {
                ADOFAI_AP.Instance.Menu.fromDebugMenu = false;
                return true; 
            }

            ADOFAI_AP.Instance.mls.LogInfo($"LoadLevel called with path: {worldAndLevel}");
            if (Data_AP.ItemsReceived.ContainsKey($"Key_Level_{worldAndLevel}") && !Data_AP.ItemsReceived[$"Key_Level_{worldAndLevel}"])
            {
                Notification.Instance.CreateNotification($"You don't have the Key_Level_{worldAndLevel}");
                scrController.instance.QuitToMainMenu();
                return false;
            }
            
            return true;
        }

    }
}
