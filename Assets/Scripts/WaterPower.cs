
using UnityEngine;

public class WaterPower : MonoBehaviour
{
    [Header("Water Spawn Position")]
    public Transform handPoint;

    [Header("Water Settings")]
    public float waterSpeed = 12f;
    public float waterSize = 0.3f;
    public float shootInterval = 0.3f;
    public float waterLifeTime = 3f;

    private float nextShootTime;

    void Update()
    {
        if (Input.GetMouseButton(0) &&Time.time >= nextShootTime){
            ShootWater();

            nextShootTime = Time.time + shootInterval;
        }
    }

    public void ShootWater()
    {
        if (handPoint == null)
            return;

        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        water.name = "Water Sphere";

        water.transform.position = handPoint.position;

        water.transform.localScale =
            Vector3.one * waterSize;

        Renderer waterRenderer = water.GetComponent<Renderer>();

        waterRenderer.material.color = Color.cyan;

        Rigidbody rb = water.AddComponent<Rigidbody>();

        rb.useGravity = false;

        Collider waterCollider = water.GetComponent<Collider>();

        Collider[] playerColliders = GetComponentsInChildren<Collider>();

        foreach (Collider playerCollider in playerColliders)
        {
            Physics.IgnoreCollision(
                waterCollider,
                playerCollider
            );
        }

        rb.linearVelocity = handPoint.forward * waterSpeed;

        Destroy(water, waterLifeTime);
    }
}