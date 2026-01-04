using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class WallProximityVisual : MonoBehaviour
{
    public Transform playerCamera;
    public float maxProximityDistance = 3f;

    [Header("Visual Response")]
    public Color farColor = Color.white;
    public Color nearColor = Color.cyan;
    public float maxEmission = 1.5f;

    private Material mat;

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        if (playerCamera == null) return;

        Vector3 wallNormal = transform.forward;
        float distance = Mathf.Abs(
            Vector3.Dot(
                playerCamera.position - transform.position,
                wallNormal
            )
        );

        float proximity = Mathf.Clamp01(1f - distance / maxProximityDistance);

        Color currentColor = Color.Lerp(farColor, nearColor, proximity);
        mat.color = currentColor;

        mat.SetColor("_EmissionColor", currentColor * (proximity * maxEmission));
    }
}
