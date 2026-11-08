
using UnityEngine;

public class PerspectiveAlignment : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform alignmentPoint;
    [SerializeField] private GameObject wallDrawing;
    [SerializeField] private GameObject hiddenObject3D;

    [Header("Tolerancias")]
    [SerializeField] private float positionTolerance = 0.6f;
    [SerializeField] private float angleTolerance = 15f;

    private bool materialized = false;

    private void Start()
    {
        if (playerCamera == null ||
            alignmentPoint == null ||
            wallDrawing == null ||
            hiddenObject3D == null)
        {
            Debug.LogError(
                "PerspectiveAlignment: faltan referencias.",
                this
            );

            enabled = false;
            return;
        }

        // Estado inicial del puzle
        wallDrawing.SetActive(true);
        hiddenObject3D.SetActive(false);
    }

    private void Update()
    {
        if (materialized)
            return;

        CheckAlignment();
    }

    private void CheckAlignment()
    {
        // Distancia entre la camara y el punto correcto
        float distance = Vector3.Distance(
            playerCamera.transform.position,
            alignmentPoint.position
        );

        // Diferencia entre las orientaciones
        float angle = Quaternion.Angle(
            playerCamera.transform.rotation,
            alignmentPoint.rotation
        );

        // Comprobar ambas condiciones
        if (distance <= positionTolerance &&
            angle <= angleTolerance)
        {
            Materialize();
        }
    }

    private void Materialize()
    {
        materialized = true;

        // Ocultar el dibujo de la pared
        wallDrawing.SetActive(false);

        // Materializar el objeto 3D
        hiddenObject3D.SetActive(true);

        Debug.Log("Objeto materializado correctamente");
    }
}
