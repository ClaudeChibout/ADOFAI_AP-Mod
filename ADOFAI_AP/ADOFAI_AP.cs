using ADOFAI_AP.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ADOFAI_AP
{

    [BepInPlugin(modGUID, modName, modVersion)]
    public class ADOFAI_AP : BaseUnityPlugin
    {
        public const string modGUID = "com.shotal.ADOFAI_AP";
        public const string modName = "ADOFAI_AP";
        public const string modVersion = "1.0.3.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ADOFAI_AP Instance;

        internal ManualLogSource mls;

        internal List<scrFloor> floors = new List<scrFloor>();

        internal MENU_AP Menu;

        internal ConfigEntry<string> pseudo;
        internal ConfigEntry<string> serverIP;
        internal ConfigEntry<string> serverPort;

        internal ConfigEntry<bool> SendLocationOnLandOnPortal;


        internal CLIENT_AP client = null;

        //internal bool speedEnabled = false;
        //internal double speed = 1.8; 



        void Awake()
        {
            if (Instance != null)
            {
                Logger.LogError("Plugin is already loaded!");
                return;
            }
            else
            {
                Instance = this;
                
            }


            // Config

            pseudo = Config.Bind("ConnectionForm", "pseudo", "Shotal", "Pseudo for ConnectionForm");
            serverIP = Config.Bind("ConnectionForm", "IP", "localhost", "IP for ConnectionForm");
            serverPort = Config.Bind("ConnectionForm", "Port", "38281", "Port for ConnectionForm");
            SendLocationOnLandOnPortal = Config.Bind("Settings", "SendLocationOnLandOnPortal", true, "Send location when landing on a portal");

            //DeathLinkMod_MaxHealth = Config.Bind("DeathLinkMod", "MaxHealth", 1, "Number of deaths before sending a DeathLink");
            //SessionDeathCount = Config.Bind("DeathLinkMod", "SessionDeathCount", 0, "Number of DeathLink sent in this session");


            // Log

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"Plugin {modName} is starting...");

            // Menu / Notif / Client are created FIRST so that a failing Harmony
            // patch (see SafePatch below) can never prevent the in-game UI from loading.

            try
            {
                var menuObject = new GameObject("ADOFAI_AP_Menu");
                UnityEngine.Object.DontDestroyOnLoad(menuObject);
                menuObject.hideFlags = HideFlags.HideAndDontSave;
                menuObject.AddComponent<MENU_AP>();
                Menu = menuObject.GetComponent<MENU_AP>();

                var notifObject = new GameObject("ADOFAI_AP_Notification");
                UnityEngine.Object.DontDestroyOnLoad(notifObject);
                notifObject.hideFlags = HideFlags.HideAndDontSave;
                notifObject.AddComponent<Notification>();

                client = new CLIENT_AP();

                mls.LogInfo("Menu, Notification and Client initialized.");
            }
            catch (Exception e)
            {
                mls.LogError($"FATAL: failed to initialize Menu/Notification/Client: {e}");
            }

            // Patches — each one is applied independently and guarded, so that a patch
            // targeting a method that does not exist in the current game version only
            // disables that single feature instead of aborting the whole Awake().

            SafePatch(typeof(ADOFAI_AP));
            SafePatch(typeof(ScrControllerPatch));
            SafePatch(typeof(PlanetarySystemPatch));
            SafePatch(typeof(PauseLevelPatch));

            mls.LogInfo($"Plugin {modName} is loaded! (Unity {Application.unityVersion})");
        }

        /// <summary>
        /// Applies all Harmony patches declared in <paramref name="patchType"/>, logging the
        /// outcome. A failure (e.g. a target method missing in the current game build) is logged
        /// and swallowed so it cannot break the rest of the mod.
        /// </summary>
        private void SafePatch(Type patchType)
        {
            try
            {
                harmony.PatchAll(patchType);
                mls.LogInfo($"[Patch] {patchType.Name} applied.");
            }
            catch (Exception e)
            {
                mls.LogError($"[Patch] FAILED to apply {patchType.Name}: {e.Message}");
                mls.LogError($"[Patch] '{patchType.Name}' is disabled; the mod keeps running without it. " +
                             "A target method probably no longer exists in this game version.");
            }
        }

        void Update()
        {
            /*if ( Input.GetKeyDown(KeyCode.Keypad2) )
            {
                Notification.Instance.CreateNotification($"nb of notif: {Notification.Instance.notificationCount} !");
            }*/

            /*if ( Input.GetKeyDown(KeyCode.Keypad3) )
            {
                speedEnabled = !speedEnabled;
            }

            if (speedEnabled )
            {
                scrController.instance.speed = speed;
            }*/


        }

        internal void ReceiveItem(string itemName)
        {
            Data_AP.ItemsReceived[itemName] = true;

            // A received "Key_Level_X" unlocks level "X". Make sure the matching location
            // exists in LocationsChecked so the level shows up in the selection menu, even
            // if that world group was not pre-loaded at connect (e.g. an item sent manually
            // from the server, or a world option that was not enabled).
            if (itemName.StartsWith("Key_Level_"))
            {
                string levelName = itemName.Substring("Key_Level_".Length);
                if (!Data_AP.LocationsChecked.ContainsKey(levelName))
                {
                    Data_AP.LocationsChecked[levelName] = false;
                    mls.LogInfo($"[Item] Registered location '{levelName}' from received item '{itemName}'.");
                }
            }

            Menu.lastItem = itemName;
        }

        internal void CollectLocation(string locationName)
        {   
            if (client == null || client.session == null)
            {
                mls.LogError("Client is not initialized or session is not connected.");
                return;
            }
            Data_AP.LocationsChecked[locationName] = true;
            client.ReportLocation(locationName);
        }

        public static void TogglePause(bool paused)
        {
            if (scrController.instance == null)
            {
                Instance?.mls.LogWarning("TogglePause skipped: scrController.instance is null (no active controller).");
                return;
            }
            scrController.instance.paused = paused;
            //scrController.instance.audioPaused = scrController.instance.paused;
            Time.timeScale = (scrController.instance.paused ? 0f : 1f);
        }

        public ConfigEntry<T> BindConfig<T>(string group, string name, T value, String description = null)
        {
            return Config.Bind(group, name, value, description);
        }

    }
}
