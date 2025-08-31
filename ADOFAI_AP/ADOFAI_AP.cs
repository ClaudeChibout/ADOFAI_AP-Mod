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
        public const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ADOFAI_AP Instance;

        internal ManualLogSource mls;

        internal List<scrFloor> floors = new List<scrFloor>();

        internal MENU_AP Menu;

        internal ConfigEntry<string> pseudo;
        internal ConfigEntry<string> serverIP;
        internal ConfigEntry<string> serverPort;

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

            // Log

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"Plugin {modName} is starting...");

            // Patch

            harmony.PatchAll(typeof(ADOFAI_AP));
            mls.LogInfo("ADOFAI_AP loaded!");
            harmony.PatchAll(typeof(ScrControllerPatch));
            mls.LogInfo("ScrControllerPatch loaded!");
            harmony.PatchAll(typeof(PlanetarySystemPatch));
            mls.LogInfo("PlanetarySystemPatch loaded!");
            harmony.PatchAll(typeof(PauseLevelPatch));
            mls.LogInfo("PauseMenuPatch loaded!");
            mls.LogInfo($"Plugin {modName} is loaded!");

            // Menu

            var menuObject = new GameObject("ADOFAI_AP_Menu");
            UnityEngine.Object.DontDestroyOnLoad(menuObject);
            menuObject.hideFlags = HideFlags.HideAndDontSave;
            menuObject.AddComponent<MENU_AP>();
            Menu = menuObject.GetComponent<MENU_AP>();

            // Notif

            var notifObject = new GameObject("ADOFAI_AP_Notification");
            UnityEngine.Object.DontDestroyOnLoad(notifObject);
            notifObject.hideFlags = HideFlags.HideAndDontSave;
            notifObject.AddComponent<Notification>();

            // Client

            client = new CLIENT_AP();

        }

        void Update()
        {
            /*if ( Input.GetKeyDown(KeyCode.Keypad2) )
            {
                scrController.instance.paused = !scrController.instance.paused;
                scrController.instance.audioPaused = scrController.instance.paused;
                Time.timeScale = (scrController.instance.paused ? 0f : 1f);
            }*/

            /*if ( Input.GetKeyDown(KeyCode.Keypad3) )
            {
                speedEnabled = !speedEnabled;
            }

            if (speedEnabled )
            {
                scrController.instance.speed = speed;
            }*/

            /*if ( Input.GetKeyDown(KeyCode.Keypad1) )
            {
                var savedProgress = Persistence.GetSavedProgress();
                savedProgress["level"] = "4-X";
                Persistence.SetSavedProgress(savedProgress);
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                var savedProgress = Persistence.GetSavedProgress();
                savedProgress["level"] = "3-X";
                Persistence.SetSavedProgress(savedProgress);
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                mls.LogInfo($"------------------------------");
                mls.LogInfo($"| vvvv SavedProgress vvvv |");
                mls.LogInfo($"------------------------------");
                foreach (KeyValuePair<string, object> pair in Persistence.GetSavedProgress())
                {
                    mls.LogInfo($"{pair.Key}: {pair.Value}");
                }
                mls.LogInfo($"------------------------------");
            }*/

        }

        internal void ReceiveItem(string itemName)
        {
            Data_AP.ItemsReceived[itemName] = true;
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
            scrController.instance.paused = paused;
            //scrController.instance.audioPaused = scrController.instance.paused;
            Time.timeScale = (scrController.instance.paused ? 0f : 1f);
        }

    }
}
