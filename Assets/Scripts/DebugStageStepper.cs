// ============================================================================
//  DECRYPTED  —  DebugStageStepper  (preview-only, NOT for the final VR build)
// ----------------------------------------------------------------------------
//  Lets you walk through the WHOLE experience on desktop with no VR controllers.
//  Press ENTER to advance to the next stage. On the rotor stage it auto-sets
//  the dials to the correct key and runs the decode, so you get to watch the
//  lampboard spell out the secret word and the vault open on its own.
//
//  A little on-screen readout (top-left) tells you what stage you're in.
// ============================================================================

using UnityEngine;

public class DebugStageStepper : MonoBehaviour
{
    RotorMachine _rotor;

    void Start()
    {
        _rotor = Object.FindObjectOfType<RotorMachine>();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Advance();
    }

    void Advance()
    {
        var gm = GameManager.Instance;
        switch (gm.CurrentStage)
        {
            case GameManager.Stage.Splash:
                gm.BeginExperience();
                break;

            case GameManager.Stage.Tutorial:
                gm.CompleteStage(GameManager.Stage.Tutorial);
                break;

            case GameManager.Stage.Exhibit1_Caesar:
                gm.CompleteStage(GameManager.Stage.Exhibit1_Caesar);
                break;

            case GameManager.Stage.Exhibit2_Rotor:
                RunRotorAutomatically();
                break;

            case GameManager.Stage.Vault:
                // The vault opens on its own; just wait and watch.
                break;

            case GameManager.Stage.Reveal:
                gm.CompleteStage(GameManager.Stage.Reveal);
                break;
        }
    }

    void RunRotorAutomatically()
    {
        if (_rotor == null) return;
        int[] pos = CipherLogic.KeyToPositions(_rotor.keyOfTheDay);
        _rotor.dialRight.Preset(pos[0]);
        _rotor.dialMiddle.Preset(pos[1]);
        _rotor.dialLeft.Preset(pos[2]);
        _rotor.RunDecode();   // auto-advances to the Vault stage on success
    }

    void OnGUI()
    {
        var box = new GUIStyle(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
        string stage = GameManager.Instance != null ? GameManager.Instance.CurrentStage.ToString() : "(loading)";
        GUI.Box(new Rect(10, 10, 380, 64),
            $"  STAGE: {stage}\n  Press ENTER to advance  ·  WASD + right-mouse to move", box);
    }
}
