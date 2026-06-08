// ============================================================================
//  DECRYPTED  —  GameManager   [PATCHED: UnityEvents pre-initialized]
// ----------------------------------------------------------------------------
//  Single source of truth for "where the visitor is" in the museum. Every
//  exhibit reports completion here; this class advances the experience, moves
//  the Guide Orb to the next interactable, swaps the music zone, and keeps the
//  whole thing inside the 2-3 minute window required by the rubric.
// ============================================================================

using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum Stage
    {
        Splash,
        Tutorial,
        Exhibit1_Caesar,
        Exhibit2_Rotor,
        Vault,
        Reveal,
        Done
    }

    [Header("Current State (read-only at runtime)")]
    [SerializeField] private Stage stage = Stage.Splash;
    public Stage CurrentStage => stage;

    [Header("Scene references")]
    public GuideOrb guideOrb;
    public AudioDirector audio;
    public RoomActivator rooms;

    [Header("Per-stage focus targets for the Guide Orb")]
    public Transform tutorialFocus;
    public Transform caesarFocus;
    public Transform rotorFocus;
    public Transform vaultFocus;
    public Transform revealFocus;

    // Events are pre-initialized so tools (like the greybox builder) can safely
    // attach listeners even before the scene is saved.
    [Header("Events (hook lights, panels, etc. in the Inspector)")]
    public UnityEvent onTutorialBegin = new UnityEvent();
    public UnityEvent onExhibit1Begin = new UnityEvent();
    public UnityEvent onExhibit2Begin = new UnityEvent();
    public UnityEvent onVaultBegin = new UnityEvent();
    public UnityEvent onRevealBegin = new UnityEvent();

    [Header("Pacing")]
    [Tooltip("Soft target so the team can rehearse to the 2-3 min window. " +
             "Purely informational; it does not force-advance the visitor.")]
    public float targetTotalSeconds = 165f;   // 2:45
    public float ElapsedSeconds { get; private set; }
    private bool running;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (running) ElapsedSeconds += Time.deltaTime;
    }

    // Called by SplashStart when the visitor presses PLAY.
    public void BeginExperience()
    {
        running = true;
        ElapsedSeconds = 0f;
        GoTo(Stage.Tutorial);
    }

    // Exhibits call this with their own stage when finished.
    public void CompleteStage(Stage finished)
    {
        if (finished != stage) return;       // ignore stale/duplicate calls
        switch (finished)
        {
            case Stage.Tutorial:        GoTo(Stage.Exhibit1_Caesar); break;
            case Stage.Exhibit1_Caesar: GoTo(Stage.Exhibit2_Rotor);  break;
            case Stage.Exhibit2_Rotor:  GoTo(Stage.Vault);           break;
            case Stage.Vault:           GoTo(Stage.Reveal);          break;
            case Stage.Reveal:          GoTo(Stage.Done);            break;
        }
    }

    void GoTo(Stage next)
    {
        stage = next;
        switch (next)
        {
            case Stage.Tutorial:
                rooms?.ShowOnly(RoomActivator.Room.Atrium);
                audio?.SetZone(AudioDirector.Zone.Atrium);
                guideOrb?.MoveTo(tutorialFocus);
                onTutorialBegin?.Invoke();
                break;

            case Stage.Exhibit1_Caesar:
                rooms?.ShowOnly(RoomActivator.Room.Ancient);
                audio?.SetZone(AudioDirector.Zone.Ancient);
                guideOrb?.MoveTo(caesarFocus);
                onExhibit1Begin?.Invoke();
                break;

            case Stage.Exhibit2_Rotor:
                rooms?.ShowOnly(RoomActivator.Room.War);
                audio?.SetZone(AudioDirector.Zone.War);
                guideOrb?.MoveTo(rotorFocus);
                onExhibit2Begin?.Invoke();
                break;

            case Stage.Vault:
                rooms?.ShowOnly(RoomActivator.Room.Modern);
                audio?.SetZone(AudioDirector.Zone.Modern);
                guideOrb?.MoveTo(vaultFocus);
                onVaultBegin?.Invoke();
                break;

            case Stage.Reveal:
                audio?.SetZone(AudioDirector.Zone.Reveal);
                guideOrb?.MoveTo(revealFocus);
                onRevealBegin?.Invoke();
                break;

            case Stage.Done:
                running = false;
                break;
        }
    }
}
