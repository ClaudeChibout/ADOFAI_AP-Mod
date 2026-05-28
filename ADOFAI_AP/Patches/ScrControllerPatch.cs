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

        static void endLevel(){
            // Collect the location of the player and send it to the server.
            // Pure perfect notification is just a fun addition, it doesn't affect the location sending in any way.
            // We log the level complete event and the current level for debugging purposes.
            ADOFAI_AP.Instance.mls.LogInfo($"LevelComplete: {scrController.currentLevel} !");
            if (PlanetarySystem.controller.mistakesManager.IsAllPurePerfect()) Notification.Instance.CreateNotification("PurePerfect !!!!!!!!!");
            ADOFAI_AP.Instance.CollectLocation(scrController.currentLevel);
        }

        [HarmonyPatch("OnLandOnPortal")]
        [HarmonyPrefix]
	    static bool PatchOnLandOnPortal(Portal portalDestination, string portalArguments)
        {
            if (ADOFAI_AP.Instance.client.session == null)
            {
                return true;
            }
            if (portalDestination == Portal.EndOfLevel && ADOFAI_AP.Instance.SendLocationOnLandOnPortal.Value)
            {

                if (GCS.practiceMode)
                {
                    Notification.Instance.CreateNotification("Practice mode detected, not sending location.");
                    return true;
                }
                ADOFAI_AP.Instance.mls.LogInfo($"OnLandOnPortal called: {portalDestination}\nargs: {portalArguments}");
                ADOFAI_AP.Instance.mls.LogInfo("SendLocationOnLandOnPortal is enabled, so we send the location on landing");
                endLevel();
            }
            return true;
        }


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

                if (GCS.practiceMode)
                {
                    return true;
                }

                if (!ADOFAI_AP.Instance.SendLocationOnLandOnPortal.Value)
                {
                    ADOFAI_AP.Instance.mls.LogInfo("SendLocationOnLandOnPortal is disabled, so we send the location on travel");
                    endLevel();
                }

                ADOFAI_AP.Instance.Menu.currentMenu = MENU_AP.MenuState.Selection;
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

        [HarmonyPatch("QuitToMainMenu")]
        [HarmonyPrefix]
        static bool PatchQuitToMainMenu()
        {
            if (ADOFAI_AP.Instance.client.session == null)
            {
                return true;
            }
            GCS.FOOL_JOKER = false;
            return true;
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
