using UnityEngine;

public class LinkedBookPuzzle : MonoBehaviour
{
    [SerializeField] private RoomPerspectiveAlignment yellowBook;
    [SerializeField] private RoomPerspectiveAlignment blueBook;
    [SerializeField] private Rigidbody yellowBody;
    [SerializeField] private Transform door;
    private Vector3 closedPosition;
    private BoxCollider yellowGrabCollider;
    private MeshCollider[] yellowProjectionColliders;
    public bool IsUnlocked { get; private set; }

    private void Start()
    {
        closedPosition = door.localPosition;
        // El primer libro debe mantenerse como superficie de la segunda ilusion.
        yellowBody.isKinematic = true;
        yellowBody.useGravity = false;
        yellowGrabCollider = yellowBody.GetComponent<BoxCollider>();
        yellowProjectionColliders = yellowBody.GetComponentsInChildren<MeshCollider>(true);
        if (yellowProjectionColliders.Length > 0)
        {
            yellowGrabCollider.enabled = false;
            foreach (var receiver in yellowProjectionColliders) receiver.enabled = true;
        }
        SetYellowLayer(LayerMask.NameToLayer("ProjectionSurface"));
    }

    private void Update()
    {
        if (!IsUnlocked && yellowBook.IsMaterialized && blueBook.IsMaterialized)
        {
            IsUnlocked = true;
            if (yellowProjectionColliders != null && yellowProjectionColliders.Length > 0)
            {
                foreach (var receiver in yellowProjectionColliders) receiver.enabled = false;
                yellowGrabCollider.enabled = true;
            }
            SetYellowLayer(LayerMask.NameToLayer("Interactable"));
            yellowBody.isKinematic = false;
            yellowBody.useGravity = true;
        }
        if (IsUnlocked)
            door.localPosition = Vector3.MoveTowards(door.localPosition,
                closedPosition + Vector3.up * 3.0f, 2.5f * Time.deltaTime);
    }

    private void SetYellowLayer(int layer)
    {
        foreach (Transform child in yellowBody.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = layer;
    }
}
