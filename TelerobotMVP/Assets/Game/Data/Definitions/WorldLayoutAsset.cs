using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class WorldLayoutAsset : ScriptableObject
    {
        public Vector3 basePosition;
        public Vector3 playerStart;
        public Vector3[] robotStarts;
        public Vector3 baseRally;
        public Vector3 chargingStation;
        public Vector3 safeSupply;
        public Vector3 riskySupply;
        public Vector3 medicalAnchor;
        public float supplyInteractionRadius;
        public float supplyExitTolerance;
        public float baseChargingRadius;
        public float chargingArrivalRadius;
    }
}
