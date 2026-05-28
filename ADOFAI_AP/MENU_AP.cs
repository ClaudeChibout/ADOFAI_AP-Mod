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
            Messages,
        }

        internal MenuState currentMenu = MenuState.None;

        internal string pseudo = ADOFAI_AP.Instance?.pseudo.Value;
        internal string serverIP = ADOFAI_AP.Instance?.serverIP.Value;
        internal string serverPort = ADOFAI_AP.Instance?.serverPort.Value;
        internal string serverPassword = "";

        internal bool sendLocationOnLandOnPortal = ADOFAI_AP.Instance?.SendLocationOnLandOnPortal.Value ?? true;

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
        internal bool arWorld = true;

        internal int nbLocationsCompleted = 0;
        internal int nbNonGoalLocations = 0;
        internal string messageInput = "";
        private Vector2 messagesScroll = Vector2.zero;
        private Rect connectionWindowRect = new Rect(200, 50, 400, 200);
        private Rect mainWindowRect = new Rect(200, 50, 200, 300);
        private Rect selectionWindowRect = new Rect(200, 50, 600, 600);
        private Rect optionsWindowRect = new Rect(200, 50, 400, 400);
        private Rect messagesWindowRect = new Rect(100, 50, 700, 520);
        private int lastMessageCount = 0;
        private float lastMessagesContentHeight = 0f;
        private bool pausedForMessageInput = false;
        private bool pauseStateBeforeMessageInput = false;
        private float timeScaleBeforeMessageInput = 1f;
        private Rect messageInputRect = Rect.zero;
        private bool resizingMessagesWindow = false;

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
                }
                else {
                    currentMenu = MenuState.None;
                    SetPausedForMessageInput(false);
                }
            }
        }

        GUIStyle boxStyle;
        GUIStyle labelStyle;
        GUIStyle messageLabelStyle;
        GUIStyle windowStyle;
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
            if (messageLabelStyle == null)
            {
                messageLabelStyle = new GUIStyle(GUI.skin.label);
                messageLabelStyle.normal.textColor = UnityEngine.Color.white;
                messageLabelStyle.richText = true;
                messageLabelStyle.wordWrap = true;
            }
            if (windowStyle == null)
            {
                windowStyle = new GUIStyle(GUIStyle.none);
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
                case MenuState.Messages:
                    DrawMessagesMenu();
                    break;
                default:
                    break;
            }
            if (currentMenu != MenuState.Messages)
            {
                SetPausedForMessageInput(false);
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
            optionsWindowRect = GUI.Window(9362, optionsWindowRect, DrawOptionsWindow, GUIContent.none, windowStyle);
        }

        void DrawOptionsWindow(int windowId)
        {
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

            GUILayout.Label("sendLocationOnLandOnPortal: " + (sendLocationOnLandOnPortal ? "Enabled" : "Disabled"), labelStyle);
            if (GUILayout.Button(sendLocationOnLandOnPortal ? "Disable" : "Enable"))
            {
                sendLocationOnLandOnPortal = !sendLocationOnLandOnPortal;
                ADOFAI_AP.Instance.SendLocationOnLandOnPortal.Value = sendLocationOnLandOnPortal;
            }

            GUILayout.Label($"DeathLink Count Before Death: {ADOFAI_AP.Instance.client.currentLife}", labelStyle);
            GUILayout.TextField("Version: " + ADOFAI_AP.modVersion);
            GUI.DragWindow(new Rect(0, 0, optionsWindowRect.width, optionsWindowRect.height));
        }

        void DrawConnectionMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            connectionWindowRect = GUI.Window(9363, connectionWindowRect, DrawConnectionWindow, GUIContent.none, windowStyle);
        }

        void DrawConnectionWindow(int windowId)
        {
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
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password:", labelStyle);
            serverPassword = GUILayout.PasswordField(serverPassword, '*', GUILayout.Width(100));
            if (GUILayout.Button("Connect"))
            {
                ADOFAI_AP.Instance.mls.LogInfo($"Connecting to {serverIP}:{serverPort}...");
                ADOFAI_AP.Instance.client.Connect(serverIP, int.Parse(serverPort), pseudo, serverPassword);
                ADOFAI_AP.Instance.mls.LogInfo("Connected to server.");

                ADOFAI_AP.Instance.pseudo.Value = pseudo;
                ADOFAI_AP.Instance.serverIP.Value = serverIP;
                ADOFAI_AP.Instance.serverPort.Value = serverPort;

            }
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, connectionWindowRect.width, connectionWindowRect.height));
        }

        void DrawSelectionMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            selectionWindowRect = GUI.Window(9364, selectionWindowRect, DrawSelectionWindow, GUIContent.none, windowStyle);
        }

        void DrawSelectionWindow(int windowId)
        {
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
            GUILayout.BeginVertical();
            arWorld = GUILayout.Toggle(arWorld, "arWorld");
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
                if ((Data_AP.ARWorld.Keys.Contains(level.Substring(10)) || Data_AP.ARWorldTuto.Keys.Contains(level.Substring(10))) && !arWorld) continue;

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
            GUI.DragWindow(new Rect(0, 0, selectionWindowRect.width, selectionWindowRect.height));

        }
        void DrawMainMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            mainWindowRect.height = Math.Max(mainWindowRect.height, 380f);
            mainWindowRect = GUI.Window(9365, mainWindowRect, DrawMainWindow, GUIContent.none, windowStyle);
        }

        void DrawMainWindow(int windowId)
        {
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
            if (GUILayout.Button("Messages"))
            {
                currentMenu = MenuState.Messages;
            }

            if (GUILayout.Button("Disconnect"))
            {
                _ = ADOFAI_AP.Instance.client.Disconnect(); // Disconnect the user to the AP server
                isConnected = false;
                currentMenu = MenuState.Connection;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close Menu"))
            {
                currentMenu = MenuState.None;
                SetPausedForMessageInput(false);
            }
            if (GUILayout.Button("Options"))
            {
                currentMenu = MenuState.Options;
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, mainWindowRect.width, mainWindowRect.height));
        }

        void DrawMessagesMenu()
        {
            GUI.backgroundColor = UnityEngine.Color.magenta;
            messagesWindowRect.width = Math.Max(messagesWindowRect.width, 420f);
            messagesWindowRect.height = Math.Max(messagesWindowRect.height, 260f);
            messagesWindowRect = GUI.Window(9361, messagesWindowRect, DrawMessagesWindow, GUIContent.none, windowStyle);
        }

        void DrawMessagesWindow(int windowId)
        {
            if (Event.current.type == EventType.MouseDown
                && GUI.GetNameOfFocusedControl() == "APMessageInput"
                && !messageInputRect.Contains(Event.current.mousePosition))
            {
                GUI.FocusControl(null);
                SetPausedForMessageInput(false);
            }

            var messages = ADOFAI_AP.Instance.client.GetMessagesSnapshot();
            float scrollHeight = Math.Max(120f, messagesWindowRect.height - 100f);
            float innerWidth = Math.Max(260f, messagesWindowRect.width - 20f);
            float scrollbarWidth = GetVerticalScrollbarWidth();
            float messageWidth = Math.Max(240f, innerWidth - scrollbarWidth);
            float previousMaxScroll = Math.Max(0f, lastMessagesContentHeight - scrollHeight);
            bool wasAtBottom = messagesScroll.y >= previousMaxScroll - 20f;
            bool hasNewMessages = messages.Count != lastMessageCount;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(40)))
            {
                SetPausedForMessageInput(false);
                currentMenu = MenuState.Main;
            }
            GUILayout.Label("Archipelago Messages", labelStyle);
            GUILayout.EndHorizontal();

            if (hasNewMessages && wasAtBottom)
            {
                messagesScroll.y = float.MaxValue;
            }

            float contentHeight = 0f;
            messagesScroll = GUILayout.BeginScrollView(messagesScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, boxStyle, GUILayout.Height(scrollHeight), GUILayout.Width(innerWidth));
            foreach (var message in messages)
            {
                string line = $"<color=#808080>[{message.Time}] [{message.Category}]</color> {message.RichText}";
                contentHeight += messageLabelStyle.CalcHeight(new GUIContent(line), messageWidth);
                GUILayout.Label(line, messageLabelStyle, GUILayout.Width(messageWidth));
            }
            GUILayout.EndScrollView();
            lastMessageCount = messages.Count;
            lastMessagesContentHeight = contentHeight;

            GUILayout.BeginHorizontal();
            bool sendMessage = Event.current.type == EventType.KeyDown
                && GUI.GetNameOfFocusedControl() == "APMessageInput"
                && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
            if (sendMessage)
            {
                Event.current.Use();
            }

            GUI.SetNextControlName("APMessageInput");
            messageInput = GUILayout.TextField(messageInput, GUILayout.Width(Math.Max(180f, innerWidth - 170f)));
            messageInputRect = GUILayoutUtility.GetLastRect();
            sendMessage = GUILayout.Button("Send", GUILayout.Width(80)) || sendMessage;
            Rect resizeHandleRect = GUILayoutUtility.GetRect(new GUIContent("Resize"), GUI.skin.button, GUILayout.Width(80));
            GUI.Box(resizeHandleRect, "Resize", GUI.skin.button);
            if (sendMessage)
            {
                ADOFAI_AP.Instance.client.HandleTextInput(messageInput);
                messageInput = "";
            }
            GUILayout.EndHorizontal();
            SetPausedForMessageInput(GUI.GetNameOfFocusedControl() == "APMessageInput");
            HandleMessagesResize(resizeHandleRect);
            GUI.DragWindow(new Rect(0, 0, messagesWindowRect.width, messagesWindowRect.height));
        }

        private void HandleMessagesResize(Rect handleRect)
        {
            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && handleRect.Contains(current.mousePosition))
            {
                resizingMessagesWindow = true;
                current.Use();
            }

            if (!resizingMessagesWindow) return;

            if (current.type == EventType.MouseDrag)
            {
                messagesWindowRect.width = Math.Max(420f, messagesWindowRect.width + current.delta.x);
                messagesWindowRect.height = Math.Max(260f, messagesWindowRect.height + current.delta.y);
                current.Use();
            }
            else if (current.type == EventType.MouseUp || current.rawType == EventType.MouseUp)
            {
                resizingMessagesWindow = false;
                current.Use();
            }
        }

        private float GetVerticalScrollbarWidth()
        {
            GUIStyle scrollbar = GUI.skin.verticalScrollbar;
            if (scrollbar == null) return 0f;

            float width = scrollbar.fixedWidth;
            if (width <= 0f)
            {
                width = scrollbar.CalcSize(GUIContent.none).x;
            }
            return width + scrollbar.margin.horizontal;
        }

        private void SetPausedForMessageInput(bool shouldPause)
        {
            if (pausedForMessageInput == shouldPause) return;

            pausedForMessageInput = shouldPause;
            if (shouldPause)
            {
                pauseStateBeforeMessageInput = scrController.instance != null && scrController.instance.paused;
                timeScaleBeforeMessageInput = Time.timeScale;
                ADOFAI_AP.TogglePause(true);
            }
            else
            {
                if (scrController.instance != null)
                {
                    scrController.instance.paused = pauseStateBeforeMessageInput;
                }
                Time.timeScale = timeScaleBeforeMessageInput;
            }
        }




    }
}
