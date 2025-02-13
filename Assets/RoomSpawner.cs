using UnityEngine;
using Meta.XR.MRUtilityKit;
using Unity.XR.CoreUtils;
using System.Collections.Generic;

public class RoomSpawner : MonoBehaviour
{
    // List of prefabs to spawn
    public List<GameObject> prefabs = new List<GameObject>();
    // Min/Max number of prefabs to spawn for each object type
    public int minObjectsPerPrefab = 1;
    public int maxObjectsPerPrefab = 5;

    public float minRadius = 2.5f;
    public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;
    public MRUK.SurfaceType surfaceType = (MRUK.SurfaceType)~0;
    public int maxTryCount = 100;
    public int maxSpawnCount = 10;

    public LayerMask layerMask = 0;
    public Vector3 boxSize;

    public int objCount = 0;

    public float distanceFromSurfaceForBoundsCheck = 0.1f;

    private void Start()
    {
        if (MRUK.Instance)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                StartSpawn(MRUK.Instance.GetCurrentRoom());
            });
        }
    }

    // This function starts the spawn process
    public void StartSpawn(MRUKRoom room)
    {
        int skipped = 0;
        int tried = 0;
        int foundPos = 0;

        // Loop through the maximum number of spawn attempts
        for (int i = 0; i < maxTryCount; i++)
        {
            tried++;

            // Generate a random position on the surface
            if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, new LabelFilter(Labels), out var pos, out var normal))
            {
                foundPos++;

                Vector3 halfExtents = boxSize / 2;
                Vector3 center = pos + normal * (halfExtents.y + distanceFromSurfaceForBoundsCheck);
                Quaternion rotation = Quaternion.LookRotation(normal);

                // Check if the position is occupied by another object
                if (Physics.CheckBox(center, halfExtents, rotation, layerMask, QueryTriggerInteraction.Collide))
                {
                    skipped++;
                    continue;
                }

                // Spawn multiple prefabs with random counts
                SpawnPrefabsAtPosition(pos, normal);

                // Stop spawning if we've reached the maximum spawn count
                if (objCount >= maxSpawnCount)
                    break;
            }
        }

        //Debug.Log($"{tried} attempts, {foundPos} valid positions, {skipped} skipped, {objCount} objects spawned.");
    }

    // Spawns multiple prefabs at a given position
    private void SpawnPrefabsAtPosition(Vector3 position, Vector3 normal)
    {
        foreach (var prefab in prefabs)
        {
            // Randomly determine how many of each prefab to spawn
            int numToSpawn = Random.Range(minObjectsPerPrefab, maxObjectsPerPrefab + 1);

            for (int j = 0; j < numToSpawn; j++)
            {
                // Randomly generate an offset on the surface
                // This will create a random offset within the defined minRadius, but applied in a manner that's aligned with the surface normal.
                Vector3 randomOffset = new Vector3(
                    Random.Range(-minRadius, minRadius),
                    Random.Range(-minRadius, minRadius),
                    Random.Range(-minRadius, minRadius)
                );

                // Calculate the final position by applying the offset along the surface's normal
                Vector3 offsetAlongSurface = Vector3.ProjectOnPlane(randomOffset, normal);

                // The final position will be the base position + the offset along the surface normal
                Vector3 spawnPosition = position + offsetAlongSurface;

                // Instantiate the prefab at the new position
                Instantiate(prefab, spawnPosition, Quaternion.LookRotation(normal), transform);
                objCount++;
            }
        }
    }

}
