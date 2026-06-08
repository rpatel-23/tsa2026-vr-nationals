// ============================================================================
//  DECRYPTED  —  CipherCodegen  (Editor-only utility)
// ----------------------------------------------------------------------------
//  Tools > DECRYPTED > Cipher Codegen
//
//  Lets the team type a secret word + a 3-letter key and instantly get:
//     - the Caesar scrambled message (for Exhibit 1)
//     - the rotor ciphertext (for Exhibit 2's interceptedCipher field)
//  so you can change the puzzle's answer for a fresh demo without editing any
//  scripts. Editor-only: it is stripped from the final headset build.
// ============================================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CipherCodegen : EditorWindow
{
    string caesarPlain = "ROME";
    int caesarShift = 3;

    string rotorPlain = "VICTORY";
    string rotorKey = "MAC";

    string caesarResult = "";
    string rotorResult = "";

    [MenuItem("Tools/DECRYPTED/Cipher Codegen")]
    static void Open() => GetWindow<CipherCodegen>("Cipher Codegen");

    void OnGUI()
    {
        GUILayout.Label("Caesar Cipher (Exhibit 1)", EditorStyles.boldLabel);
        caesarPlain = EditorGUILayout.TextField("Plain word", caesarPlain).ToUpper();
        caesarShift = EditorGUILayout.IntSlider("Shift", caesarShift, 1, 25);
        if (GUILayout.Button("Generate Caesar"))
            caesarResult = CipherLogic.CaesarString(caesarPlain, caesarShift);
        EditorGUILayout.HelpBox(
            string.IsNullOrEmpty(caesarResult) ? "—" :
            $"scrambledMessage = {caesarResult}\ntargetWord = {caesarPlain}",
            MessageType.Info);

        GUILayout.Space(16);
        GUILayout.Label("Rotor Machine (Exhibit 2)", EditorStyles.boldLabel);
        rotorPlain = EditorGUILayout.TextField("Secret word", rotorPlain).ToUpper();
        rotorKey = EditorGUILayout.TextField("Key (3 letters)", rotorKey).ToUpper();
        if (GUILayout.Button("Generate Cipher"))
        {
            if (rotorKey.Length == 3)
                rotorResult = CipherLogic.EnigmaProcess(rotorPlain, CipherLogic.KeyToPositions(rotorKey));
            else
                rotorResult = "KEY MUST BE 3 LETTERS";
        }
        EditorGUILayout.HelpBox(
            string.IsNullOrEmpty(rotorResult) ? "—" :
            $"interceptedCipher = {rotorResult}\nexpectedPlaintext = {rotorPlain}\nkeyOfTheDay = {rotorKey}",
            MessageType.Info);
    }
}
#endif
