using UnityEngine;

[ExecuteInEditMode]
public class GridWallFade : MonoBehaviour
{
    public Transform playerCamera;
    public float maxDistance = 10f; // Distance where it starts appearing
    public float minDistance = 2f;  // Distance where it is fully solid

    [Range(0, 1)]
    public float maxOpacity = 1f;

    private MeshRenderer meshRenderer;
    private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    void Update()
    {
        if (playerCamera == null || meshRenderer == null) return;

        // 1. Calculate distance
        float distance = Vector3.Distance(transform.position, playerCamera.position);

        // 2. Calculate Alpha (1 at minDistance, 0 at maxDistance)
        float alpha = Mathf.InverseLerp(maxDistance, minDistance, distance);
        alpha = Mathf.Clamp01(alpha) * maxOpacity;

        // 3. Apply to Material
        Color c = meshRenderer.material.GetColor(ColorProperty);
        c.a = alpha;
        meshRenderer.material.SetColor(ColorProperty, c);
    }
}