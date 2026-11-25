using UnityEngine;

// The small fixed door materializes beside the column; a separate full-size door is revealed.
public class RevealDoorBehindSmallDoor : MonoBehaviour
{
    [SerializeField] private RoomPerspectiveAlignment smallDoorAlignment;
    [SerializeField] private GameObject realDoor;
    [SerializeField] private GameObject blockedOpening;
    [SerializeField] private Transform leafHinge;
    private bool revealed;

    private void Start()
    {
        realDoor.SetActive(false);
        blockedOpening.SetActive(true);
        leafHinge.localRotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (revealed || !smallDoorAlignment.IsMaterialized) return;
        realDoor.SetActive(true);
        leafHinge.localRotation = Quaternion.Euler(0, -100, 0);
        blockedOpening.SetActive(false);
        revealed = true;
    }
}
