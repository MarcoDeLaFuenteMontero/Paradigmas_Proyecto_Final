using System.Collections.Generic;
using UnityEngine;

public class WeightedPressurePlate : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private float minimumMass = 8f;
    [SerializeField] private float playerMass = 80f;
    [SerializeField] private Vector3 sensingSize = new Vector3(1.12f, 0.34f, 1.12f);
    private Vector3 closedDoor;
    private Vector3 releasedButton;
    private readonly HashSet<Rigidbody> weighedBodies = new HashSet<Rigidbody>();
    public bool IsPressed { get; private set; }
    public float CurrentMass { get; private set; }

    private void Start()
    {
        closedDoor = door.localPosition;
        releasedButton = buttonVisual.localPosition;
    }

    private void FixedUpdate()
    {
        weighedBodies.Clear();
        CurrentMass = 0f;
        bool playerCounted = false;
        float surface = transform.position.y + 0.103f;
        Vector3 centre = transform.position + Vector3.up * (0.103f + sensingSize.y * 0.5f);
        foreach (Collider col in Physics.OverlapBox(centre, sensingSize * 0.5f,
            transform.rotation, ~0, QueryTriggerInteraction.Ignore))
        {
            if (col.bounds.min.y > surface + 0.15f) continue;
            CharacterController player = col as CharacterController;
            if (player != null && !playerCounted)
            {
                CurrentMass += playerMass;
                playerCounted = true;
                continue;
            }
            Rigidbody body = col.attachedRigidbody;
            // Los objetos sujetos en la mano no presionan la placa.
            if (body == null || body.isKinematic || !body.useGravity || !weighedBodies.Add(body)) continue;
            CurrentMass += body.mass;
        }
        IsPressed = CurrentMass >= minimumMass;
        Vector3 destination = closedDoor + (IsPressed ? Vector3.up * 3f : Vector3.zero);
        door.localPosition = Vector3.MoveTowards(door.localPosition, destination, 3.5f * Time.fixedDeltaTime);
        buttonVisual.localPosition = releasedButton + (IsPressed ? Vector3.down * 0.025f : Vector3.zero);
    }
}
