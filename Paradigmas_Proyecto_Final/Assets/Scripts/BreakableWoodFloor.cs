using UnityEngine;

// The apple must be enlarged and dropped: resting weight alone is insufficient.
public class BreakableWoodFloor : MonoBehaviour
{
    [SerializeField] private Rigidbody apple;
    [SerializeField] private float minimumMass = 40f;
    [SerializeField] private float minimumVerticalSpeed = 3.5f;
    [SerializeField] private GameObject boards;
    [SerializeField] private Collider floorCollider;
    [SerializeField] private GameObject returnDoor;
    public bool IsBroken { get; private set; }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsBroken || collision.rigidbody != apple || apple == null) return;
        if (apple.mass < minimumMass || Mathf.Abs(collision.relativeVelocity.y) < minimumVerticalSpeed) return;
        bool topImpact = false;
        for (int i = 0; i < collision.contactCount; i++)
            if (Mathf.Abs(collision.GetContact(i).normal.y) > .65f) topImpact = true;
        if (!topImpact) return;
        IsBroken = true;
        floorCollider.enabled = false;
        boards.SetActive(false);
        if (returnDoor != null) returnDoor.SetActive(false);
    }
}
