using UnityEngine;

public class WallCollisionLogger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // DEBUG: This will now log NO MATTER WHAT hits the wall
        Debug.Log($"<color=cyan>[PHYSICS]</color> {gameObject.name} HIT BY: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        // If it's your Main Camera, it should at least have the "MainCamera" tag by default
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("MainCamera"))
        {
            CSVDataManager.Instance.RecordCollision();
        }
    }
}