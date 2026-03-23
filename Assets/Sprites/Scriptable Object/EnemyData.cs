using UnityEngine;

[CreateAssetMenu(menuName = "TD Merge/Enemy Data", fileName = "EnemyData_")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string id;               // e.g. "Grunt", "Brute"
    public Enemy prefab;

    [Header("Stats")]
    public int hp           = 3;
    public float speed      = 1f;
    public int attackDamage = 1;
    public float attackCooldown = 1f;

    [Header("Reward")]
    public int goldReward   = 5;
}
