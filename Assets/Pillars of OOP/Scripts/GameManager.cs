using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    [SerializeField] private UIManager uiManager; // Reference to UIManager assigned in Inspector
    [SerializeField] private InspectionManager inspectionManager; // Reference to InspectionManager assigned in Inspector
    public CameraController cameraScript;
    public GameObject[] players; // Array of potential player characters
    private bool haveCharacter = false;
    GameObject selectedCharacter;
    [SerializeField] private List<EnemyPrefabMapping> enemyPrefabMappings;
    private Dictionary<EnemyType, GameObject> enemyPrefabs;
    private Dictionary<EnemyType, List<Transform>> spawnPointsByType;
    public Dictionary<Transform, List<GameObject>> activeEnemiesBySpawnPoint = new Dictionary<Transform, List<GameObject>>();
    public int maxEnemiesPerSpawnPoint = 5;

    private void Awake() {
        uiManager = FindObjectOfType<UIManager>(); // Dynamically assign UIManager
        if (uiManager == null) {
            Debug.LogError("UIManager not found in the scene. Make sure there's a UIManager in the scene.");
        } else {
            // Subscribe to the event
            uiManager.OnCharacterSelected += HandleCharacterSelected;
        }

        inspectionManager = FindObjectOfType<InspectionManager>(); // Dynamically assign InspectionManager
        if (inspectionManager == null) {
            Debug.LogError("InspectionManager not found in the scene. Make sure there's an InspectionManager in the scene.");
        }
    }
    private void Start() {
        // Initial camera settings for intro
        cameraScript.DisableFollowing();
        haveCharacter = false;

        // Ensure the character selection panel is shown initially
        uiManager.ShowCharacterSelection(true);

        enemyPrefabs = new Dictionary<EnemyType, GameObject>();
        foreach (var mapping in enemyPrefabMappings) {
            enemyPrefabs[mapping.enemyType] = mapping.prefab;
        }

        InitializeSpawnPoints();        
        SpawnEnemies();

    }


    private void Update() {
        if (!haveCharacter) {
            if (Input.GetMouseButtonDown(0)) {
                Debug.Log("Game Manager registered a click.");

                // Get the clicked object using ObjectSelector
                GameObject clickedObject = ObjectSelector.GetClickedObject();

                if (clickedObject != null) {
                    Player playerComponent = clickedObject.GetComponent<Player>();
                    if (playerComponent != null) {
                        Debug.Log("Selected a player: " + playerComponent.name);
                        selectedCharacter = clickedObject;
                        inspectionManager.Inspect(selectedCharacter);
                        uiManager.ReadyToSelect(true);  // Enable the character selection button
                    } else {
                        Debug.Log("The clicked object is not a player.");
                        uiManager.ReadyToSelect(false);
                    }
                }
            }
        }
    }

    private void HandleCharacterSelected() {
        // Call ActivatePlayer when the selection button is clicked
        Debug.Log("Game Manager character selection handling");
        if (selectedCharacter != null) {
            Player playerComponent = selectedCharacter.GetComponent<Player>();
            if (playerComponent != null) {
                ActivatePlayer(playerComponent);
            }
        }
    }

    private void ActivatePlayer(Player selectedPlayer) {
        // Activate only the selected player
        Debug.Log("Game Manager character activation");
        for (int i = 0; i < players.Length; i++) {
            Player player = players[i].GetComponent<Player>();
            player.Activation(player == selectedPlayer);
        }

        // Update other relevant game states
        haveCharacter = true;
        cameraScript.AttachToPlayer(selectedPlayer.transform);

        // Hide the character selection and inspection UI after activation
        uiManager.ShowCharacterSelection(false);
        uiManager.ShowInspectionPanel(false);
    }

    private void InitializeSpawnPoints() {

        spawnPointsByType = new Dictionary<EnemyType, List<Transform>>();
        SpawnPoint[] allSpawnPoints = FindObjectsOfType<SpawnPoint>();

        foreach (var spawnPoint in allSpawnPoints) {
            if (!spawnPointsByType.ContainsKey(spawnPoint.enemyType)) {
                spawnPointsByType[spawnPoint.enemyType] = new List<Transform>();
            }

            spawnPointsByType[spawnPoint.enemyType].Add(spawnPoint.transform);
        }
    }

    public void SpawnEnemy(EnemyType enemyType) {
        // Utility function to spawn singular enemies
        if (spawnPointsByType.ContainsKey(enemyType) && spawnPointsByType[enemyType].Count > 0) {
            List<Transform> spawnPoints = spawnPointsByType[enemyType];
            Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            if (enemyPrefabs.TryGetValue(enemyType, out GameObject enemyPrefab)) {
                Instantiate(enemyPrefab, selectedSpawnPoint.position, Quaternion.identity);
            } else {
                Debug.LogWarning($"No prefab assigned for enemy type {enemyType}");
            }
        }
    }

    public void SpawnEnemies() {

        Debug.Log("Spawning enemies:");
        foreach (var spawnPointEntry in spawnPointsByType) {
            Debug.Log($"  Spawn points for enemy type: {spawnPointEntry.Key}");

            foreach (Transform point in spawnPointEntry.Value) {
                Debug.Log($"    Location: {point}");

                if (enemyPrefabs.TryGetValue(spawnPointEntry.Key, out GameObject enemyPrefab)) {
                    int spawnCount = Random.Range(1, maxEnemiesPerSpawnPoint + 1);

                    for (int i = 0; i < spawnCount; i++) {
                        Debug.Log($"      Spawning enemy {i + 1} of {spawnCount}");
                        GameObject enemyInstance = Instantiate(enemyPrefab, point.position, Quaternion.identity);
                        
                        // Add to tracking dictionary
                        if (!activeEnemiesBySpawnPoint.ContainsKey(point)) {
                            activeEnemiesBySpawnPoint[point] = new List<GameObject>();
                        }

                        activeEnemiesBySpawnPoint[point].Add(enemyInstance);
                        
                        // Assign GameManager to the Enemy script for self-removal on death
                        enemyInstance.GetComponent<Enemy>().SetGameManager(this);
                    }

                } else {
                    Debug.LogWarning($"No prefab assigned for enemy type {spawnPointEntry.Key}");
                }
            }
        }
    }

    public void RemoveEnemyFromList(GameObject enemy) {
        foreach (var spawnPoint in activeEnemiesBySpawnPoint.Keys) {
            if (activeEnemiesBySpawnPoint[spawnPoint].Contains(enemy)) {
                activeEnemiesBySpawnPoint[spawnPoint].Remove(enemy);
                break; // Exit loop once the enemy is found and removed
            }
        }
    }


    private GameObject GetEnemyPrefabByType(EnemyType enemyType) {
        // Implement logic to return the appropriate prefab for each enemy type
        return null;
    }


}
