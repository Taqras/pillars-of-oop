using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Configurations/Enemy Config")]
public class EnemyConfig : ScriptableObject {
    public int ExperienceReward;
    public int HealthReward;
    public int ManaReward;
}