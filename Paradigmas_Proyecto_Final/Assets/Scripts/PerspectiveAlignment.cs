
using UnityEngine;

public class PerspectiveAlignment : MonoBehaviour
{
    [Header("Camaras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera projectionCamera;

    [Header("Objetos")]
    [SerializeField] private GameObject referenceObject3D;
    [SerializeField] private GameObject hiddenObject3D;

    [Header("Tolerancias")]
    [SerializeField] private float positionTolerance = 0.15f;
    [SerializeField] private float angleTolerance = 3f;

    private static readonly int ProjectionVisibility =
        Shader.PropertyToID("_PerspectiveProjectionVisibility");

    private bool materialized = false;

    private void Start()
    {
        if (playerCamera == null ||
            projectionCamera == null ||
            referenceObject3D == null ||
            hiddenObject3D == null)
        {
            Debug.LogError(
                "PerspectiveAlignment: faltan referencias.",
                this
            );

            enabled = false;
            return;
        }

        materialized = false;

        // Estado inicial: proyeccion visible
        referenceObject3D.SetActive(true);
        hiddenObject3D.SetActive(false);

        SetProjectionVisibility(true);
    }

    private void LateUpdate()
    {
        if (materialized)
            return;

        CheckAlignment();
    }

    private void CheckAlignment()
    {
        // Distancia entre ambas camaras
        float distance = Vector3.Distance(
            playerCamera.transform.position,
            projectionCamera.transform.position
        );

        // Diferencia entre sus orientaciones
        float angle = Quaternion.Angle(
            playerCamera.transform.rotation,
            projectionCamera.transform.rotation
        );

        if (distance <= positionTolerance &&
            angle <= angleTolerance)
        {
            Materialize();
        }
    }

    private void Materialize()
    {
        materialized = true;

        // Copiar la transformacion del objeto de referencia
        hiddenObject3D.transform.SetPositionAndRotation(
            referenceObject3D.transform.position,
            referenceObject3D.transform.rotation
        );

        hiddenObject3D.transform.localScale =
            referenceObject3D.transform.lossyScale;

        // Ocultar la proyeccion
        SetProjectionVisibility(false);

        // Ocultar el objeto de referencia
        referenceObject3D.SetActive(false);

        // Activar el objeto fisico
        hiddenObject3D.SetActive(true);

        Debug.Log("Perspective puzzle solved: object materialized.");
    }

    private void SetProjectionVisibility(bool visible)
    {
        Shader.SetGlobalFloat(
            ProjectionVisibility,
            visible ? 1f : 0f
        );
    }

    private void OnDisable()
    {
        // Restaurar el Shader cuando se desactiva el componente
        SetProjectionVisibility(true);
    }
}
