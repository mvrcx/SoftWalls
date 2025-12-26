using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class wiggleWallGenerator : MonoBehaviour
{
    [Header("Wall Dimensions")]
    public float width = 5f;
    public float height = 4f;

    [Header("Subdivisions")]
    public int widthSegments = 50;
    public int heightSegments = 50;
    
    [Header("Proximity Detection")]
    public Transform playerCamera; 
    public float maxProximityDistance = 5f;
    public float proximityFalloff = 1f;

    // --- NEW: Transparency Settings ---
    [Header("Transparency Settings")]
    [Range(0f, 1f)]
    public float minOpacity = 0.2f; // How see-through it is when far away
    [Range(0f, 1f)]
    public float maxOpacity = 1.0f; // How solid it is when close
    
    [Header("Wiggle Settings")]
    public float maxWiggleStrength = 0.5f; 
    public float wiggleSpeed = 1f;      
    public float waveScale = 0.5f;        

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private Vector3[] baseVertices; 
    private Vector3[] workingVertices;
    private float currentWiggleStrength; 

    // Cache the material property ID for better performance
    private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        SetupMeshAndMaterial();
        GenerateWall();
        InitializeArrays();
    }

    void OnValidate()
    {
        SetupMeshAndMaterial();
        GenerateWall();
        InitializeArrays();
    }

    void SetupMeshAndMaterial()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Wiggly Wall Mesh";
            GetComponent<MeshFilter>().mesh = mesh;
        }
        
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        // Ensure material exists
        if (meshRenderer.sharedMaterial == null)
        {
            // Note: For transparency to work, the shader must be set to "Transparent" mode in the Inspector
            meshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
    }

    void InitializeArrays()
    {
        if (baseVertices != null && baseVertices.Length > 0)
        {
            workingVertices = new Vector3[baseVertices.Length];
            System.Array.Copy(baseVertices, workingVertices, baseVertices.Length);
        }
    }

    void GenerateWall()
    {
        if (mesh == null) return;
        
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
        baseVertices = vertices; 
        mesh.RecalculateNormals();
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(width, height, 1f));
        mesh.MarkDynamic();
    }

    void Update()
    {
        if (!Application.isPlaying || mesh == null || playerCamera == null) return;

        // 1. Calculate Proximity
        float rawDistance = Mathf.Abs(Vector3.Dot(playerCamera.position - transform.position, transform.forward));
        float proximityValue = Mathf.InverseLerp(maxProximityDistance, maxProximityDistance - proximityFalloff, rawDistance);
        proximityValue = Mathf.Clamp01(proximityValue);
        
        // 2. Update Wiggle Strength
        currentWiggleStrength = maxWiggleStrength * proximityValue;

        // --- NEW: Update Transparency ---
        UpdateOpacity(proximityValue);

        // 3. Vertex Deformation (Same as before)
        if (currentWiggleStrength <= 0.0001f)
        {
            if (mesh.vertices[0] != baseVertices[0])
            {
                System.Array.Copy(baseVertices, workingVertices, baseVertices.Length);
                mesh.vertices = workingVertices;
                mesh.RecalculateNormals();
            }
            return;
        }

        ApplyWiggle();
    }

    void UpdateOpacity(float proximity)
    {
        if (meshRenderer == null) return;

        // Calculate alpha based on proximity
        float targetAlpha = Mathf.Lerp(minOpacity, maxOpacity, proximity);

        // Get the current color, update alpha, and push back to material
        // We use .material (not .sharedMaterial) to ensure we only change THIS instance
        Color color = meshRenderer.material.GetColor(ColorProperty);
        color.a = targetAlpha;
        meshRenderer.material.SetColor(ColorProperty, color);
    }

    void ApplyWiggle()
    {
        float time = Time.time * wiggleSpeed;
        float maxRadius = new Vector2(width * 0.5f, height * 0.5f).magnitude;

        for (int i = 0; i < workingVertices.Length; i++)
        {
            Vector3 vertex = baseVertices[i];
            float distanceToCenter = new Vector2(vertex.x, vertex.y).magnitude;
            float falloff = Mathf.Cos(Mathf.Min(distanceToCenter / maxRadius, 1f) * Mathf.PI * 0.5f);
            float waveInput = distanceToCenter * waveScale + time;
            float finalWave = (Mathf.Abs(Mathf.Cos(waveInput)) * 0.5f) + 0.5f;
            float displacement_outward = (finalWave - 0.5f) * currentWiggleStrength * falloff * 2f; 

            workingVertices[i].z = vertex.z + displacement_outward;
        }

        mesh.vertices = workingVertices;
        mesh.RecalculateNormals(); 
    }
}