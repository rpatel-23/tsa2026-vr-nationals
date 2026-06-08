// ============================================================================
//  DECRYPTED  —  SplashStart
// ----------------------------------------------------------------------------
//  TSA rule: "A splash screen is acceptable, provided the PLAY command is
//  easily visible" and "Each simulation must advance automatically once it has
//  been started." So: a floating title card with one big glowing PLAY button.
//  Pressing it hands control to the GameManager and the museum begins.
//
//  The PLAY button is an XR Simple Interactable (poke or trigger to press).
//  No voiceover is used anywhere — onboarding is fully visual + text + audio
//  cues, which keeps us inside the "sound but no narration" rule.
// ============================================================================

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SplashStart : MonoBehaviour
{
    [Header("References")]
    public GameObject splashRoot;          // the title card + PLAY button
    public XRSimpleInteractable playButton;
    public AudioDirector audio;
    public AudioClip startStingClip;       // short original "power-on" chord

    [Header("Idle hint")]
    [Tooltip("Gently pulse the PLAY button so a judge instantly knows to press it.")]
    public Renderer playButtonRenderer;
    public float pulseSpeed = 2f;
    public Color pulseColor = new Color(0.2f, 1f, 0.7f);

    MaterialPropertyBlock _mpb;
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    bool _started;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        if (playButton != null)
            playButton.selectEntered.AddListener(_ => Play());
    }

    void Update()
    {
        if (_started || playButtonRenderer == null) return;
        // Cheap emissive pulse via MaterialPropertyBlock (no new materials,
        // no transparency) -> safe on Quest 1.
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        playButtonRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissionId, pulseColor * Mathf.Lerp(0.15f, 1.2f, t));
        playButtonRenderer.SetPropertyBlock(_mpb);
    }

    public void Play()
    {
        if (_started) return;
        _started = true;

        if (audio != null && startStingClip != null)
            audio.PlaySfx(startStingClip);

        if (splashRoot != null) splashRoot.SetActive(false);
        GameManager.Instance.BeginExperience();
    }
}
