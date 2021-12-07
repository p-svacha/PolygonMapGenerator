using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public static class GlobalSettings
    {
        public static bool DebugMode;

        public static void Update(bool debugMode)
        {
            DebugMode = debugMode;
        }
    }
}
