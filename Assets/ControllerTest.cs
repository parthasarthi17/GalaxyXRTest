
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControllerTest : MonoBehaviour
{
    public TMP_Text outputText;
    public InputActionAsset inputActions;
    public ConcaveScreen screen;

    [Header("Size Controls")]
    public float startingAngle = 50f;
    public float minimumAngle = 20f;
    public float maximumAngle = 70f;
    public float degreesPerSecond = 25f;
    public float stickDeadzone = 0.2f;

    private InputAction leftStickAction;
    private Vector3 originalScale;
    private float requestedAngle;

    private void Start()
    {
        requestedAngle = startingAngle;

        if (screen != null)
            originalScale = screen.transform.localScale;

        FindStickAction();
    }

    private void FindStickAction()
    {
        if (inputActions == null || leftStickAction != null)
            return;

        leftStickAction =
            inputActions.FindAction("XRI Left/Thumbstick", false);

        if (leftStickAction != null)
            leftStickAction.Enable();
    }

    private void Update()
    {
        FindStickAction();

        if (leftStickAction == null)
        {
            if (outputText != null)
                outputText.text = "Left thumbstick action not found";
            return;
        }

        Vector2 stick = leftStickAction.ReadValue<Vector2>();

        if (screen != null)
        {
            if (Mathf.Abs(stick.y) > stickDeadzone)
            {
                requestedAngle +=
                    stick.y * degreesPerSecond * Time.deltaTime;

                requestedAngle = Mathf.Clamp(
                    requestedAngle, minimumAngle, maximumAngle);
            }

            float scaleFactor =
                Mathf.Tan(requestedAngle * 0.5f * Mathf.Deg2Rad) /
                Mathf.Tan(startingAngle * 0.5f * Mathf.Deg2Rad);

            screen.transform.localScale = originalScale * scaleFactor;
        }

        if (outputText != null)
        {
            outputText.text =
                "LEFT THUMBSTICK\n\n" +
                "X: " + stick.x.ToString("F2") + "\n" +
                "Y: " + stick.y.ToString("F2") + "\n\n" +
                "Screen size: " + requestedAngle.ToString("F1") + "\u00B0";
        }
    }

    private void OnDisable()
    {
        if (leftStickAction != null)
            leftStickAction.Disable();

        leftStickAction = null;
    }
}
