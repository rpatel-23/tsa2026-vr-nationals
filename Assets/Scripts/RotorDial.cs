// ============================================================================
//  DECRYPTED  —  RotorDial  (part of Exhibit 2)   [UPDATED]
// ----------------------------------------------------------------------------
//  One of the three dials the visitor sets to enter the "key of the day."
//  Grab and twist; it snaps to one of 26 positions (A-Z). Reports its current
//  letter (0-25) to the RotorMachine.
//
//  UPDATE: added Preset(int) so tools/tests (like the greybox preview) can set
//  a dial to a specific letter from code without physically twisting it.
// ============================================================================

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class RotorDial : MonoBehaviour
{
    [Header("References")]
    public Transform dialMesh;          // the visible rotating drum
    public XRBaseInteractable grab;
    public TMP_Text letterLabel;        // shows the current letter through a window

    [Header("Audio")]
    public AudioDirector audio;
    public AudioClip clickClip;

    public int Value { get; private set; }   // 0 = A .. 25 = Z

    bool _held;
    Transform _interactor;
    float _grabStart, _meshStart;

    void Awake()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(a => { _held = true; _interactor = a.interactorObject.transform; _grabStart = Angle(_interactor); _meshStart = dialMesh.localEulerAngles.z; });
            grab.selectExited.AddListener(_ => { _held = false; _interactor = null; Snap(); });
        }
        Refresh();
    }

    void Update()
    {
        if (!_held || _interactor == null) return;
        float z = _meshStart + (Angle(_interactor) - _grabStart);
        dialMesh.localEulerAngles = new Vector3(0, 0, z);

        int v = Mod(Mathf.RoundToInt(z / (360f / 26f)), 26);
        if (v != Value)
        {
            Value = v;
            if (clickClip != null) audio?.PlaySfx(clickClip);
            Refresh();
        }
    }

    // Set the dial from code (used by tools / the greybox preview).
    public void Preset(int value)
    {
        Value = Mod(value, 26);
        if (dialMesh != null)
            dialMesh.localEulerAngles = new Vector3(0, 0, Value * (360f / 26f));
        Refresh();
    }

    void Snap()
    {
        float step = 360f / 26f;
        dialMesh.localEulerAngles = new Vector3(0, 0, Mathf.Round(dialMesh.localEulerAngles.z / step) * step);
    }

    void Refresh()
    {
        if (letterLabel != null)
            letterLabel.text = ((char)('A' + Value)).ToString();
    }

    float Angle(Transform t)
    {
        Vector3 l = transform.InverseTransformPoint(t.position);
        return Mathf.Atan2(l.y, l.x) * Mathf.Rad2Deg;
    }

    static int Mod(int a, int m) => ((a % m) + m) % m;
}
