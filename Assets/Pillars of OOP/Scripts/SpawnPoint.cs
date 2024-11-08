using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnPoint : MonoBehaviour {
    public EnemyType enemyType; // This references the enum in EnemyPrefabMapping.cs


// Optionally, add a gizmo for easy visualization
private void OnDrawGizmos() {
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(transform.position, 0.1f);
}

}