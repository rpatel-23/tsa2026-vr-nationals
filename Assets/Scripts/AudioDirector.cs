// ============================================================================
//  DECRYPTED  —  AudioDirector
// ----------------------------------------------------------------------------
//  Central audio brain. Crossfades between original music zones as the visitor
//  moves through the museum and plays one-shot SFX through a small voice pool
//  (so overlapping clicks never spawn unbounded AudioSources).
//
//  RULES NOTE: the rubric requires sound but FORBIDS voiceover narration that
//  describes the simulation. Every clip here is music, ambience, or a mechanical
//  sound effect. There is no spoken narration anywhere. All audio must be
//  ORIGINAL (composed by the team) or properly licensed free/open assets — see
//  the build guide's audio section for the asset list and where it gets cited.
// ============================================================================

using UnityEngine;

public class AudioDirector : MonoBehaviour
{
    public enum Zone { Atrium, Ancient, War, Modern, Reveal }

    [System.Serializable]
    public class ZoneTrack
    {
        public Zone zone;
        public AudioClip music;       // original looping track for this room
        [Range(0, 1)] public float volume = 0.6f;
    }

    [Header("One looping music track per zone")]
    public ZoneTrack[] zoneTracks;

    [Header("Two music sources for crossfading")]
    public AudioSource musicA;
    public AudioSource musicB;
    public float crossfadeTime = 1.5f;

    [Header("SFX voice pool")]
    public int sfxVoices = 8;
    [Range(0, 1)] public float sfxVolume = 0.8f;

    AudioSource[] _sfx;
    int _sfxIndex;
    bool _useA = true;
    float _fadeT;
    bool _fading;
    float _fromVol, _toVol;

    void Awake()
    {
        _sfx = new AudioSource[sfxVoices];
        for (int i = 0; i < sfxVoices; i++)
        {
            var go = new GameObject("SFX_" + i);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;     // 2D; keep it simple and cheap
            _sfx[i] = src;
        }
        if (musicA != null) { musicA.loop = true; musicA.volume = 0; }
        if (musicB != null) { musicB.loop = true; musicB.volume = 0; }
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        var s = _sfx[_sfxIndex];
        _sfxIndex = (_sfxIndex + 1) % _sfx.Length;
        s.Stop();
        s.clip = clip;
        s.volume = sfxVolume;
        s.Play();
    }

    public void SetZone(Zone zone)
    {
        ZoneTrack track = null;
        foreach (var z in zoneTracks) if (z.zone == zone) { track = z; break; }
        if (track == null || track.music == null) return;

        AudioSource incoming = _useA ? musicB : musicA;
        incoming.clip = track.music;
        incoming.volume = 0;
        incoming.Play();
        _toVol = track.volume;
        _useA = !_useA;
        _fading = true;
        _fadeT = 0;
    }

    void Update()
    {
        if (!_fading) return;
        _fadeT += Time.deltaTime / crossfadeTime;
        float k = Mathf.Clamp01(_fadeT);

        AudioSource incoming = _useA ? musicA : musicB;
        AudioSource outgoing = _useA ? musicB : musicA;

        if (incoming != null) incoming.volume = Mathf.Lerp(0, _toVol, k);
        if (outgoing != null) outgoing.volume = Mathf.Lerp(outgoing.volume, 0, k);

        if (k >= 1f)
        {
            _fading = false;
            if (outgoing != null) outgoing.Stop();
        }
    }
}
