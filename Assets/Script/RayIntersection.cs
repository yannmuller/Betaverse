using UnityEngine;
using System.Collections;
using Oculus.Haptics;

public class RayIntersection : MonoBehaviour
{
    [Header("Ray Settings")]
    [Tooltip("The maximum length of the ray.")]
    public float rayLength = 10f;

    [Tooltip("Layer mask for collision detection.")]
    public LayerMask collisionMask = ~0; // Default: All layers

    // Properties for intersection information.
    public Vector3 IntersectionPoint { get; private set; }
    public Vector3 IntersectionNormal { get; private set; }
    public bool HasIntersection { get; private set; }

    [Header("Cylinder Ray Settings")]
    [Tooltip("Assign the first cylinder GameObject that represents the ray.")]
    public GameObject cylinderRay;
    
    [Tooltip("Thickness (radius) of the first cylinder.")]
    public float cylinderRadius = 0.05f;

    [Header("Thicker Cylinder Ray Settings")]
    [Tooltip("Assign the second (thicker) cylinder GameObject that represents the ray.")]
    public GameObject cylinderRayThick;
    
    [Tooltip("Thickness (radius) of the thicker cylinder. Set this to be slightly larger than the first.")]
    public float cylinderThickRadius = 0.07f;  // Slightly thicker than cylinderRadius

    [Header("Particle System")]
    [Tooltip("Reference to the particle system in the scene.")]
    public ParticleSystem waveParticles;

    [Header("Hole Instantiation")]
    [Tooltip("Assign the Hole prefab (the sphere to be drawn).")]
    public GameObject holePrefab;

    // Spawn interval in seconds.
    private float spawnInterval = 0.02f;
    // Time until the next spawn is allowed.
    private float nextSpawnTime = 0f;

    // ---------------------------
    // Cumulative trigger time
    // ---------------------------
    private float cumulativeTriggerTime = 0f;  // Total time (in seconds) that the trigger has been held

    [Header("Water Container Settings")]
    [Tooltip("Assign the water container GameObject (with a Collider) that resets the timer.")]
    public GameObject waterContainer;
    private Collider waterContainerCollider;

    // Timer to track how long the object has been inside the water container.
    private float waterDwellTime = 0f;

    void Start()
    {
        // Check if the first cylinder object is assigned.
        if (cylinderRay == null)
        {
            Debug.LogError("Cylinder Ray GameObject is not assigned in the Inspector.");
        }
        else
        {
            // Optionally, disable it on start.
            cylinderRay.SetActive(false);
        }

        // Check if the thicker cylinder object is assigned.
        if (cylinderRayThick == null)
        {
            Debug.LogError("Thicker Cylinder Ray GameObject is not assigned in the Inspector.");
        }
        else
        {
            cylinderRayThick.SetActive(false);
        }

        // Ensure the particle system is assigned.
        if (waveParticles == null)
        {
            GameObject psObject = GameObject.Find("waveParticles");
            if (psObject != null)
            {
                waveParticles = psObject.GetComponent<ParticleSystem>();
            }
            else
            {
                Debug.LogError("Particle system 'waveParticles' not found in the scene.");
            }
        }

        // Locate or validate the water container.
        if (waterContainer == null)
        {
            // Try to find it by name if not manually assigned.
            waterContainer = GameObject.Find("water container");
        }
        if (waterContainer != null)
        {
            waterContainerCollider = waterContainer.GetComponent<Collider>();
            if (waterContainerCollider == null)
            {
                Debug.LogError("The water container GameObject does not have a Collider component.");
            }
        }
        else
        {
            Debug.LogError("Water container GameObject not found in the scene.");
        }
    }

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        // Perform the raycast.
        RaycastHit hit;
        bool isHit = Physics.Raycast(origin, direction, out hit, rayLength, collisionMask);

        if (isHit)
        {
            IntersectionPoint = hit.point;
            IntersectionNormal = hit.normal;
            HasIntersection = true;
        }
        else
        {
            HasIntersection = false;
            IntersectionPoint = origin + direction * rayLength;
            IntersectionNormal = Vector3.zero;
        }

        // Check if the right index trigger is pressed.
        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            // Only add to the cumulative trigger time and process the ray if we haven't exceeded 10 seconds.
            if (cumulativeTriggerTime < 10f)
            {
                // Set vibration for the ray shooting (this will be overridden by water container feedback if inside water).
                OVRInput.SetControllerVibration(1.0f, 10.0f, OVRInput.Controller.RTouch);
               
                // Increment the cumulative time by the time elapsed since the last frame.
                cumulativeTriggerTime += Time.deltaTime;

                // Compute the distance from the origin to the intersection point.
                float distance = Vector3.Distance(origin, IntersectionPoint);

                // Update the first (regular) cylinder ray.
                if (cylinderRay != null)
                {
                    if (!cylinderRay.activeSelf)
                        cylinderRay.SetActive(true);

                    // Position at the midpoint.
                    cylinderRay.transform.position = origin + direction * (distance / 2f);
                    // Rotate so its Y axis aligns with the ray.
                    cylinderRay.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
                    // Adjust scale (note: Unity's default cylinder height is 2, so we use distance/2f).
                    cylinderRay.transform.localScale = new Vector3(cylinderRadius, distance / 2f, cylinderRadius);
                }

                // Update the thicker cylinder ray.
                if (cylinderRayThick != null)
                {
                    if (!cylinderRayThick.activeSelf)
                        cylinderRayThick.SetActive(true);

                    cylinderRayThick.transform.position = origin + direction * (distance / 2f);
                    cylinderRayThick.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
                    cylinderRayThick.transform.localScale = new Vector3(cylinderThickRadius, distance / 2f, cylinderThickRadius);
                }

                // Start the particle system if it's not already playing.
                if (waveParticles != null && !waveParticles.isPlaying)
                {
                    waveParticles.Play();
                }

                // Instantiate holes at the intersection point at defined intervals only if the hit object has the tag "Erasable".
                if (isHit && hit.transform.CompareTag("Erasable") && Time.time >= nextSpawnTime)
                {
                    if (holePrefab != null)
                    {
                        // Create a rotation aligning the prefab's up with the surface normal.
                        Quaternion holeRotation = Quaternion.FromToRotation(Vector3.up, IntersectionNormal);
                        GameObject newHole = Instantiate(holePrefab, IntersectionPoint, holeRotation);

                        // Save the intended scale of the hole.
                        Vector3 intendedScale = newHole.transform.localScale;
                        // Start with scale zero for animation.
                        newHole.transform.localScale = Vector3.zero;
                        // Animate scaling using the HoleScaleAnimation component.
                        HoleScaleAnimation anim = newHole.AddComponent<HoleScaleAnimation>();
                        anim.targetScale = intendedScale;
                    }
                    else
                    {
                        Debug.LogError("Hole prefab is not assigned.");
                    }

                    nextSpawnTime = Time.time + spawnInterval;
                }
            }
            else
            {
                // Once 10 cumulative seconds have been reached, disable the ray shooting.
                Debug.Log("Ray shooting disabled: 10 cumulative seconds reached.");
                OVRInput.SetControllerVibration(0.0f, 0.0f, OVRInput.Controller.RTouch);
                
                // Hide both cylinder rays if they are active.
                if (cylinderRay != null && cylinderRay.activeSelf)
                {
                    cylinderRay.SetActive(false);
                }
                if (cylinderRayThick != null && cylinderRayThick.activeSelf)
                {
                    cylinderRayThick.SetActive(false);
                }

                // Stop the particle system if it's playing.
                if (waveParticles != null && waveParticles.isPlaying)
                {
                    waveParticles.Stop();
                }
            }
        }
        else
        {
            // When the trigger is not pressed, disable ray shooting vibration.
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
            // Hide the cylinder rays and stop the particle system.
            if (cylinderRay != null && cylinderRay.activeSelf)
            {
                cylinderRay.SetActive(false);
            }
            if (cylinderRayThick != null && cylinderRayThick.activeSelf)
            {
                cylinderRayThick.SetActive(false);
            }
            if (waveParticles != null && waveParticles.isPlaying)
            {
                waveParticles.Stop();
            }
        }

        // --- Check for water container collision to reset the timer ---
        // If the object is inside the water container, accumulate a dwell time.
        if (waterContainerCollider != null)
        {
            // Using ClosestPoint to determine if the current position is inside the water container.
            Vector3 closestPoint = waterContainerCollider.ClosestPoint(transform.position);
            // If the distance is very small, assume the point is inside.
            if (Vector3.Distance(closestPoint, transform.position) < 0.01f)
            {
                // Increase the dwell timer.
                waterDwellTime += Time.deltaTime;

                // Compute a vibration amplitude that ramps from 0 to 1 over 3 seconds.
                float hapticAmplitude = Mathf.Clamp01(waterDwellTime / 3f);
                // Apply haptic feedback (using the same frequency as before, adjust as needed).
                OVRInput.SetControllerVibration(10.0f, hapticAmplitude, OVRInput.Controller.RTouch);

                // Only reset the cumulative trigger timer if the player has remained for 3 seconds.
                if (waterDwellTime >= 3f)
                {
                    cumulativeTriggerTime = 0f;
                    waterDwellTime = 0f; // Reset the dwell timer.
                    Debug.Log("Cumulative trigger timer reset because the object has been inside the water container for 3 seconds.");
                    OVRInput.SetControllerVibration(0.0f, 0.0f, OVRInput.Controller.RTouch);
                }
            }
            else
            {
                // Not inside the water container: reset the water dwell timer.
                waterDwellTime = 0f;
            }
        }

        // (Optional) Draw a debug ray in the Scene view.
        Debug.DrawRay(origin, direction * rayLength, isHit ? Color.green : Color.red);
    }
}

// This helper component animates the scale of the instantiated hole from 0 to its intended scale.
public class HoleScaleAnimation : MonoBehaviour
{
    [Tooltip("Duration for the scale animation in seconds.")]
    public float animationDuration = 0.25f;
    
    [Tooltip("The target scale the hole will reach (set by the spawner).")]
    public Vector3 targetScale = Vector3.one;

    void Start()
    {
        StartCoroutine(AnimateScale());
    }

    IEnumerator AnimateScale()
    {
        float elapsedTime = 0f;
        Vector3 startingScale = Vector3.zero;
        while (elapsedTime < animationDuration)
        {
            transform.localScale = Vector3.Lerp(startingScale, targetScale, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
