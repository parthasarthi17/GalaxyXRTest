using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class GazeMoveQuad : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform referenceCamera;

    [Header("Movement")]
    [SerializeField] private float distance = 1.0f;
    [SerializeField] private float maxHorizontalAngle = 25.0f;
    [SerializeField] private float maxVerticalAngle = 25.0f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.05f;

    private InputDevice eyeDevice;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        FindEyeDevice();
    }

    private void Update()
    {
        if (!eyeDevice.isValid)
            FindEyeDevice();

        if (!eyeDevice.isValid)
            return;

        if (!eyeDevice.TryGetFeatureValue(
                EyeTrackingUsages.gazeRotation,
                out Quaternion gazeRotation))
        {
            return;
        }

        // Gaze direction in world space.
        Vector3 worldDirection =
            gazeRotation * Vector3.forward;

        // Convert gaze direction into camera-local coordinates.
        Vector3 localDirection =
            referenceCamera.InverseTransformDirection(worldDirection);

        if (localDirection.z <= 0.01f)
            return;

        // Calculate horizontal and vertical gaze angles.
        float horizontalAngle =
            Mathf.Atan2(localDirection.x, localDirection.z)
            * Mathf.Rad2Deg;

        float verticalAngle =
            Mathf.Atan2(localDirection.y, localDirection.z)
            * Mathf.Rad2Deg;

        // Limit the movement area.
        horizontalAngle = Mathf.Clamp(
            horizontalAngle,
            -maxHorizontalAngle,
            maxHorizontalAngle
        );

        verticalAngle = Mathf.Clamp(
            verticalAngle,
            -maxVerticalAngle,
            maxVerticalAngle
        );

        // Where the gaze ray intersects a plane at "distance".
        float x =
            Mathf.Tan(horizontalAngle * Mathf.Deg2Rad)
            * distance;

        float y =
            Mathf.Tan(verticalAngle * Mathf.Deg2Rad)
            * distance;

        Vector3 targetPosition = new Vector3(
            x,
            y,
            distance
        );

        // Smooth movement so eye jitter doesn't make the Quad jump.
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    private void FindEyeDevice()
    {
        var devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.EyeTracking,
            devices
        );

        if (devices.Count > 0)
        {
            eyeDevice = devices[0];

            Debug.Log(
                "GazeMoveQuad found eye device: " +
                eyeDevice.name
            );
        }
    }
}