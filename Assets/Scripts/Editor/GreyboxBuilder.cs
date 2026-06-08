// ============================================================================
//  DECRYPTED  —  GreyboxBuilder   (EDITOR-ONLY learning/preview tool)
// ----------------------------------------------------------------------------
//  Menu:  Tools > DECRYPTED > Build Greybox Museum
//
//  Click it once and this constructs the ENTIRE experience out of plain Unity
//  shapes (cubes/cylinders), attaches every DECRYPTED script, and wires all the
//  Inspector references for you. Then press Play and walk through it on your
//  desktop (WASD + right-mouse, ENTER to advance stages).
//
//  THIS IS A LEARNING / PREVIEW SCAFFOLD. It is intentionally ugly. The real
//  competition entry must be your own original 3D models + design — use this to
//  understand how the pieces connect, then rebuild it properly in Blender.
//
//  HOW TO USE:
//    1. File > New Scene  (start empty so cameras don't conflict)
//    2. Window > TextMeshPro > Import TMP Essential Resources  (if you haven't)
//    3. Tools > DECRYPTED > Build Greybox Museum
//    4. Press Play. Move with WASD + hold right-mouse to look. Press ENTER to
//       step through each stage and watch the whole thing work.
// ============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

public static class GreyboxBuilder
{
    // ---- materials reused across the build ----
    static Material _stone, _metal, _steel, _accent, _dark, _glow, _lamp, _purple;

    [MenuItem("Tools/DECRYPTED/Build Greybox Museum")]
    public static void Build()
    {
        MakeMaterials();

        var root = new GameObject("DECRYPTED_Museum (GREYBOX)").transform;

        // --- a light so we can see anything ---
        var lightGO = new GameObject("Sun");
        lightGO.transform.SetParent(root);
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        var lit = lightGO.AddComponent<Light>();
        lit.type = LightType.Directional;
        lit.intensity = 1.1f;

        // --- desktop preview camera (replace with XR Origin for the real build) ---
        var camGO = new GameObject("PreviewCamera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(root);
        camGO.transform.position = new Vector3(0, 1.6f, -3f);
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<SimpleWalker>();
        Transform head = camGO.transform;

        // ====================================================================
        //  MANAGERS
        // ====================================================================
        var managers = new GameObject("Managers").transform;
        managers.SetParent(root);

        var audio = managers.gameObject.AddComponent<AudioDirector>();
        var mA = new GameObject("MusicA").AddComponent<AudioSource>(); mA.transform.SetParent(managers);
        var mB = new GameObject("MusicB").AddComponent<AudioSource>(); mB.transform.SetParent(managers);
        audio.musicA = mA; audio.musicB = mB;

        var rooms = managers.gameObject.AddComponent<RoomActivator>();
        managers.gameObject.AddComponent<PerformanceConfig>();

        var gm = managers.gameObject.AddComponent<GameManager>();
        gm.audio = audio;
        gm.rooms = rooms;

        managers.gameObject.AddComponent<DebugStageStepper>();

        // --- guide orb ---
        var orbGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orbGO.name = "GuideOrb";
        orbGO.transform.SetParent(root);
        orbGO.transform.localScale = Vector3.one * 0.2f;
        Paint(orbGO, _glow);
        var orb = orbGO.AddComponent<GuideOrb>();
        orb.playerHead = head;
        gm.guideOrb = orb;

        // ====================================================================
        //  ROOM SHELLS  (atrium z=0, ancient z=12, war z=24, modern z=36, reveal z=46)
        // ====================================================================
        var atrium  = MakeRoom(root, "Room_Atrium",  0f,  _stone);
        var ancient = MakeRoom(root, "Room_Ancient", 12f, _stone);
        var war     = MakeRoom(root, "Room_War",     24f, _metal);
        var modern  = MakeRoom(root, "Room_Modern",  36f, _steel);
        var reveal  = MakeRoom(root, "Room_Reveal",  46f, _steel);

        rooms.atrium = atrium.gameObject;
        rooms.ancient = ancient.gameObject;
        rooms.war = war.gameObject;
        rooms.modern = modern.gameObject;
        rooms.reveal = reveal.gameObject;

        // focus points for the guide orb
        gm.tutorialFocus = Empty(atrium,  "Focus", new Vector3(0, 1.5f, 1f));
        gm.caesarFocus   = Empty(ancient, "Focus", new Vector3(0, 1.2f, 12f));
        gm.rotorFocus    = Empty(war,     "Focus", new Vector3(0, 1.2f, 24f));
        gm.vaultFocus    = Empty(modern,  "Focus", new Vector3(0, 1.5f, 36f));
        gm.revealFocus   = Empty(reveal,  "Focus", new Vector3(0, 1.5f, 46f));

        // ====================================================================
        //  ATRIUM  — splash + tutorial
        // ====================================================================
        WorldText(atrium, "DECRYPTED", new Vector3(0, 2.6f, 3.8f), 1.4f, _white());
        WorldText(atrium, "A Walk Through the History of Secret Writing",
                  new Vector3(0, 2.1f, 3.8f), 0.35f, _white());

        var splashRoot = new GameObject("SplashRoot").transform;
        splashRoot.SetParent(atrium);
        WorldText(splashRoot, "PRESS  ENTER  TO  BEGIN", new Vector3(0, 1.3f, 2f), 0.5f, _white());
        var playBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playBtn.name = "PLAY_Button";
        playBtn.transform.SetParent(splashRoot);
        playBtn.transform.position = new Vector3(0, 0.9f, 2f);
        playBtn.transform.localScale = new Vector3(0.6f, 0.3f, 0.1f);
        Paint(playBtn, _glow);
        WorldText(splashRoot, "PLAY", new Vector3(0, 0.9f, 1.93f), 0.4f, Color.black);

        var splash = atrium.gameObject.AddComponent<SplashStart>();
        splash.splashRoot = splashRoot.gameObject;
        splash.audio = audio;
        splash.playButtonRenderer = playBtn.GetComponent<Renderer>();

        // tutorial cards
        var tut = atrium.gameObject.AddComponent<TutorialController>();
        tut.audio = audio;
        var card1 = TutorialCard(atrium, "HOW TO PLAY (1/2)\n\nPOINT + TRIGGER\nto press buttons");
        var card2 = TutorialCard(atrium, "HOW TO PLAY (2/2)\n\nGRAB + TWIST\nto turn wheels & dials");
        tut.panels = new GameObject[] { card1, card2 };
        HookEvent(gm.onTutorialBegin, tut.Begin);   // when tutorial stage begins, show cards

        // ====================================================================
        //  EXHIBIT 1 — Caesar wheel
        // ====================================================================
        WorldText(ancient, "EXHIBIT I — THE CAESAR CIPHER (c. 50 BC)",
                  new Vector3(0, 2.4f, 15.7f), 0.4f, _white());
        WorldText(ancient, "Turn the wheel until the message reads a real word.",
                  new Vector3(0, 2.0f, 15.7f), 0.28f, _white());

        var plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinth.name = "Plinth"; plinth.transform.SetParent(ancient);
        plinth.transform.position = new Vector3(0, 0.5f, 12f);
        plinth.transform.localScale = new Vector3(1f, 1f, 1f);
        Paint(plinth, _stone);

        var outerRing = MakeDisk(ancient, "OuterRing", new Vector3(0, 1.2f, 12f), 0.9f, _accent);
        var innerRing = MakeDisk(ancient, "InnerRing", new Vector3(0, 1.22f, 12f), 0.6f, _metal);

        var caesar = ancient.gameObject.AddComponent<CaesarWheel>();
        caesar.scrambledMessage = "URPH";
        caesar.targetWord = "ROME";
        caesar.innerRing = innerRing;
        caesar.audio = audio;
        caesar.scrambledDisplay = WorldText(ancient, "URPH", new Vector3(0, 1.7f, 12.6f), 0.4f, _white());
        caesar.decodedDisplay   = WorldText(ancient, "----", new Vector3(0, 1.45f, 12.6f), 0.4f, Color.cyan);
        caesar.shiftDisplay     = WorldText(ancient, "SHIFT 0", new Vector3(0, 1.25f, 12.6f), 0.25f, _white());

        // ====================================================================
        //  EXHIBIT 2 — rotor machine
        // ====================================================================
        WorldText(war, "EXHIBIT II — THE CIPHER MACHINE (1940s)",
                  new Vector3(0, 2.4f, 27.7f), 0.4f, _white());
        WorldText(war, "Set the dials to the KEY, then pull the lever.",
                  new Vector3(0, 2.0f, 27.7f), 0.28f, _white());
        WorldText(war, "INTERCEPTED  KEY:  M  A  C", new Vector3(-1.6f, 1.6f, 24f), 0.3f, Color.yellow);

        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "MachineBody"; body.transform.SetParent(war);
        body.transform.position = new Vector3(0, 1.0f, 24f);
        body.transform.localScale = new Vector3(2.2f, 0.6f, 1.2f);
        Paint(body, _metal);

        var rotor = war.gameObject.AddComponent<RotorMachine>();
        rotor.keyOfTheDay = "MAC";
        rotor.expectedPlaintext = "VICTORY";
        rotor.interceptedCipher = "ZLDFDQO";   // MAC encodes VICTORY -> ZLDFDQO
        rotor.audio = audio;

        // three dials + their spinning drums
        rotor.dialRight  = MakeDial(war, "Dial_Right",  new Vector3(-0.6f, 1.4f, 23.7f), audio, out var drumR);
        rotor.dialMiddle = MakeDial(war, "Dial_Middle", new Vector3( 0.0f, 1.4f, 23.7f), audio, out var drumM);
        rotor.dialLeft   = MakeDial(war, "Dial_Left",   new Vector3( 0.6f, 1.4f, 23.7f), audio, out var drumL);
        rotor.drumRight = drumR; rotor.drumMiddle = drumM; rotor.drumLeft = drumL;

        // lever
        var leverPivot = new GameObject("LeverPivot").transform;
        leverPivot.SetParent(war);
        leverPivot.position = new Vector3(1.3f, 1.3f, 24f);
        var leverArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leverArm.name = "LeverArm"; leverArm.transform.SetParent(leverPivot);
        leverArm.transform.localPosition = new Vector3(0, 0.25f, 0);
        leverArm.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
        Paint(leverArm, _accent);
        var lever = war.gameObject.AddComponent<RotorLever>();
        lever.leverArm = leverPivot;
        rotor.lever = lever;

        // lampboard (26 lamps, A..Z)
        var board = new GameObject("Lampboard").transform;
        board.SetParent(war);
        board.position = new Vector3(0, 1.35f, 24.4f);
        var lamp = war.gameObject.AddComponent<Lampboard>();
        lamp.lamps = new Renderer[26];
        for (int i = 0; i < 26; i++)
        {
            var l = GameObject.CreatePrimitive(PrimitiveType.Cube);
            l.name = "Lamp_" + (char)('A' + i);
            l.transform.SetParent(board);
            int col = i % 13, row = i / 13;
            l.transform.localPosition = new Vector3((col - 6) * 0.14f, 0.1f - row * 0.16f, 0);
            l.transform.localScale = Vector3.one * 0.1f;
            Paint(l, _lamp);
            lamp.lamps[i] = l.GetComponent<Renderer>();
        }
        rotor.lampboard = lamp;
        rotor.outputDisplay = WorldText(war, "", new Vector3(0, 1.75f, 24.4f), 0.45f, Color.green);

        // ====================================================================
        //  EXHIBIT 3 — vault + reveal
        // ====================================================================
        WorldText(modern, "EXHIBIT III — MODERN ENCRYPTION",
                  new Vector3(0, 2.4f, 39.7f), 0.4f, _white());

        var vaultGO = modern.gameObject.AddComponent<VaultDoor>();

        // door
        var doorPivot = new GameObject("DoorPivot").transform;
        doorPivot.SetParent(modern);
        doorPivot.position = new Vector3(-0.9f, 1.5f, 38.5f);
        var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "VaultDoor"; door.transform.SetParent(doorPivot);
        door.transform.localPosition = new Vector3(0.9f, 0, 0);
        door.transform.localScale = new Vector3(1.8f, 2.6f, 0.2f);
        Paint(door, _steel);
        vaultGO.door = doorPivot;

        // 3 lock rings (flattened cylinders standing in for rings)
        vaultGO.lockRings = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "LockRing_" + i;
            ring.transform.SetParent(modern);
            ring.transform.position = new Vector3(0, 1.5f, 38.4f);
            ring.transform.rotation = Quaternion.Euler(90, 0, 0);
            float s = 0.7f - i * 0.18f;
            ring.transform.localScale = new Vector3(s, 0.05f, s);
            Paint(ring, _accent);
            vaultGO.lockRings[i] = ring.transform;
        }

        // reveal room content
        vaultGO.revealRoom = reveal.gameObject;
        vaultGO.decodedWord = "VICTORY";
        vaultGO.revealWord = WorldText(reveal, "VICTORY", new Vector3(0, 1.8f, 49.5f), 1.2f, _white());
        WorldText(reveal, "Modern keys are millions of digits long —\nfar too large to ever crack by hand.",
                  new Vector3(0, 1.1f, 49.5f), 0.3f, _white());
        vaultGO.audio = audio;

        // when the Vault stage begins, open the vault
        HookEvent(gm.onVaultBegin, vaultGO.Open);

        // ====================================================================
        //  finish
        // ====================================================================
        Selection.activeTransform = root;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("<color=cyan>DECRYPTED greybox built.</color> Press Play, then ENTER to step through. " +
                  "Move with WASD + hold right-mouse to look.");
    }

    // ===================== helpers =====================

    static Transform MakeRoom(Transform parent, string name, float z, Material wallMat)
    {
        var room = new GameObject(name).transform;
        room.SetParent(parent);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor"; floor.transform.SetParent(room);
        floor.transform.position = new Vector3(0, 0, z);
        floor.transform.localScale = new Vector3(6, 0.2f, 10);
        Paint(floor, _dark);

        // back + 2 side walls (no front so you can walk in)
        MakeWall(room, new Vector3(0, 2, z + 5),  new Vector3(6, 4, 0.2f), wallMat);
        MakeWall(room, new Vector3(-3, 2, z),     new Vector3(0.2f, 4, 10), wallMat);
        MakeWall(room, new Vector3(3, 2, z),      new Vector3(0.2f, 4, 10), wallMat);
        return room;
    }

    static void MakeWall(Transform parent, Vector3 pos, Vector3 scale, Material m)
    {
        var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = "Wall"; w.transform.SetParent(parent);
        w.transform.position = pos; w.transform.localScale = scale;
        Paint(w, m);
    }

    static Transform MakeDisk(Transform parent, string name, Vector3 pos, float radius, Material m)
    {
        var d = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        d.name = name; d.transform.SetParent(parent);
        d.transform.position = pos;
        d.transform.rotation = Quaternion.Euler(90, 0, 0);  // face the player
        d.transform.localScale = new Vector3(radius, 0.05f, radius);
        Paint(d, m);
        return d.transform;
    }

    static RotorDial MakeDial(Transform parent, string name, Vector3 pos, AudioDirector audio, out Transform drum)
    {
        var dialGO = new GameObject(name);
        dialGO.transform.SetParent(parent);
        dialGO.transform.position = pos;

        var d = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        d.name = "Drum"; d.transform.SetParent(dialGO.transform);
        d.transform.localPosition = Vector3.zero;
        d.transform.rotation = Quaternion.Euler(90, 0, 0);
        d.transform.localScale = new Vector3(0.18f, 0.06f, 0.18f);
        Paint(d, _steel);
        drum = d.transform;

        var dial = dialGO.AddComponent<RotorDial>();
        dial.dialMesh = d.transform;
        dial.audio = audio;
        dial.letterLabel = WorldText(parent, "A", pos + new Vector3(0, 0.28f, 0), 0.25f, _white());
        return dial;
    }

    static GameObject TutorialCard(Transform parent, string msg)
    {
        var card = new GameObject("TutorialCard").transform;
        card.SetParent(parent);
        var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bg.name = "BG"; bg.transform.SetParent(card);
        bg.transform.position = new Vector3(0, 1.4f, 1.6f);
        bg.transform.localScale = new Vector3(1.6f, 1.0f, 0.05f);
        Paint(bg, _dark);
        WorldText(card, msg, new Vector3(0, 1.4f, 1.55f), 0.22f, _white());
        card.gameObject.SetActive(false);
        return card.gameObject;
    }

    static Transform Empty(Transform parent, string name, Vector3 worldPos)
    {
        var g = new GameObject(name).transform;
        g.SetParent(parent);
        g.position = worldPos;
        return g;
    }

    static TMP_Text WorldText(Transform parent, string s, Vector3 worldPos, float size, Color col)
    {
        var go = new GameObject("Text_" + (s.Length > 8 ? s.Substring(0, 8) : s));
        go.transform.SetParent(parent);
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = s;
        tmp.fontSize = size * 12f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = col;
        var rt = tmp.GetComponent<RectTransform>();
        rt.position = worldPos;
        rt.sizeDelta = new Vector2(6, 3);
        return tmp;
    }

    static void HookEvent(UnityEngine.Events.UnityEvent evt, UnityEngine.Events.UnityAction call)
    {
        UnityEventTools.AddPersistentListener(evt, call);
    }

    static void Paint(GameObject go, Material m)
    {
        var r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = m;
    }

    static Color _white() => new Color(0.95f, 0.95f, 0.92f);

    static void MakeMaterials()
    {
        _stone  = Lit(new Color(0.55f, 0.52f, 0.48f));
        _metal  = Lit(new Color(0.35f, 0.37f, 0.40f));
        _steel  = Lit(new Color(0.45f, 0.48f, 0.52f));
        _accent = Lit(new Color(0.70f, 0.30f, 0.20f));
        _dark   = Lit(new Color(0.15f, 0.15f, 0.17f));
        _purple = Lit(new Color(0.45f, 0.25f, 0.65f));
        _glow   = LitEmissive(new Color(0.2f, 1f, 0.7f));
        _lamp   = LitEmissive(new Color(1f, 0.85f, 0.4f));
    }

    static Material Lit(Color c)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        var m = new Material(sh);
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }

    static Material LitEmissive(Color c)
    {
        var m = Lit(c * 0.4f);
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c);
        return m;
    }
}
#endif
