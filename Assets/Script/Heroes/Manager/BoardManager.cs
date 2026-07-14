using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private Lane lanePrefab;
    [SerializeField] private int laneCount;
    [SerializeField] private Transform boardOrigin;

    [Header("Lane Settings")]
    [SerializeField] private float laneSpacing;

    private void Awake()
    {
        GenerateLanes();
    }

    private void GenerateLanes()
    {
        Vector3 currentLaneSpawnPos = boardOrigin.position;

        for (int i = 0; i < laneCount; i++)
        {
            Lane lane = Instantiate(lanePrefab, currentLaneSpawnPos, Quaternion.identity, boardOrigin);
            //lane.Initialize();

            currentLaneSpawnPos.y -= laneSpacing;
        }
    }
}
