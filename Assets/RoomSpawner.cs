using UnityEngine;
using Meta.XR.MRUtilityKit;

public class RoomSpawner : MonoBehaviour
{
    public GameObject prefab;
    public float minRadius = 0.5f;
    public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;

    public MRUK.SurfaceType surfaceType = (MRUK.SurfaceType)~0;
    public int maxTryCount = 100;
    public int maxSpawnCount = 10;
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
    // Update is called once per frame
    public void StartSpawn(MRUKRoom room)
    {
        int objCount = 0;
        for (int i = 0; i < maxTryCount; i++)
        {
            if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, new LabelFilter(Labels), out var pos, out var normal))
            {
                GameObject obj = Instantiate(prefab, pos, Quaternion.LookRotation(normal));
                objCount++;
            }
        }
    }
}
