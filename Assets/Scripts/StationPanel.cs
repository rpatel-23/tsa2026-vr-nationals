// ============================================================================
//  DECRYPTED  —  StationPanel
// ----------------------------------------------------------------------------
//  A small helper for the text plaques at each exhibit. Each plaque has:
//     - a TITLE  (e.g. "EXHIBIT I — THE CAESAR CIPHER, c. 50 BC")
//     - a GOAL line in plain language (e.g. "Turn the wheel until the message
//       reads a real word.")
//     - one EDUCATIONAL line (feeds the Knowledge + Graphical criteria)
//
//  GameManager's per-stage UnityEvents toggle these on/off, so the active
//  station's instructions are always visible and finished ones go quiet. This
//  is how the judge knows what to do — visually, with no narration.
// ============================================================================

using UnityEngine;

public class StationPanel : MonoBehaviour
{
    [Header("Root object to show/hide")]
    public GameObject panelRoot;

    [Header("Optional gentle highlight on the active interactable")]
    public Renderer highlightTarget;
    public Color highlightColor = new Color(0.2f, 0.9f, 1f);
    public float pulseSpeed = 2.2f;

    MaterialPropertyBlock _mpb;
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    bool _active;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void Activate()
    {
        _active = true;
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void Deactivate()
    {
        _active = false;
        if (panelRoot != null) panelRoot.SetActive(false);
        if (highlightTarget != null)
        {
            highlightTarget.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionId, Color.black);
            highlightTarget.SetPropertyBlock(_mpb);
        }
    }

    void Update()
    {
        if (!_active || highlightTarget == null) return;
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        highlightTarget.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissionId, highlightColor * Mathf.Lerp(0.1f, 0.8f, t));
        highlightTarget.SetPropertyBlock(_mpb);
    }
}
