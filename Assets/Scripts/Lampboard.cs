// ============================================================================
//  DECRYPTED  —  Lampboard  (part of Exhibit 2)
// ----------------------------------------------------------------------------
//  26 letter lamps. When the rotor machine outputs a letter, that lamp glows.
//  Glow is done by toggling an emissive color through a MaterialPropertyBlock
//  on an OPAQUE material -> zero extra draw calls, zero transparency, perfect
//  for Quest 1. (Lit transparency / bloom is exactly what we are avoiding.)
//
//  Lamps are stored A..Z in array order. Index 0 = A.
// ============================================================================

using System.Collections;
using UnityEngine;

public class Lampboard : MonoBehaviour
{
    [Header("Exactly 26 lamp renderers, in order A..Z")]
    public Renderer[] lamps = new Renderer[26];

    [Header("Look")]
    public Color litColor = new Color(1f, 0.85f, 0.4f);
    public float litIntensity = 1.5f;
    public float flashHold = 0.18f;
    public float fadeTime = 0.25f;

    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        AllOff();
    }

    public void Flash(char letter)
    {
        int i = char.ToUpper(letter) - 'A';
        if (i < 0 || i >= lamps.Length || lamps[i] == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(i));
    }

    IEnumerator FlashRoutine(int i)
    {
        SetLamp(i, litIntensity);
        yield return new WaitForSeconds(flashHold);
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            SetLamp(i, Mathf.Lerp(litIntensity, 0f, t / fadeTime));
            yield return null;
        }
        SetLamp(i, 0f);
    }

    void SetLamp(int i, float intensity)
    {
        lamps[i].GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissionId, litColor * intensity);
        lamps[i].SetPropertyBlock(_mpb);
    }

    public void AllOff()
    {
        for (int i = 0; i < lamps.Length; i++)
            if (lamps[i] != null) SetLamp(i, 0f);
    }
}
