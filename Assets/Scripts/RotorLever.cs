// ============================================================================
//  DECRYPTED  —  RotorLever  (part of Exhibit 2)
// ----------------------------------------------------------------------------
//  A pull lever. Grab it and rotate it past a threshold; it fires onPulled once.
//  Used to "run" the rotor machine. Constrained to a single rotation axis so it
//  feels mechanical and never flies off.
// ============================================================================

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class RotorLever : MonoBehaviour
{
    [Header("Pivot of the lever arm")]
    public Transform leverArm;
    public XRBaseInteractable grab;

    [Header("Rotation limits (degrees around local X)")]
    public float restAngle = 0f;
    public float pulledAngle = 55f;
    [Tooltip("Fraction of the way to 'pulled' that counts as a pull.")]
    [Range(0.5f, 1f)] public float triggerFraction = 0.85f;

    [Header("Event")]
    public UnityEvent onPulled;

    bool _held, _fired;
    Transform _interactor;
    float _grabStart, _armStart;

    void Awake()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(a => { _held = true; _interactor = a.interactorObject.transform; _grabStart = _interactor.eulerAngles.x; _armStart = leverArm.localEulerAngles.x; });
            grab.selectExited.AddListener(_ => { _held = false; _interactor = null; if (!_fired) Spring(); });
        }
    }

    void Update()
    {
        if (!_held || _interactor == null) return;
        float delta = Mathf.DeltaAngle(_grabStart, _interactor.eulerAngles.x);
        float angle = Mathf.Clamp(_armStart + delta, restAngle, pulledAngle);
        leverArm.localEulerAngles = new Vector3(angle, 0, 0);

        float frac = Mathf.InverseLerp(restAngle, pulledAngle, angle);
        if (!_fired && frac >= triggerFraction)
        {
            _fired = true;
            onPulled?.Invoke();
        }
    }

    void Spring()
    {
        leverArm.localEulerAngles = new Vector3(restAngle, 0, 0);
    }

    public void ResetLever()
    {
        _fired = false;
        Spring();
    }
}
