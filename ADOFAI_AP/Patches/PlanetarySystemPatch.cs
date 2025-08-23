using HarmonyLib;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOFAI_AP.Patches
{
    [HarmonyPatch(typeof(PlanetarySystem))]
    internal class PlanetarySystemPatch
    {
        [HarmonyPatch("Die")]
        [HarmonyPrefix]
        static bool PatchDie()
        {   
            if (ADOFAI_AP.Instance.client.session != null)
            {
                ADOFAI_AP.Instance.client?.DL?.SendDeathLink(new DeathLink(ADOFAI_AP.Instance.Menu.pseudo, null));
                Notification.Instance.CreateNotification($"You're dead, and your friends too !");
                //Notification.Instance.CreateNotification($"");
            }
            return true; // Continue with the original method
        }
    }
}
