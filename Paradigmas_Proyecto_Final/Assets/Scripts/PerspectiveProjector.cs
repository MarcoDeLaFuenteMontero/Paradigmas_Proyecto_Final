
using UnityEngine;

[ExecuteAlways]
public class PerspectiveProjector : MonoBehaviour
{
    [Header("Camaras")]
    [SerializeField] private Camera projectionCamera;
    [SerializeField] private Camera projectionDepthCamera;

    [Header("Proyeccion")]
    [SerializeField] private Material projectionMaterial;
    [SerializeField] private RenderTexture depthTexture;

    private static readonly int ProjectionVP =
        Shader.PropertyToID("_ProjectionVP");

    private static readonly int ProjectionView =
        Shader.PropertyToID("_ProjectionView");

    private static readonly int ProjectionFar =
        Shader.PropertyToID("_ProjectionFar");

    private static readonly int ProjectionDepthTex =
        Shader.PropertyToID("_ProjectionDepthTex");

    private void LateUpdate()
    {
        UpdateProjection();
    }

    private void UpdateProjection()
    {
        if (projectionCamera == null ||
            projectionDepthCamera == null ||
            projectionMaterial == null ||
            depthTexture == null)
        {
            return;
        }

        // Sincronizar la camara de profundidad
        projectionDepthCamera.transform.SetPositionAndRotation(
            projectionCamera.transform.position,
            projectionCamera.transform.rotation
        );

        projectionDepthCamera.fieldOfView =
            projectionCamera.fieldOfView;

        projectionDepthCamera.orthographic =
            projectionCamera.orthographic;

        projectionDepthCamera.nearClipPlane =
            projectionCamera.nearClipPlane;

        projectionDepthCamera.farClipPlane =
            projectionCamera.farClipPlane;

        projectionDepthCamera.aspect =
            projectionCamera.aspect;

        projectionDepthCamera.projectionMatrix =
            projectionCamera.projectionMatrix;

        // Matriz de vista de la camara de referencia
        Matrix4x4 viewMatrix =
            projectionCamera.worldToCameraMatrix;

        // Matriz de proyeccion para Render Texture
        Matrix4x4 projectionMatrix =
            GL.GetGPUProjectionMatrix(
                projectionCamera.projectionMatrix,
                true
            );

        Matrix4x4 viewProjectionMatrix =
            projectionMatrix * viewMatrix;

        // Enviar datos al Shader
        projectionMaterial.SetMatrix(
            ProjectionVP,
            viewProjectionMatrix
        );

        projectionMaterial.SetMatrix(
            ProjectionView,
            viewMatrix
        );

        projectionMaterial.SetFloat(
            ProjectionFar,
            projectionCamera.farClipPlane
        );

        projectionMaterial.SetTexture(
            ProjectionDepthTex,
            depthTexture
        );
    }
}

