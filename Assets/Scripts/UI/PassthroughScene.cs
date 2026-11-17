using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.NativeTypes;

//turns the scene it lives in into a passthrough scene: the player sees their real room
//with the scene's objects (like the title menu) floating in it
//passthrough switches off again when the scene unloads, so the next scene is normal full VR
public class PassthroughScene : MonoBehaviour
{
    [Tooltip("The headset camera. Leave empty to use the main camera.")]
    [SerializeField] Camera headCamera;
    [Tooltip("Objects hidden while passthrough is showing, like the virtual floor and walls.")]
    [SerializeField] GameObject[] hideInPassthrough;
    [Tooltip("How many frames to wait for the headset to accept passthrough before giving up.")]
    [SerializeField, Min(1)] int framesToWait = 30;

    bool requested;

    IEnumerator Start()
    {
        //no headset running (the simulator, for example) means there is no room to show
        XRManagerSettings xr = XRGeneralSettings.Instance != null ? XRGeneralSettings.Instance.Manager : null;
        if (xr == null || xr.activeLoader == null) yield break;

        if (!BlendMode.TrySet(XrEnvironmentBlendMode.AlphaBlend)) yield break;
        requested = true;

        //the headset falls back to its own choice if it cannot do passthrough, so check it took
        for (int frame = 0; frame < framesToWait && BlendMode.Current != XrEnvironmentBlendMode.AlphaBlend; frame++)
            yield return null;

        if (BlendMode.Current != XrEnvironmentBlendMode.AlphaBlend)
        {
            Debug.LogWarning("[PassthroughScene] This headset or runtime did not accept passthrough. Showing the normal scene instead.");
            yield break;
        }

        ShowRoom();
    }

    void ShowRoom()
    {
        //the room shows through wherever the picture is see-through,
        //so clear to fully transparent instead of drawing a skybox
        Camera cam = headCamera != null ? headCamera : Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.clear;
        }

        foreach (GameObject item in hideInPassthrough)
            if (item != null) item.SetActive(false);
    }

    void OnDestroy()
    {
        //back to full VR for whatever scene comes next
        if (requested) BlendMode.TrySet(XrEnvironmentBlendMode.Opaque);
    }

    //OpenXR only lets a feature class change the blend mode, so this exists to reach those calls
    //it is never added to the OpenXR feature list
    abstract class BlendMode : OpenXRFeature
    {
        public static XrEnvironmentBlendMode Current
        {
            get
            {
                try { return GetEnvironmentBlendMode(); }
                catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException) { return XrEnvironmentBlendMode.Opaque; }
            }
        }

        public static bool TrySet(XrEnvironmentBlendMode mode)
        {
            try
            {
                SetEnvironmentBlendMode(mode);
                return true;
            }
            catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException)
            {
                //the OpenXR plug-in is not present on this platform
                return false;
            }
        }
    }
}
