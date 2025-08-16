using ADOFAI_AP.Patches;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ADOFAI_AP
{
    internal class CLIENT_AP
    {

        internal ArchipelagoSession session;

        public DeathLinkService DL = null;

        public void Connect(string addr, int port, string slot)
        {
            string[] tags = new string[] { "DeathLink" };
            session = ArchipelagoSessionFactory.CreateSession(addr, port);
            ADOFAI_AP.Instance.mls.LogInfo($"session crée...");
            var isConnected = session.TryConnectAndLogin("A Dance of Fire and Ice", slot,
                ItemsHandlingFlags.AllItems, new Version(0, 6, 3), tags);
            ADOFAI_AP.Instance.mls.LogInfo($"is Connected ready {isConnected.Successful}");




            if (isConnected.Successful)
            {
                // Initialize the mod data
                foreach (long levelId in session.Locations.AllLocationsChecked)
                {
                    var LevelName = session.Locations.GetLocationNameFromId(levelId, session.ConnectionInfo.Game);
                    Data_AP.LocationsChecked[LevelName] = true;
                }

                ADOFAI_AP.Instance.mls.LogInfo($"Connected to Archipelago server at {addr}:{port} as {slot}.");
                session.Items.ItemReceived += (helper) =>
                {   
                    if (ADOFAI_AP.Instance.Menu.lastItem == "None")
                    {
                        foreach (var item in helper.AllItemsReceived)
                        {
                            ADOFAI_AP.Instance.Menu.lastItem = item.ItemName;
                            ADOFAI_AP.Instance.ReceiveItem(ADOFAI_AP.Instance.Menu.lastItem);
                        }
                    }
                    
                    var lastItem = helper.AllItemsReceived[helper.Index - 1];
                    ADOFAI_AP.Instance.mls.LogInfo($"Received item: {lastItem.ItemName} (ID: {lastItem.ItemId})");
                    ADOFAI_AP.Instance.ReceiveItem(lastItem.ItemName);
                };

                // Handle death link
                DL = DeathLinkProvider.CreateDeathLinkService(session);
                DL.OnDeathLinkReceived += (deathLink) =>
                {
                    ADOFAI_AP.Instance.mls.LogInfo($"DeathLink received: {deathLink.Source} died at {deathLink.Cause}");

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
            else
            {
                ADOFAI_AP.Instance.mls.LogError($"Failed to connect to Archipelago server");
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
        }

    }
}

