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
using Color = UnityEngine.Color;

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

        // debug var
        internal long idLoc = 0;
        internal string idName = "";
        internal string lvlName = string.Empty;
        internal bool fromDebugMenu = false;
        internal int recupCheckpoint = 0;

        // selection menu scroll position
        private Vector2 levelScroll = Vector2.zero;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ADOFAI_AP.Instance.mls.LogInfo("MENU_AP initialized — press 'M' in-game to open the menu.");
            }
            else
            {
                ADOFAI_AP.Instance.mls.LogWarning("A second MENU_AP instance was created and is being ignored.");
            }

            StartCoroutine(DelayedMenuOpen());
        }

        IEnumerator DelayedMenuOpen()
        {
            yield return new WaitForSeconds(2f);
            ADOFAI_AP.Instance.mls.LogInfo("Opening the menu");
            currentMenu = MenuState.Connection; // Set the initial menu state to Connection
        }

        // True while WE are holding the game paused because a menu is open.
        private bool menuPauseActive = false;

        void Update()
        {
            // Check for a key press to toggle the menu. We only flip the menu state here;
            // pausing/unpausing is handled centrally by SyncPause() so that EVERY way of
            // opening the menu (key, connect, post-level selection...) pauses the game.
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (currentMenu == MenuState.None)
                {
                    currentMenu = isConnected ? MenuState.Main : MenuState.Connection;
                    ADOFAI_AP.Instance.mls.LogInfo($"Menu opened (state: {currentMenu}).");
                }
                else
                {
                    ADOFAI_AP.Instance.mls.LogInfo("Menu closed.");
                    currentMenu = MenuState.None;
                }
            }

            SyncPause();
        }

        /// <summary>
        /// Single source of truth for pausing: the game is forced paused for as long as any
        /// menu is open, and unpaused exactly once when every menu is closed. Enforced each
        /// frame so the game can never end up running while a menu is visible.
        /// </summary>
        private void SyncPause()
        {
            bool shouldPause = currentMenu != MenuState.None;

            // No active controller yet (e.g. main-menu scene): just remember the intent.
            if (scrController.instance == null)
            {
                menuPauseActive = shouldPause;
                return;
            }

            if (shouldPause)
            {
                // Re-assert every frame in case the game unpaused itself in the meantime.
                if (!scrController.instance.paused || Time.timeScale != 0f)
                    ADOFAI_AP.TogglePause(true);
                menuPauseActive = true;
            }
            else if (menuPauseActive)
            {
                ADOFAI_AP.TogglePause(false);
                menuPauseActive = false;
            }
        }

        // ---------------------------------------------------------------------
        //  THEME / STYLING
        // ---------------------------------------------------------------------

        // ADOFAI colour language: dark navy backdrop, red planet + blue planet accents.
        static readonly Color ColBackdrop   = new Color(0f, 0f, 0f, 0.55f);
        static readonly Color ColPanel      = new Color(0.07f, 0.08f, 0.14f, 0.97f);
        static readonly Color ColBorder     = new Color(0.30f, 0.34f, 0.52f, 1f);
        static readonly Color ColText       = new Color(0.92f, 0.93f, 0.97f, 1f);
        static readonly Color ColMuted      = new Color(0.62f, 0.65f, 0.80f, 1f);
        static readonly Color ColAccent     = new Color(0.94f, 0.27f, 0.42f, 1f); // red planet
        static readonly Color ColAccentHi   = new Color(1.00f, 0.42f, 0.55f, 1f);
        static readonly Color ColBlue       = new Color(0.30f, 0.62f, 0.95f, 1f); // blue planet
        static readonly Color ColBlueHi     = new Color(0.46f, 0.72f, 1.00f, 1f);
        static readonly Color ColBtn        = new Color(0.15f, 0.17f, 0.27f, 1f);
        static readonly Color ColBtnHi      = new Color(0.23f, 0.26f, 0.40f, 1f);
        static readonly Color ColTrack      = new Color(0.16f, 0.18f, 0.26f, 1f);
        static readonly Color ColAvailable  = new Color(0.32f, 0.80f, 0.50f, 1f); // green
        static readonly Color ColGoal       = new Color(1.00f, 0.82f, 0.30f, 1f); // gold
        static readonly Color ColDone        = new Color(0.42f, 0.46f, 0.55f, 1f); // grey

        private bool stylesReady = false;
        private readonly Dictionary<int, Texture2D> texCache = new Dictionary<int, Texture2D>();

        GUIStyle panelStyle, titleStyle, subtitleStyle, headerStyle;
        GUIStyle labelStyle, valueStyle, badgeOkStyle, badgeNoStyle;
        GUIStyle textFieldStyle, fieldLabelStyle;
        GUIStyle btnStyle, btnAccentStyle, btnBlueStyle, btnSmallStyle;
        GUIStyle toggleOnStyle, toggleOffStyle, levelBtnStyle, legendStyle;

        Texture2D Tex(Color c)
        {
            int key = c.GetHashCode();
            Texture2D t;
            if (texCache.TryGetValue(key, out t) && t != null) return t;
            t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, c);
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply();
            texCache[key] = t;
            return t;
        }

        GUIStyle MakeButton(Color bg, Color hover, Color text, int fontSize, FontStyle fs)
        {
            var s = new GUIStyle(GUI.skin.button);
            s.normal.background = Tex(bg);
            s.normal.textColor = text;
            s.hover.background = Tex(hover);
            s.hover.textColor = text;
            s.active.background = Tex(hover);
            s.active.textColor = text;
            s.focused.background = Tex(bg);
            s.focused.textColor = text;
            s.border = new RectOffset(0, 0, 0, 0);
            s.margin = new RectOffset(4, 4, 4, 4);
            s.padding = new RectOffset(10, 10, 8, 8);
            s.fontSize = fontSize;
            s.fontStyle = fs;
            s.alignment = TextAnchor.MiddleCenter;
            return s;
        }

        void InitStyles()
        {
            if (stylesReady) return;

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = Tex(ColPanel);
            panelStyle.border = new RectOffset(0, 0, 0, 0);
            panelStyle.padding = new RectOffset(0, 0, 0, 0);

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 21;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = ColText;
            titleStyle.wordWrap = false;

            subtitleStyle = new GUIStyle(GUI.skin.label);
            subtitleStyle.fontSize = 12;
            subtitleStyle.normal.textColor = ColMuted;

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 14;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = ColBlueHi;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 13;
            labelStyle.normal.textColor = ColText;
            labelStyle.wordWrap = true;

            valueStyle = new GUIStyle(GUI.skin.label);
            valueStyle.fontSize = 13;
            valueStyle.fontStyle = FontStyle.Bold;
            valueStyle.normal.textColor = ColText;
            valueStyle.alignment = TextAnchor.MiddleRight;

            fieldLabelStyle = new GUIStyle(GUI.skin.label);
            fieldLabelStyle.fontSize = 13;
            fieldLabelStyle.normal.textColor = ColMuted;
            fieldLabelStyle.alignment = TextAnchor.MiddleLeft;

            badgeOkStyle = new GUIStyle(GUI.skin.label);
            badgeOkStyle.fontSize = 13;
            badgeOkStyle.fontStyle = FontStyle.Bold;
            badgeOkStyle.normal.textColor = ColAvailable;

            badgeNoStyle = new GUIStyle(badgeOkStyle);
            badgeNoStyle.normal.textColor = ColAccentHi;

            textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.normal.background = Tex(ColTrack);
            textFieldStyle.focused.background = Tex(ColTrack);
            textFieldStyle.hover.background = Tex(ColBtnHi);
            textFieldStyle.normal.textColor = ColText;
            textFieldStyle.focused.textColor = ColText;
            textFieldStyle.fontSize = 13;
            textFieldStyle.padding = new RectOffset(8, 8, 6, 6);
            textFieldStyle.margin = new RectOffset(4, 4, 4, 4);
            textFieldStyle.border = new RectOffset(0, 0, 0, 0);

            btnStyle = MakeButton(ColBtn, ColBtnHi, ColText, 13, FontStyle.Normal);
            btnAccentStyle = MakeButton(ColAccent, ColAccentHi, Color.white, 14, FontStyle.Bold);
            btnBlueStyle = MakeButton(ColBlue, ColBlueHi, Color.white, 13, FontStyle.Bold);
            btnSmallStyle = MakeButton(ColBtn, ColBtnHi, ColText, 14, FontStyle.Bold);
            btnSmallStyle.padding = new RectOffset(4, 4, 2, 2);

            toggleOnStyle = MakeButton(new Color(0.16f, 0.30f, 0.24f, 1f), new Color(0.20f, 0.40f, 0.32f, 1f), ColAvailable, 12, FontStyle.Bold);
            toggleOnStyle.alignment = TextAnchor.MiddleLeft;
            toggleOnStyle.padding = new RectOffset(8, 8, 5, 5);
            toggleOffStyle = MakeButton(ColBtn, ColBtnHi, ColMuted, 12, FontStyle.Normal);
            toggleOffStyle.alignment = TextAnchor.MiddleLeft;
            toggleOffStyle.padding = new RectOffset(8, 8, 5, 5);

            // Light texture so the per-level GUI.backgroundColor tint shows true colour.
            levelBtnStyle = MakeButton(Color.white, Color.white, new Color(0.06f, 0.07f, 0.12f, 1f), 12, FontStyle.Bold);
            levelBtnStyle.margin = new RectOffset(3, 3, 3, 3);
            levelBtnStyle.padding = new RectOffset(2, 2, 4, 4);

            legendStyle = new GUIStyle(GUI.skin.label);
            legendStyle.fontSize = 11;
            legendStyle.normal.textColor = ColMuted;

            stylesReady = true;
        }

        // ---------------------------------------------------------------------
        //  SMALL DRAWING HELPERS
        // ---------------------------------------------------------------------

        Rect Centered(float w, float h)
        {
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            return new Rect(x, y, w, h);
        }

        // Draws the dim backdrop + panel box and returns the inner content rect.
        Rect BeginPanel(float w, float h)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Tex(ColBackdrop));
            Rect panel = Centered(w, h);
            // border then fill, for a subtle outline
            GUI.DrawTexture(new Rect(panel.x - 2, panel.y - 2, panel.width + 4, panel.height + 4), Tex(ColBorder));
            GUI.Box(panel, GUIContent.none, panelStyle);
            const float pad = 22f;
            Rect inner = new Rect(panel.x + pad, panel.y + pad, panel.width - 2 * pad, panel.height - 2 * pad);
            GUILayout.BeginArea(inner);
            return inner;
        }

        void EndPanel()
        {
            GUILayout.EndArea();
        }

        void Header(string title, string subtitle)
        {
            GUILayout.Label(title, titleStyle);
            if (!string.IsNullOrEmpty(subtitle))
                GUILayout.Label(subtitle, subtitleStyle);
            Divider();
        }

        void Divider()
        {
            GUILayout.Space(8);
            Rect r = GUILayoutUtility.GetRect(1, 2, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(r, Tex(ColBorder));
            GUILayout.Space(8);
        }

        void StatRow(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, fieldLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(value, valueStyle);
            GUILayout.EndHorizontal();
        }

        bool ToggleButton(bool value, string label)
        {
            string glyph = value ? "●  " : "○  "; // ● / ○
            if (GUILayout.Button(glyph + label, value ? toggleOnStyle : toggleOffStyle))
                value = !value;
            return value;
        }

        void ProgressBar(float frac, string overlay)
        {
            frac = Mathf.Clamp01(frac);
            Rect r = GUILayoutUtility.GetRect(1, 24, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(r, Tex(ColTrack));
            if (frac > 0f)
                GUI.DrawTexture(new Rect(r.x, r.y, r.width * frac, r.height), Tex(ColAvailable));
            var s = new GUIStyle(labelStyle);
            s.alignment = TextAnchor.MiddleCenter;
            s.fontStyle = FontStyle.Bold;
            s.fontSize = 12;
            GUI.Label(r, overlay, s);
        }

        void LegendSwatch(Color c, string label)
        {
            Rect r = GUILayoutUtility.GetRect(14, 14, GUILayout.Width(14), GUILayout.Height(14));
            GUI.DrawTexture(r, Tex(c));
            GUILayout.Space(2);
            GUILayout.Label(label, legendStyle);
            GUILayout.Space(12);
        }

        // ---------------------------------------------------------------------
        //  MAIN ENTRY
        // ---------------------------------------------------------------------

        void OnGUI()
        {
            if (currentMenu == MenuState.None) return;

            InitStyles();
            GUI.backgroundColor = Color.white;

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

        // ---------------------------------------------------------------------
        //  CONNECTION
        // ---------------------------------------------------------------------

        void DrawConnectionMenu()
        {
            BeginPanel(440, 420);
            Header("A Dance of Fire and Ice", "Archipelago — connect to a MultiWorld server");

            GUILayout.Label("Username", fieldLabelStyle);
            pseudo = GUILayout.TextField(pseudo, textFieldStyle, GUILayout.ExpandWidth(true));

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Server IP", fieldLabelStyle);
            serverIP = GUILayout.TextField(serverIP, textFieldStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();
            GUILayout.Space(8);
            GUILayout.BeginVertical(GUILayout.Width(90));
            GUILayout.Label("Port", fieldLabelStyle);
            serverPort = GUILayout.TextField(serverPort, textFieldStyle, GUILayout.Width(90));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Password (optional)", fieldLabelStyle);
            serverPassword = GUILayout.PasswordField(serverPassword, '*', textFieldStyle, GUILayout.ExpandWidth(true));

            GUILayout.Space(14);
            if (GUILayout.Button("Connect", btnAccentStyle, GUILayout.Height(38)))
            {
                ADOFAI_AP.Instance.mls.LogInfo($"Connecting to {serverIP}:{serverPort}...");
                ADOFAI_AP.Instance.client.Connect(serverIP, int.Parse(serverPort), pseudo, serverPassword);
                ADOFAI_AP.Instance.mls.LogInfo("Connected to server.");

                ADOFAI_AP.Instance.pseudo.Value = pseudo;
                ADOFAI_AP.Instance.serverIP.Value = serverIP;
                ADOFAI_AP.Instance.serverPort.Value = serverPort;
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("Press  M  to toggle this menu", legendStyle);
            EndPanel();
        }

        // ---------------------------------------------------------------------
        //  MAIN
        // ---------------------------------------------------------------------

        void DrawMainMenu()
        {
            BeginPanel(420, 480);
            Header("ADOFAI × Archipelago", isConnected ? "Connected" : "Offline");

            float frac = nbNonGoalLocations > 0 ? (float)nbLocationsCompleted / nbNonGoalLocations : 0f;
            GUILayout.Label("Percentage goal progress", headerStyle);
            GUILayout.Space(4);
            ProgressBar(frac, $"{nbLocationsCompleted} / {nbNonGoalLocations} levels");

            GUILayout.Space(10);
            bool allGoals = ADOFAI_AP.Instance.client.IsAllGoalLevelsCompleted();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Goal levels", fieldLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(allGoals ? "✓  All completed" : "✗  Not yet", allGoals ? badgeOkStyle : badgeNoStyle);
            GUILayout.EndHorizontal();

            Divider();

            StatRow("Last item received", lastItem);
            StatRow("Session deaths", ADOFAI_AP.Instance.client.SessionDeathCount.Value.ToString());
            StatRow("DeathLink",
                ADOFAI_AP.Instance.client.DeathLinkMod_Disable
                    ? "Disabled"
                    : $"{ADOFAI_AP.Instance.client.currentLife} live(s) left");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Level selection", btnAccentStyle, GUILayout.Height(36)))
            {
                currentMenu = MenuState.Selection;
            }

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Options", btnStyle, GUILayout.Height(32)))
            {
                currentMenu = MenuState.Options;
            }
            if (GUILayout.Button("Close (M)", btnStyle, GUILayout.Height(32)))
            {
                currentMenu = MenuState.None;
                ADOFAI_AP.TogglePause(false);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            if (GUILayout.Button("Disconnect", btnBlueStyle, GUILayout.Height(30)))
            {
                ADOFAI_AP.Instance.client.Disconnect();
                isConnected = false;
                currentMenu = MenuState.Connection;
            }

            EndPanel();
        }

        // ---------------------------------------------------------------------
        //  OPTIONS
        // ---------------------------------------------------------------------

        void DrawOptionsMenu()
        {
            BeginPanel(440, 420);
            Header("Options", "Tune DeathLink and check behaviour");

            var client = ADOFAI_AP.Instance.client;

            GUILayout.Label("DeathLink Mod", headerStyle);
            GUILayout.Space(4);
            bool dlEnabled = !client.DeathLinkMod_Disable;
            bool newDl = ToggleButton(dlEnabled, dlEnabled ? "Enabled" : "Disabled");
            if (newDl != dlEnabled)
                client.DeathLinkMod_Disable = !client.DeathLinkMod_Disable;

            GUILayout.Space(10);
            GUILayout.Label("Deaths before sending a DeathLink (MaxHealth)", fieldLabelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("−", btnSmallStyle, GUILayout.Width(40), GUILayout.Height(30)))
            {
                if (client.DeathLinkMod_MaxHealth.Value > 1)
                {
                    client.DeathLinkMod_MaxHealth.Value--;
                    client.currentLife = client.DeathLinkMod_MaxHealth.Value;
                    Notification.Instance.CreateNotification($"MaxHealth set to {client.DeathLinkMod_MaxHealth.Value}\nLifes reset");
                }
            }
            GUILayout.FlexibleSpace();
            var bigVal = new GUIStyle(valueStyle);
            bigVal.alignment = TextAnchor.MiddleCenter;
            bigVal.fontSize = 18;
            GUILayout.Label(client.DeathLinkMod_MaxHealth.Value.ToString(), bigVal, GUILayout.Width(60));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", btnSmallStyle, GUILayout.Width(40), GUILayout.Height(30)))
            {
                client.DeathLinkMod_MaxHealth.Value++;
                client.currentLife = client.DeathLinkMod_MaxHealth.Value;
                Notification.Instance.CreateNotification($"Threshold set to {client.DeathLinkMod_MaxHealth.Value}\nDeathCount reset");
            }
            GUILayout.EndHorizontal();

            Divider();

            GUILayout.Label("Send location when landing on a portal", fieldLabelStyle);
            GUILayout.Space(4);
            bool newSend = ToggleButton(sendLocationOnLandOnPortal, sendLocationOnLandOnPortal ? "Enabled" : "Disabled");
            if (newSend != sendLocationOnLandOnPortal)
            {
                sendLocationOnLandOnPortal = newSend;
                ADOFAI_AP.Instance.SendLocationOnLandOnPortal.Value = sendLocationOnLandOnPortal;
            }

            GUILayout.FlexibleSpace();
            StatRow("DeathLink lives left", client.currentLife.ToString());
            StatRow("Mod version", ADOFAI_AP.modVersion);

            GUILayout.Space(10);
            if (GUILayout.Button("←  Back", btnStyle, GUILayout.Height(34)))
            {
                currentMenu = MenuState.Main;
            }

            EndPanel();
        }

        // ---------------------------------------------------------------------
        //  LEVEL SELECTION
        // ---------------------------------------------------------------------

        void DrawSelectionMenu()
        {
            float w = Mathf.Min(820, Screen.width - 80);
            float h = Mathf.Min(620, Screen.height - 80);
            Rect inner = BeginPanel(w, h);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Select a level", titleStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("←  Back", btnStyle, GUILayout.Width(110), GUILayout.Height(30)))
            {
                currentMenu = MenuState.Main;
            }
            GUILayout.EndHorizontal();
            Divider();

            // Filters
            GUILayout.Label("Filters", headerStyle);
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            showCheckedLocation = ToggleButton(showCheckedLocation, "Show checked levels");
            hideGoalLocation = ToggleButton(hideGoalLocation, "Hide goal levels");
            normalWorld = ToggleButton(normalWorld, "Normal worlds");
            extraWorld = ToggleButton(extraWorld, "Extra worlds");
            GUILayout.EndVertical();
            GUILayout.Space(6);
            GUILayout.BeginVertical();
            starWorld = ToggleButton(starWorld, "Star worlds");
            crownWorld = ToggleButton(crownWorld, "Crown worlds");
            aprilFoolsWorld = ToggleButton(aprilFoolsWorld, "April Fools worlds");
            BWorld = ToggleButton(BWorld, "B world");
            GUILayout.EndVertical();
            GUILayout.Space(6);
            GUILayout.BeginVertical();
            neonWorlds = ToggleButton(neonWorlds, "Neon worlds");
            neonExtraWorlds = ToggleButton(neonExtraWorlds, "Neon extra worlds");
            arWorld = ToggleButton(arWorld, "AR world");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            // Legend
            GUILayout.BeginHorizontal();
            LegendSwatch(ColAvailable, "Available");
            LegendSwatch(ColGoal, "Goal");
            LegendSwatch(ColDone, "Completed");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            Divider();

            // Responsive grid inside a scroll view
            const float btnW = 74f;
            const float btnH = 34f;
            int columns = Mathf.Max(1, (int)((inner.width - 16) / (btnW + 6)));

            levelScroll = GUILayout.BeginScrollView(levelScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            int shown = 0;
            bool rowOpen = false;
            foreach (var level in Data_AP.ItemsReceived.Keys)
            {
                if (level.Length < 11 || level == "Filler Note") continue;
                string name = level.Substring(10);

                if (Data_AP.goalLevels.Contains(name) && hideGoalLocation) continue;
                // || name.StartsWith("1-") à retirer lorsque le start avec des levels random sera dispo
                if ((Data_AP.MainWorlds.Keys.Contains(name) || Data_AP.MainWorldsTuto.Keys.Contains(name) || name.StartsWith("1-")) && !normalWorld) continue;
                if ((Data_AP.XtraWorlds.Keys.Contains(name) || Data_AP.XtraTuto.Keys.Contains(name)) && !extraWorld) continue;
                if ((Data_AP.StarWorlds.Keys.Contains(name) || Data_AP.StarWorldsTuto.Keys.Contains(name)) && !starWorld) continue;
                if ((Data_AP.CrownWorlds.Keys.Contains(name) || Data_AP.CrownWorldsTuto.Keys.Contains(name)) && !crownWorld) continue;
                if (Data_AP.AprilFoolsWorlds.Keys.Contains(name) && !aprilFoolsWorld) continue;
                if ((Data_AP.BWorld.Keys.Contains(name) || Data_AP.BWorldTuto.Keys.Contains(name)) && !BWorld) continue;
                if ((Data_AP.NeonCosmosWorlds.Keys.Contains(name) || Data_AP.NeonCosmosWorldsTuto.Keys.Contains(name)) && !neonWorlds) continue;
                if ((Data_AP.NeonCosmosWorldsEX.Keys.Contains(name) || Data_AP.NeonCosmosWorldsEXTuto.Keys.Contains(name)) && !neonExtraWorlds) continue;
                if ((Data_AP.ARWorld.Keys.Contains(name) || Data_AP.ARWorldTuto.Keys.Contains(name)) && !arWorld) continue;

                try
                {
                    if (!Data_AP.ItemsReceived[level]) continue;

                    // Defensive: a received Key_Level_X may not have a pre-loaded location
                    // (manual server give, disabled world option...). Treat a missing entry as
                    // "not checked" instead of throwing every frame.
                    bool isChecked = Data_AP.LocationsChecked.TryGetValue(name, out bool checkedVal) && checkedVal;
                    if (isChecked && !showCheckedLocation) continue;

                    if (shown % columns == 0)
                    {
                        if (rowOpen) GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        rowOpen = true;
                    }
                    shown++;

                    if (isChecked)
                        GUI.backgroundColor = ColDone;
                    else
                        GUI.backgroundColor = Data_AP.goalLevels.Contains(name) ? ColGoal : ColAvailable;

                    if (GUILayout.Button(name, levelBtnStyle, GUILayout.Width(btnW), GUILayout.Height(btnH)))
                    {
                        ADOFAI_AP.Instance.mls.LogInfo($"Selected level: {name}");
                        currentMenu = MenuState.None;
                        GUI.backgroundColor = Color.white;
                        ADOBase.controller.EnterLevel(name, false);
                    }
                    GUI.backgroundColor = Color.white;
                }
                catch (Exception e)
                {
                    ADOFAI_AP.Instance.mls.LogError($"Error processing level {level}: {e.Message}");
                }
            }
            if (rowOpen) GUILayout.EndHorizontal();

            if (shown == 0)
                GUILayout.Label("No level matches the current filters.", subtitleStyle);

            GUILayout.EndScrollView();

            GUI.backgroundColor = Color.white;
            EndPanel();
        }
    }
}
