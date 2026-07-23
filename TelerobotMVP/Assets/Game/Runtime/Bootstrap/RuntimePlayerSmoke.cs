using System;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    internal static class RuntimePlayerSmoke
    {
        private const string SmokeArgument = "-telerobot-smoke";
        private const string ReadyMarker = "TELEROBOT_STANDALONE_SMOKE_READY";

        public static bool IsRequested
        {
            get
            {
                foreach (var argument in Environment.GetCommandLineArgs())
                    if (string.Equals(argument, SmokeArgument, StringComparison.OrdinalIgnoreCase)) return true;
                return false;
            }
        }

        public static void MarkGameplayReadyAndQuit()
        {
            if (!IsRequested) return;
            Debug.Log(ReadyMarker);
            Application.Quit(0);
        }
    }
}
