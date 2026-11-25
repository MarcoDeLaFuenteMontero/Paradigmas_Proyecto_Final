using UnityEngine;

// Only the puzzle-room door; the main house entrance remains static scenery.
public class MaterializedDoorway : MonoBehaviour
{
    [SerializeField] private RoomPerspectiveAlignment alignment;
    [SerializeField] private GameObject blockedOpening;
    [SerializeField] private Transform leafHinge;
    private bool opened;
    private void Update()
    {
        if (opened || alignment == null || !alignment.IsMaterialized) return;
        blockedOpening.SetActive(false);
        leafHinge.localRotation = Quaternion.Euler(0, -100, 0);
        opened = true;
    }
}
