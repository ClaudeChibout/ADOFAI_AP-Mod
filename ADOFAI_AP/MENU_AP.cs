using ADOFAI_AP.Patches;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ADOFAI_AP
{
    internal class MENU_AP : MonoBehaviour
    {

        public static MENU_AP Instance = null;
        internal bool isConnected = false; // This should be set to true when the connection is established
        //private bool toggleFloor = false;

        public enum MenuState
        {
            None,           // Aucun menu
            Connection,     // Menu de connexion
            Main,           // Menu principal
            Selection,     // Menu de sélection de niveau
        }

        internal MenuState currentMenu = MenuState.None;

        internal string pseudo = ADOFAI_AP.Instance?.pseudo.Value;
        internal string serverIP = ADOFAI_AP.Instance?.serverIP.Value;
        internal string serverPort = ADOFAI_AP.Instance?.serverPort.Value;

        internal string lastItem = "None";
        internal bool showCheckedLocation = false;

        internal int nbLocationsCompleted = 0;
        internal int nbNonGoalLocations = 0;

        // debug var
        internal long idLoc = 0;
        internal string idName = "";
        internal string lvlName = string.Empty;
        internal bool fromDebugMenu = false;
        internal int recupCheckpoint = 0;

        void Awake()
        {   
            if (Instance == null)
            {   
                Instance = this;
                ADOFAI_AP.Instance.mls.LogError("MENU_AP is initialized!");
            }

            StartCoroutine(DelayedMenuOpen());
        }

        IEnumerator DelayedMenuOpen()
        {
            yield return new WaitForSeconds(2f);
            ADOFAI_AP.Instance.mls.LogInfo("Opening the menu");
            currentMenu = MenuState.Connection; // Set the initial menu state to Connection
        }

        void Update()
        {
            // Check for a key press to toggle the menu
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (currentMenu == MenuState.None)
                {
                    currentMenu = isConnected ? MenuState.Main : MenuState.Connection;
                    ADOFAI_AP.TogglePause(true);
                }
                else {
                    currentMenu = MenuState.None;
                    ADOFAI_AP.TogglePause(false);
                }
            }
        }

        void OnGUI()
        {
            switch(currentMenu)
            {
                case MenuState.Connection:
                    DrawConnectionMenu();
                    break;
                case MenuState.Main:
                    DrawMainMenu();
                    break;
                case MenuState.Selection:
                    DrawSelectionMenu();
                    break;
                default:
                    break;
            }

            // DrawDebugMenu();

        }

        void DrawDebugMenu()
        {
            GUILayout.BeginArea(new Rect(20, 300, 200, 200));
            //GUILayout.Label($"speedEnabled: {ADOFAI_AP.Instance?.speedEnabled} ({ADOFAI_AP.Instance.speed}-{scrController.instance?.speed})");
            

            /*// AP WORLD IDs
            GUILayout.BeginHorizontal();
            idName = GUILayout.TextField(idName, GUILayout.Width(100));
            if (GUILayout.Button("idApWorld", GUILayout.Width(100)))
            {
                idLoc = (long)ADOFAI_AP.Instance.client?.session.Locations.GetLocationIdFromName(ADOFAI_AP.Instance.client?.session.ConnectionInfo.Game, idName);
            }
            GUILayout.Label($"{idLoc}");
            GUILayout.EndHorizontal();*/

            GUILayout.BeginHorizontal();
            lvlName = GUILayout.TextField(lvlName, GUILayout.Width(100));
            if (GUILayout.Button("EnterLevel", GUILayout.Width(100)))
            {
                fromDebugMenu = true;
                scrController.instance?.EnterLevel(lvlName);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label($"currentSpeedTrial: {GCS.currentSpeedTrial}");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"checkpointNum: {GCS.checkpointNum}");
            if (GUILayout.Button("<", GUILayout.Width(50)))
            {
                GCS.checkpointNum -= 1;
            }
            if (GUILayout.Button(">", GUILayout.Width(50)))
            {
                GCS.checkpointNum += 1;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label($"savedCheckpointNum: {GCS.savedCheckpointNum}");
            

            if (GUILayout.Button("checkWin", GUILayout.Width(100)))
            {
                ADOFAI_AP.Instance.client.CheckWin();
            }

            GUILayout.EndArea();
        }

        void DrawConnectionMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.BeginArea(new Rect(200, 50, 400, 200));
            GUILayout.Label("Connect to server");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Username:");
            pseudo = GUILayout.TextField(pseudo, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Server IP:");
            serverIP = GUILayout.TextField(serverIP, GUILayout.Width(100));
            GUILayout.Label("Port:");
            serverPort = GUILayout.TextField(serverPort, GUILayout.Width(50));
            if (GUILayout.Button("Connect"))
            {
                ADOFAI_AP.Instance.mls.LogInfo($"Connecting to {serverIP}:{serverPort}...");
                ADOFAI_AP.Instance.client.Connect(serverIP, int.Parse(serverPort), pseudo);
                ADOFAI_AP.Instance.mls.LogInfo("Connected to server.");

                ADOFAI_AP.Instance.pseudo.Value = pseudo;
                ADOFAI_AP.Instance.serverIP.Value = serverIP;
                ADOFAI_AP.Instance.serverPort.Value = serverPort;

            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawSelectionMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.BeginArea(new Rect(200, 50, 600, 600));
            GUILayout.Label("Select a level");
            if (GUILayout.Button("<"))
            {
                currentMenu = MenuState.Main; // Switch back to main menu
            }

            if (GUILayout.Toggle(showCheckedLocation, "showCheckedLevel"))
            {
                showCheckedLocation = true;
            }
            else
            {
                showCheckedLocation = false;
            }

            GUILayout.BeginHorizontal();
            var cpt = 0;
            foreach (var level in Data_AP.ItemsReceived.Keys)
            {
                // Skip levels that are not in the format "Key_Level_X-Y"
                //ADOFAI_AP.Instance.mls.LogInfo($"Checking level: {level}");

                if (level == "Filler Note")
                {
                    // Skip the filler note
                    //ADOFAI_AP.Instance.mls.LogInfo("Skipping Filler Note level.");
                    continue;
                }

                try
                {
                    if (Data_AP.ItemsReceived[level] && (!Data_AP.LocationsChecked[level.Substring(10)] || showCheckedLocation))
                    {
                        var levelName = level.Substring(10); // Extract the level name
                        cpt++;
                        if (cpt % 11 == 0)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                        }

                        if (Data_AP.LocationsChecked[levelName])
                        {
                            GUI.backgroundColor = UnityEngine.Color.gray;
                        }
                        else
                        {   
                            GUI.backgroundColor = Data_AP.goalLevels.Contains(levelName) ? UnityEngine.Color.yellow : UnityEngine.Color.green;
                        }

                        if (GUILayout.Button(levelName))
                        {   
                            ADOFAI_AP.Instance.mls.LogInfo($"Selected level: {levelName}");
                            currentMenu = MenuState.None;
                            ADOBase.controller.EnterLevel(levelName, false);
                        }
                    }
                }
                catch (Exception e)
                {
                    ADOFAI_AP.Instance.mls.LogError($"Error processing level {level}: {e.Message}");
                }

            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.EndArea();

        }
        void DrawMainMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.BeginArea(new Rect(200, 50, 200, 250));

            GUILayout.Label("ADOFAI AP Menu");
            GUILayout.Label($"LastItem: {lastItem}");
            GUILayout.Label($"{nbLocationsCompleted}/{nbNonGoalLocations} nongoal levels completed to reach your percentage goal");
            GUILayout.Label(ADOFAI_AP.Instance.client.IsAllGoalLevelsCompleted()?"You completed all your goal levels": "You not completed all your goal levels");
            if (GUILayout.Button("Level's selection"))
            {
                currentMenu = MenuState.Selection; // Switch to connection menu
            }

            if (GUILayout.Button("Disconnect"))
            {
                ADOFAI_AP.Instance.client.Disconnect(); // Disconnect the user to the AP server
                isConnected = false;
                currentMenu = MenuState.Connection;
            }

            if (GUILayout.Button("Close Menu"))
            {
                currentMenu = MenuState.None;
                ADOFAI_AP.TogglePause(false);
            }
            GUILayout.EndArea();
        }
        


    }
}
