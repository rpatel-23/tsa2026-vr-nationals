// ============================================================================
//  DECRYPTED  —  SimpleWalker  (preview-only, NOT for the final VR build)
// ----------------------------------------------------------------------------
//  A plain desktop fly/walk camera so you can explore the greybox museum in
//  Play mode without a headset. The real submission uses an XR Origin instead;
//  this is purely a learning/testing tool.
//
//  Controls:  W A S D = move    Q / E = down / up
//             Hold RIGHT MOUSE  = look around
//             Shift             = move faster
// ============================================================================

using UnityEngine;

public class SimpleWalker : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float fastMultiplier = 2.5f;
    public float lookSpeed = 2f;

    float _yaw, _pitch;

    void Start()
    {
        Vector3 e = transform.eulerAngles;
        _yaw = e.y; _pitch = e.x;
    }

    void Update()
    {
        // Look (hold right mouse)
        if (Input.GetMouseButton(1))
        {
            _yaw   += Input.GetAxis("Mouse X") * lookSpeed;
            _pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            _pitch  = Mathf.Clamp(_pitch, -89f, 89f);
            transform.eulerAngles = new Vector3(_pitch, _yaw, 0);
        }

        // Move
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += transform.forward;
        if (Input.GetKey(KeyCode.S)) dir -= transform.forward;
        if (Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.A)) dir -= transform.right;
        if (Input.GetKey(KeyCode.E)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) dir += Vector3.down;

        transform.position += dir.normalized * speed * Time.deltaTime;
    }
}
