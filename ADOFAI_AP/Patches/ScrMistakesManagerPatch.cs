using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ADOFAI_AP.Patches
{
    [HarmonyPatch(typeof(scrMistakesManager))]
    internal class ScrMistakesManagerPatch
    {
        [HarmonyPatch("SaveProgress")]
        [HarmonyPrefix]
        public static bool PatchSaveProgress()
        {
            if (ADOFAI_AP.Instance.client.session != null)
            {
                string saveFolder = Path.Combine(Paths.ConfigPath, "ArchipelagoSessions");
                Directory.CreateDirectory(saveFolder);
                string sessionSeed = ADOFAI_AP.Instance.client.session.RoomState.Seed;
                string filePath = Path.Combine(saveFolder, $"{sessionSeed}.dat");
                ADOFAI_AP.Instance.mls.LogInfo($"save on seed:{sessionSeed}");
                string json = JsonConvert.SerializeObject(Persistence.GetSavedProgress(), Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            return true;
        }
    }
}
