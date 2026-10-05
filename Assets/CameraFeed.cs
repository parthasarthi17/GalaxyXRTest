using UnityEngine;
using UnityEngine.Android;

public class CameraFeed : MonoBehaviour
{
    [SerializeField]
    private Renderer targetRenderer;

    private WebCamTexture webcam;

    private void Start()
    {
        // Ask Android for camera permission if necessary.
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        // Wait one frame after requesting permission.
        Invoke(nameof(StartCamera), 0.5f);
    }

    private void StartCamera()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.LogError("Camera permission was denied.");
            return;
        }

        WebCamDevice[] devices = WebCamTexture.devices;

        Debug.Log("Number of cameras found: " + devices.Length);

        if (devices.Length == 0)
        {
            Debug.LogError("No camera devices found.");
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log(
                $"Camera {i}: {devices[i].name} | " +
                $"Front-facing: {devices[i].isFrontFacing}"
            );
        }

        // First camera for the initial test.
        webcam = new WebCamTexture(
            devices[0].name,
            1280,
            720,
            30
        );

        webcam.Play();

        targetRenderer.material.mainTexture = webcam;

        Debug.Log(
            "Started camera: " + devices[0].name
        );
    }

    private void Update()
    {
        if (webcam == null || !webcam.isPlaying)
            return;

        // Rotate image so the camera feed is oriented correctly.
        targetRenderer.transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                -webcam.videoRotationAngle
            );

        // Correct vertical mirroring.
        Vector3 scale = targetRenderer.transform.localScale;

        scale.y = webcam.videoVerticallyMirrored
            ? -Mathf.Abs(scale.y)
            : Mathf.Abs(scale.y);

        targetRenderer.transform.localScale = scale;
    }

    private void OnDestroy()
    {
        if (webcam != null && webcam.isPlaying)
        {
            webcam.Stop();
        }
    }
}