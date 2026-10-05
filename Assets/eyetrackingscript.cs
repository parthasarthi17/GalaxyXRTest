//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.XR;
//using UnityEngine.XR.OpenXR.Features.Interactions;

//public class EyeTrackingScript : MonoBehaviour
//{
//    [SerializeField] private TMP_Text eyeText;

//    private InputDevice eyeDevice;

//    private static readonly InputFeatureUsage<Vector3> GazePosition =
//        EyeTrackingUsages.gazePosition;

//    private static readonly InputFeatureUsage<Quaternion> GazeRotation =
//        EyeTrackingUsages.gazeRotation;

//    void Start()
//    {
//        FindEyeDevice();
//    }

//    void Update()
//    {
//        if (!eyeDevice.isValid)
//        {
//            FindEyeDevice();
//        }

//        if (!eyeDevice.isValid)
//        {
//            eyeText.text = "Eye tracking device NOT FOUND";
//            return;
//        }

//        bool gotPosition =
//            eyeDevice.TryGetFeatureValue(GazePosition, out Vector3 position);

//        bool gotRotation =
//            eyeDevice.TryGetFeatureValue(GazeRotation, out Quaternion rotation);

//        if (!gotPosition || !gotRotation)
//        {
//            eyeText.text =
//                $"Eye device found:\n{eyeDevice.name}\n\n" +
//                "But gaze data unavailable.";
//            return;
//        }

//        Vector3 gazeDirection = rotation * Vector3.forward;

//        eyeText.text =
//            $"EYE TRACKING WORKING\n\n" +
//            $"Device:\n{eyeDevice.name}\n\n" +
//            $"Gaze Position:\n" +
//            $"X = {position.x:F4}\n" +
//            $"Y = {position.y:F4}\n" +
//            $"Z = {position.z:F4}\n\n" +
//            $"Gaze Direction:\n" +
//            $"X = {gazeDirection.x:F4}\n" +
//            $"Y = {gazeDirection.y:F4}\n" +
//            $"Z = {gazeDirection.z:F4}\n\n" +
//            $"Rotation:\n" +
//            $"{rotation.eulerAngles}";
//    }

//    void FindEyeDevice()
//    {
//        var devices = new List<InputDevice>();

//        InputDevices.GetDevicesWithCharacteristics(
//            InputDeviceCharacteristics.EyeTracking,
//            devices);

//        if (devices.Count > 0)
//        {
//            eyeDevice = devices[0];
//            Debug.Log("FOUND EYE DEVICE: " + eyeDevice.name);
//        }
//        else
//        {
//            eyeDevice = default;
//        }
//    }
//}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class EyeTrackingScript : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text eyeText;

    [Header("Gaze Ray")]
    [SerializeField] private LineRenderer gazeLine;
    [SerializeField] private float rayLength = 3.0f;

    private InputDevice eyeDevice;

    private void Start()
    {
        FindEyeDevice();

        if (gazeLine != null)
        {
            gazeLine.useWorldSpace = true;
            gazeLine.positionCount = 2;
            gazeLine.startWidth = 0.005f;
            gazeLine.endWidth = 0.005f;
            gazeLine.enabled = false;
        }
    }

    private void Update()
    {
        // Try again if the device wasn't found yet.
        if (!eyeDevice.isValid)
        {
            FindEyeDevice();
        }

        if (!eyeDevice.isValid)
        {
            if (eyeText != null)
                eyeText.text = "EYE TRACKING DEVICE NOT FOUND";

            if (gazeLine != null)
                gazeLine.enabled = false;

            return;
        }

        // ---------------------------------------------------------
        // GET GAZE POSITION
        // ---------------------------------------------------------

        bool gotPosition =
            eyeDevice.TryGetFeatureValue(
                EyeTrackingUsages.gazePosition,
                out Vector3 gazePosition);

        // ---------------------------------------------------------
        // GET GAZE ROTATION
        // ---------------------------------------------------------

        bool gotRotation =
            eyeDevice.TryGetFeatureValue(
                EyeTrackingUsages.gazeRotation,
                out Quaternion gazeRotation);

        if (!gotPosition || !gotRotation)
        {
            if (eyeText != null)
            {
                eyeText.text =
                    "EYE DEVICE FOUND\n\n" +
                    "But gaze data is unavailable.";
            }

            if (gazeLine != null)
                gazeLine.enabled = false;

            return;
        }

        // ---------------------------------------------------------
        // CONVERT ROTATION TO GAZE DIRECTION
        // ---------------------------------------------------------

        Vector3 gazeDirection =
            gazeRotation * Vector3.forward;

        // ---------------------------------------------------------
        // DISPLAY DATA
        // ---------------------------------------------------------

        if (eyeText != null)
        {
            eyeText.text =
                "EYE TRACKING WORKING\n\n" +

                "GAZE POSITION\n" +
                $"X = {gazePosition.x:F4}\n" +
                $"Y = {gazePosition.y:F4}\n" +
                $"Z = {gazePosition.z:F4}\n\n" +

                "GAZE DIRECTION\n" +
                $"X = {gazeDirection.x:F4}\n" +
                $"Y = {gazeDirection.y:F4}\n" +
                $"Z = {gazeDirection.z:F4}\n\n" +

                "ROTATION\n" +
                $"X = {gazeRotation.eulerAngles.x:F2}\n" +
                $"Y = {gazeRotation.eulerAngles.y:F2}\n" +
                $"Z = {gazeRotation.eulerAngles.z:F2}";
        }

        // ---------------------------------------------------------
        // DRAW GAZE RAY
        // ---------------------------------------------------------

        if (gazeLine != null)
        {
            gazeLine.enabled = true;

            gazeLine.SetPosition(0, gazePosition);

            gazeLine.SetPosition(
                1,
                gazePosition + gazeDirection * rayLength
            );
        }
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
                "Eye tracking device found: " +
                eyeDevice.name
            );
        }
    }
}