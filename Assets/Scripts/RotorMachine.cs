// ============================================================================
//  DECRYPTED  —  RotorMachine  (Exhibit 2: WWII cipher machine — THE WOW STAGE)
// ----------------------------------------------------------------------------
//  The visitor sets three dials to the "key of the day" (shown on an intercepted
//  note prop), then pulls the lever. The machine reads the dials, feeds the
//  intercepted ciphertext through the reciprocal rotor cipher (CipherLogic),
//  and plays it back letter-by-letter:
//      - the three rotor drums physically step (odometer style)
//      - the matching lamp flashes on the Lampboard
//      - a mechanical click plays
//      - the decoded letter appears on the output display
//  If the dials are correct, the output spells the secret word and the vault
//  unlocks. If not, it spells gibberish, a reset buzz plays, and the visitor can
//  re-set the dials and try again. (For the recording, you set it correctly.)
//
//  Embed values produced by CipherCodegen / the included Python check:
//      keyOfTheDay = "MAC", interceptedCipher = "ZLDFDQO", plaintext = "VICTORY"
// ============================================================================

using System.Collections;
using UnityEngine;
using TMPro;

public class RotorMachine : MonoBehaviour
{
    [Header("The puzzle (generate with CipherCodegen)")]
    [Tooltip("Ciphertext the machine will process.")]
    public string interceptedCipher = "ZLDFDQO";
    [Tooltip("What it should decode to when the key is correct.")]
    public string expectedPlaintext = "VICTORY";
    [Tooltip("3-letter key the visitor must dial in, e.g. MAC.")]
    public string keyOfTheDay = "MAC";

    [Header("Dials (right, middle, left)")]
    public RotorDial dialRight;
    public RotorDial dialMiddle;
    public RotorDial dialLeft;

    [Header("Rotor drums to spin visually (right, middle, left)")]
    public Transform drumRight;
    public Transform drumMiddle;
    public Transform drumLeft;

    [Header("Output")]
    public Lampboard lampboard;
    public TMP_Text outputDisplay;
    public RotorLever lever;

    [Header("Audio")]
    public AudioDirector audio;
    public AudioClip keyClickClip;     // mechanical clack per letter
    public AudioClip leverClip;
    public AudioClip successClip;
    public AudioClip failBuzzClip;

    [Header("Timing")]
    public float secondsPerLetter = 0.28f;

    bool _running, _solved;

    void Awake()
    {
        if (lever != null) lever.onPulled.AddListener(RunDecode);
        if (outputDisplay != null) outputDisplay.text = "";
    }

    public void RunDecode()
    {
        if (_running || _solved) return;
        StartCoroutine(RunRoutine());
    }

    IEnumerator RunRoutine()
    {
        _running = true;
        if (leverClip != null) audio?.PlaySfx(leverClip);
        if (outputDisplay != null) outputDisplay.text = "";
        lampboard?.AllOff();

        // Read the key straight off the three physical dials.
        int[] pos = { dialRight.Value, dialMiddle.Value, dialLeft.Value };
        int[] work = (int[])pos.Clone();

        var sb = new System.Text.StringBuilder();
        float stepDeg = 360f / 26f;

        foreach (char c in interceptedCipher)
        {
            if (!char.IsLetter(c)) { sb.Append(c); continue; }

            // step the rotors (logic) and spin the drums (visual) together
            CipherLogic.Step(work);
            SpinDrum(drumRight,  work[0] * stepDeg);
            SpinDrum(drumMiddle, work[1] * stepDeg);
            SpinDrum(drumLeft,   work[2] * stepDeg);

            char outC = CipherLogic.EnigmaChar(c, work);
            sb.Append(outC);

            lampboard?.Flash(outC);
            if (keyClickClip != null) audio?.PlaySfx(keyClickClip);
            if (outputDisplay != null) outputDisplay.text = sb.ToString();

            yield return new WaitForSeconds(secondsPerLetter);
        }

        string result = sb.ToString();
        _running = false;

        if (result.Equals(expectedPlaintext, System.StringComparison.OrdinalIgnoreCase))
        {
            _solved = true;
            if (successClip != null) audio?.PlaySfx(successClip);
            yield return new WaitForSeconds(0.6f);
            GameManager.Instance.CompleteStage(GameManager.Stage.Exhibit2_Rotor);
        }
        else
        {
            // wrong key -> let them retry
            if (failBuzzClip != null) audio?.PlaySfx(failBuzzClip);
            lever?.ResetLever();
        }
    }

    void SpinDrum(Transform drum, float zDeg)
    {
        if (drum != null) drum.localEulerAngles = new Vector3(0, 0, zDeg);
    }

#if UNITY_EDITOR
    // Right-click the component header -> "Regenerate Cipher From Plaintext+Key"
    [ContextMenu("Regenerate Cipher From Plaintext+Key")]
    void Regenerate()
    {
        int[] pos = CipherLogic.KeyToPositions(keyOfTheDay);
        interceptedCipher = CipherLogic.EnigmaProcess(expectedPlaintext, pos);
        Debug.Log($"[RotorMachine] key={keyOfTheDay} plaintext={expectedPlaintext} -> cipher={interceptedCipher}");
    }
#endif
}
