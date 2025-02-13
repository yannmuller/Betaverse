using UnityEngine;

public class MoveSphereToIntersection : MonoBehaviour
{
    [Tooltip("Reference to the GameObject that has the RayIntersection script.")]
    public RayIntersection rayIntersection;

    void Update()
    {
        // Check if the rayIntersection reference is set.
        if (rayIntersection == null)
        {
            Debug.LogWarning("RayIntersection reference is not set.");
            return;
        }

        // Option 1: Only move the sphere if an intersection occurs.
        if (rayIntersection.HasIntersection)
        {
            transform.position = rayIntersection.IntersectionPoint;
            transform.rotation = Quaternion.LookRotation(rayIntersection.IntersectionNormal);
        }

        // Option 2: If you want the sphere to always follow the ray's end point (even with no hit),
        // simply uncomment the line below and remove the conditional above.
        transform.position = rayIntersection.IntersectionPoint;
        transform.rotation = Quaternion.LookRotation(rayIntersection.IntersectionNormal);
    }
}
