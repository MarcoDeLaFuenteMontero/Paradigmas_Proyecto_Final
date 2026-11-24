using UnityEngine;

public class ChainedPerspectiveGate : MonoBehaviour
{
    [SerializeField] private RoomPerspectiveAlignment firstPuzzle;
    [SerializeField] private RoomPerspectiveAlignment secondPuzzle;
    [SerializeField] private Rigidbody firstBody;
    [SerializeField] private Collider grabCollider;
    [SerializeField] private Collider[] receiverColliders;
    [SerializeField] private Transform door;
    private Vector3 closedPosition;
    public bool IsUnlocked { get; private set; }

    private void Start()
    {
        if (door != null) closedPosition = door.localPosition;
        firstBody.isKinematic = true;
        firstBody.useGravity = false;
    }

    private void Update()
    {
        if (!IsUnlocked && firstPuzzle.IsMaterialized && secondPuzzle.IsMaterialized)
        {
            IsUnlocked = true;
            foreach (Collider receiver in receiverColliders) receiver.enabled = false;
            grabCollider.enabled = true;
            foreach (Transform child in firstBody.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = LayerMask.NameToLayer("Interactable");
            firstBody.isKinematic = false;
            firstBody.useGravity = true;
        }
        if (IsUnlocked && door != null)
            door.localPosition = Vector3.MoveTowards(door.localPosition,
                closedPosition + Vector3.up * 3f, 2.5f * Time.deltaTime);
    }
}
