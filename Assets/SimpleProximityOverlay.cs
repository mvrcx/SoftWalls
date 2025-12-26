using UnityEngine;

public class SimpleProximityOverlay : MonoBehaviour
{
    public Transform playerCamera;
    public float maxDistance = 2.5f;

    private Material mat;

    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        if (playerCamera == null)
            playerCamera = Camera.main.transform;
    }

    void Update()
    {
        Vector3 wallNormal = transform.parent.forward;
        float distance = Mathf.Abs(
            Vector3.Dot(playerCamera.position - transform.parent.position, wallNormal)
        );

        float proximity = Mathf.Clamp01(1f - distance / maxDistance);

        // Alpha
        Color c = mat.color;
        c.a = proximity * 0.6f; // nie komplett deckend
        mat.color = c;

        // leichte „Kompression“
        float scale = Mathf.Lerp(1f, 1.05f, proximity);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
