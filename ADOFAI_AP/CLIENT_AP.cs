using ADOFAI_AP.Patches;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ADOFAI_AP
{
    internal class CLIENT_AP
    {

        internal ArchipelagoSession session = null;

        public DeathLinkService DL = null;


        private bool gameEnded = false;
        private bool ready = false;
        private const int MaxMessages = 200;
        private readonly object messagesLock = new object();
        private readonly List<APMessage> messages = new List<APMessage>();
        public bool DeathLinkMod_Disable = false;
        public ConfigEntry<int> DeathLinkMod_MaxHealth;
        public ConfigEntry<int> SessionDeathCount;
        public int currentLife = 0;
        private Dictionary<string, object> progress;

        public void Connect(string addr, int port, string slot, string password = null)
        {
            DeathLinkMod_Disable = true;
            string[] tags = new string[] { "DeathLink" };
            session = ArchipelagoSessionFactory.CreateSession(addr, port);
            ADOFAI_AP.Instance.mls.LogInfo($"session crée...");
            var isConnected = session.TryConnectAndLogin("A Dance of Fire and Ice", slot,
                ItemsHandlingFlags.AllItems, new Version(0, 6, 3), tags, null, password);
            ADOFAI_AP.Instance.mls.LogInfo($"is Connected ready {isConnected.Successful}");




            if (isConnected.Successful)
            {
                scrController.instance.QuitToMainMenu();
                // reset the checkpoint on connect
                progress = new Dictionary<string, object>();

                DeathLinkMod_MaxHealth = ADOFAI_AP.Instance.BindConfig<int>("DeathLinkMod_" + session.RoomState.Seed, "MaxHealth", 1, "Number of deaths before sending a DeathLink");
                SessionDeathCount = ADOFAI_AP.Instance.BindConfig<int>("DeathLinkMod_" + session.RoomState.Seed, "SessionDeathCount", 0, "Number of DeathLink sent in this session");
                currentLife = DeathLinkMod_MaxHealth.Value;
                // load saved progress if exist
                string saveFolder = Path.Combine(Paths.ConfigPath, "ArchipelagoSessions");
                Directory.CreateDirectory(saveFolder);
                string sessionSeed = session.RoomState.Seed;
                string filePath = Path.Combine(saveFolder, $"{sessionSeed}.dat");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    progress = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    ADOFAI_AP.Instance.mls.LogInfo($"Loaded saved progress for seed:{sessionSeed}");
                    foreach (KeyValuePair<string, object> pair in progress)
                    {
                        ADOFAI_AP.Instance.mls.LogInfo($"{pair.Key}: {pair.Value}");
                    }
                    // Utilise les données comme tu veux
                    try
                    {
                        progress["checkpointNum"] = Convert.ToInt32(progress["checkpointNum"]);
                        progress["lastHitMarginsSize"] = Convert.ToInt32(progress["lastHitMarginsSize"]);
                        progress["checkpointsUsed"] = Convert.ToInt32(progress["checkpointsUsed"]);
                    }
                    catch (Exception e)
                    {
                        ADOFAI_AP.Instance.mls.LogInfo($"Error while parsing saved progress: {e.Message}");
                    }
                }
                Persistence.SetSavedProgress(progress);
                ADOFAI_AP.Instance.mls.LogInfo($"Connected to Archipelago server at {addr}:{port} as {slot}.");
                AddMessage("Status", $"Connected to Archipelago server at {addr}:{port} as {slot}.");
                Notification.Instance.CreateNotification($"Connected to Archipelago server at {addr}:{port} as {slot}.");
                ADOFAI_AP.Instance.Menu.isConnected = true;
                ADOFAI_AP.Instance.Menu.currentMenu = MENU_AP.MenuState.Main;

                ADOFAI_AP.Instance.mls.LogInfo("SlotData:");
                var slotData = session.DataStorage.GetSlotData();

                foreach (KeyValuePair<string, object> opt in slotData)
                {
                    ADOFAI_AP.Instance.mls.LogInfo($"{opt.Key}: {opt.Value}");
                }


                // Load base Levels
                LoadWorlds("main_worlds", Data_AP.MainWorlds, Data_AP.MainWorldsKeys);

                // Load the levels specified in the YAML
                if ((bool)slotData["main_worlds_tuto"])
                {
                    LoadWorlds("main_worlds_tuto", Data_AP.MainWorldsTuto, Data_AP.MainWorldsTutoKeys);
                }
                if ((bool)slotData["xtra_worlds"])
                {
                    LoadWorlds("xtra_worlds", Data_AP.XtraWorlds, Data_AP.XtraWorldsKeys);
                }
                if ((bool)slotData["xtra_worlds_tuto"])
                {
                    LoadWorlds("xtra_worlds_tuto", Data_AP.XtraTuto, Data_AP.XtraTutoKeys);
                }
                if ((bool)slotData["b_world"])
                {
                    LoadWorlds("b_world", Data_AP.BWorld, Data_AP.BWorldKeys);
                }
                if ((bool)slotData["b_world_tuto"])
                {
                    LoadWorlds("b_world_tuto", Data_AP.BWorldTuto, Data_AP.BWorldTutoKeys);
                }
                if ((bool)slotData["crown_worlds"])
                {
                    LoadWorlds("crown_worlds", Data_AP.CrownWorlds, Data_AP.CrownWorldsKeys);
                }
                if ((bool)slotData["crown_worlds_tuto"])
                {
                    LoadWorlds("crown_worlds_tuto", Data_AP.CrownWorldsTuto, Data_AP.CrownWorldsTutoKeys);
                }
                if ((bool)slotData["star_worlds"])
                {
                    LoadWorlds("star_worlds", Data_AP.StarWorlds, Data_AP.StarWorldsKeys);
                }
                if ((bool)slotData["star_worlds_tuto"])
                {
                    LoadWorlds("star_worlds_tuto", Data_AP.StarWorldsTuto, Data_AP.StarWorldsTutoKeys);
                }
                if ((bool)slotData["neon_cosmos_worlds"])
                {
                    LoadWorlds("neon_cosmos_worlds", Data_AP.NeonCosmosWorlds, Data_AP.NeonCosmosWorldsKeys);
                }
                if ((bool)slotData["neon_cosmos_worlds_tuto"])
                {
                    LoadWorlds("neon_cosmos_worlds_tuto", Data_AP.NeonCosmosWorldsTuto, Data_AP.NeonCosmosWorldsTutoKeys);
                }
                if ((bool)slotData["neon_cosmos_worlds_ex"])
                {
                    LoadWorlds("neon_cosmos_worlds_ex", Data_AP.NeonCosmosWorldsEX, Data_AP.NeonCosmosWorldsEXKeys);
                }
                if ((bool)slotData["neon_cosmos_worlds_ex_tuto"])
                {
                    LoadWorlds("neon_cosmos_worlds_ex_tuto", Data_AP.NeonCosmosWorldsEXTuto, Data_AP.NeonCosmosWorldsEXTutoKeys);
                }
                if ((bool)slotData["april_fools_worlds"])
                {
                    LoadWorlds("april_fools_worlds", Data_AP.AprilFoolsWorlds, Data_AP.AprilFoolsWorldsKeys);
                }
                if ((bool)slotData["ar_world"])
                {
                    LoadWorlds("ar_world", Data_AP.ARWorld, Data_AP.ARWorldKeys);
                }
                if ((bool)slotData["ar_world_tuto"])
                {
                    LoadWorlds("ar_world_tuto", Data_AP.ARWorldTuto, Data_AP.ARWorldTutoKeys);
                }

                // Load goalLevels
                foreach (var level in ((string)slotData["goal_levels"]).Split())
                {
                    if (Data_AP.LocationsChecked.ContainsKey(level))
                    {
                        Data_AP.goalLevels.Add(level);
                    }
                }

                // Initialize the mod data
                foreach (long levelId in session.Locations.AllLocationsChecked)
                {
                    var LevelName = session.Locations.GetLocationNameFromId(levelId, session.ConnectionInfo.Game);
                    //ADOFAI_AP.Instance.mls.LogInfo($"LevelName: {LevelName}");
                    Data_AP.LocationsChecked[LevelName] = true;
                }

                foreach (var item in session.Items.AllItemsReceived)
                {
                    ADOFAI_AP.Instance.ReceiveItem(item.ItemName);
                }

                // set the menu variables
                ADOFAI_AP.Instance.Menu.nbNonGoalLocations = GetHowMuchNonGoalLevels();
                ADOFAI_AP.Instance.Menu.nbLocationsCompleted = GetHowMuchLevelsCompleted();

                ADOFAI_AP.Instance.mls.LogInfo($"Connected to Archipelago server at {addr}:{port} as {slot}.");
                session.MessageLog.OnMessageReceived += HandleMessage;
                session.Items.ItemReceived += (helper) =>
                {

                    var lastItem = helper.AllItemsReceived[helper.Index - 1];
                    ADOFAI_AP.Instance.ReceiveItem(lastItem.ItemName);
                    if (gameEnded) return;
                    ADOFAI_AP.Instance.mls.LogInfo($"Received item: {lastItem.ItemName} (ID: {lastItem.ItemId})");
                };

                session.Socket.SocketClosed += _ =>
                {
                    Data_AP.goalLevels.Clear();
                    session = null;
                    DL = null;
                    scrController.instance.QuitToMainMenu();
                    AddMessage("Status", $"Connection lost to {addr}:{port}.");
                    Notification.Instance.CreateNotification($"Connection lost to {addr}:{port}.");
                };

                // Handle death link
                if ((bool)slotData["death_link"])
                {
                    ADOFAI_AP.Instance.client.DeathLinkMod_Disable = false;
                    DL = DeathLinkProvider.CreateDeathLinkService(session);
                    DL.OnDeathLinkReceived += (deathLink) =>
                    {   
                        if (DeathLinkMod_Disable) return;
                        ADOFAI_AP.Instance.mls.LogInfo($"DeathLink received: {deathLink.Source} died at {deathLink.Cause}");
                        Notification.Instance.CreateNotification($"{deathLink.Source} has died ! {deathLink.Cause}");

                        // fake a death to the planetary system
                        // to not send another Death in the world
                        scrFlash.Flash(new UnityEngine.Color?(UnityEngine.Color.white.WithAlpha(0.3f)), -1f);
                        SfxSound sfxSound = (ADOBase.controller.endLevelInfo.newBestType == NewBestType.Jingle) ? SfxSound.PlanetExplosionHighscore : SfxSound.PlanetExplosion;
                        if (GCS.playDeathSound)
                        {
                            scrSfx.instance.PlaySfx(sfxSound, MixerGroup.SfxParent, 0.5f, 1f, 0f);
                        }
                        if (GCS.playWilhelm)
                        {
                            scrSfx.instance.PlaySfx(SfxSound.Wilhelm, MixerGroup.SfxParent, 0.6f, 1f, 0f);
                        }

                        for (int i = 0; i < scrController.instance.planetarySystem.planetList.Count; i++)
                        {
                            scrController.instance.planetarySystem.planetList[i].planetRenderer.Explode(1f);
                        }
                        Task.Delay(500).ContinueWith(_ =>
                        {
                            scrController.instance.paused = !scrController.instance.paused;
                            scrController.instance.audioPaused = scrController.instance.paused;
                            Time.timeScale = (scrController.instance.paused ? 0f : 1f);
                        });

                        Task.Delay(1000).ContinueWith(_ =>
                        {
                            scrController.instance.Restart(true);
                        });
                    };
                }



            }
            else
            {
                session = null;
                ADOFAI_AP.Instance.mls.LogError($"Failed to connect to Archipelago server");
                AddMessage("Error", "Failed to connect to Archipelago server");
                Notification.Instance.CreateNotification("Failed to connect to Archipelago server");
                ADOFAI_AP.Instance.Menu.currentMenu = MENU_AP.MenuState.Connection;
            }
        }

        public void SendMessage(string message)
        {
            session.Say(message);
        }

        public List<APMessage> GetMessagesSnapshot()
        {
            lock (messagesLock)
            {
                return new List<APMessage>(messages);
            }
        }

        public void AddMessage(string category, string text, bool notify = false, string richText = null)
        {
            if (string.IsNullOrEmpty(text)) return;

            lock (messagesLock)
            {
                messages.Add(new APMessage(category, text, richText ?? EscapeRichText(text)));
                while (messages.Count > MaxMessages)
                {
                    messages.RemoveAt(0);
                }
            }

            if (notify)
            {
                Notification.Instance.CreateNotification(text);
            }
        }

        private void HandleMessage(LogMessage message)
        {
            string text = message.ToString();
            string category = GetMessageCategory(message);
            bool notify = message is ItemSendLogMessage || message is GoalLogMessage;

            AddMessage(category, text, notify, FormatMessageParts(message));
            ADOFAI_AP.Instance.mls.LogInfo($"AP {category}: {text}");
        }

        private string FormatMessageParts(LogMessage message)
        {
            StringBuilder builder = new StringBuilder();
            foreach (MessagePart part in message.Parts)
            {
                builder.Append(FormatMessagePart(part));
            }
            return builder.ToString();
        }

        private string FormatMessagePart(MessagePart part)
        {
            string text = EscapeRichText(part.Text);
            string color = GetRichTextColor(part);
            if (string.IsNullOrEmpty(color))
            {
                return text;
            }
            return $"<color=#{color}>{text}</color>";
        }

        private string GetRichTextColor(MessagePart part)
        {
            if (!part.PaletteColor.HasValue) return "FFFFFF";

            switch (part.PaletteColor.Value)
            {
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Black:
                    return "000000";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Red:
                    return "EE0000";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Green:
                    return "00FF7F";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Blue:
                    return "6495ED";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Cyan:
                    return "00EEEE";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Magenta:
                    return "EE00EE";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Yellow:
                    return "FAFAD2";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.SlateBlue:
                    return "6D8BE8";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Salmon:
                    return "FA8072";
                case Archipelago.MultiClient.Net.Colors.PaletteColor.Plum:
                    return "AF99EF";
                default:
                    return "FFFFFF";
            }
        }

        private string EscapeRichText(string text)
        {
            return (text ?? string.Empty)
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        private string GetMessageCategory(LogMessage message)
        {
            if (message is HintItemSendLogMessage) return "Hint";
            if (message is ItemSendLogMessage) return "Item";
            if (message is ChatLogMessage || message is ServerChatLogMessage) return "Chat";
            if (message is GoalLogMessage) return "Goal";
            if (message is ReleaseLogMessage) return "Release";
            if (message is CollectLogMessage) return "Collect";
            if (message is CommandResultLogMessage || message is AdminCommandResultLogMessage) return "Command";
            if (message is JoinLogMessage) return "Join";
            if (message is LeaveLogMessage) return "Leave";
            return "Server";
        }

        public void HandleTextInput(string raw)
        {
            string text = (raw ?? string.Empty).Trim();
            if (text.Length == 0) return;

            if (text.StartsWith("/"))
            {
                RunLocalCommand(text);
                return;
            }

            if (session == null)
            {
                AddMessage("Error", "Not connected.");
                return;
            }

            SendMessage(text);
        }

        private void RunLocalCommand(string rawCommand)
        {
            string[] parts = rawCommand.Substring(1).Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string command = parts.Length > 0 ? parts[0].ToLowerInvariant() : string.Empty;
            string arg = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            switch (command)
            {
                case "help":
                    AddMessage("Command", "/help /disconnect /received /missing [filter] /items /locations /item_groups [group] /location_groups [group] /ready");
                    break;
                case "disconnect":
                    if (session == null)
                    {
                        AddMessage("Command", "Not connected.");
                    }
                    else
                    {
                        _ = Disconnect();
                        AddMessage("Command", "Disconnected.");
                    }
                    break;
                case "received":
                    PrintReceivedItems();
                    break;
                case "missing":
                    PrintMissingLocations(arg);
                    break;
                case "items":
                    PrintMany("Items", AllKnownItems());
                    break;
                case "locations":
                    PrintMany("Locations", AllKnownLocations());
                    break;
                case "item_groups":
                    PrintGroups(session?.DataStorage.GetItemNameGroups(), arg, "Item groups");
                    break;
                case "location_groups":
                    PrintGroups(session?.DataStorage.GetLocationNameGroups(), arg, "Location groups");
                    break;
                case "ready":
                    if (session == null)
                    {
                        AddMessage("Command", "Not connected.");
                    }
                    else
                    {
                        ready = !ready;
                        session.SetClientState(ready ? ArchipelagoClientState.ClientReady : ArchipelagoClientState.ClientConnected);
                        AddMessage("Command", ready ? "Marked ready." : "Marked connected.");
                    }
                    break;
                default:
                    if (session != null)
                    {
                        SendMessage(rawCommand);
                    }
                    else
                    {
                        AddMessage("Error", $"Unknown command: /{command}");
                    }
                    break;
            }
        }

        private void PrintReceivedItems()
        {
            if (session == null)
            {
                AddMessage("Command", "Not connected.");
                return;
            }

            var lines = session.Items.AllItemsReceived.Select((item, index) =>
                $"#{index + 1} {item.ItemDisplayName} from {item.Player} at {item.LocationDisplayName}");
            PrintMany("Received", lines);
        }

        private void PrintMissingLocations(string filter)
        {
            if (session == null)
            {
                AddMessage("Command", "Not connected.");
                return;
            }

            var lines = session.Locations.AllMissingLocations
                .Select(id => session.Locations.GetLocationNameFromId(id, session.ConnectionInfo.Game))
                .Where(name => string.IsNullOrEmpty(filter) || name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            PrintMany("Missing", lines);
        }

        private IEnumerable<string> AllKnownItems()
        {
            return Data_AP.ItemsReceived.Keys
                .Where(name => name != "Filler Note")
                .Select(StripKeyPrefix)
                .OrderBy(name => name);
        }

        private IEnumerable<string> AllKnownLocations()
        {
            return Data_AP.LocationsChecked.Keys.OrderBy(name => name);
        }

        private string StripKeyPrefix(string itemName)
        {
            const string prefix = "Key_Level_";
            if (itemName.StartsWith(prefix)) return itemName.Substring(prefix.Length);
            return itemName;
        }

        private void PrintMany(string category, IEnumerable<string> lines, int max = 100)
        {
            var list = lines.Take(max + 1).ToList();
            if (list.Count == 0)
            {
                AddMessage(category, "None.");
                return;
            }

            foreach (string line in list.Take(max))
            {
                AddMessage(category, line);
            }

            if (list.Count > max)
            {
                AddMessage(category, $"Showing first {max} results.");
            }
        }

        private void PrintGroups(Dictionary<string, string[]> groups, string key, string label)
        {
            if (groups == null)
            {
                AddMessage("Command", "Not connected.");
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                AddMessage("Command", $"{label}: {string.Join(", ", groups.Keys.OrderBy(name => name).ToArray())}");
                return;
            }

            if (!groups.ContainsKey(key))
            {
                AddMessage("Command", $"Unknown group: {key}");
                return;
            }

            PrintMany("Command", groups[key].OrderBy(name => name));
        }

        public void ReportLocation(string name)
        {
            var id = session.Locations.GetLocationIdFromName(session.ConnectionInfo.Game, name);
            session.Locations.CompleteLocationChecks(id);
            CheckWin();
            ADOFAI_AP.Instance.mls.LogInfo($"id:{id} submited ");
            Notification.Instance.CreateNotification($"You succeeded: {name} !");
        }

        public void CheckWin()
        {
            ADOFAI_AP.Instance.Menu.nbNonGoalLocations = GetHowMuchNonGoalLevels();
            ADOFAI_AP.Instance.Menu.nbLocationsCompleted = GetHowMuchLevelsCompleted();
            if (IsAllGoalLevelsCompleted() && IsPercentageComplete())
            {
                gameEnded = true;
                session.SetGoalAchieved();
                Task.Delay(2000).ContinueWith(_ =>
                {
                    foreach (long levelId in session.Locations.AllLocationsChecked)
                    {
                        var LevelName = session.Locations.GetLocationNameFromId(levelId, session.ConnectionInfo.Game);
                        //ADOFAI_AP.Instance.mls.LogInfo($"LevelName: {LevelName}");
                        Data_AP.LocationsChecked[LevelName] = true;
                    }
                });
                Notification.Instance.CreateNotification("You complete the game GG !!!");
            }
        }
        public bool IsPercentageComplete()
        {
            int nb = GetHowMuchLevelsCompleted();
            int total = GetHowMuchNonGoalLevels();

            if (nb < total)
            {
                return false;
            }
            return true;
        }

        public bool IsAllGoalLevelsCompleted()
        {
            foreach (var levelName in Data_AP.goalLevels)
            {
                if (!Data_AP.LocationsChecked[levelName])
                {
                    return false;
                }
            }

            return true;

        }

        public int GetHowMuchLevelsCompleted()
        {
            int nb = 0;

            foreach (KeyValuePair<string, bool> kvp in Data_AP.LocationsChecked)
            {
                if (kvp.Value && !Data_AP.goalLevels.Contains(kvp.Key)) nb++;
            }

            return nb;
        }

        public int GetHowMuchNonGoalLevels()
        {
            var slotData = ADOFAI_AP.Instance.client.session.DataStorage.GetSlotData();
            if (int.TryParse(slotData["percentage_goal_completion"]?.ToString(), out int goalPercent))
            {
                float completeLevelsNeeded = (float)goalPercent / (float)100;
                completeLevelsNeeded *= (Data_AP.LocationsChecked.Count() - Data_AP.goalLevels.Count());
                return (int)completeLevelsNeeded;
            }
            return 0;
        }

        public void LoadWorlds(string worldsOptionName, Dictionary<string, bool> worldsNames, Dictionary<string, bool> worldsKeys)
        {
            foreach (var kvp in worldsNames)
            {
                Data_AP.LocationsChecked[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in worldsKeys)
            {
                Data_AP.ItemsReceived[kvp.Key] = kvp.Value;
            }
            ADOFAI_AP.Instance.mls.LogInfo($"{worldsOptionName} loaded");
        }

        public async Task Disconnect()
        {
            await session.Socket.DisconnectAsync();
            Data_AP.goalLevels.Clear();
            session = null;
            DL = null;
            scrController.instance.QuitToMainMenu();
        }

    }
}
