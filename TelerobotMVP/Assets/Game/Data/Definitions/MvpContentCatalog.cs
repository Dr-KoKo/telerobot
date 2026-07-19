using System;
using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class MvpContentCatalog : ScriptableObject
    {
        public string dataVersion;
        public GameConfigAsset game;
        public WeaponDefinitionAsset weapon;
        public GrenadeDefinitionAsset grenade;
        public BatteryConfigAsset battery;
        public RobotDefinitionAsset robot;
        public MedicalRobotDefinitionAsset medical;
        public BarrierConfigAsset barrier;
        public WarningConfigAsset warnings;
        public WorldLayoutAsset world;
        public CommandConfigAsset commands;
        public HudConfigAsset hud;
        public PlayerSettingsAsset playerSettings;
        public TelemetryConfigAsset telemetry;
        public ValidationConfigAsset validation;
        public SupplyPointDefinitionAsset[] supplyPoints;
        public ZombieDefinitionAsset[] zombies;
        public PhaseDefinitionAsset[] phases;
        public RouteDefinitionAsset[] routes;
        public UpgradeDefinitionAsset[] upgrades;
        public StringTableAsset strings;
        public Material runtimeMaterialTemplate;

        public ZombieDefinitionAsset Zombie(ZombieType type)
        {
            return Array.Find(zombies, item => item != null && item.type == type);
        }

        public PhaseDefinitionAsset Phase(int number)
        {
            return Array.Find(phases, item => item != null && item.number == number);
        }

        public RouteDefinitionAsset Route(RouteId id)
        {
            return Array.Find(routes, item => item != null && item.id == id);
        }
    }
}
