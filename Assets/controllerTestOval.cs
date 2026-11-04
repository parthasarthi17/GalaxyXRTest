//using UnityEngine;
//using UnityEngine.InputSystem;
//using TMPro;

//public class ControllerTestOval : MonoBehaviour
//{
//    [Header("Debug Output")]
//    public TMP_Text outputText;

//    [Header("Controller Input")]
//    public InputActionAsset inputActions;

//    [Header("Screen to Resize")]
//    public Transform screen;

//    [Header("Fisheye")]
//    public OvalScreen ovalScreen;
//    public float fisheyeDegreesPerSecond = 20f;

//    [Header("Size Controls")]
//    public float startingAngle = 50f;
//    public float minimumAngle = 20f;
//    public float maximumAngle = 70f;
//    public float degreesPerSecond = 25f;
//    public float stickDeadzone = 0.2f;

//    private InputAction leftStickAction;
//    private InputAction modeButtonAction;

//    private Vector3 originalScale;
//    private float requestedAngle;
//    private float baselineAngle;

//    private void Start()
//    {
//        minimumAngle = Mathf.Max(1f, minimumAngle);
//        maximumAngle = Mathf.Max(minimumAngle, maximumAngle);

//        baselineAngle = Mathf.Clamp(startingAngle, minimumAngle, maximumAngle);
//        requestedAngle = baselineAngle;

//        if (screen != null)
//        {
//            originalScale = screen.localScale;
//            ApplyScreenScale();
//        }
//        else
//        {
//            Debug.LogError("ControllerTestOval: Screen Transform is not assigned.");
//        }

//        if (ovalScreen == null)
//            Debug.LogError("ControllerTestOval: OvalScreen is not assigned.");

//        FindStickAction();

//        // Left controller X button (primary button) cycles the fisheye mode.
//        //modeButtonAction = new InputAction(
//        //    "FisheyeMode",
//        //    InputActionType.Button,
//        //    "<XRController>{LeftHand}/{PrimaryButton}");

//        modeButtonAction = new InputAction("FisheyeMode", InputActionType.Button);
//        modeButtonAction.AddBinding("<XRController>{LeftHand}/primaryButton");      // X
//        modeButtonAction.AddBinding("<XRController>{LeftHand}/{PrimaryButton}");    // X
//        modeButtonAction.AddBinding("<XRController>{LeftHand}/secondaryButton");    // Y
//        modeButtonAction.AddBinding("<XRController>{LeftHand}/{SecondaryButton}");  // Y
//        modeButtonAction.Enable();

//        //modeButtonAction.Enable();


//    }

//    private void FindStickAction()
//    {
//        if (inputActions == null || leftStickAction != null)
//            return;

//        leftStickAction = inputActions.FindAction("XRI Left/Thumbstick", false);

//        if (leftStickAction != null)
//            leftStickAction.Enable();
//        else
//            Debug.LogError("ControllerTestOval: XRI Left/Thumbstick action not found.");
//    }

//    private void Update()
//    {
//        FindStickAction();

//        if (leftStickAction == null)
//        {
//            if (outputText != null)
//                outputText.text = "Left thumbstick action not found";
//            return;
//        }

//        Vector2 stick = leftStickAction.ReadValue<Vector2>();

//        // Stick up/down: screen size.
//        if (screen != null && Mathf.Abs(stick.y) > stickDeadzone)
//        {
//            requestedAngle = Mathf.Clamp(
//                requestedAngle + stick.y * degreesPerSecond * Time.deltaTime,
//                minimumAngle,
//                maximumAngle);

//            ApplyScreenScale(); // only when the size changes
//        }

//        if (ovalScreen != null)
//        {
//            // Stick left/right: fisheye strength.
//            if (Mathf.Abs(stick.x) > stickDeadzone)
//            {
//                ovalScreen.SetFisheyeStrength(
//                    ovalScreen.Strength + stick.x * fisheyeDegreesPerSecond * Time.deltaTime);
//            }

//            // X button: Off -> Equidistant -> Equisolid -> Off.
//            if (modeButtonAction != null && modeButtonAction.WasPressedThisFrame())
//            {
//                ovalScreen.SetFisheyeMode(((int)ovalScreen.Mode + 1) % 3);
//            }
//        }

//        if (outputText != null)
//        {
//            string fisheye = ovalScreen == null
//                ? "OvalScreen not assigned"
//                : "Fisheye: " + ovalScreen.Mode + "\n" +
//                  "Strength: " + ovalScreen.Strength.ToString("F1") + "°\n" +
//                  "Center zoom: " + ovalScreen.CenterMagnification.ToString("F2") + "x";

//            string modeButtonDebug = modeButtonAction == null
//                ? "Mode button: ACTION IS NULL"
//                : "Mode button: " +
//                  (modeButtonAction.controls.Count > 0
//                      ? modeButtonAction.controls.Count + " control(s) found"
//                      : "NO CONTROL FOUND");

//            outputText.text =
//                "OVAL SCREEN\n\n" +
//                "Size: " + requestedAngle.ToString("F1") + "° (stick up/down)\n" +
//                fisheye + "\n\n" +
//                "Stick left/right = strength\n" +
//                "X button = change mode\n" +
//                modeButtonDebug;
//        }
//    }

//    private void ApplyScreenScale()
//    {
//        if (screen == null)
//            return;

//        float baselineTangent = Mathf.Tan(baselineAngle * 0.5f * Mathf.Deg2Rad);

//        if (Mathf.Abs(baselineTangent) < 0.0001f)
//            return;

//        float scaleFactor =
//            Mathf.Tan(requestedAngle * 0.5f * Mathf.Deg2Rad) / baselineTangent;

//        screen.localScale = originalScale * scaleFactor;
//    }

//    private void OnDisable()
//    {
//        if (leftStickAction != null)
//            leftStickAction.Disable();
//        leftStickAction = null;

//        if (modeButtonAction != null)
//        {
//            modeButtonAction.Disable();
//            modeButtonAction.Dispose();
//        }
//        modeButtonAction = null;
//    }
//}
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControllerTestOval : MonoBehaviour
{
    [Header("Debug Output")]
    public TMP_Text outputText;

    [Header("Controller Input")]
    public InputActionAsset inputActions;

    [Header("Screen to Resize")]
    public Transform screen;

    [Header("Fisheye")]
    public OvalScreen ovalScreen;
    public float fisheyeDegreesPerSecond = 20f;

    [Header("Size Controls")]
    public float startingAngle = 50f;
    public float minimumAngle = 20f;
    public float maximumAngle = 70f;
    public float degreesPerSecond = 25f;
    public float stickDeadzone = 0.2f;

    private InputAction leftStickAction;
    private InputAction modeButtonAction;

    private Vector3 originalScale;
    private float requestedAngle;
    private float baselineAngle;

    private void Start()
    {
        minimumAngle = Mathf.Max(1f, minimumAngle);
        maximumAngle = Mathf.Max(minimumAngle, maximumAngle);

        baselineAngle = Mathf.Clamp(startingAngle, minimumAngle, maximumAngle);
        requestedAngle = baselineAngle;

        if (screen != null)
        {
            originalScale = screen.localScale;
            ApplyScreenScale();
        }
        else
        {
            Debug.LogError("ControllerTestOval: Screen Transform is not assigned.");
        }

        if (ovalScreen == null)
            Debug.LogError("ControllerTestOval: OvalScreen is not assigned.");

        FindStickAction();

        // Left controller X (or Y) button cycles the fisheye mode.
        // Several names, because controllers name these buttons differently.
        modeButtonAction = new InputAction("FisheyeMode", InputActionType.Button);
        modeButtonAction.AddBinding("<XRController>{LeftHand}/primaryButton");      // X
        modeButtonAction.AddBinding("<XRController>{LeftHand}/{PrimaryButton}");    // X
        modeButtonAction.AddBinding("<XRController>{LeftHand}/secondaryButton");    // Y
        modeButtonAction.AddBinding("<XRController>{LeftHand}/{SecondaryButton}");  // Y
        modeButtonAction.Enable();
    }

    private void FindStickAction()
    {
        if (inputActions == null || leftStickAction != null)
            return;

        leftStickAction = inputActions.FindAction("XRI Left/Thumbstick", false);

        if (leftStickAction != null)
            leftStickAction.Enable();
        else
            Debug.LogError("ControllerTestOval: XRI Left/Thumbstick action not found.");
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

        // Stick up/down: screen size.
        if (screen != null && Mathf.Abs(stick.y) > stickDeadzone)
        {
            requestedAngle = Mathf.Clamp(
                requestedAngle + stick.y * degreesPerSecond * Time.deltaTime,
                minimumAngle,
                maximumAngle);

            ApplyScreenScale(); // only when the size changes
        }

        if (ovalScreen != null)
        {
            // Stick left/right: fisheye strength.
            if (Mathf.Abs(stick.x) > stickDeadzone)
            {
                ovalScreen.SetFisheyeStrength(
                    ovalScreen.Strength + stick.x * fisheyeDegreesPerSecond * Time.deltaTime);
            }

            // X button: Off -> Equidistant -> Equisolid -> Stereographic -> Orthographic -> Off.
            if (modeButtonAction != null && modeButtonAction.WasPressedThisFrame())
            {
                ovalScreen.SetFisheyeMode(((int)ovalScreen.Mode + 1) % OvalScreen.ModeCount);
            }
        }

        if (outputText != null)
        {
            string fisheye = ovalScreen == null
                ? "OvalScreen not assigned"
                : "Fisheye: " + ovalScreen.Mode + "\n" +
                  "Strength: " + ovalScreen.Strength.ToString("F1") + "°\n" +
                  "Center zoom: " + ovalScreen.CenterMagnification.ToString("F2") + "x";

            outputText.text =
                "OVAL SCREEN\n\n" +
                "Size: " + requestedAngle.ToString("F1") + "°  (stick up/down)\n" +
                fisheye + "\n\n" +
                "Stick left/right = strength\n" +
                "X button = change mode\n" +
                "Mode button: " + (modeButtonAction != null && modeButtonAction.controls.Count > 0
                    ? modeButtonAction.controls.Count + " control(s) found"
                    : "NO CONTROL FOUND");
        }
    }

    private void ApplyScreenScale()
    {
        if (screen == null)
            return;

        float baselineTangent = Mathf.Tan(baselineAngle * 0.5f * Mathf.Deg2Rad);

        if (Mathf.Abs(baselineTangent) < 0.0001f)
            return;

        float scaleFactor =
            Mathf.Tan(requestedAngle * 0.5f * Mathf.Deg2Rad) / baselineTangent;

        screen.localScale = originalScale * scaleFactor;
    }

    private void OnDisable()
    {
        if (leftStickAction != null)
            leftStickAction.Disable();
        leftStickAction = null;

        if (modeButtonAction != null)
        {
            modeButtonAction.Disable();
            modeButtonAction.Dispose();
        }
        modeButtonAction = null;
    }
}