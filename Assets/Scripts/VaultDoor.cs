// ============================================================================
//  DECRYPTED  —  VaultDoor  (Exhibit 3: Modern encryption — the payoff)
// ----------------------------------------------------------------------------
//  The decoded word from the rotor machine is the passphrase to the modern
//  vault. When Exhibit 2 completes, the vault's concentric lock rings spin into
//  alignment, the heavy door swings open with a rumble + success chord, and the
//  Reveal room behind it activates (the decoded word blazes on the back wall, a
//  short closing plaque explains that modern encryption uses keys far too long
//  to ever brute-force by hand).
//
//  Pure transform animation -> cheap and reliable on Quest 1.
// ============================================================================

using System.Collections;
using UnityEngine;
using TMPro;

public class VaultDoor : MonoBehaviour
{
    [Header("Lock rings (spin to 'align' before opening)")]
    public Transform[] lockRings;
    public float ringSpinTime = 1.2f;
    public float ringSpinSpeed = 720f;

    [Header("Door")]
    public Transform door;
    public Vector3 openLocalEuler = new Vector3(0, 100f, 0);
    public float doorOpenTime = 2.0f;

    [Header("Reveal")]
    public GameObject revealRoom;
    public TMP_Text revealWord;
    public string decodedWord = "VICTORY";

    [Header("Audio")]
    public AudioDirector audio;
    public AudioClip ringClickClip;
    public AudioClip rumbleClip;
    public AudioClip successChordClip;

    Vector3 _doorClosed;

    void Awake()
    {
        if (door != null) _doorClosed = door.localEulerAngles;
        if (revealRoom != null) revealRoom.SetActive(false);
    }

    // Hook to GameManager.onVaultBegin in the Inspector.
    public void Open()
    {
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        // 1) spin the lock rings into alignment
        if (ringClickClip != null) audio?.PlaySfx(ringClickClip);
        float t = 0;
        while (t < ringSpinTime)
        {
            t += Time.deltaTime;
            float speed = Mathf.Lerp(ringSpinSpeed, 0f, t / ringSpinTime);
            for (int i = 0; i < lockRings.Length; i++)
                if (lockRings[i] != null)
                    lockRings[i].Rotate(0, 0, speed * Time.deltaTime * (i % 2 == 0 ? 1 : -1));
            yield return null;
        }
        // snap rings to zero ("aligned")
        foreach (var r in lockRings) if (r != null) r.localEulerAngles = Vector3.zero;
        if (ringClickClip != null) audio?.PlaySfx(ringClickClip);

        // 2) open the door
        if (rumbleClip != null) audio?.PlaySfx(rumbleClip);
        Vector3 target = _doorClosed + openLocalEuler;
        t = 0;
        while (t < doorOpenTime && door != null)
        {
            t += Time.deltaTime;
            door.localEulerAngles = Vector3.Lerp(_doorClosed, target, Mathf.SmoothStep(0, 1, t / doorOpenTime));
            yield return null;
        }

        // 3) reveal
        if (revealWord != null) revealWord.text = decodedWord;
        if (revealRoom != null) revealRoom.SetActive(true);
        if (successChordClip != null) audio?.PlaySfx(successChordClip);

        yield return new WaitForSeconds(2.5f);
        GameManager.Instance.CompleteStage(GameManager.Stage.Vault);
    }
}
