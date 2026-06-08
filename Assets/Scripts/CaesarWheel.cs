// ============================================================================
//  DECRYPTED  —  CaesarWheel  (Exhibit 1: Ancient cryptography)
// ----------------------------------------------------------------------------
//  A two-ring cipher disk. The OUTER ring (fixed) shows the alphabet. The INNER
//  ring rotates. A window at the top shows the current shift. Two small TMP
//  displays show the scrambled message and the live decode result.
//
//  Interaction: grab the inner ring and twist your wrist; the ring snaps to the
//  nearest of 26 detents. Each detent = a Caesar shift. When the decoded text
//  matches the target word, the wheel locks, plays a success ding, and reports
//  completion. Visual, immediate, and obvious on a screen recording.
//
//  No physics joints, no transparency. Rotation is read from the held
//  interactor each frame and applied directly -> rock-solid on Quest 1.
// ============================================================================

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class CaesarWheel : MonoBehaviour
{
    [Header("The puzzle")]
    [Tooltip("What the plaque shows as scrambled. e.g. 'URPH'")]
    public string scrambledMessage = "URPH";   // 'ROME' shifted +3
    [Tooltip("The word the visitor must reveal. e.g. 'ROME'")]
    public string targetWord = "ROME";

    [Header("References")]
    public Transform innerRing;                 // the part that physically turns
    public XRBaseInteractable grabRim;          // grabbable rim collider
    public TMP_Text scrambledDisplay;
    public TMP_Text decodedDisplay;
    public TMP_Text shiftDisplay;

    [Header("Audio")]
    public AudioDirector audio;
    public AudioClip detentClick;               // soft click each letter step
    public AudioClip successDing;

    [Header("Feel")]
    public bool snapToLetters = true;

    int _currentShift;
    int _lastShift = -999;
    bool _solved;
    bool _held;
    Transform _interactor;
    float _grabStartAngle;
    float _ringStartAngle;

    void Awake()
    {
        if (grabRim != null)
        {
            grabRim.selectEntered.AddListener(OnGrab);
            grabRim.selectExited.AddListener(OnRelease);
        }
        if (scrambledDisplay != null) scrambledDisplay.text = scrambledMessage;
        UpdateDecode(0);
    }

    void OnGrab(SelectEnterEventArgs a)
    {
        if (_solved) return;
        _held = true;
        _interactor = a.interactorObject.transform;
        _grabStartAngle = AngleAroundAxis(_interactor);
        _ringStartAngle = innerRing.localEulerAngles.z;
    }

    void OnRelease(SelectExitEventArgs a)
    {
        _held = false;
        _interactor = null;
        if (snapToLetters) SnapRing();
    }

    void Update()
    {
        if (_solved || !_held || _interactor == null) return;

        float delta = AngleAroundAxis(_interactor) - _grabStartAngle;
        float z = _ringStartAngle + delta;
        innerRing.localEulerAngles = new Vector3(0, 0, z);

        // 26 detents around 360 degrees -> shift = round(z / (360/26))
        int shift = Mod(Mathf.RoundToInt(z / (360f / 26f)), 26);
        if (shift != _currentShift)
        {
            _currentShift = shift;
            if (detentClick != null) audio?.PlaySfx(detentClick);
            UpdateDecode(shift);
        }
    }

    void UpdateDecode(int shift)
    {
        // The visitor is UNDOING a +N encryption, so we decode by shifting back.
        string decoded = CipherLogic.CaesarString(scrambledMessage, -shift);
        if (decodedDisplay != null) decodedDisplay.text = decoded;
        if (shiftDisplay != null)   shiftDisplay.text = "SHIFT  " + shift;

        if (!_solved && decoded.Equals(targetWord, System.StringComparison.OrdinalIgnoreCase))
            Solve();
    }

    void Solve()
    {
        _solved = true;
        _held = false;
        if (successDing != null) audio?.PlaySfx(successDing);
        GameManager.Instance.CompleteStage(GameManager.Stage.Exhibit1_Caesar);
    }

    void SnapRing()
    {
        float step = 360f / 26f;
        float z = Mathf.Round(innerRing.localEulerAngles.z / step) * step;
        innerRing.localEulerAngles = new Vector3(0, 0, z);
    }

    // Angle of the interactor around the wheel's local Z axis, in degrees.
    float AngleAroundAxis(Transform t)
    {
        Vector3 local = transform.InverseTransformPoint(t.position);
        return Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
    }

    static int Mod(int a, int m) => ((a % m) + m) % m;
}
