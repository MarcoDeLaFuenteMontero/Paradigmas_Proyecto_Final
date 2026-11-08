
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProjectionOcclusionBaker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera projectionCamera;
    [SerializeField] private Material projectionMaterial;
    [SerializeField] private LayerMask projectionSurfaceMask;

    [Header("Bake Settings")]
    [SerializeField, Range(64, 2048)]
    private int resolution = 512;

    [SerializeField, Min(0.001f)]
    private float occlusionTolerance = 0.05f;

#if UNITY_EDITOR
    [ContextMenu("Bake Projection Occlusion")]
    private void BakeProjectionOcclusion()
    {
        if (projectionCamera == null || projectionMaterial == null)
        {
            Debug.LogError("Missing projection camera or material.");
            return;
        }

        if (projectionCamera.orthographic)
        {
            Debug.LogError(
                "This baker currently supports perspective cameras only."
            );
            return;
        }

        if (projectionSurfaceMask.value == 0)
        {
            Debug.LogError("Projection Surface Mask is empty.");
            return;
        }

        Matrix4x4 gpuProjection = GL.GetGPUProjectionMatrix(
            projectionCamera.projectionMatrix,
            true
        );

        Matrix4x4 vp =
            gpuProjection * projectionCamera.worldToCameraMatrix;

        Matrix4x4 inverseVP = vp.inverse;

        Texture2D positionTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBAFloat,
            false,
            true
        );

        positionTexture.filterMode = FilterMode.Point;
        positionTexture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[resolution * resolution];

        Vector3 cameraPosition =
            projectionCamera.transform.position;

        int hitCount = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (x + 0.5f) / resolution;
                float v = (y + 0.5f) / resolution;

                Vector4 clipPosition = new Vector4(
                    u * 2f - 1f,
                    v * 2f - 1f,
                    0.5f,
                    1f
                );

                Vector4 worldH =
                    inverseVP * clipPosition;

                if (Mathf.Abs(worldH.w) < 0.000001f)
                    continue;

                Vector3 worldPosition = new Vector3(
                    worldH.x / worldH.w,
                    worldH.y / worldH.w,
                    worldH.z / worldH.w
                );

                Vector3 direction =
                    (worldPosition - cameraPosition).normalized;

                Ray ray = new Ray(
                    cameraPosition,
                    direction
                );

                int index = y * resolution + x;

                if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    projectionCamera.farClipPlane,
                    projectionSurfaceMask,
                    QueryTriggerInteraction.Ignore
                ))
                {
                    Vector3 p = hit.point;

                    pixels[index] = new Color(
                        p.x,
                        p.y,
                        p.z,
                        1f
                    );

                    hitCount++;
                }
                else
                {
                    pixels[index] = Color.clear;
                }
            }
        }

        positionTexture.SetPixels(pixels);
        positionTexture.Apply(false, false);

        string path = AssetDatabase.GenerateUniqueAssetPath(
            "Assets/RT_ProjectionOcclusion.asset"
        );

        AssetDatabase.CreateAsset(positionTexture, path);

        projectionMaterial.SetTexture(
            "_OcclusionTex",
            positionTexture
        );

        projectionMaterial.SetFloat(
            "_OcclusionTolerance",
            occlusionTolerance
        );

        projectionMaterial.SetFloat(
            "_OcclusionEnabled",
            1f
        );

        EditorUtility.SetDirty(projectionMaterial);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Occlusion bake completed: {hitCount} hits. " +
            $"Texture saved at {path}"
        );
    }
#endif
}
