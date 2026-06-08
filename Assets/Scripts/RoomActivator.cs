// ============================================================================
//  DECRYPTED  —  RoomActivator
// ----------------------------------------------------------------------------
//  The single most important performance script. The museum is a straight hall,
//  but we NEVER render the whole hall at once. Only the room the visitor is in
//  (plus a thin "doorway" peek into the next) is active. Everything else is
//  SetActive(false), so its meshes, lights and audio cost nothing.
//
//  This is what makes a multi-room museum viable on a Snapdragon 835.
// ============================================================================

using UnityEngine;

public class RoomActivator : MonoBehaviour
{
    public enum Room { Atrium, Ancient, War, Modern, Reveal }

    [Header("Drag each room's parent GameObject here")]
    public GameObject atrium;
    public GameObject ancient;
    public GameObject war;
    public GameObject modern;
    public GameObject reveal;

    [Header("Optional: keep the NEXT room's doorway shell visible for continuity")]
    public bool peekNextRoom = false;

    void Start()
    {
        ShowOnly(Room.Atrium);
    }

    public void ShowOnly(Room room)
    {
        Set(atrium, room == Room.Atrium);
        Set(ancient, room == Room.Ancient);
        Set(war,     room == Room.War);
        Set(modern,  room == Room.Modern || room == Room.Reveal);
        Set(reveal,  room == Room.Reveal);
    }

    static void Set(GameObject go, bool on)
    {
        if (go != null && go.activeSelf != on) go.SetActive(on);
    }
}
