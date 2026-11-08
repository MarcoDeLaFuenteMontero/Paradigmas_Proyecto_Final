
using UnityEngine;

[ExecuteAlways]
public class PerspectiveProjector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera projectionCamera;
    [SerializeField] private Material projectionMaterial;

    private static readonly int ProjectionVP =
        Shader.PropertyToID("_ProjectionVP");

    private void LateUpdate()
    {
        UpdateProjectionMatrix();
    }

    private void UpdateProjectionMatrix()
    {
        if (projectionCamera == null ||
            projectionMaterial == null)
            return;

        Matrix4x4 viewMatrix =
            projectionCamera.worldToCameraMatrix;

        Matrix4x4 projectionMatrix =
            GL.GetGPUProjectionMatrix(
                projectionCamera.projectionMatrix,
                true
            );

        Matrix4x4 viewProjectionMatrix =
            projectionMatrix * viewMatrix;

        projectionMaterial.SetMatrix(
            ProjectionVP,
            viewProjectionMatrix
        );
    }
}
