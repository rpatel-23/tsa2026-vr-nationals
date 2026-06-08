// ============================================================================
//  DECRYPTED  —  PerformanceConfig
// ----------------------------------------------------------------------------
//  Everything that keeps this running at a locked 72 fps on the ORIGINAL Quest
//  (Snapdragon 835 / Adreno 540 / 4 GB). The single biggest enemy on this chip
//  is fillrate/overdraw, so the design rules live in the build guide; this file
//  handles the runtime knobs.
//
//  Attach to one GameObject in the scene (e.g. the GameManager object) and let
//  it run once in Awake.
//
//  NOTE ON API NAMES: the foveation/clock calls live in the Oculus XR Plugin
//  (com.unity.xr.oculus). On the older Unity LTS you will use to target Quest 1
//  the namespace is `Unity.XR.Oculus`. They are wrapped in try/catch so the
//  project still compiles if the package version differs slightly.
// ============================================================================

using UnityEngine;

public class PerformanceConfig : MonoBehaviour
{
    [Header("Display")]
    [Tooltip("Quest 1 panel is 72 Hz. Lock the app to match.")]
    public int targetFrameRate = 72;

    [Tooltip("Render-target scale. 1.0 is native. Drop toward 0.8 only if the " +
             "profiler shows you are fillrate-bound after everything else.")]
    [Range(0.7f, 1.2f)] public float eyeResolutionScale = 1.0f;

    [Header("Foveated rendering (0 off .. 3 high)")]
    [Tooltip("Higher = more GPU saved at the cost of peripheral sharpness. " +
             "On Quest 1, 2-3 is a big, almost-invisible win.")]
    [Range(0, 3)] public int foveationLevel = 3;

    void Awake()
    {
        // Match the headset refresh; never let it free-run.
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;

        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = eyeResolutionScale;

        // Pin the CPU/GPU clocks high and turn on fixed foveated rendering.
        // These calls are Oculus-plugin specific; guard them so the build is safe.
        TrySetOculusPerformance();
    }

    void TrySetOculusPerformance()
    {
        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Fixed Foveated Rendering — the cheapest large GPU saving on Quest.
            Unity.XR.Oculus.Utils.SetFoveationLevel(foveationLevel);

            // Hold clocks up so we don't thermal-throttle mid-demo. For a short
            // 2-3 min recorded run this is safe; for long sessions you would let
            // dynamic throttling manage heat instead.
            Unity.XR.Oculus.Performance.TrySetCPULevel(3);
            Unity.XR.Oculus.Performance.TrySetGPULevel(3);
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PerformanceConfig] Oculus perf API not available " +
                             "in this package version: " + e.Message);
        }
    }
}
