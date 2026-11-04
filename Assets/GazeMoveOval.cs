
using UnityEngine;
using UnityEngine.InputSystem;

public class GazeMoveOval : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform referenceCamera;
    [SerializeField] private InputActionAsset inputActions;

    [Header("Gaze Movement")]
    [SerializeField] private float distance = 1.0f;
    [SerializeField] private float maxHorizontalAngle = 25f;
    [SerializeField] private float maxVerticalAngle = 25f;

    [Header("Right Thumbstick Position Adjustment")]
    [SerializeField] private float stickMoveSpeed = 0.20f;
    [SerializeField] private float maxHorizontalOffset = 0.30f;
    [SerializeField] private float maxVerticalOffset = 0.20f;
    [SerializeField] private float stickDeadzone = 0.20f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.05f;

    private InputAction gazeRotationAction;
    private InputAction rightStickAction;

    private Vector2 controllerOffset = Vector2.zero;
    private Vector3 velocity = Vector3.zero;

    private bool warnedAboutGaze;

    private void OnEnable()
    {
        // OpenXR eye-gaze rotation binding.
        if (gazeRotationAction == null)
        {
            gazeRotationAction = new InputAction(
                name: "Oval Eye Gaze",
                type: InputActionType.Value,
                binding: "<EyeGaze>/pose/rotation",
                expectedControlType: "Quaternion"
            );
        }

        gazeRotationAction.Enable();

        FindRightStickAction();
    }

    private void Update()
    {
        if (referenceCamera == null)
            return;

        FindRightStickAction();
        UpdateControllerOffset();

        float gazeX = 0f;
        float gazeY = 0f;

        if (TryGetGazeDirection(out Vector3 localDirection))
        {
            float horizontalAngle =
                Mathf.Atan2(
                    localDirection.x,
                    localDirection.z
                ) * Mathf.Rad2Deg;

            float verticalAngle =
                Mathf.Atan2(
                    localDirection.y,
                    localDirection.z
                ) * Mathf.Rad2Deg;

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

            gazeX =
                Mathf.Tan(horizontalAngle * Mathf.Deg2Rad)
                * distance;

            gazeY =
                Mathf.Tan(verticalAngle * Mathf.Deg2Rad)
                * distance;
        }

        // Combine eye-gaze movement and the right-controller offset.
        Vector3 targetPosition = new Vector3(
            controllerOffset.x + gazeX,
            controllerOffset.y + gazeY,
            distance
        );

        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    private bool TryGetGazeDirection(out Vector3 localDirection)
    {
        localDirection = Vector3.forward;

        if (gazeRotationAction == null ||
            gazeRotationAction.controls.Count == 0)
        {
            if (!warnedAboutGaze)
            {
                Debug.LogWarning(
                    "GazeMoveOval: EyeGaze rotation binding unavailable. " +
                    "Check the Eye Gaze Interaction Profile and permission."
                );

                warnedAboutGaze = true;
            }

            return false;
        }

        Quaternion gazeRotation =
            gazeRotationAction.ReadValue<Quaternion>();

        // Convert gaze direction from XR tracking space to world space.
        Vector3 trackingDirection =
            gazeRotation * Vector3.forward;

        Vector3 worldDirection =
            referenceCamera.root.TransformDirection(
                trackingDirection
            );

        // Convert into Main Camera local coordinates.
        localDirection =
            referenceCamera.InverseTransformDirection(
                worldDirection
            );

        return localDirection.z > 0.01f;
    }

    private void UpdateControllerOffset()
    {
        if (rightStickAction == null)
            return;

        Vector2 stick =
            rightStickAction.ReadValue<Vector2>();

        if (Mathf.Abs(stick.x) < stickDeadzone)
            stick.x = 0f;

        if (Mathf.Abs(stick.y) < stickDeadzone)
            stick.y = 0f;

        controllerOffset.x = Mathf.Clamp(
            controllerOffset.x +
            stick.x * stickMoveSpeed * Time.deltaTime,
            -maxHorizontalOffset,
            maxHorizontalOffset
        );

        controllerOffset.y = Mathf.Clamp(
            controllerOffset.y +
            stick.y * stickMoveSpeed * Time.deltaTime,
            -maxVerticalOffset,
            maxVerticalOffset
        );
    }

    private void FindRightStickAction()
    {
        if (inputActions == null || rightStickAction != null)
            return;

        rightStickAction =
            inputActions.FindAction(
                "XRI Right/Thumbstick",
                false
            );

        if (rightStickAction != null)
            rightStickAction.Enable();
    }

    private void OnDisable()
    {
        if (gazeRotationAction != null)
            gazeRotationAction.Disable();

        if (rightStickAction != null)
            rightStickAction.Disable();
    }

    private void OnDestroy()
    {
        if (gazeRotationAction != null)
        {
            gazeRotationAction.Dispose();
            gazeRotationAction = null;
        }
    }
}
