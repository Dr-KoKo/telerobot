using System.Collections.Generic;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonPlayerController : MonoBehaviour
    {
        private MvpGameController game;
        private WeaponConfig weapon;
        private CharacterController character;
        private Camera viewCamera;
        private IPlayerInput input;
        private GameRulesConfig gameRules;
        private WeaponDefinitionAsset weaponPresentation;
        private Renderer playerRenderer;
        private AudioSource combatAudio;
        private AudioClip fireSound;
        private AudioClip bodyHitSound;
        private AudioClip headshotSound;
        private IDeterministicRng recoilRng;
        private float yaw;
        private float pitch = 5f;
        private float recoilPitchOffset;
        private float recoilYawOffset;
        private float fireCooldownRemaining;
        private float verticalVelocity;
        private int shotSequence;
        private bool jumpRequestedForTests;

        public PlayerState State { get; private set; }
        public Camera ViewCamera { get { return viewCamera; } }
        public CameraPerspective Perspective { get; private set; }
        public float VerticalVelocity { get { return verticalVelocity; } }
        public bool IsGrounded { get { return character != null && character.isGrounded; } }
        public bool IsBodyVisible
        {
            get
            {
                if (Perspective != CameraPerspective.ThirdPerson) return false;
                var presentationRoot = transform.Find(LowPolyModelFactory.VisualRootName);
                if (presentationRoot == null) return playerRenderer != null && playerRenderer.enabled;
                foreach (var renderer in presentationRoot.GetComponentsInChildren<Renderer>(true))
                    if (renderer.enabled) return true;
                return false;
            }
        }
        public float CurrentPlanarSpeed { get; private set; }
        public float RecoilMagnitude { get { return Mathf.Abs(recoilPitchOffset) + Mathf.Abs(recoilYawOffset); } }
        public bool HasCombatAudio { get { return fireSound != null && bodyHitSound != null && headshotSound != null; } }
        public int ShotFeedbackCount { get; private set; }
        public int HitSoundCount { get; private set; }
        public HitRegion LastHitSoundRegion { get; private set; }
        public GameObject LastMuzzleFlash { get; private set; }
        public GameObject LastImpactEffect { get; private set; }
        public CharacterController CharacterForTests { get { return character; } }
        public float LastRecoilPitchDegrees { get; private set; }
        public float LastRecoilYawDegrees { get; private set; }

        public void Initialize(MvpGameController owner, PlayerState state, WeaponConfig weaponConfig, IPlayerInput inputSource)
        {
            game = owner;
            State = state;
            weapon = weaponConfig;
            input = inputSource;
            gameRules = owner.Config.Game;
            weaponPresentation = owner.Catalog.weapon;
            recoilRng = new XorShiftRng(owner.Session.Seed ^ unchecked((int)0xA341316C));
            character = GetComponent<CharacterController>();
            character.height = 1.8f;
            character.radius = 0.4f;
            character.center = Vector3.zero;
            playerRenderer = GetComponent<Renderer>();
            combatAudio = gameObject.AddComponent<AudioSource>();
            combatAudio.playOnAwake = false;
            combatAudio.spatialBlend = 0f;
            fireSound = ProceduralCombatAudio.CreateTransient("Rifle Fire", weaponPresentation.fireSoundFrequency,
                weaponPresentation.combatSoundSeconds, 0.72f);
            bodyHitSound = ProceduralCombatAudio.CreateTransient("Body Hit", weaponPresentation.bodyHitSoundFrequency,
                weaponPresentation.combatSoundSeconds, 0.18f);
            headshotSound = ProceduralCombatAudio.CreateTransient("Headshot Hit", weaponPresentation.headshotSoundFrequency,
                weaponPresentation.combatSoundSeconds, 0.08f);

            var cameraObject = new GameObject("Player View Camera");
            viewCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            yaw = transform.eulerAngles.y;
            SetPerspective(PlayerPreferences.IsInitialized
                ? PlayerPreferences.DefaultPerspective : CameraPerspective.ThirdPerson, false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (game == null || State == null) return;
            CombatRules.TickReload(State.Ammo, Time.deltaTime);
            var frame = input == null ? default : input.ReadFrame();
            if (frame.PausePressed)
            {
                game.TogglePause();
                return;
            }
            if (frame.SpecializationPressed) game.ToggleSpecializationPanel();
            if (game.IsPaused) return;
            UpdateRecoil();
            if (frame.TogglePerspectivePressed && !game.InputBlocked) TogglePerspective();
            UpdateCamera(frame);
            if (game.InputBlocked) return;
            UpdateMovement(frame);
            UpdateCombat(frame);
        }

        private void LateUpdate()
        {
            if (viewCamera == null) return;
            var rotation = CameraRotation();
            if (Perspective == CameraPerspective.FirstPerson)
            {
                viewCamera.transform.SetPositionAndRotation(FirstPersonPosition(), rotation);
                return;
            }
            var target = transform.position + Vector3.up * 0.55f;
            var desired = ResolveThirdPersonPosition(target, rotation);
            viewCamera.transform.position = Vector3.Lerp(viewCamera.transform.position, desired, 16f * Time.deltaTime);
            viewCamera.transform.rotation = rotation;
        }

        private Vector3 ResolveThirdPersonPosition(Vector3 target, Quaternion rotation)
        {
            var backward = -(rotation * Vector3.forward);
            var distance = gameRules.CameraDistance;
            Physics.SyncTransforms();
            var hits = Physics.SphereCastAll(target, gameRules.CameraCollisionRadius, backward,
                distance, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (IsOwnCollider(hit.collider)) continue;
                distance = Mathf.Max(0.25f, hit.distance - gameRules.CameraCollisionPadding);
                break;
            }
            return target + backward * distance;
        }

        private Vector3 FirstPersonPosition()
        {
            return transform.position + Vector3.up * gameRules.FirstPersonEyeHeight;
        }

        private Quaternion CameraRotation()
        {
            return Quaternion.Euler(pitch + recoilPitchOffset, yaw + recoilYawOffset, 0f);
        }

        private void UpdateRecoil()
        {
            var recovery = weaponPresentation.recoilRecoveryDegreesPerSecond * Time.deltaTime;
            recoilPitchOffset = Mathf.MoveTowards(recoilPitchOffset, 0f, recovery);
            recoilYawOffset = Mathf.MoveTowards(recoilYawOffset, 0f, recovery);
        }

        private void UpdateCamera(PlayerInputFrame frame)
        {
            if (game.MenuConsumesPointer) return;
            var sensitivity = PlayerPreferences.IsInitialized
                ? PlayerPreferences.MouseSensitivity : gameRules.MouseSensitivity;
            yaw += frame.Look.X * sensitivity;
            pitch = Mathf.Clamp(pitch - frame.Look.Y * sensitivity, -15f, 65f);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void UpdateMovement(PlayerInputFrame frame)
        {
            var movement = Vector2.ClampMagnitude(new Vector2(frame.Move.X, frame.Move.Y), 1f);

            var forward = viewCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            var right = viewCamera.transform.right;
            right.y = 0f;
            right.Normalize();
            var planar = forward * movement.y + right * movement.x;
            var moveSpeed = gameRules.PlayerMoveSpeed * (frame.SprintHeld ? gameRules.SprintMultiplier : 1f);
            CurrentPlanarSpeed = planar.sqrMagnitude > 0.0001f ? moveSpeed : 0f;
            if (character.isGrounded)
            {
                verticalVelocity = gameRules.GroundedVelocity;
                if (frame.JumpPressed || jumpRequestedForTests)
                {
                    verticalVelocity = Mathf.Sqrt(2f * gameRules.Gravity * gameRules.JumpHeight);
                    game.Emit("player_jumped", "height", gameRules.JumpHeight.ToString("F2"));
                }
            }
            else
            {
                verticalVelocity -= gameRules.Gravity * Time.deltaTime;
            }
            jumpRequestedForTests = false;
            character.Move((planar * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void UpdateCombat(PlayerInputFrame frame)
        {
            fireCooldownRemaining = Mathf.Max(0f, fireCooldownRemaining - Time.deltaTime);
            if ((frame.FireHeld || frame.FirePressed) && fireCooldownRemaining <= 0f && Shoot())
                fireCooldownRemaining = weapon.FireIntervalSeconds;
            if (frame.ReloadPressed) CombatRules.BeginReload(State.Ammo, weapon.ReloadSeconds);
            if (frame.GrenadePressed) ThrowGrenade();
            if (frame.InteractPressed) game.TryResupply(transform.position);
        }

        private bool Shoot()
        {
            if (!CombatRules.TryFire(State.Ammo)) return false;
            TriggerShotFeedback();
            var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            Physics.SyncTransforms();
            var hits = Physics.RaycastAll(ray, weapon.Range, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            if (!game.Modifiers.PiercingRounds)
            {
                foreach (var hit in hits)
                {
                    if (IsOwnCollider(hit.collider)) continue;
                    ApplyBullet(hit);
                    break;
                }
                return true;
            }

            var zombieHits = 0;
            foreach (var hit in hits)
            {
                if (IsOwnCollider(hit.collider)) continue;
                var zombie = hit.collider.GetComponentInParent<ZombieActor>();
                if (zombie == null)
                {
                    SpawnImpact(hit.point, HitRegion.Body, false);
                    break;
                }
                if (zombieHits > 0 && zombie.Type != ZombieType.Runner) break;
                ApplyBullet(hit);
                zombieHits++;
                if (zombieHits >= 2) break;
            }
            return true;
        }

        private void TriggerShotFeedback()
        {
            shotSequence++;
            if (recoilRng == null) recoilRng = new XorShiftRng(shotSequence);
            LastRecoilPitchDegrees = Mathf.Lerp(weaponPresentation.recoilPitchMinimumDegrees,
                weaponPresentation.recoilPitchMaximumDegrees, recoilRng.NextFloat());
            LastRecoilYawDegrees = (recoilRng.NextFloat() * 2f - 1f) *
                weaponPresentation.recoilYawMaximumDegrees;
            recoilPitchOffset -= LastRecoilPitchDegrees;
            recoilYawOffset += LastRecoilYawDegrees;
            if (combatAudio != null && fireSound != null)
                combatAudio.PlayOneShot(fireSound, weaponPresentation.fireSoundVolume * PlayerPreferences.EffectsVolume);
            var muzzlePosition = viewCamera.transform.position + viewCamera.transform.forward * 0.62f;
            LastMuzzleFlash = game.SpawnPulse(muzzlePosition, weaponPresentation.muzzleFlashSize,
                new Color(1f, 0.68f, 0.12f, 0.9f), weaponPresentation.muzzleFlashSeconds, "Muzzle Flash", 2.2f);
            ShotFeedbackCount++;
        }

        private bool IsOwnCollider(Collider candidate)
        {
            return candidate != null && (candidate.transform == transform || candidate.transform.IsChildOf(transform));
        }

        private void ApplyBullet(RaycastHit hit)
        {
            var zombie = hit.collider.GetComponentInParent<ZombieActor>();
            if (zombie == null)
            {
                SpawnImpact(hit.point, HitRegion.Body, false);
                return;
            }
            var headLine = zombie.transform.position.y + zombie.VisualHeight * 0.28f;
            var region = hit.point.y >= headLine ? HitRegion.Head : HitRegion.Body;
            var damage = CombatRules.CalculateBulletDamage(weapon, region);
            SpawnImpact(hit.point, region, true);
            zombie.ReceiveDamage(damage, DamageSource.Player("player"));
            PlayHitSound(region);
            game.NotifyPlayerHit(region, damage, zombie.State.Health.IsDead);
        }

        private void SpawnImpact(Vector3 position, HitRegion region, bool zombieHit)
        {
            var color = !zombieHit ? new Color(0.7f, 0.78f, 0.85f, 0.8f) :
                region == HitRegion.Head ? new Color(1f, 0.18f, 0.04f, 0.9f) :
                new Color(1f, 0.82f, 0.24f, 0.85f);
            LastImpactEffect = game.SpawnPulse(position, weaponPresentation.impactPulseSize, color,
                weaponPresentation.muzzleFlashSeconds * 1.8f, "Bullet Impact");
        }

        private void PlayHitSound(HitRegion region)
        {
            if (combatAudio != null)
            {
                var clip = region == HitRegion.Head ? headshotSound : bodyHitSound;
                if (clip != null) combatAudio.PlayOneShot(clip,
                    weaponPresentation.hitSoundVolume * PlayerPreferences.EffectsVolume);
            }
            LastHitSoundRegion = region;
            HitSoundCount++;
        }

        private void ThrowGrenade()
        {
            if (State.Grenades <= 0) return;
            State.Grenades--;
            var center = transform.position + viewCamera.transform.forward * game.Config.Grenade.ThrowDistance;
            center.y = 0.5f;
            var candidates = new List<GrenadeTarget>();
            var lookup = new Dictionary<string, ZombieActor>();
            foreach (var zombie in game.AliveZombies)
            {
                var distance = Vector3.Distance(center, zombie.transform.position);
                candidates.Add(new GrenadeTarget(zombie.State.Id, distance, zombie.State.Health));
                lookup[zombie.State.Id] = zombie;
            }
            var affected = CombatRules.ApplyGrenade(game.Config.Grenade, candidates);
            foreach (var id in affected) lookup[id].RefreshAfterCoreDamage(DamageSource.Player("player"));
            game.Emit("grenade_used", new Dictionary<string, string>
            {
                { "affectedCount", affected.Count.ToString() },
                { "center", center.ToString("F1") }
            });
            game.SpawnPulse(center, game.Config.Grenade.Radius, new Color(1f, 0.45f, 0.05f, 0.55f));
        }

        public void ReceiveDamage(float amount)
        {
            ReceiveDamage(amount, transform.position + transform.forward);
        }

        public void ReceiveDamage(float amount, Vector3 sourcePosition)
        {
            var applied = CombatRules.ApplyDamage(State.Health, amount);
            var toSource = sourcePosition - transform.position;
            toSource.y = 0f;
            var directionAngle = toSource.sqrMagnitude < 0.0001f
                ? 0f : Vector3.SignedAngle(transform.forward, toSource.normalized, Vector3.up);
            game.Emit("player_damaged", new Dictionary<string, string>
            {
                { "amount", applied.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) },
                { "directionAngle", directionAngle.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) }
            });
            if (!State.Health.IsDead) return;
            game.Emit("player_died");
            game.CheckImmediateDefeat();
        }

        public void FireForTests()
        {
            Shoot();
        }

        public void TogglePerspective()
        {
            var next = Perspective == CameraPerspective.ThirdPerson
                ? CameraPerspective.FirstPerson : CameraPerspective.ThirdPerson;
            SetPerspective(next, true);
        }

        private void SetPerspective(CameraPerspective value, bool emitTelemetry)
        {
            Perspective = value;
            if (viewCamera != null)
                viewCamera.fieldOfView = value == CameraPerspective.FirstPerson
                    ? gameRules.FirstPersonFieldOfView : gameRules.ThirdPersonFieldOfView;
            RefreshBodyVisibility();
            SnapCameraForTests();
            if (emitTelemetry && game != null)
                game.Emit("camera_perspective_changed", "perspective", value.ToString());
        }

        public void RefreshBodyVisibility()
        {
            var bodyVisible = Perspective == CameraPerspective.ThirdPerson;
            var presentationRoot = transform.Find(LowPolyModelFactory.VisualRootName);
            if (playerRenderer != null) playerRenderer.enabled = bodyVisible && presentationRoot == null;
            if (presentationRoot == null) return;
            foreach (var renderer in presentationRoot.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = bodyVisible;
        }

        public void RequestJumpForTests()
        {
            jumpRequestedForTests = true;
        }

        public void SetInputForTests(IPlayerInput inputSource)
        {
            input = inputSource;
        }

        public void SnapCameraForTests()
        {
            if (viewCamera == null || gameRules == null) return;
            var rotation = CameraRotation();
            if (Perspective == CameraPerspective.FirstPerson)
            {
                viewCamera.transform.SetPositionAndRotation(FirstPersonPosition(), rotation);
                return;
            }
            var target = transform.position + Vector3.up * 0.55f;
            viewCamera.transform.position = ResolveThirdPersonPosition(target, rotation);
            viewCamera.transform.rotation = rotation;
        }

        private void OnDestroy()
        {
            if (fireSound != null) Destroy(fireSound);
            if (bodyHitSound != null) Destroy(bodyHitSound);
            if (headshotSound != null) Destroy(headshotSound);
        }
    }
}
