using UnityEngine;

public enum EnemyType {
    Rabbit,
    Wolf,
    Goblin,
    // Add other enemy types here as needed
}

[System.Serializable]
public class EnemyPrefabMapping {
    public EnemyType enemyType;
    public GameObject prefab;
}
