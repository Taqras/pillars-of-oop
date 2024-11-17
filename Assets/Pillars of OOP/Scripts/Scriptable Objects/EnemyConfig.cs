using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Configurations/Enemy Config")]
public class EnemyConfig : ScriptableObject {
    public EnemyType enemyType;
    public int MaxHealth;
    public int ExperienceReward;
    public int HealthReward;
    public int ManaReward;
    public float aggressionRange;
    public float pursueDistance;
    public float attackDelay;
    public float attackDistance;
    public int attackPower;
    public float walkSpeed;
    public float runSpeed;
    public string characterName = "Unnamed";
    public string description = "No description";

}