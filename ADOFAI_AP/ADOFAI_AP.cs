using ADOFAI_AP.Patches;
using BepInEx;
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

        internal CLIENT_AP client = null;



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


            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"Plugin {modName} is starting...");

            
            harmony.PatchAll(typeof(ADOFAI_AP));
            harmony.PatchAll(typeof(ScrControllerPatch));
            harmony.PatchAll(typeof(PlanetarySystemPatch));
            mls.LogInfo($"Plugin {modName} is loaded!");

            var menuObject = new GameObject("ADOFAI_AP_Menu");
            UnityEngine.Object.DontDestroyOnLoad(menuObject);
            menuObject.hideFlags = HideFlags.HideAndDontSave;
            menuObject.AddComponent<MENU_AP>();
            Menu = menuObject.GetComponent<MENU_AP>();

            client = new CLIENT_AP();

        }

        void Update()
        {

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
            client.ReportLocation(locationName);
            Data_AP.LocationsChecked[locationName] = true;
        }

    }
}
