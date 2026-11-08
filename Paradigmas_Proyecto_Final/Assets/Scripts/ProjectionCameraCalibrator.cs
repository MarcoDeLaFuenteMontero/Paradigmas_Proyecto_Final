
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ProjectionCameraCalibrator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera projectionCamera;

    [System.Serializable]
    private class CalibrationData
    {
        public Vector3 position;
        public Quaternion rotation;
    }

#if UNITY_EDITOR
    private string StorageKey =>
        "PerspectiveCalibration_" +
        Application.dataPath + "_" +
        gameObject.scene.path + "_" +
        gameObject.name;
#endif

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            Calibrate();
        }
    }

    private void Calibrate()
    {
        if (playerCamera == null || projectionCamera == null)
        {
            Debug.LogError("Calibration: Missing camera references.");
            return;
        }

        Vector3 position = playerCamera.transform.position;
        Quaternion rotation = playerCamera.transform.rotation;

        projectionCamera.transform.SetPositionAndRotation(
            position,
            rotation
        );

#if UNITY_EDITOR
        CalibrationData data = new CalibrationData
        {
            position = position,
            rotation = rotation
        };

        EditorPrefs.SetString(
            StorageKey,
            JsonUtility.ToJson(data)
        );
#endif

        Debug.Log(
            $"Projection camera calibrated. Position: {position}, Rotation: {rotation.eulerAngles}"
        );
    }

#if UNITY_EDITOR
    [ContextMenu("Apply Saved Calibration")]
    private void ApplySavedCalibration()
    {
        if (projectionCamera == null)
        {
            Debug.LogError("Projection camera is missing.");
            return;
        }

        if (!EditorPrefs.HasKey(StorageKey))
        {
            Debug.LogWarning("No saved calibration found.");
            return;
        }

        CalibrationData data = JsonUtility.FromJson<CalibrationData>(
            EditorPrefs.GetString(StorageKey)
        );

        Undo.RecordObject(
            projectionCamera.transform,
            "Apply Projection Camera Calibration"
        );

        projectionCamera.transform.SetPositionAndRotation(
            data.position,
            data.rotation
        );

        EditorUtility.SetDirty(projectionCamera.transform);

        EditorSceneManager.MarkSceneDirty(gameObject.scene);

        Debug.Log("Saved projection camera calibration applied.");
    }
#endif
}
