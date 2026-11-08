
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectInteraction : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Interaccion")]
    [SerializeField] private float grabDistance = 4f;
    [SerializeField] private float maxPlacementDistance = 12f;

    [Header("Escalado")]
    [SerializeField] private float minScale = 0.25f;
    [SerializeField] private float maxScale = 5f;

    private Rigidbody heldRigidbody;
    private Collider heldCollider;
    private Transform heldObject;

    private Vector3 originalScale;
    private float initialDistance;
    private float originalRadius;

    private void Update()
    {
        if (Mouse.current == null || playerCamera == null)
            return;

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (heldObject == null)
                GrabObject();
            else
                DropObject();
        }
    }

    private void LateUpdate()
    {
        if (heldObject != null)
            UpdateHeldObject();
    }

    private void GrabObject()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, grabDistance, interactableLayer))
            return;

        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null)
            return;

        heldRigidbody = rb;
        heldObject = rb.transform;
        heldCollider = hit.collider;

        originalScale = heldObject.localScale;

        initialDistance = Vector3.Distance(playerCamera.transform.position, heldObject.position);

        initialDistance = Mathf.Max(initialDistance, 0.1f);

        originalRadius = heldCollider.bounds.extents.magnitude;

        heldRigidbody.isKinematic = true;
        heldRigidbody.useGravity = false;

        // Evitar que el raycast choque con el objeto agarrado
        heldCollider.enabled = false;
    }

    private void UpdateHeldObject()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        float surfaceDistance = maxPlacementDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            surfaceDistance = hit.distance;
        }

        // Mantener el objeto delante de la superficie
        float targetDistance = surfaceDistance / (1f + originalRadius / initialDistance);

        targetDistance = Mathf.Clamp(targetDistance, 0.75f, maxPlacementDistance);

        // Escalar proporcionalmente a la distancia
        float scaleFactor = targetDistance / initialDistance;

        scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale);

        heldObject.localScale = originalScale * scaleFactor;

        // Posicionar el objeto siguiendo la camara
        heldObject.position = ray.GetPoint(targetDistance);
    }

    private void DropObject()
    {
        heldCollider.enabled = true;

        heldRigidbody.isKinematic = false;
        heldRigidbody.useGravity = true;

        heldObject = null;
        heldRigidbody = null;
        heldCollider = null;
    }
}

