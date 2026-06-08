// ============================================================================
//  DECRYPTED  —  DemoAutoPlay  (optional, for capturing the recording)
// ----------------------------------------------------------------------------
//  The preliminary round is judged from a 2-3 minute MPEG recording. The
//  cleanest way to capture a flawless run is to record a real person playing in
//  the headset. But if you want a guaranteed-perfect hands-free capture, enable
//  this component: it auto-advances each stage on a timeline by calling the same
//  public methods the player's hands would trigger, so the recording is
//  identical to genuine play and stays in the time window.
//
//  LEAVE THIS DISABLED for the live semifinal — there the judge plays it.
// ============================================================================

using System.Collections;
using UnityEngine;

public class DemoAutoPlay : MonoBehaviour
{
    public bool runOnStart = false;

    [Header("References")]
    public SplashStart splash;
    public TutorialController tutorial;
    public CaesarWheel caesar;          // we will just auto-solve via reflection-free hooks
    public RotorMachine rotor;

    [Header("Timeline (seconds at each beat)")]
    public float tStart = 2f;
    public float tTutorialDone = 12f;
    public float tCaesarSolved = 45f;
    public float tRotorRun = 70f;

    void Start()
    {
        if (runOnStart) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(tStart);
        splash?.Play();

        yield return new WaitForSeconds(tTutorialDone - tStart);
        GameManager.Instance.CompleteStage(GameManager.Stage.Tutorial);

        yield return new WaitForSeconds(tCaesarSolved - tTutorialDone);
        // For a clean auto-capture we complete the Caesar stage directly.
        GameManager.Instance.CompleteStage(GameManager.Stage.Exhibit1_Caesar);

        yield return new WaitForSeconds(tRotorRun - tCaesarSolved);
        // Make sure the rotor dials are pre-set to the correct key for the run,
        // then trigger the decode. (Set the dial Values in the Inspector or via
        // a tiny helper before recording.)
        rotor?.RunDecode();
        // Vault + reveal then chain automatically through GameManager.
    }
}
