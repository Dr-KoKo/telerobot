using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class AmmoConfigAsset : ScriptableObject
    {
        public int startReserveAmmo;
        public int reserveAmmoMax;
        public ResupplyPolicy resupplyPolicy;
        public int resupplyAmount;
        public float resupplyUseSeconds;
        public float resupplyCooldownSeconds;
        public GrenadeResupplyPolicy grenadeResupplyPolicy;
    }
}
