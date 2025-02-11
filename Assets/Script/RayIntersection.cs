using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RayIntersection : MonoBehaviour
{
    [Header("Ray Settings")]
    [Tooltip("The maximum length of the ray.")]
    public float rayLength = 10f;

    [Tooltip("Layer mask for collision detection.")]
    public LayerMask collisionMask = ~0; // Default: All layers

    // These properties allow other classes to access the intersection info.
    public Vector3 IntersectionPoint { get; private set; }
    public bool HasIntersection { get; private set; }

    private LineRenderer lineRenderer;

    void Start()
    {
        // Get and configure the LineRenderer component.
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component is missing.");
            return;
        }
        
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Initialize with red color.
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit hit;
        bool isHit = Physics.Raycast(origin, direction, out hit, rayLength, collisionMask);

        if (isHit)
        {
            // When a hit occurs, store the hit point and mark that an intersection occurred.
            IntersectionPoint = hit.point;
            HasIntersection = true;

            // Set the line renderer to green and adjust its length.
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            // No hit: you can choose whether to mark this as no intersection or use the full ray length.
            HasIntersection = false;
            IntersectionPoint = origin + direction * rayLength; // Default to the end of the ray.

            // Set the ray to red.
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, IntersectionPoint);
        }

        // (Optional) Draw a debug ray in the Scene view.
        Debug.DrawRay(origin, direction * rayLength, isHit ? Color.green : Color.red);
    }
}
