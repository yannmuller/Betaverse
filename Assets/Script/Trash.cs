using UnityEngine;

public class BinScript : MonoBehaviour
{
    // Counter for how many trash objects have been collected
    private int trashCounter = 0;

    // This method is called when another collider enters the trigger collider attached to this GameObject.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "trash" tag
        if (other.CompareTag("trash"))
        {
            // Increment the counter
            trashCounter++;
            Debug.Log("Trash collected: " + trashCounter);

            // Destroy the trash object
            Destroy(other.gameObject);
        }
    }
}
