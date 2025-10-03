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
                // Si DeathLink est désactivé dans le menu, on ne fait rien.
                if (ADOFAI_AP.Instance.client.DeathLinkMod_Disable) return true;

                ++ADOFAI_AP.Instance.client.SessionDeathCount;
                var count = ++ADOFAI_AP.Instance.client.DeathCount % ADOFAI_AP.Instance.client.DeathLinkMod_CountBeforeDeath;
                if (count == 0)
                {
                    ADOFAI_AP.Instance.client?.DL?.SendDeathLink(new DeathLink(ADOFAI_AP.Instance.Menu.pseudo, null));
                    Notification.Instance.CreateNotification($"You're dead, and your friends too !");
                    ADOFAI_AP.Instance.mls.LogInfo($"DeathLink sent, you have died {ADOFAI_AP.Instance.client.SessionDeathCount} times in this session.");
                }
                else
                {
                    ADOFAI_AP.Instance.mls.LogInfo($"You have died {ADOFAI_AP.Instance.client.SessionDeathCount} times in this session. You need to die {ADOFAI_AP.Instance.client.DeathLinkMod_CountBeforeDeath - count} more time(s) to send a DeathLink.");
                    Notification.Instance.CreateNotification($"You need to die {ADOFAI_AP.Instance.client.DeathLinkMod_CountBeforeDeath - count} more time(s) to send a DeathLink.");
                }

            }
            return true; // Continue with the original method
        }
    }
}
