// ============================================================================
//  DECRYPTED  —  Cipher Logic (pure C#, no Unity dependencies)
// ----------------------------------------------------------------------------
//  This is the mathematical heart of the museum. It is intentionally written
//  as plain C# with NO Unity references so the team can unit-test it, and so a
//  judge in the semifinal interview can read it and immediately understand the
//  algorithms. Everything here is ORIGINAL code. The historical rotor wirings
//  are public-domain historical facts (the actual WWII rotor permutations),
//  reproduced here as data, not as copyrighted assets.
//
//  Two ciphers are implemented:
//    1. Caesar shift        -> Exhibit 1 (Ancient cryptography)
//    2. Reciprocal rotor    -> Exhibit 2 (WWII cipher machine)
//
//  KEY PROPERTY of the rotor machine: it is RECIPROCAL. Running plaintext
//  through it at key K produces ciphertext; running that ciphertext back
//  through it at the SAME key K reproduces the plaintext. That is exactly how
//  the real machines worked, and it is why the puzzle "set the dials, run the
//  message, watch it decode" makes sense to a judge.
// ============================================================================

using System.Text;

public static class CipherLogic
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // --- Historical rotor wirings (public-domain fact, used here as data) ---
    public static readonly string RotorI    = "EKMFLGDQVZNTOWYHXUSPAIBRCJ";
    public static readonly string RotorII   = "AJDKSIRUXBLHWTMCQGZNPYFVOE";
    public static readonly string RotorIII  = "BDFHJLCPRTXVZNYEIWGAKMUSQO";
    public static readonly string ReflectorB = "YRUHQSLDPXNGOKMIEBFZCWVJAT";

    // ------------------------------------------------------------------------
    //  CAESAR CIPHER  (Exhibit 1)
    // ------------------------------------------------------------------------
    static int Mod(int a, int m) => ((a % m) + m) % m;

    public static char CaesarChar(char c, int shift)
    {
        if (c < 'A' || c > 'Z') c = char.ToUpper(c);
        if (c < 'A' || c > 'Z') return c;            // leave non-letters alone
        return (char)('A' + Mod((c - 'A') + shift, 26));
    }

    public static string CaesarString(string s, int shift)
    {
        var sb = new StringBuilder(s.Length);
        foreach (char c in s) sb.Append(CaesarChar(c, shift));
        return sb.ToString();
    }

    // ------------------------------------------------------------------------
    //  ROTOR MACHINE  (Exhibit 2)  — three rotors + reflector, reciprocal
    // ------------------------------------------------------------------------

    // Signal travelling RIGHT->LEFT through one rotor sitting at offset 'pos'.
    static int Forward(int i, int pos, string wiring)
    {
        int shifted = Mod(i + pos, 26);
        int w = wiring[shifted] - 'A';
        return Mod(w - pos, 26);
    }

    // Signal travelling LEFT->RIGHT back through the same rotor (inverse wiring).
    static int Backward(int i, int pos, string wiring)
    {
        int shifted = Mod(i + pos, 26);
        char target = (char)('A' + shifted);
        int j = wiring.IndexOf(target);   // inverse of the permutation
        return Mod(j - pos, 26);
    }

    // Encode ONE character through the full machine at the given rotor offsets.
    // Because of the reflector, this function is an involution:
    //     EnigmaChar(EnigmaChar(c, pos), pos) == c
    public static char EnigmaChar(char c, int[] pos)
    {
        int i = char.ToUpper(c) - 'A';

        // forward: right rotor (III) -> middle (II) -> left (I)
        i = Forward(i, pos[0], RotorIII);
        i = Forward(i, pos[1], RotorII);
        i = Forward(i, pos[2], RotorI);

        // reflector turns the signal around
        i = ReflectorB[i] - 'A';

        // back out: left (I) -> middle (II) -> right (III)
        i = Backward(i, pos[2], RotorI);
        i = Backward(i, pos[1], RotorII);
        i = Backward(i, pos[0], RotorIII);

        return (char)('A' + i);
    }

    // Advance the rotors one step (classic odometer: right rotor every key).
    // Stepping depends only on how many keys have been pressed, never on the
    // letters, which is what guarantees encrypt/decrypt symmetry.
    public static void Step(int[] pos)
    {
        pos[0] = Mod(pos[0] + 1, 26);
        if (pos[0] == 0)
        {
            pos[1] = Mod(pos[1] + 1, 26);
            if (pos[1] == 0)
                pos[2] = Mod(pos[2] + 1, 26);
        }
    }

    // Process a whole message. Pass plaintext -> get ciphertext.
    // Pass that ciphertext back at the same startPos -> get plaintext.
    public static string EnigmaProcess(string text, int[] startPos)
    {
        int[] pos = (int[])startPos.Clone();
        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            if (c < 'A' || char.ToUpper(c) > 'Z' || !char.IsLetter(c))
            {
                sb.Append(c);          // spaces / punctuation pass straight through
                continue;
            }
            Step(pos);                 // step BEFORE encoding, like the real machine
            sb.Append(EnigmaChar(c, pos));
        }
        return sb.ToString();
    }

    // Convenience: turn a 3-letter key like "MAC" into rotor offsets {12,0,2}.
    public static int[] KeyToPositions(string threeLetters)
    {
        threeLetters = threeLetters.ToUpper();
        return new int[]
        {
            threeLetters[0] - 'A',
            threeLetters[1] - 'A',
            threeLetters[2] - 'A'
        };
    }
}
