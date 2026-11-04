using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class GazeImageCrop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Transform referenceCamera;

    [Header("Image Crop")]
    [Range(0.1f, 1f)]
    [SerializeField] private float visibleWidthFraction = 0.8f;

    [Range(0.1f, 1f)]
    [SerializeField] private float visibleHeightFraction = 0.8f;

    [SerializeField] private float sourceMinimum = -100f;
    [SerializeField] private float sourceMaximum = 100f;

    [Header("Gaze Mapping")]
    [SerializeField] private float gazeShiftGain = 0.5f;

    private UnityEngine.XR.InputDevice eyeDevice;
    private Mesh displayMesh;
    private Vector2[] originalUVs;
    private Vector2[] workingUVs;

    private IEnumerator Start()
    {
        // Allow the curved screen mesh to be generated first.
        yield return null;

        if (targetRenderer == null || referenceCamera == null)
        {
            Debug.LogError(
                "GazeImageCrop: Assign Target Renderer and Reference Camera.");
            enabled = false;
            yield break;
        }

        MeshFilter meshFilter =
            targetRenderer.GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError(
                "GazeImageCrop: Target Renderer has no MeshFilter.");
            enabled = false;
            yield break;
        }

        displayMesh = meshFilter.mesh;
        originalUVs = displayMesh.uv;

        if (originalUVs == null || originalUVs.Length == 0)
        {
            Debug.LogError("GazeImageCrop: Mesh has no UV coordinates.");
            enabled = false;
            yield break;
        }

        workingUVs = new Vector2[originalUVs.Length];

        FindEyeDevice();
        ApplyCrop(0f, 0f);
    }

    private void Update()
    {
        if (displayMesh == null)
            return;

        if (!eyeDevice.isValid)
            FindEyeDevice();

        if (!eyeDevice.isValid)
            return;

        if (!eyeDevice.TryGetFeatureValue(
            EyeTrackingUsages.gazeRotation,
            out Quaternion gazeRotation))
            return;

        Vector3 worldDirection =
            gazeRotation * Vector3.forward;

        Vector3 localDirection =
            referenceCamera.InverseTransformDirection(worldDirection);

        if (localDirection.z <= 0.01f)
            return;

        // Horizontal and vertical gaze angles.
        float gazeAngleX =
            Mathf.Atan2(localDirection.x, localDirection.z)
            * Mathf.Rad2Deg;

        float gazeAngleY =
            Mathf.Atan2(localDirection.y, localDirection.z)
            * Mathf.Rad2Deg;

        ApplyCrop(gazeAngleX, gazeAngleY);
    }

    private void ApplyCrop(float gazeAngleX, float gazeAngleY)
    {
        float totalSpan = sourceMaximum - sourceMinimum;

        if (totalSpan <= 0f)
            return;

        float width = Mathf.Clamp(visibleWidthFraction, 0.1f, 1f);
        float height = Mathf.Clamp(visibleHeightFraction, 0.1f, 1f);

        // Maximum permitted UV offset in each direction.
        float maxShiftX = (1f - width) * 0.5f;
        float maxShiftY = (1f - height) * 0.5f;

        // Convert gaze angles into normalized texture offsets.
        float shiftX = Mathf.Clamp(
            gazeAngleX * gazeShiftGain / totalSpan,
            -maxShiftX, maxShiftX);

        float shiftY = Mathf.Clamp(
            gazeAngleY * gazeShiftGain / totalSpan,
            -maxShiftY, maxShiftY);

        float offsetX = maxShiftX + shiftX;
        float offsetY = maxShiftY + shiftY;

        // Crop and shift the image in BOTH directions.
        for (int i = 0; i < originalUVs.Length; i++)
        {
            workingUVs[i] = new Vector2(
                originalUVs[i].x * width + offsetX,
                originalUVs[i].y * height + offsetY
            );
        }

        displayMesh.uv = workingUVs;
    }

    private void FindEyeDevice()
    {
        var devices = new List<UnityEngine.XR.InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.EyeTracking,
            devices);

        if (devices.Count > 0)
            eyeDevice = devices[0];
    }

    private void OnDestroy()
    {
        if (displayMesh != null && originalUVs != null)
            displayMesh.uv = originalUVs;
    }
}