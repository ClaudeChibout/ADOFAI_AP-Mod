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
        internal bool isMenuOpen = false;
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

        internal string pseudo = "Shotal";
        internal string serverIP = "localhost";
        internal string serverPort = "38281";

        internal string lastItem = "None";
        internal bool showCheckedLocation = false;

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
                if (currentMenu == MenuState.None && isConnected)
                    currentMenu = MenuState.Main;
                else if (currentMenu == MenuState.Main)
                    currentMenu = MenuState.None;
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
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawSelectionMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.BeginArea(new Rect(200, 50, 400, 400));
            GUILayout.Label("Select a level");
            if (GUILayout.Button("<"))
            {
                currentMenu = MenuState.Main; // Switch back to main menu
            }

            if (GUILayout.Toggle(showCheckedLocation, "showCheckedLevel")) { 
                showCheckedLocation = true;
            } else {
                showCheckedLocation = false;
            }

            GUILayout.BeginHorizontal();
                var cpt = 0;
                foreach (var level in Data_AP.ItemsReceived.Keys)
                {   
                    
                    
                    if (level.StartsWith("Key_Level_") && Data_AP.ItemsReceived[level] && (!Data_AP.LocationsChecked[level.Substring(10)] || showCheckedLocation) )
                    {
                        var levelName = level.Substring(10); // Extract the level name
                        cpt++;
                        if (cpt % 11 == 0)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                        }

                        if (Data_AP.LocationsChecked[levelName]) { 
                            GUI.backgroundColor = UnityEngine.Color.gray;
                        } else {
                            GUI.backgroundColor = UnityEngine.Color.green;
                        }

                    if (GUILayout.Button(levelName))
                        {
                            ADOFAI_AP.Instance.mls.LogInfo($"Selected level: {levelName}");
                            currentMenu = MenuState.None; 
                            ADOBase.controller.EnterLevel(levelName, false);
                        }
                    }
                }

            GUILayout.EndHorizontal();
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.EndArea();
        }

        void DrawMainMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            GUILayout.BeginArea(new Rect(200, 50, 200, 200));

            GUILayout.Label("ADOFAI AP Menu");
            GUILayout.Label($"LastItem: {lastItem}");
            /*try
            {
                Vector3 pos = scrController.instance.chosenPlanet.storedPosition;
                scrFloor floor = scrController.instance.chosenPlanet.GetFloorAtPosition(new Vector2(pos.x, pos.y));
                GUILayout.Label($"current floor id: {floor.seqID} - {floor.gameObject.name}");

            }
            catch (NullReferenceException)
            {
                
            }*/
            
            /*var t = toggleFloor ? "disable" : "enable";
            if (GUILayout.Button($"{t} floors"))
            {
                foreach (scrFloor f in GameObject.FindObjectsOfType<scrFloor>())
                {
                    if (f.seqID == 14)
                    {
                        f.gameObject.SetActive(toggleFloor);
                    }
                }
                toggleFloor = !toggleFloor;
            }*/

            if (GUILayout.Button("Level's selection"))
            {
                currentMenu = MenuState.Selection; // Switch to connection menu
            }

            if (GUILayout.Button("Close Menu"))
            {
                currentMenu = MenuState.None;
            }
            GUILayout.EndArea();
        }
        

        public void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;
            //ADOFAI_AP.Instance.mls.LogInfo($"Menu is now {(isMenuOpen ? "open" : "closed")}.");
        }

    }
}
