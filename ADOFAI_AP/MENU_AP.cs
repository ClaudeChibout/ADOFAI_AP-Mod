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
            Options,         // Menu des options
        }

        internal MenuState currentMenu = MenuState.None;

        internal string pseudo = ADOFAI_AP.Instance?.pseudo.Value;
        internal string serverIP = ADOFAI_AP.Instance?.serverIP.Value;
        internal string serverPort = ADOFAI_AP.Instance?.serverPort.Value;

        internal string lastItem = "None";
        internal bool showCheckedLocation = false;
        internal bool hideGoalLocation = false;

        internal bool normalWorld = true;
        internal bool extraWorld = true;
        internal bool starWorld = true;
        internal bool crownWorld = true;
        internal bool aprilFoolsWorld = true;
        internal bool BWorld = true;
        internal bool neonWorlds = true;
        internal bool neonExtraWorlds = true;

        internal int nbLocationsCompleted = 0;
        internal int nbNonGoalLocations = 0;

        // debug var
        internal long idLoc = 0;
        internal string idName = "";
        internal string lvlName = string.Empty;
        internal bool fromDebugMenu = false;
        internal int recupCheckpoint = 0;


        Texture2D boxTexture;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ADOFAI_AP.Instance.mls.LogError("MENU_AP is initialized!");
            }

            if (boxTexture == null)
            {
                boxTexture = new Texture2D(1, 1);
                boxTexture.SetPixel(0, 0, new UnityEngine.Color(0.039f, 0.072f, 0.325f, 0.6f)); // Semi-transparent black
                boxTexture.Apply();
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

        GUIStyle boxStyle;
        GUIStyle labelStyle;
        void OnGUI()
        {   
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = boxTexture;
                boxStyle.normal.textColor = UnityEngine.Color.magenta;
            }
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = UnityEngine.Color.magenta;
            }
            switch (currentMenu)
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
                case MenuState.Options:
                    DrawOptionsMenu();
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


        void DrawOptionsMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.BeginArea(new Rect(200, 50, 400, 400));
            GUILayout.Label("Options", labelStyle);
            // Add your options here
            if (GUILayout.Button("Back"))
            {
                currentMenu = MenuState.Main; // Switch back to main menu
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("DeathLink Mod:" + (ADOFAI_AP.Instance.client.DeathLinkMod_Disable ? "Disabled" : "Enabled"), labelStyle);
            if (GUILayout.Button(ADOFAI_AP.Instance.client.DeathLinkMod_Disable ? "Enable" : "Disable"))
            {
                ADOFAI_AP.Instance.client.DeathLinkMod_Disable = !ADOFAI_AP.Instance.client.DeathLinkMod_Disable;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Threshold of deaths (MaxHealth) before sending a DeathLink:", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-"))
            {
                if ( ADOFAI_AP.Instance.client.DeathLinkMod_MaxHealth.Value > 1)
                {
                    ADOFAI_AP.Instance.client.DeathLinkMod_MaxHealth.Value--;
                    ADOFAI_AP.Instance.client.currentLife = ADOFAI_AP.Instance.client.DeathLinkMod_MaxHealth.Value; // Reset death count when changing threshold
                    Notification.Instance.CreateNotification($"MaxHealth set to {ADOFAI_AP.Instance.client.DeathLinkMod_MaxHealth.Value}\nLifes reset");
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("" + ADOFAI_AP.Instance.client.DeathLinkMod_MaxHealth.Value, labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("+"))
            {
                ADOFAI_AP.Instance.client.DeathLinkMod_MaxHealth.Value++;
                ADOFAI_AP.Instance.client.currentLife = ADOFAI_AP.Instance.client.DeathLinkMod_MaxHealth.Value; // Reset death count when changing threshold
                Notification.Instance.CreateNotification($"Threshold set to {ADOFAI_AP.Instance.client.DeathLinkMod_MaxHealth.Value}\nDeathCount reset");
            }
            GUILayout.EndHorizontal();
            GUILayout.Label($"DeathLink Count Before Death: {ADOFAI_AP.Instance.client.currentLife}", labelStyle);
            GUILayout.TextField("Version: " + ADOFAI_AP.modVersion);
            GUILayout.EndArea();
        }

        void DrawConnectionMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.BeginArea(new Rect(200, 50, 400, 200));
            GUILayout.Label("Connect to server", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Username:", labelStyle);
            pseudo = GUILayout.TextField(pseudo, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Server IP:", labelStyle);
            serverIP = GUILayout.TextField(serverIP, GUILayout.Width(100));
            GUILayout.Label("Port:", labelStyle);
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
            GUILayout.Label("Select a level", labelStyle);
            if (GUILayout.Button("<"))
            {
                currentMenu = MenuState.Main; // Switch back to main menu
            }
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            showCheckedLocation = GUILayout.Toggle(showCheckedLocation, "showCheckedLevel");
            hideGoalLocation = GUILayout.Toggle(hideGoalLocation, "hideGoalLevel");
            normalWorld = GUILayout.Toggle(normalWorld, "normalWorld");
            extraWorld = GUILayout.Toggle(extraWorld, "extraWorld");
            starWorld = GUILayout.Toggle(starWorld, "starWorld");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            crownWorld = GUILayout.Toggle(crownWorld, "crownWorld");
            aprilFoolsWorld = GUILayout.Toggle(aprilFoolsWorld, "aprilFoolsWorld");
            BWorld = GUILayout.Toggle(BWorld, "BWorld");
            neonWorlds = GUILayout.Toggle(neonWorlds, "neonWorlds");
            neonExtraWorlds = GUILayout.Toggle(neonExtraWorlds, "neonExtraWorlds");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            var cpt = 0;
            foreach (var level in Data_AP.ItemsReceived.Keys)
            {
                if (Data_AP.goalLevels.Contains(level.Substring(10)) && hideGoalLocation) continue;
                // || level.Substring(10).StartsWith("1-") à retirer lorsque le start avec des levels ramdom sera dispo
                if ((Data_AP.MainWorlds.Keys.Contains(level.Substring(10)) || Data_AP.MainWorldsTuto.Keys.Contains(level.Substring(10)) || level.Substring(10).StartsWith("1-")) && !normalWorld) continue;
                if ((Data_AP.XtraWorlds.Keys.Contains(level.Substring(10)) || Data_AP.XtraTuto.Keys.Contains(level.Substring(10))) && !extraWorld) continue;
                if ((Data_AP.StarWorlds.Keys.Contains(level.Substring(10)) || Data_AP.StarWorldsTuto.Keys.Contains(level.Substring(10))) && !starWorld) continue;
                if ((Data_AP.CrownWorlds.Keys.Contains(level.Substring(10)) || Data_AP.CrownWorldsTuto.Keys.Contains(level.Substring(10))) && !crownWorld) continue;
                if (Data_AP.AprilFoolsWorlds.Keys.Contains(level.Substring(10)) && !aprilFoolsWorld) continue;
                if ((Data_AP.BWorld.Keys.Contains(level.Substring(10)) || Data_AP.BWorldTuto.Keys.Contains(level.Substring(10))) && !BWorld) continue;
                if ((Data_AP.NeonCosmosWorlds.Keys.Contains(level.Substring(10)) || Data_AP.NeonCosmosWorldsTuto.Keys.Contains(level.Substring(10))) && !neonWorlds) continue;
                if ((Data_AP.NeonCosmosWorldsEX.Keys.Contains(level.Substring(10)) || Data_AP.NeonCosmosWorldsEXTuto.Keys.Contains(level.Substring(10))) && !neonExtraWorlds) continue;

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
                    //if (Data_AP.goalLevels.Contains(level.Substring(10)) && hideGoalLocation) continue; 
                    if (Data_AP.ItemsReceived[level] && (!Data_AP.LocationsChecked[level.Substring(10)] || showCheckedLocation)
                        )
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
            GUILayout.BeginArea(new Rect(200, 50, 200, 300));

            GUILayout.Label("ADOFAI AP Menu", labelStyle);
            GUILayout.Label($"LastItem: {lastItem}", labelStyle);
            GUILayout.Label($"{nbLocationsCompleted}/{nbNonGoalLocations} nongoal levels completed to reach your percentage goal", labelStyle);
            GUILayout.Label(ADOFAI_AP.Instance.client.IsAllGoalLevelsCompleted()?"You completed all your goal levels": "You not completed all your goal levels", labelStyle);
            GUILayout.Label($"Session Deaths: {ADOFAI_AP.Instance.client.SessionDeathCount.Value}", labelStyle);
            GUILayout.Label(ADOFAI_AP.Instance.client.DeathLinkMod_Disable ? "Deathlink disabled" :$"Deaths before sending a death: {ADOFAI_AP.Instance.client.currentLife}", labelStyle);
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
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close Menu"))
            {
                currentMenu = MenuState.None;
                ADOFAI_AP.TogglePause(false);
            }
            if (GUILayout.Button("Options"))
            {
                currentMenu = MenuState.Options;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }




    }
}
