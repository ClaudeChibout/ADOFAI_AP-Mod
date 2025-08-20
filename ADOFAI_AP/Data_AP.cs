using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOFAI_AP.Patches
{
    internal class Data_AP
    {
        public static Dictionary<string, bool> ItemsReceived = new Dictionary<string, bool>()
        {
            { "Key_Level_1-X", true },
            { "Key_Level_1-1", true },
            { "Key_Level_1-2", true },
            { "Key_Level_1-3", true },
            { "Key_Level_1-4", true },
            { "Key_Level_1-5", true },
            { "Key_Level_1-6", true },

        };

        public static Dictionary<string, bool> LocationsChecked = new Dictionary<string, bool>
        {
            { "1-X", false },
            { "1-1", false },
            { "1-2", false },
            { "1-3", false },
            { "1-4", false },
            { "1-5", false },
            { "1-6", false },
        };


        public static Dictionary<string, bool> MainWorlds = new Dictionary<string, bool>()
        {
            // { "1-X", true },
            { "2-X", false },
            { "3-X", false },
            { "4-X", false },
            { "5-X", false },
            { "6-X", false },
            { "7-X", false },
            { "8-X", false },
            { "9-X", false },
            { "10-X", false },
            { "11-X", false },
            { "12-X", false }
        };

        public static Dictionary<string, bool> MainWorldsKeys = new Dictionary<string, bool>()
        {
            // { "Key_Level_1-X", true },
            { "Key_Level_2-X", false },
            { "Key_Level_3-X", false },
            { "Key_Level_4-X", false },
            { "Key_Level_5-X", false },
            { "Key_Level_6-X", false },
            { "Key_Level_7-X", false },
            { "Key_Level_8-X", false },
            { "Key_Level_9-X", false },
            { "Key_Level_10-X", false },
            { "Key_Level_11-X", false },
            { "Key_Level_12-X", false }
        };

        public static Dictionary<string, bool> MainWorldsTuto = new Dictionary<string, bool>()
        {
            // { "1-1", true },
            // { "1-2", true },
            // { "1-3", true },
            // { "1-4", true },
            // { "1-5", true },
            // { "1-6", true },

            { "2-1", false },
            { "2-2", false },
            { "2-3", false },

            { "3-1", false },
            { "3-2", false },
            { "3-3", false },
            { "3-4", false },
            { "3-5", false },
            { "3-6", false },

            { "4-1", false },
            { "4-2", false },
            { "4-3", false },
            { "4-4", false },
            { "4-5", false },

            { "5-1", false },
            { "5-2", false },
            { "5-3", false },
            { "5-4", false },

            { "6-1", false },
            { "6-2", false },
            { "6-3", false },
            { "6-4", false },

            { "7-1", false },
            { "7-2", false },
            { "7-3", false },
            { "7-4", false },
            { "7-5", false },

            { "8-1", false },
            { "8-2", false },
            { "8-3", false },
            { "8-4", false },
            { "8-5", false },
            { "8-6", false },
            { "8-7", false },
            { "8-8", false },

            { "9-1", false },
            { "9-2", false },
            { "9-3", false },
            { "9-4", false },
            { "9-5", false },

            { "10-1", false },
            { "10-2", false },
            { "10-3", false },
            { "10-4", false },
            { "10-5", false },
            { "10-6", false },
            { "10-7", false },
            { "10-8", false },

            { "11-1", false },
            { "11-2", false },
            { "11-3", false },
            { "11-4", false },
            { "11-5", false },
            { "11-6", false },

            { "12-1", false },
            { "12-2", false },
            { "12-3", false },
            { "12-4", false },
            { "12-5", false },
            { "12-6", false }
        };

        public static Dictionary<string, bool> MainWorldsTutoKeys = new Dictionary<string, bool>()
        {
            // { "Key_Level_1-1", true },
            // { "Key_Level_1-2", true },
            // { "Key_Level_1-3", true },
            // { "Key_Level_1-4", true },
            // { "Key_Level_1-5", true },
            // { "Key_Level_1-6", true },

            { "Key_Level_2-1", false },
            { "Key_Level_2-2", false },
            { "Key_Level_2-3", false },

            { "Key_Level_3-1", false },
            { "Key_Level_3-2", false },
            { "Key_Level_3-3", false },
            { "Key_Level_3-4", false },
            { "Key_Level_3-5", false },
            { "Key_Level_3-6", false },

            { "Key_Level_4-1", false },
            { "Key_Level_4-2", false },
            { "Key_Level_4-3", false },
            { "Key_Level_4-4", false },
            { "Key_Level_4-5", false },

            { "Key_Level_5-1", false },
            { "Key_Level_5-2", false },
            { "Key_Level_5-3", false },
            { "Key_Level_5-4", false },

            { "Key_Level_6-1", false },
            { "Key_Level_6-2", false },
            { "Key_Level_6-3", false },
            { "Key_Level_6-4", false },

            { "Key_Level_7-1", false },
            { "Key_Level_7-2", false },
            { "Key_Level_7-3", false },
            { "Key_Level_7-4", false },
            { "Key_Level_7-5", false },

            { "Key_Level_8-1", false },
            { "Key_Level_8-2", false },
            { "Key_Level_8-3", false },
            { "Key_Level_8-4", false },
            { "Key_Level_8-5", false },
            { "Key_Level_8-6", false },
            { "Key_Level_8-7", false },
            { "Key_Level_8-8", false },

            { "Key_Level_9-1", false },
            { "Key_Level_9-2", false },
            { "Key_Level_9-3", false },
            { "Key_Level_9-4", false },
            { "Key_Level_9-5", false },

            { "Key_Level_10-1", false },
            { "Key_Level_10-2", false },
            { "Key_Level_10-3", false },
            { "Key_Level_10-4", false },
            { "Key_Level_10-5", false },
            { "Key_Level_10-6", false },
            { "Key_Level_10-7", false },
            { "Key_Level_10-8", false },

            { "Key_Level_11-1", false },
            { "Key_Level_11-2", false },
            { "Key_Level_11-3", false },
            { "Key_Level_11-4", false },
            { "Key_Level_11-5", false },
            { "Key_Level_11-6", false },

            { "Key_Level_12-1", false },
            { "Key_Level_12-2", false },
            { "Key_Level_12-3", false },
            { "Key_Level_12-4", false },
            { "Key_Level_12-5", false },
            { "Key_Level_12-6", false }
        };

        public static Dictionary<string, bool> XtraWorlds = new Dictionary<string, bool>
        {
            {"XS-X", false},
            {"PA-X", false},
            {"XH-X", false},
            {"XC-X", false},
            {"XF-X", false},
            {"XR-X", false},
            {"RJ-X", false},
            {"XN-X", false},
            {"XM-X", false}
        };

        public static Dictionary<string, bool> XtraWorldsKeys = new Dictionary<string, bool>
        {
            {"Key_Level_XS-X", false},
            {"Key_Level_PA-X", false},
            {"Key_Level_XH-X", false},
            {"Key_Level_XC-X", false},
            {"Key_Level_XF-X", false},
            {"Key_Level_XR-X", false},
            {"Key_Level_RJ-X", false},
            {"Key_Level_XN-X", false},
            {"Key_Level_XM-X", false}
        };

        public static Dictionary<string, bool> XtraTuto = new Dictionary<string, bool>
        {
            {"XS-1", false},
            {"XS-2", false},
            {"XS-3", false},
            {"XS-4", false},
            {"XS-5", false},
            {"XS-6", false},
            {"XS-7", false},
            {"XS-8", false},
        
            {"PA-1", false},
        
            {"XH-1", false},
            {"XH-2", false},
            {"XH-3", false},
        
            {"XC-1", false},
            {"XC-2", false},
            {"XC-3", false},
            {"XC-4", false},
            {"XC-5", false},
        
            {"XF-1", false},
            {"XF-2", false},
            {"XF-3", false},
        
            {"XR-1", false},
            {"XR-2", false},
            {"XR-3", false},
        
            {"RJ-1", false},
            {"RJ-2", false},
            {"RJ-3", false},
            {"RJ-4", false},
            {"RJ-5", false},
            {"RJ-6", false},
            {"RJ-7", false},
        
            {"XN-1", false},
            {"XN-2", false},
            {"XN-3", false},
        
            {"XM-1", false},
            {"XM-2", false},
            {"XM-3", false},
            {"XM-4", false}
        };

        public static Dictionary<string, bool> XtraTutoKeys = new Dictionary<string, bool>
        {
            {"Key_Level_XS-1", false},
            {"Key_Level_XS-2", false},
            {"Key_Level_XS-3", false},
            {"Key_Level_XS-4", false},
            {"Key_Level_XS-5", false},
            {"Key_Level_XS-6", false},
            {"Key_Level_XS-7", false},
            {"Key_Level_XS-8", false},
        
            {"Key_Level_PA-1", false},
        
            {"Key_Level_XH-1", false},
            {"Key_Level_XH-2", false},
            {"Key_Level_XH-3", false},
        
            {"Key_Level_XC-1", false},
            {"Key_Level_XC-2", false},
            {"Key_Level_XC-3", false},
            {"Key_Level_XC-4", false},
            {"Key_Level_XC-5", false},
        
            {"Key_Level_XF-1", false},
            {"Key_Level_XF-2", false},
            {"Key_Level_XF-3", false},
        
            {"Key_Level_XR-1", false},
            {"Key_Level_XR-2", false},
            {"Key_Level_XR-3", false},
        
            {"Key_Level_RJ-1", false},
            {"Key_Level_RJ-2", false},
            {"Key_Level_RJ-3", false},
            {"Key_Level_RJ-4", false},
            {"Key_Level_RJ-5", false},
            {"Key_Level_RJ-6", false},
            {"Key_Level_RJ-7", false},
        
            {"Key_Level_XN-1", false},
            {"Key_Level_XN-2", false},
            {"Key_Level_XN-3", false},
        
            {"Key_Level_XM-1", false},
            {"Key_Level_XM-2", false},
            {"Key_Level_XM-3", false},
            {"Key_Level_XM-4", false}
        };

    }
}
