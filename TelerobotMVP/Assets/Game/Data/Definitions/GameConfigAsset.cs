using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class GameConfigAsset : ScriptableObject
    {
        public float playerMaxHealth;
        public float baseMaxHealth;
        [Range(0f, 1f)] public float baseRecoveryFraction;
        [Range(0f, 1f)] public float baseWarningFraction;
        public float fixedStepSeconds;
        public float playerMoveSpeed;
        public float sprintMultiplier;
        public float gravity;
        public float mouseSensitivity;
        public float cameraDistance;
        public float thirdPersonFieldOfView;
        public float firstPersonFieldOfView;
        public float firstPersonEyeHeight;
        public float cameraCollisionRadius;
        public float cameraCollisionPadding;
        public float jumpHeight;
        public float groundedVelocity;
        public float minimumSpawnInterval;
    }
}
