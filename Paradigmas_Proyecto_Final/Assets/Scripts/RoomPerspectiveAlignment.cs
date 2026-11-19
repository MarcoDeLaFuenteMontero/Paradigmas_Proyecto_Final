using UnityEngine;

// Mantiene la visibilidad de cada acertijo en su propio material.
public class RoomPerspectiveAlignment : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera projectionCamera;
    [SerializeField] private GameObject referenceObject;
    [SerializeField] private GameObject physicalObject;
    [SerializeField] private Material projectionMaterial;
    [SerializeField] private float positionTolerance = 0.15f;
    [SerializeField] private float angleTolerance = 8f;
    [SerializeField] private RoomPerspectiveAlignment prerequisite;

    public bool IsMaterialized { get; private set; }

    private void Start()
    {
        if (playerCamera == null || projectionCamera == null || referenceObject == null
            || physicalObject == null || projectionMaterial == null)
        {
            Debug.LogError("RoomPerspectiveAlignment: faltan referencias.", this);
            enabled = false;
            return;
        }

        IsMaterialized = false;
        projectionCamera.enabled = true;
        referenceObject.SetActive(true);
        physicalObject.SetActive(false);
        projectionMaterial.SetFloat("_RoomProjectionVisibility", 1f);
    }

    private void LateUpdate()
    {
        if (IsMaterialized) return;
        if (prerequisite != null && !prerequisite.IsMaterialized) return;
        if (Vector3.Distance(playerCamera.transform.position, projectionCamera.transform.position)
            > positionTolerance) return;
        if (Quaternion.Angle(playerCamera.transform.rotation, projectionCamera.transform.rotation)
            > angleTolerance) return;

        physicalObject.transform.SetPositionAndRotation(referenceObject.transform.position,
            referenceObject.transform.rotation);
        physicalObject.transform.localScale = referenceObject.transform.lossyScale;
        projectionMaterial.SetFloat("_RoomProjectionVisibility", 0f);
        referenceObject.SetActive(false);
        physicalObject.SetActive(true);
        projectionCamera.enabled = false;
        IsMaterialized = true;
    }

    private void OnDisable()
    {
        if (projectionMaterial != null)
            projectionMaterial.SetFloat("_RoomProjectionVisibility", 0f);
    }
}
