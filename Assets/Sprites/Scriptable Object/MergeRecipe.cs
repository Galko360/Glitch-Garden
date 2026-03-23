using UnityEngine;

[CreateAssetMenu(menuName = "TD Merge/Merge Recipe", fileName = "MergeRecipe_")]
public class MergeRecipe : ScriptableObject
{
    public UnitData inputA;
    public UnitData inputB;
    public UnitData output;
}
