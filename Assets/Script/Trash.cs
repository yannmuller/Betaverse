using UnityEngine;
using System.Collections;

public class BinScript : MonoBehaviour
{
    // Counter for how many trash objects have been collected
    private int trashCounter = 0;

    // When another collider enters the trigger attached to this GameObject
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "trash" tag
        if (other.CompareTag("trash"))
        {
            trashCounter++;
            Debug.Log("Trash collected: " + trashCounter);

            // Start the scale down animation coroutine on the trash object
            StartCoroutine(ScaleDownAndDestroy(other.gameObject));
        }
    }

    // Coroutine to smoothly scale the trash object down to zero before destroying it
    private IEnumerator ScaleDownAndDestroy(GameObject trash)
    {
        float duration = 0.5f; // Duration of the scale down animation in seconds
        float elapsed = 0f;
        Vector3 originalScale = trash.transform.localScale;

        // Optionally disable the collider and physics to prevent further interactions during the animation
        Collider trashCollider = trash.GetComponent<Collider>();
        if (trashCollider != null)
        {
            trashCollider.enabled = false;
        }
        Rigidbody trashRigidbody = trash.GetComponent<Rigidbody>();
        if (trashRigidbody != null)
        {
            trashRigidbody.isKinematic = true;
        }

        // Animate the scale down over 'duration' seconds
        while (elapsed < duration)
        {
            trash.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the final scale is set to zero
        trash.transform.localScale = Vector3.zero;
        Destroy(trash);
    }
}
