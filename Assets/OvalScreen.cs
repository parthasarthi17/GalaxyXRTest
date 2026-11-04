////using UnityEngine;

////[RequireComponent(typeof(MeshFilter))]
////public class OvalScreen : MonoBehaviour
////{
////    [Header("Image Size (copy your tuned values from ConcaveScreen)")]
////    [SerializeField] private float height = 0.933f;

////    [Tooltip("Image width ÷ height. Use the value you tuned with the tilt test.")]
////    [SerializeField] private float widthStretch = 1.78f;

////    [Header("Oval Shape")]
////    [Tooltip("1 = oval touches the image edges (image corners hidden).\n" +
////             "1.41 = whole image fits inside the oval (empty gaps near the oval's edge).\n" +
////             "Circle: whole image fits at about 2.04 when widthStretch is 1.78.")]
////    [SerializeField, Range(1f, 2.5f)] private float ovalScale = 1f;

////    [Tooltip("On = perfect circle (its size follows the image height).")]
////    [SerializeField] private bool circle = false;

////    [Header("Curvature")]
////    [Tooltip("Larger radius = flatter. Smaller radius = more curved.")]
////    [SerializeField] private float radius = 2.0f;

////    [Header("Mesh Resolution")]
////    [SerializeField] private int rings = 30;
////    [SerializeField] private int segments = 96;

////    private Mesh mesh;

////    private float width => height * widthStretch;

////    private void Awake()
////    {
////        GenerateMesh();
////    }

////    private void OnValidate()
////    {
////        if (!Application.isPlaying)
////            GenerateMesh();
////    }

////    // Hook these to sliders to tune on the headset.
////    public void SetWidthStretch(float value)
////    {
////        widthStretch = value;
////        GenerateMesh();
////    }

////    public void SetOvalScale(float value)
////    {
////        ovalScale = value;
////        GenerateMesh();
////    }

////    private void GenerateMesh()
////    {
////        rings = Mathf.Max(1, rings);
////        segments = Mathf.Max(8, segments);

////        // Half-sizes of the oval.
////        float halfX = (circle ? height : width) * 0.5f * ovalScale;
////        float halfY = height * 0.5f * ovalScale;

////        // Radius must be larger than the farthest edge point.
////        float maxR = Mathf.Max(halfX, halfY);
////        radius = Mathf.Max(radius, maxR + 0.001f);

////        // Shift so the farthest edge points sit at Z = 0
////        // and the center is pushed away (concave toward the user).
////        float edgeOffset = Mathf.Sqrt(radius * radius - maxR * maxR);

////        if (mesh != null)
////        {
////            if (Application.isPlaying) Destroy(mesh);
////            else DestroyImmediate(mesh);
////        }

////        mesh = new Mesh { name = "OvalScreenMesh" };

////        // One center vertex, then rings of vertices going outward.
////        int count = 1 + rings * segments;
////        var vertices = new Vector3[count];
////        var uv = new Vector2[count];

////        vertices[0] = CurvedPoint(0f, 0f, edgeOffset);
////        uv[0] = new Vector2(0.5f, 0.5f);

////        for (int i = 1; i <= rings; i++)
////        {
////            float r = (float)i / rings;

////            for (int j = 0; j < segments; j++)
////            {
////                float angle = 2f * Mathf.PI * j / segments;

////                float x = r * halfX * Mathf.Cos(angle);
////                float y = r * halfY * Mathf.Sin(angle);

////                int k = 1 + (i - 1) * segments + j;

////                vertices[k] = CurvedPoint(x, y, edgeOffset);

////                // The image keeps its tuned size; the oval only decides
////                // how much of it shows. Outside 0..1 = beyond the image.
////                uv[k] = new Vector2(0.5f + x / width, 0.5f + y / height);
////            }
////        }

////        var triangles = new int[segments * 3 + (rings - 1) * segments * 6];
////        int t = 0;

////        // Center fan. Clockwise when seen from -Z, so it faces the user.
////        for (int j = 0; j < segments; j++)
////        {
////            int p0 = 1 + j;
////            int p1 = 1 + (j + 1) % segments;

////            triangles[t++] = 0;
////            triangles[t++] = p1;
////            triangles[t++] = p0;
////        }

////        // Strips between rings.
////        for (int i = 1; i < rings; i++)
////        {
////            int inner = 1 + (i - 1) * segments;
////            int outer = 1 + i * segments;

////            for (int j = 0; j < segments; j++)
////            {
////                int jn = (j + 1) % segments;

////                int a = inner + j;
////                int b = inner + jn;
////                int c = outer + jn;
////                int d = outer + j;

////                triangles[t++] = a;
////                triangles[t++] = b;
////                triangles[t++] = c;

////                triangles[t++] = a;
////                triangles[t++] = c;
////                triangles[t++] = d;
////            }
////        }

////        mesh.vertices = vertices;
////        mesh.uv = uv;
////        mesh.triangles = triangles;

////        mesh.RecalculateNormals();
////        mesh.RecalculateBounds();

////        GetComponent<MeshFilter>().sharedMesh = mesh;
////    }

////    private Vector3 CurvedPoint(float x, float y, float edgeOffset)
////    {
////        float z = Mathf.Sqrt(radius * radius - (x * x + y * y)) - edgeOffset;
////        return new Vector3(x, y, z);
////    }
////}

//using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
//public class OvalScreen : MonoBehaviour
//{
//    public enum FisheyeMode { Off, Equidistant, Equisolid }

//    [Header("Image Size (copy your tuned values from ConcaveScreen)")]
//    [SerializeField] private float height = 0.933f;

//    [Tooltip("Image width ÷ height. Use the value you tuned with the tilt test.")]
//    [SerializeField] private float widthStretch = 1.78f;

//    [Header("Oval Shape")]
//    [Tooltip("1 = oval touches the image edges (image corners hidden).\n" +
//             "1.41 = whole image fits inside the oval (empty gaps near the oval's edge).\n" +
//             "Circle: whole image fits at about 2.04 when widthStretch is 1.78.")]
//    [SerializeField, Range(1f, 2.5f)] private float ovalScale = 1f;

//    [Tooltip("On = perfect circle (its size follows the image height).")]
//    [SerializeField] private bool circle = false;

//    [Header("Fisheye Magnification")]
//    [Tooltip("Off = no warp. Equidistant: R = f * theta. Equisolid: R = 2f * sin(theta / 2).")]
//    [SerializeField] private FisheyeMode fisheyeMode = FisheyeMode.Off;

//    [Tooltip("Lens half-angle. Bigger = stronger center magnification. Near 0 = no effect.")]
//    [SerializeField, Range(1f, 85f)] private float fisheyeStrength = 60f;

//    [Header("Curvature")]
//    [Tooltip("Larger radius = flatter. Smaller radius = more curved.")]
//    [SerializeField] private float radius = 2.0f;

//    [Header("Mesh Resolution")]
//    [SerializeField] private int rings = 60;
//    [SerializeField] private int segments = 96;

//    private Mesh mesh;

//    private float width => height * widthStretch;

//    private void Awake()
//    {
//        GenerateMesh();
//    }

//    private void OnValidate()
//    {
//        if (!Application.isPlaying)
//            GenerateMesh();
//    }

//    // Hook these to sliders to tune on the headset.
//    public void SetWidthStretch(float value)
//    {
//        widthStretch = value;
//        GenerateMesh();
//    }

//    public void SetOvalScale(float value)
//    {
//        ovalScale = value;
//        GenerateMesh();
//    }

//    public void SetFisheyeMode(int mode)
//    {
//        fisheyeMode = (FisheyeMode)Mathf.Clamp(mode, 0, 2);
//        GenerateMesh();
//    }

//    public void SetFisheyeStrength(float degrees)
//    {
//        fisheyeStrength = Mathf.Clamp(degrees, 1f, 85f);
//        GenerateMesh();
//    }

//    public FisheyeMode Mode => fisheyeMode;
//    public float Strength => fisheyeStrength;

//    // Center magnification for the current settings (1 = none).
//    public float CenterMagnification => SourceRadius(0.001f) > 0f ? 0.001f / SourceRadius(0.001f) : 1f;

//    // r = how far a point is from the center on the screen (0 = center, 1 = oval edge).
//    // Returns how far from the center to sample the camera image (same 0..1 scale).
//    // The edge still shows the edge, so nothing is lost; the center is
//    // magnified and the outer part is squeezed to make room.
//    private float SourceRadius(float r)
//    {
//        if (fisheyeMode == FisheyeMode.Off)
//            return r;

//        float thetaMax = fisheyeStrength * Mathf.Deg2Rad;
//        float theta;

//        if (fisheyeMode == FisheyeMode.Equidistant)
//            theta = r * thetaMax;                                          // R = f * theta
//        else
//            theta = 2f * Mathf.Asin(r * Mathf.Sin(thetaMax * 0.5f));       // R = 2f * sin(theta / 2)

//        // The camera image is a normal (rectilinear) image: R = f * tan(theta).
//        return Mathf.Tan(theta) / Mathf.Tan(thetaMax);
//    }

//    private void GenerateMesh()
//    {
//        rings = Mathf.Max(1, rings);
//        segments = Mathf.Max(8, segments);

//        // Half-sizes of the oval.
//        float halfX = (circle ? height : width) * 0.5f * ovalScale;
//        float halfY = height * 0.5f * ovalScale;

//        // Radius must be larger than the farthest edge point.
//        float maxR = Mathf.Max(halfX, halfY);
//        radius = Mathf.Max(radius, maxR + 0.001f);

//        // Shift so the farthest edge points sit at Z = 0
//        // and the center is pushed away (concave toward the user).
//        float edgeOffset = Mathf.Sqrt(radius * radius - maxR * maxR);

//        if (mesh != null)
//        {
//            if (Application.isPlaying) Destroy(mesh);
//            else DestroyImmediate(mesh);
//        }

//        mesh = new Mesh { name = "OvalScreenMesh" };

//        // One center vertex, then rings of vertices going outward.
//        int count = 1 + rings * segments;
//        var vertices = new Vector3[count];
//        var uv = new Vector2[count];

//        vertices[0] = CurvedPoint(0f, 0f, edgeOffset);
//        uv[0] = new Vector2(0.5f, 0.5f);

//        for (int i = 1; i <= rings; i++)
//        {
//            float r = (float)i / rings;

//            for (int j = 0; j < segments; j++)
//            {
//                float angle = 2f * Mathf.PI * j / segments;

//                float x = r * halfX * Mathf.Cos(angle);
//                float y = r * halfY * Mathf.Sin(angle);

//                int k = 1 + (i - 1) * segments + j;

//                vertices[k] = CurvedPoint(x, y, edgeOffset);

//                // Fisheye: sample the camera image at a warped radius.
//                float rs = SourceRadius(r);
//                float sx = rs * halfX * Mathf.Cos(angle);
//                float sy = rs * halfY * Mathf.Sin(angle);

//                // The image keeps its tuned size; the oval only decides
//                // how much of it shows. Outside 0..1 = beyond the image.
//                uv[k] = new Vector2(0.5f + sx / width, 0.5f + sy / height);
//            }
//        }

//        var triangles = new int[segments * 3 + (rings - 1) * segments * 6];
//        int t = 0;

//        // Center fan. Clockwise when seen from -Z, so it faces the user.
//        for (int j = 0; j < segments; j++)
//        {
//            int p0 = 1 + j;
//            int p1 = 1 + (j + 1) % segments;

//            triangles[t++] = 0;
//            triangles[t++] = p1;
//            triangles[t++] = p0;
//        }

//        // Strips between rings.
//        for (int i = 1; i < rings; i++)
//        {
//            int inner = 1 + (i - 1) * segments;
//            int outer = 1 + i * segments;

//            for (int j = 0; j < segments; j++)
//            {
//                int jn = (j + 1) % segments;

//                int a = inner + j;
//                int b = inner + jn;
//                int c = outer + jn;
//                int d = outer + j;

//                triangles[t++] = a;
//                triangles[t++] = b;
//                triangles[t++] = c;

//                triangles[t++] = a;
//                triangles[t++] = c;
//                triangles[t++] = d;
//            }
//        }

//        mesh.vertices = vertices;
//        mesh.uv = uv;
//        mesh.triangles = triangles;

//        mesh.RecalculateNormals();
//        mesh.RecalculateBounds();

//        GetComponent<MeshFilter>().sharedMesh = mesh;
//    }

//    private Vector3 CurvedPoint(float x, float y, float edgeOffset)
//    {
//        float z = Mathf.Sqrt(radius * radius - (x * x + y * y)) - edgeOffset;
//        return new Vector3(x, y, z);
//    }
//}
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class OvalScreen : MonoBehaviour
{
    public enum FisheyeMode { Off, Equidistant, Equisolid, Stereographic, Orthographic }

    [Header("Image Size (copy your tuned values from ConcaveScreen)")]
    [SerializeField] private float height = 0.933f;

    [Tooltip("Image width ÷ height. Use the value you tuned with the tilt test.")]
    [SerializeField] private float widthStretch = 1.78f;

    [Header("Oval Shape")]
    [Tooltip("1 = oval touches the image edges (image corners hidden).\n" +
             "1.41 = whole image fits inside the oval (empty gaps near the oval's edge).\n" +
             "Circle: whole image fits at about 2.04 when widthStretch is 1.78.")]
    [SerializeField, Range(1f, 2.5f)] private float ovalScale = 1f;

    [Tooltip("On = perfect circle (its size follows the image height).")]
    [SerializeField] private bool circle = false;

    [Header("Fisheye Magnification")]
    [Tooltip("Off = no warp.\n" +
             "Stereographic: R = 2f * tan(theta / 2)  (gentlest)\n" +
             "Equidistant:   R = f * theta\n" +
             "Equisolid:     R = 2f * sin(theta / 2)\n" +
             "Orthographic:  R = f * sin(theta)  (strongest)")]
    [SerializeField] private FisheyeMode fisheyeMode = FisheyeMode.Off;

    [Tooltip("Lens half-angle. Bigger = stronger center magnification. Near 0 = no effect.")]
    [SerializeField, Range(1f, 85f)] private float fisheyeStrength = 60f;

    [Header("Curvature")]
    [Tooltip("Larger radius = flatter. Smaller radius = more curved.")]
    [SerializeField] private float radius = 2.0f;

    [Header("Mesh Resolution")]
    [SerializeField] private int rings = 60;
    [SerializeField] private int segments = 96;

    private Mesh mesh;

    private float width => height * widthStretch;

    private void Awake()
    {
        GenerateMesh();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            GenerateMesh();
    }

    // Hook these to sliders to tune on the headset.
    public void SetWidthStretch(float value)
    {
        widthStretch = value;
        GenerateMesh();
    }

    public void SetOvalScale(float value)
    {
        ovalScale = value;
        GenerateMesh();
    }

    public void SetFisheyeMode(int mode)
    {
        fisheyeMode = (FisheyeMode)Mathf.Clamp(mode, 0, ModeCount - 1);
        GenerateMesh();
    }

    public void SetFisheyeStrength(float degrees)
    {
        fisheyeStrength = Mathf.Clamp(degrees, 1f, 85f);
        GenerateMesh();
    }

    public const int ModeCount = 5;

    public FisheyeMode Mode => fisheyeMode;
    public float Strength => fisheyeStrength;

    // Center magnification for the current settings (1 = none).
    public float CenterMagnification => SourceRadius(0.001f) > 0f ? 0.001f / SourceRadius(0.001f) : 1f;

    // r = how far a point is from the center on the screen (0 = center, 1 = oval edge).
    // Returns how far from the center to sample the camera image (same 0..1 scale).
    // The edge still shows the edge, so nothing is lost; the center is
    // magnified and the outer part is squeezed to make room.
    private float SourceRadius(float r)
    {
        if (fisheyeMode == FisheyeMode.Off)
            return r;

        float thetaMax = fisheyeStrength * Mathf.Deg2Rad;
        float theta;

        switch (fisheyeMode)
        {
            case FisheyeMode.Equidistant:     // R = f * theta
                theta = r * thetaMax;
                break;

            case FisheyeMode.Equisolid:       // R = 2f * sin(theta / 2)
                theta = 2f * Mathf.Asin(r * Mathf.Sin(thetaMax * 0.5f));
                break;

            case FisheyeMode.Stereographic:   // R = 2f * tan(theta / 2)
                theta = 2f * Mathf.Atan(r * Mathf.Tan(thetaMax * 0.5f));
                break;

            default:                          // Orthographic: R = f * sin(theta)
                theta = Mathf.Asin(Mathf.Min(1f, r * Mathf.Sin(thetaMax)));
                break;
        }

        // The camera image is a normal (rectilinear) image: R = f * tan(theta).
        return Mathf.Tan(theta) / Mathf.Tan(thetaMax);
    }

    private void GenerateMesh()
    {
        rings = Mathf.Max(1, rings);
        segments = Mathf.Max(8, segments);

        // Half-sizes of the oval.
        float halfX = (circle ? height : width) * 0.5f * ovalScale;
        float halfY = height * 0.5f * ovalScale;

        // Radius must be larger than the farthest edge point.
        float maxR = Mathf.Max(halfX, halfY);
        radius = Mathf.Max(radius, maxR + 0.001f);

        // Shift so the farthest edge points sit at Z = 0
        // and the center is pushed away (concave toward the user).
        float edgeOffset = Mathf.Sqrt(radius * radius - maxR * maxR);

        if (mesh != null)
        {
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
        }

        mesh = new Mesh { name = "OvalScreenMesh" };

        // One center vertex, then rings of vertices going outward.
        int count = 1 + rings * segments;
        var vertices = new Vector3[count];
        var uv = new Vector2[count];

        vertices[0] = CurvedPoint(0f, 0f, edgeOffset);
        uv[0] = new Vector2(0.5f, 0.5f);

        for (int i = 1; i <= rings; i++)
        {
            float r = (float)i / rings;

            for (int j = 0; j < segments; j++)
            {
                float angle = 2f * Mathf.PI * j / segments;

                float x = r * halfX * Mathf.Cos(angle);
                float y = r * halfY * Mathf.Sin(angle);

                int k = 1 + (i - 1) * segments + j;

                vertices[k] = CurvedPoint(x, y, edgeOffset);

                // Fisheye: sample the camera image at a warped radius.
                float rs = SourceRadius(r);
                float sx = rs * halfX * Mathf.Cos(angle);
                float sy = rs * halfY * Mathf.Sin(angle);

                // The image keeps its tuned size; the oval only decides
                // how much of it shows. Outside 0..1 = beyond the image.
                uv[k] = new Vector2(0.5f + sx / width, 0.5f + sy / height);
            }
        }

        var triangles = new int[segments * 3 + (rings - 1) * segments * 6];
        int t = 0;

        // Center fan. Clockwise when seen from -Z, so it faces the user.
        for (int j = 0; j < segments; j++)
        {
            int p0 = 1 + j;
            int p1 = 1 + (j + 1) % segments;

            triangles[t++] = 0;
            triangles[t++] = p1;
            triangles[t++] = p0;
        }

        // Strips between rings.
        for (int i = 1; i < rings; i++)
        {
            int inner = 1 + (i - 1) * segments;
            int outer = 1 + i * segments;

            for (int j = 0; j < segments; j++)
            {
                int jn = (j + 1) % segments;

                int a = inner + j;
                int b = inner + jn;
                int c = outer + jn;
                int d = outer + j;

                triangles[t++] = a;
                triangles[t++] = b;
                triangles[t++] = c;

                triangles[t++] = a;
                triangles[t++] = c;
                triangles[t++] = d;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private Vector3 CurvedPoint(float x, float y, float edgeOffset)
    {
        float z = Mathf.Sqrt(radius * radius - (x * x + y * y)) - edgeOffset;
        return new Vector3(x, y, z);
    }
}