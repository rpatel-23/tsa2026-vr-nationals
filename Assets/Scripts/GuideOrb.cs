// ============================================================================
//  DECRYPTED  —  GuideOrb
// ----------------------------------------------------------------------------
//  Non-verbal wayfinding. A small glowing orb drifts to the next thing the
//  visitor should touch and bobs gently with a soft looping hum, so a judge is
//  never lost and never needs a narrator telling them where to go (which the
//  rules forbid). When an exhibit is finished, GameManager moves the orb to the
//  next focus point.
//
//  Cheap to render: one small opaque sphere with emission + one halo quad.
//  No particle systems, no transparency stacks.
// ============================================================================

using UnityEngine;

public class GuideOrb : MonoBehaviour
{
    [Header("Movement")]
    public float followSpeed = 1.5f;
    public float bobAmplitude = 0.05f;
    public float bobSpeed = 2f;

    [Header("Look-at (optional: keep the orb facing the player)")]
    public Transform playerHead;

    [Header("Audio")]
    public AudioSource hum;          // looping, low volume, original ambient tone
    public AudioClip arriveChime;    // soft chime when it reaches a new target

    Transform _target;
    Vector3 _baseOffset;

    void Start() => _baseOffset = Vector3.zero;

    public void MoveTo(Transform target)
    {
        _target = target;
        if (arriveChime != null && hum != null)
            hum.PlayOneShot(arriveChime);
    }

    void Update()
    {
        if (_target != null)
        {
            Vector3 bob = Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = Vector3.Lerp(
                transform.position,
                _target.position + bob,
                followSpeed * Time.deltaTime);
        }

        if (playerHead != null)
        {
            Vector3 dir = playerHead.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(-dir);
        }
    }
}
