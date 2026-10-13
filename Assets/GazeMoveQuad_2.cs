using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.InputSystem;

public class GazeMoveQuad_2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform referenceCamera;
    [SerializeField] private InputActionAsset inputActions;

    [Header("Gaze Movement")]
    [SerializeField] private float distance = 1.0f;
    [SerializeField] private float maxHorizontalAngle = 25.0f;
    [SerializeField] private float maxVerticalAngle = 25.0f;

    [Header("Right Thumbstick Position Adjustment")]
    [SerializeField] private float stickMoveSpeed = 0.20f;
    [SerializeField] private float maxHorizontalOffset = 0.30f;
    [SerializeField] private float maxVerticalOffset = 0.20f;
    [SerializeField] private float stickDeadzone = 0.20f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.05f;

    private UnityEngine.XR.InputDevice eyeDevice;
    private InputAction rightStickAction;

    // Persistent offset selected with the right thumbstick.
    private Vector2 controllerOffset = Vector2.zero;

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        FindEyeDevice();
        FindRightStickAction();
    }

    private void Update()
    {
        if (referenceCamera == null)
            return;

        if (!eyeDevice.isValid)
            FindEyeDevice();

        UpdateControllerOffset();

        // Gaze movement is zero if eye tracking is unavailable.
        float gazeX = 0f;
        float gazeY = 0f;

        if (eyeDevice.isValid &&
            eyeDevice.TryGetFeatureValue(
                EyeTrackingUsages.gazeRotation,
                out Quaternion gazeRotation))
        {
            Vector3 worldDirection =
                gazeRotation * Vector3.forward;

            Vector3 localDirection =
                referenceCamera.InverseTransformDirection(
                    worldDirection);

            if (localDirection.z > 0.01f)
            {
                float horizontalAngle =
                    Mathf.Atan2(
                        localDirection.x,
                        localDirection.z) * Mathf.Rad2Deg;

                float verticalAngle =
                    Mathf.Atan2(
                        localDirection.y,
                        localDirection.z) * Mathf.Rad2Deg;

                horizontalAngle = Mathf.Clamp(
                    horizontalAngle,
                    -maxHorizontalAngle,
                    maxHorizontalAngle);

                verticalAngle = Mathf.Clamp(
                    verticalAngle,
                    -maxVerticalAngle,
                    maxVerticalAngle);

                gazeX =
                    Mathf.Tan(horizontalAngle * Mathf.Deg2Rad)
                    * distance;

                gazeY =
                    Mathf.Tan(verticalAngle * Mathf.Deg2Rad)
                    * distance;
            }
        }

        // Combine controller-selected position with gaze movement.
        Vector3 targetPosition = new Vector3(
            controllerOffset.x + gazeX,
            controllerOffset.y + gazeY,
            distance);

        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetPosition,
            ref velocity,
            smoothTime);
    }

    private void UpdateControllerOffset()
    {
        if (rightStickAction == null)
            FindRightStickAction();

        if (rightStickAction == null)
            return;

        Vector2 stick = rightStickAction.ReadValue<Vector2>();

        if (Mathf.Abs(stick.x) < stickDeadzone)
            stick.x = 0f;

        if (Mathf.Abs(stick.y) < stickDeadzone)
            stick.y = 0f;

        controllerOffset.x = Mathf.Clamp(
            controllerOffset.x +
            stick.x * stickMoveSpeed * Time.deltaTime,
            -maxHorizontalOffset,
            maxHorizontalOffset);

        controllerOffset.y = Mathf.Clamp(
            controllerOffset.y +
            stick.y * stickMoveSpeed * Time.deltaTime,
            -maxVerticalOffset,
            maxVerticalOffset);
    }

    private void FindRightStickAction()
    {
        if (inputActions == null || rightStickAction != null)
            return;

        rightStickAction =
            inputActions.FindAction("XRI Right/Thumbstick", false);

        if (rightStickAction != null)
            rightStickAction.Enable();
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

    private void OnDisable()
    {
        if (rightStickAction != null)
            rightStickAction.Disable();

        rightStickAction = null;
    }
}