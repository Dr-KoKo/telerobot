using System.Collections.Generic;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class VisualEffectFactory
    {
        private readonly PresentationMaterialLibrary materials;
        private readonly Queue<GameObject> activeEffects = new Queue<GameObject>();
        private const int AbsoluteMaximumEffects = 96;

        public VisualEffectFactory(PresentationMaterialLibrary materialLibrary)
        {
            materials = materialLibrary;
        }

        public int ActiveCount
        {
            get
            {
                TrimDestroyed();
                return activeEffects.Count;
            }
        }

        public GameObject Pulse(Vector3 position, float diameter, Color color, float lifetime, string name)
        {
            EnsureCapacity();

            var pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulse.name = name;
            pulse.transform.position = position;
            pulse.transform.localScale = Vector3.one * Mathf.Max(0.02f, diameter);
            var collider = pulse.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }
            materials.ApplyAccent(pulse.GetComponent<Renderer>(), "ally.energy", color);
            var lifetimeComponent = pulse.AddComponent<PresentationEffectLifetime>();
            lifetimeComponent.Initialize(Mathf.Max(0.02f, lifetime), true);
            activeEffects.Enqueue(pulse);
            return pulse;
        }

        public GameObject TelegraphLine(Vector3 start, Vector3 end, Color color, float lifetime, string name)
        {
            EnsureCapacity();
            var root = new GameObject(name);
            var line = root.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = 0.08f;
            line.endWidth = 0.02f;
            line.sharedMaterial = materials.Get("enemy.ripper");
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0f);
            root.AddComponent<PresentationEffectLifetime>().Initialize(Mathf.Max(0.02f, lifetime), false);
            activeEffects.Enqueue(root);
            return root;
        }

        private void TrimDestroyed()
        {
            if (activeEffects.Count == 0) return;
            var live = new Queue<GameObject>(activeEffects.Count);
            while (activeEffects.Count > 0)
            {
                var effect = activeEffects.Dequeue();
                if (effect != null) live.Enqueue(effect);
            }
            while (live.Count > 0) activeEffects.Enqueue(live.Dequeue());
        }

        private void EnsureCapacity()
        {
            TrimDestroyed();
            while (activeEffects.Count >= AbsoluteMaximumEffects)
            {
                var oldest = activeEffects.Dequeue();
                if (oldest != null) UnityEngine.Object.Destroy(oldest);
            }
        }
    }

    public sealed class PresentationEffectLifetime : MonoBehaviour
    {
        private float duration;
        private float elapsed;
        private Vector3 startScale;
        private bool shrink;

        public void Initialize(float seconds, bool shrinkOverTime)
        {
            duration = seconds;
            shrink = shrinkOverTime;
            startScale = transform.localScale;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (shrink) transform.localScale = Vector3.Lerp(startScale, Vector3.zero, Mathf.Clamp01(elapsed / duration));
            if (elapsed >= duration) Destroy(gameObject);
        }
    }
}
