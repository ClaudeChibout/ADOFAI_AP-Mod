using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace ADOFAI_AP.Patches
{
    [HarmonyPatch(typeof(PauseLevel))]
    internal class PauseLevelPatch
    {

        [HarmonyPatch("InstantiantePauseLevels")]
        [HarmonyPrefix]
        public static bool InstantiantePauseLevels()
        {
            if (ADOFAI_AP.Instance.client.session == null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
