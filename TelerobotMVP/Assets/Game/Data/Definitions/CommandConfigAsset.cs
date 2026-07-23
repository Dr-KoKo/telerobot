using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class CommandConfigAsset : ScriptableObject
    {
        public RobotCommand[] commands;
    }
}
