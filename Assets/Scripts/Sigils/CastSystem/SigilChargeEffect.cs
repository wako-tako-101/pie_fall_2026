using System;
using System.Collections;
using SigilInteractions;
using UnityEngine;

namespace SigilCasting
{
    //particle visuals for the loaded sigil
    //shows a glowing ball while a sigil is loaded, flies it to the target on cast,
    //then sparkles around the target to show if the cast worked
    public class SigilChargeEffect : MonoBehaviour
    {
        [Serializable]
        public struct SigilColor
        {
            public SigilTags.SigilTag sigil;
            [ColorUsage(false, true)] public Color color;
        }

        [Header("References")]
        [Tooltip("The SigilCaster/ParticleGlow shader.")]
        [SerializeField] Shader particleShader;
        [Tooltip("Where the loaded ball sits. Uses this object if left empty.")]
        [SerializeField] Transform anchor;

        [Header("Loaded Ball")]
        [Tooltip("Radius of the ball of particles, in meters.")]
        [SerializeField, Min(0.001f)] float ballRadius = 0.045f;
        [SerializeField, Min(0.001f)] float ballParticleSize = 0.014f;
        [SerializeField, Min(0f)] float ballParticlesPerSecond = 70f;
        [Tooltip("Seconds each particle in the ball lasts.")]
        [SerializeField, Min(0.05f)] float ballParticleLifetime = 0.8f;
        [Tooltip("How fast the particles circle the ball.")]
        [SerializeField] float ballSwirlSpeed = 3f;

        [Header("Ball Colors")]
        [Tooltip("Color used for any sigil not listed below.")]
        [SerializeField, ColorUsage(false, true)] Color defaultColor = new Color(0.8f, 0.6f, 1f);
        [SerializeField] SigilColor[] sigilColors =
        {
            new SigilColor { sigil = SigilTags.SigilTag.Levitate, color = new Color(0.5f, 1f, 0.8f) },
            new SigilColor { sigil = SigilTags.SigilTag.Electricute, color = new Color(1f, 0.9f, 0.3f) },
        };

        [Header("Cast Flight")]
        [Tooltip("How fast the ball flies to its target, in meters per second.")]
        [SerializeField, Min(0.1f)] float travelSpeed = 8f;
        [Tooltip("Seconds the trail behind the flying ball lasts.")]
        [SerializeField, Min(0.05f)] float trailLifetime = 0.35f;

        [Header("Success Sparkle")]
        [SerializeField, ColorUsage(false, true)] Color successColor = new Color(1f, 0.9f, 0.5f);
        [SerializeField, Min(0)] int successParticles = 45;
        [SerializeField, Min(0.001f)] float successParticleSize = 0.035f;
        [SerializeField, Min(0.05f)] float successLifetime = 1f;
        [Tooltip("How fast the sparkles drift away from the object.")]
        [SerializeField, Min(0f)] float successSpeed = 0.5f;

        [Header("Failure Sparkle")]
        [SerializeField, ColorUsage(false, true)] Color failureColor = new Color(1f, 0.15f, 0.1f);
        [SerializeField, Min(0)] int failureParticles = 12;
        [SerializeField, Min(0.001f)] float failureParticleSize = 0.02f;
        [SerializeField, Min(0.05f)] float failureLifetime = 0.5f;
        [SerializeField, Min(0f)] float failureSpeed = 0.25f;

        [Header("Sparkle Placement")]
        [Tooltip("Extra room between the object and the ring of sparkles, in meters.")]
        [SerializeField, Min(0f)] float sparklePadding = 0.05f;
        [Tooltip("Sparkle radius used when the cast hits nothing.")]
        [SerializeField, Min(0.01f)] float missRadius = 0.15f;

        Material particleMaterial;
        ParticleSystem ball;

        Transform Anchor => anchor != null ? anchor : transform;

        void Awake()
        {
            if (particleShader == null) particleShader = Shader.Find("SigilCaster/ParticleGlow");
            if (particleShader == null)
            {
                Debug.LogError("SigilChargeEffect needs the SigilCaster/ParticleGlow shader assigned");
                return;
            }
            particleMaterial = new Material(particleShader);
        }
        void OnDestroy()
        {
            if (particleMaterial != null) Destroy(particleMaterial);
        }

        //shows the ball for the sigil that was just saved
        public void ShowLoaded(SigilTags.SigilTag sigil)
        {
            if (particleMaterial == null) return;
            if (ball == null) ball = CreateSystem("Loaded Sigil Ball", Anchor, true, ParticleSystemSimulationSpace.Local);

            ball.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ConfigureBall(ball, ColorFor(sigil), ballParticlesPerSecond, ballParticleLifetime);
            ball.Play();
        }
        //removes the ball without casting it
        public void Hide()
        {
            if (ball != null) ball.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        //flies the ball to the target, then calls resolve to apply the sigil
        //resolve returns true if the sigil worked on the target
        public void Launch(SigilTags.SigilTag sigil, Vector3 targetPoint, GameObject target, Func<bool> resolve)
        {
            Hide();
            if (particleMaterial == null)
            {
                resolve();
                return;
            }
            StartCoroutine(Fly(sigil, targetPoint, target, resolve));
        }

        IEnumerator Fly(SigilTags.SigilTag sigil, Vector3 targetPoint, GameObject target, Func<bool> resolve)
        {
            //world space so the particles are left behind as a trail
            ParticleSystem projectile = CreateSystem("Sigil Projectile", null, true, ParticleSystemSimulationSpace.World);
            projectile.transform.position = Anchor.position;
            ConfigureBall(projectile, ColorFor(sigil), ballParticlesPerSecond * 2f, trailLifetime);
            ParticleSystem.MainModule main = projectile.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            projectile.Play();

            //remember where on the target we hit, so the ball follows it if it moves
            Transform targetTransform = target != null ? target.transform : null;
            Vector3 localPoint = targetTransform != null ? targetTransform.InverseTransformPoint(targetPoint) : Vector3.zero;

            while (true)
            {
                if (targetTransform != null) targetPoint = targetTransform.TransformPoint(localPoint);

                Vector3 position = Vector3.MoveTowards(projectile.transform.position, targetPoint, travelSpeed * Time.deltaTime);
                projectile.transform.position = position;
                if ((position - targetPoint).sqrMagnitude < 0.0001f) break;
                yield return null;
            }

            //let the trail finish, the system destroys itself when it is empty
            projectile.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            bool success = resolve();

            Vector3 center = targetPoint;
            float radius = missRadius;
            if (target != null && target.TryGetComponent(out Collider targetCollider))
            {
                Bounds bounds = targetCollider.bounds;
                center = bounds.center;
                radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) + sparklePadding;
            }

            if (success) Sparkle(center, radius, successColor, successParticles, successParticleSize, successLifetime, successSpeed);
            else Sparkle(center, radius, failureColor, failureParticles, failureParticleSize, failureLifetime, failureSpeed);
        }
        //one burst of twinkling particles on a shell around a point
        void Sparkle(Vector3 center, float radius, Color color, int count, float size, float lifetime, float speed)
        {
            if (count <= 0) return;

            ParticleSystem sparkle = CreateSystem("Sigil Sparkle", null, false, ParticleSystemSimulationSpace.World);
            sparkle.transform.position = center;

            ParticleSystem.MainModule main = sparkle.main;
            main.duration = lifetime;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.5f, lifetime);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.4f, size);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f, speed);
            main.startColor = color;
            main.maxParticles = count;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = sparkle.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Min(count, short.MaxValue)) });

            ParticleSystem.ShapeModule shape = sparkle.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            //0 = spawn only on the surface of the sphere, so they ring the object
            shape.radiusThickness = 0f;

            Twinkle(sparkle);
            sparkle.Play();
        }
        void ConfigureBall(ParticleSystem system, Color color, float rate, float lifetime)
        {
            ParticleSystem.MainModule main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.5f, lifetime);
            main.startSize = new ParticleSystem.MinMaxCurve(ballParticleSize * 0.5f, ballParticleSize);
            main.startSpeed = 0f;
            main.startColor = color;
            main.maxParticles = Mathf.CeilToInt(rate * lifetime) + 16;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = rate;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = ballRadius;
            shape.radiusThickness = 1f;

            ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.orbitalY = ballSwirlSpeed;

            Twinkle(system);
        }
        //particles grow in, shrink out, and fade as they die
        void Twinkle(ParticleSystem system)
        {
            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0f));
            size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            ParticleSystem.ColorOverLifetimeModule fade = system.colorOverLifetime;
            fade.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            fade.color = new ParticleSystem.MinMaxGradient(gradient);
        }
        ParticleSystem CreateSystem(string systemName, Transform parent, bool loop, ParticleSystemSimulationSpace space)
        {
            GameObject systemObject = new GameObject(systemName);
            if (parent != null) systemObject.transform.SetParent(parent, false);

            ParticleSystem system = systemObject.AddComponent<ParticleSystem>();
            //a new system starts playing on its own, stop it so it can be set up first
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.playOnAwake = false;
            main.loop = loop;
            main.simulationSpace = space;

            ParticleSystemRenderer systemRenderer = systemObject.GetComponent<ParticleSystemRenderer>();
            systemRenderer.sharedMaterial = particleMaterial;
            systemRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            systemRenderer.receiveShadows = false;
            return system;
        }
        Color ColorFor(SigilTags.SigilTag sigil)
        {
            foreach (SigilColor entry in sigilColors)
                if (entry.sigil == sigil) return entry.color;
            return defaultColor;
        }
    }
}
