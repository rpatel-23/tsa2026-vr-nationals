// ============================================================================
//  DECRYPTED  —  TutorialController
// ----------------------------------------------------------------------------
//  The "what do I do?" moment. Two quick visual panels (NEXT to advance, like
//  the reference team's tutorial) that teach the only two interactions the
//  whole museum uses:
//     1. POINT + TRIGGER to press buttons
//     2. GRAB + TWIST to turn wheels and dials
//  Each panel is an image/icon + a one-line text label. No narration.
//
//  When the visitor finishes the last panel, the tutorial stage completes and
//  the Guide Orb leads them to Exhibit 1.
// ============================================================================

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialController : MonoBehaviour
{
    [Header("Panels shown in order (each is a GameObject with an icon + label)")]
    public GameObject[] panels;

    [Header("Buttons")]
    public XRSimpleInteractable nextButton;
    public XRSimpleInteractable backButton;   // optional

    [Header("Audio")]
    public AudioDirector audio;
    public AudioClip pageClip;                 // soft original page/click sound

    int _index = -1;
    bool _active;

    void Awake()
    {
        if (nextButton != null) nextButton.selectEntered.AddListener(_ => Next());
        if (backButton != null) backButton.selectEntered.AddListener(_ => Back());
        HideAll();
    }

    // Hook to GameManager.onTutorialBegin in the Inspector.
    public void Begin()
    {
        _active = true;
        _index = 0;
        Show(_index);
    }

    void Next()
    {
        if (!_active) return;
        if (pageClip != null) audio?.PlaySfx(pageClip);

        if (_index >= panels.Length - 1)
        {
            HideAll();
            _active = false;
            GameManager.Instance.CompleteStage(GameManager.Stage.Tutorial);
            return;
        }
        Show(++_index);
    }

    void Back()
    {
        if (!_active || _index <= 0) return;
        if (pageClip != null) audio?.PlaySfx(pageClip);
        Show(--_index);
    }

    void Show(int i)
    {
        for (int p = 0; p < panels.Length; p++)
            if (panels[p] != null) panels[p].SetActive(p == i);
    }

    void HideAll()
    {
        foreach (var p in panels) if (p != null) p.SetActive(false);
    }
}
