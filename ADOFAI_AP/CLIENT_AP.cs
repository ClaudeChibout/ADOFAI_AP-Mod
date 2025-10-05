using ADOFAI_AP.Patches;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
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
        public bool DeathLinkMod_Disable = false;
        public ConfigEntry<int> DeathLinkMod_MaxHealth;
        public ConfigEntry<int> SessionDeathCount;
        public int currentLife = 0;
        private Dictionary<string, object> progress;

        public void Connect(string addr, int port, string slot)
        {
            DeathLinkMod_Disable = true;
            string[] tags = new string[] { "DeathLink" };
            session = ArchipelagoSessionFactory.CreateSession(addr, port);
            ADOFAI_AP.Instance.mls.LogInfo($"session crée...");
            var isConnected = session.TryConnectAndLogin("A Dance of Fire and Ice", slot,
                ItemsHandlingFlags.AllItems, new Version(0, 6, 3), tags);
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
                Notification.Instance.CreateNotification($"Connected to Archipelago server at {addr}:{port} as {slot}.");
                ADOFAI_AP.Instance.Menu.isConnected = true;
                ADOFAI_AP.Instance.Menu.currentMenu = MENU_AP.MenuState.Main;

                Task.Delay(1000).ContinueWith(_ =>
                {
                    ADOFAI_AP.TogglePause(true);
                });

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
                session.Items.ItemReceived += (helper) =>
                {

                    var lastItem = helper.AllItemsReceived[helper.Index - 1];
                    ADOFAI_AP.Instance.ReceiveItem(lastItem.ItemName);
                    if (gameEnded) return;
                    Notification.Instance.CreateNotification($"You received: {lastItem.ItemName} from {lastItem.Player} !");
                    ADOFAI_AP.Instance.mls.LogInfo($"Received item: {lastItem.ItemName} (ID: {lastItem.ItemId})");
                };

                session.Socket.SocketClosed += _ =>
                {
                    Data_AP.goalLevels.Clear();
                    session = null;
                    DL = null;
                    scrController.instance.QuitToMainMenu();
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
                Notification.Instance.CreateNotification("Failed to connect to Archipelago server");
                ADOFAI_AP.Instance.Menu.currentMenu = MENU_AP.MenuState.Connection;
            }
        }

        public void SendMessage(string message)
        {
            session.Socket.SendPacket(new SayPacket { Text = message });
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

