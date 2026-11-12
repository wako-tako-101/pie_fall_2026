using UnityEngine;

namespace SigilCasting
{
    //glowing orb that sits at the tip of the controller
    //place this object where the orb should be, sigils are drawn and cast from it
    public class WandOrb : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The drawer this orb reacts to. The orb changes color while it is drawing.")]
        [SerializeField] SigilDrawer drawer;
        [Tooltip("The SigilCaster/OrbGlow shader.")]
        [SerializeField] Shader orbShader;

        [Header("Size")]
        [Tooltip("Diameter of the bright inner ball, in meters.")]
        [SerializeField, Min(0.001f)] float coreDiameter = 0.035f;
        [Tooltip("Diameter of the soft glow around the ball, in meters.")]
        [SerializeField, Min(0.001f)] float haloDiameter = 0.09f;

        [Header("Colors")]
        [SerializeField, ColorUsage(false, true)] Color idleColor = new Color(0.4f, 0.75f, 1f);
        [Tooltip("Color while the trigger is held and a sigil is being drawn.")]
        [SerializeField, ColorUsage(false, true)] Color drawingColor = new Color(1f, 0.7f, 0.3f);
        [Tooltip("How fast the orb blends between the two colors. Higher is snappier.")]
        [SerializeField, Min(0.01f)] float colorChangeSpeed = 8f;

        [Header("Glow")]
        [Tooltip("How see-through the inner ball is. 0 = invisible, 1 = solid light.")]
        [SerializeField, Range(0f, 1f)] float coreOpacity = 0.9f;
        [SerializeField, Min(0f)] float coreBrightness = 1.6f;
        [Tooltip("How far the center of the ball is pushed toward white.")]
        [SerializeField, Range(0f, 1f)] float whiteCore = 0.6f;
        [Tooltip("How quickly the inner ball fades toward its edge. Higher = smaller, tighter ball.")]
        [SerializeField, Range(0.1f, 8f)] float coreFalloff = 1.5f;
        [SerializeField, Range(0f, 1f)] float haloOpacity = 0.35f;
        [SerializeField, Min(0f)] float haloBrightness = 1f;
        [Tooltip("How quickly the outer glow fades toward its edge. Higher = softer, wispier glow.")]
        [SerializeField, Range(0.1f, 8f)] float haloFalloff = 3f;

        [Header("Pulse")]
        [Tooltip("How much the orb grows and shrinks as it breathes. 0 turns the pulse off.")]
        [SerializeField, Range(0f, 0.5f)] float pulseAmount = 0.08f;
        [Tooltip("Pulses per second.")]
        [SerializeField, Min(0f)] float pulseSpeed = 0.6f;

        [Header("While Drawing")]
        [Tooltip("Size multiplier while drawing. 1 = no change.")]
        [SerializeField, Min(0.01f)] float drawingScale = 1.3f;
        [Tooltip("Pulse speed multiplier while drawing.")]
        [SerializeField, Min(0f)] float drawingPulseSpeed = 3f;

        [Header("Light")]
        [Tooltip("Lets the orb light up nearby surfaces.")]
        [SerializeField] bool castLight = true;
        [SerializeField, Min(0f)] float lightIntensity = 0.6f;
        [SerializeField, Min(0f)] float lightRange = 1.5f;

        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int FalloffId = Shader.PropertyToID("_Falloff");
        static readonly int WhiteCoreId = Shader.PropertyToID("_WhiteCore");

        Transform core, halo;
        Material coreMaterial, haloMaterial;
        Light orbLight;
        Color currentColor;
        float drawBlend;
        float pulsePhase;

        void Awake()
        {
            if (orbShader == null) orbShader = Shader.Find("SigilCaster/OrbGlow");
            if (orbShader == null)
            {
                Debug.LogError("WandOrb needs the SigilCaster/OrbGlow shader assigned");
                enabled = false;
                return;
            }

            core = CreateSphere("Orb Core", out coreMaterial);
            halo = CreateSphere("Orb Halo", out haloMaterial);
            //draw the halo first so the core always sits on top of it
            haloMaterial.renderQueue = coreMaterial.renderQueue - 1;

            GameObject lightObject = new GameObject("Orb Light");
            lightObject.transform.SetParent(transform, false);
            orbLight = lightObject.AddComponent<Light>();
            orbLight.type = LightType.Point;
            orbLight.shadows = LightShadows.None;

            currentColor = idleColor;
            Apply();
        }
        void Update()
        {
            bool drawing = drawer != null && drawer.IsDrawing;

            //blend toward the drawing look, or back to idle
            float step = colorChangeSpeed * Time.deltaTime;
            drawBlend = Mathf.MoveTowards(drawBlend, drawing ? 1f : 0f, step);
            currentColor = Color.Lerp(idleColor, drawingColor, drawBlend);

            pulsePhase += Time.deltaTime * pulseSpeed * Mathf.Lerp(1f, drawingPulseSpeed, drawBlend);
            Apply();
        }
        void OnDestroy()
        {
            if (coreMaterial != null) Destroy(coreMaterial);
            if (haloMaterial != null) Destroy(haloMaterial);
        }
        Transform CreateSphere(string sphereName, out Material material)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = sphereName;
            //visual only, it should never block casts or physics
            Destroy(sphere.GetComponent<Collider>());
            sphere.transform.SetParent(transform, false);

            material = new Material(orbShader);
            MeshRenderer sphereRenderer = sphere.GetComponent<MeshRenderer>();
            sphereRenderer.sharedMaterial = material;
            sphereRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sphereRenderer.receiveShadows = false;
            return sphere.transform;
        }
        //pushes the inspector values onto the orb, runs every frame so
        //changes made in play mode show up right away
        void Apply()
        {
            float pulse = 1f + Mathf.Sin(pulsePhase * Mathf.PI * 2f) * pulseAmount;
            float scale = pulse * Mathf.Lerp(1f, drawingScale, drawBlend);

            core.localScale = Vector3.one * (coreDiameter * scale);
            halo.localScale = Vector3.one * (haloDiameter * scale);

            SetMaterial(coreMaterial, coreOpacity, coreBrightness, coreFalloff, whiteCore);
            SetMaterial(haloMaterial, haloOpacity, haloBrightness * pulse, haloFalloff, 0f);

            orbLight.enabled = castLight;
            orbLight.color = currentColor;
            orbLight.intensity = lightIntensity * pulse;
            orbLight.range = lightRange;
        }
        void SetMaterial(Material material, float opacity, float brightness, float falloff, float white)
        {
            Color color = currentColor;
            color.a = opacity;
            material.SetColor(ColorId, color);
            material.SetFloat(IntensityId, brightness);
            material.SetFloat(FalloffId, falloff);
            material.SetFloat(WhiteCoreId, white);
        }
    }
}
