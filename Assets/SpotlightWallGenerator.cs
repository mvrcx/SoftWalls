using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SpotlightWallGenerator : MonoBehaviour
{
    [Header("Wall Dimensions")]
    public float width = 5f;
    public float height = 4f;
    [Tooltip("Keep this high (50+) for a smooth circle")]
    public int widthSegments = 50;
    public int heightSegments = 50;

    [Header("Spotlight Settings")]
    public Transform playerCamera;
    public float maxDetectionDistance = 10f;
    public float maxSpotlightRadius = 5f;

    private Mesh mesh;
    private MeshRenderer meshRenderer;

    void Awake() => Setup();
    void OnValidate() => Setup();

    void Setup()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Spotlight Mesh";
            GetComponent<MeshFilter>().mesh = mesh;
        }
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        GenerateWall();
    }

    // This is your recovered code from the original file
    void GenerateWall()
    {
        int vertCountX = widthSegments + 1;
        int vertCountY = heightSegments + 1;
        int totalVertices = vertCountX * vertCountY;

        Vector3[] vertices = new Vector3[totalVertices];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[widthSegments * heightSegments * 6]; 

        int v = 0;
        for (int y = 0; y <= heightSegments; y++)
        {
            for (int x = 0; x <= widthSegments; x++)
            {
                float px = (x / (float)widthSegments) * width - width * 0.5f;
                float py = (y / (float)heightSegments) * height - height * 0.5f;
                vertices[v] = new Vector3(px, py, 0f);
                uvs[v] = new Vector2(x / (float)widthSegments, y / (float)heightSegments);
                v++;
            }
        }

        int t = 0;
        for (int y = 0; y < heightSegments; y++)
        {
            for (int x = 0; x < widthSegments; x++)
            {
                int bl = y * vertCountX + x;
                int br = bl + 1;
                int tl = bl + vertCountX;
                int tr = tl + 1;
                triangles[t++] = bl; triangles[t++] = tl; triangles[t++] = br;
                triangles[t++] = br; triangles[t++] = tl; triangles[t++] = tr;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.MarkDynamic(); // Critical for per-frame color updates
    }

    void Update()
    {
        if (!Application.isPlaying || playerCamera == null || mesh == null) return;

        // 1. Calculate how big the spotlight should be based on player distance
        float distToWall = Vector3.Distance(transform.position, playerCamera.position);
        float proximity = 1f - Mathf.Clamp01(distToWall / maxDetectionDistance);
        float currentRadius = maxSpotlightRadius * proximity;

        // 2. Loop through vertices and set colors
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            // Calculate distance between THIS vertex and the player
            Vector3 worldVtx = transform.TransformPoint(vertices[i]);
            float distToPlayer = Vector3.Distance(worldVtx, playerCamera.position);

            // 3. Create the fade effect (1 = opaque, 0 = transparent)
            // This logic creates the circle that expands from the center
            float alpha = 1f - Mathf.Clamp01(distToPlayer / currentRadius);
            alpha *= 1.5f; // Boosts the opacity
            alpha = Mathf.Clamp01(alpha);
            // Apply transparency (White color with custom Alpha)
            colors[i] = new Color(1, 1, 1, alpha);
        }

        mesh.colors = colors;
    }
}