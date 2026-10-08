using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ConcaveScreen : MonoBehaviour
{
    [Header("Screen Size")]
    [SerializeField] private float width = 0.933f;
    [SerializeField] private float height = 0.933f;

    [Header("Curvature")]
    [Tooltip("Larger radius = flatter. Smaller radius = more curved.")]
    [SerializeField] private float radius = 2.0f;

    [Header("Mesh Resolution")]
    [SerializeField] private int horizontalSegments = 40;
    [SerializeField] private int verticalSegments = 40;

    private Mesh mesh;

    private void Awake()
    {
        GenerateMesh();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            GenerateMesh();
    }

    private void GenerateMesh()
    {
        horizontalSegments = Mathf.Max(2, horizontalSegments);
        verticalSegments = Mathf.Max(2, verticalSegments);

        // Radius must be larger than the distance
        // from the center to the farthest corner.
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        float maxRadiusNeeded =
            Mathf.Sqrt(
                halfWidth * halfWidth +
                halfHeight * halfHeight
            );

        radius = Mathf.Max(
            radius,
            maxRadiusNeeded + 0.001f
        );

        if (mesh != null)
        {
            DestroyImmediate(mesh);
        }

        mesh = new Mesh();
        mesh.name = "ConcaveScreenMesh";

        int verticesX = horizontalSegments + 1;
        int verticesY = verticalSegments + 1;

        Vector3[] vertices =
            new Vector3[verticesX * verticesY];

        Vector2[] uv =
            new Vector2[verticesX * verticesY];

        /*
         * We want the EDGES at local Z = 0
         * and the CENTER farther away at positive Z.
         *
         * Therefore:
         *
         * center  -> larger Z
         * edges   -> Z = 0
         *
         * This produces a concave surface facing the user.
         */

        float cornerRadius =
            Mathf.Sqrt(
                radius * radius -
                maxRadiusNeeded * maxRadiusNeeded
            );

        int index = 0;

        for (int y = 0; y <= verticalSegments; y++)
        {
            float v = (float)y / verticalSegments;

            float localY =
                (v - 0.5f) * height;

            for (int x = 0; x <= horizontalSegments; x++)
            {
                float u = (float)x / horizontalSegments;

                float localX =
                    (u - 0.5f) * width;

                float radialDistanceSquared =
                    localX * localX +
                    localY * localY;

                float sphereZ =
                    Mathf.Sqrt(
                        radius * radius -
                        radialDistanceSquared
                    );

                // Shift so the edges/corners are approximately Z = 0.
                float localZ =
                    sphereZ - cornerRadius;

                vertices[index] =
                    new Vector3(
                        localX,
                        localY,
                        localZ
                    );

                uv[index] =
                    new Vector2(u, v);

                index++;
            }
        }

        int[] triangles =
            new int[
                horizontalSegments *
                verticalSegments *
                6
            ];

        int t = 0;

        for (int y = 0; y < verticalSegments; y++)
        {
            for (int x = 0; x < horizontalSegments; x++)
            {
                int a =
                    y * verticesX + x;

                int b = a + 1;

                int d =
                    (y + 1) * verticesX + x;

                int c = d + 1;

                // Front-facing toward the user (-Z).
                triangles[t++] = a;
                triangles[t++] = c;
                triangles[t++] = b;

                triangles[t++] = a;
                triangles[t++] = d;
                triangles[t++] = c;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}