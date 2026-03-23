using UnityEngine;

[CreateAssetMenu(menuName = "TD Merge/Unit Data", fileName = "UnitData_")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    public string id; // e.g., "Warrior", "Mage"

    [Header("Visuals + Spawn")]
    public GameObject prefab; // world unit to spawn
    public Sprite icon;       // UI icon
}
