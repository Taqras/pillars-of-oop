using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class GameManager : MonoBehaviour {
    // GameManager acts as the central hub, managing game states like Combat Mode
    // and coordinating interactions between various game objects.
    public UIManager UIManager { get; private set; }
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
    public bool IsCombatMode { get; private set; } = false;

    private void Awake() {

        UIManager = FindObjectOfType<UIManager>(); // Dynamically assign UIManager

        if (UIManager == null) {
            Debug.LogError("UIManager not found in the scene. Make sure there's a UIManager in the scene.");
        } else {
            // Subscribe to the event
            UIManager.OnCharacterSelected += HandleCharacterSelected;
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
        UIManager.ShowCharacterSelection(true);
        UIManager.ShowInteractionPanel(
            "Controls:\n" +
            "  Move: W=Forwards, A=Strafe left, S=Strafe right, S=Backwards.\n" +
            "  Left mouse button: Click to inspect or attack. Drag to turn.\n" +
            "  Right mouse button: Click to interact or defend. Drag to free-look.\n" +
            "  Toggle walk/run: Shift\n" +
            "  Toggle fight/explore: Tab"
        );

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
                        UIManager.ReadyToSelect(true);  // Enable the character selection button
                    } else {
                        Debug.Log("The clicked object is not a player.");
                        UIManager.ReadyToSelect(false);
                    }
                }
            }
        } else { // we have a character
            if (Input.GetKeyDown(KeyCode.Tab)) {
                ToggleCombatMode();
            }
        }
    }

    public void ToggleCombatMode() {
        IsCombatMode = !IsCombatMode;

        Debug.Log(IsCombatMode ? "Entering Combat Mode" : "Exiting Combat Mode");

        // Observer Pattern: Notify all enemies of combat mode changes without requiring direct references,
        // maintaining loose coupling between GameManager and individual enemies.
        var allEnemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in allEnemies) {
            Debug.Log($"{enemy.CharacterName}: Toggling health bar to {(IsCombatMode ? "visible" : "hidden")}");
            if (IsCombatMode) {
                enemy.ShowHealthBar();
            } else {
                enemy.HideHealthBar();
            }
        }

        // Notify the active player
        Player activePlayer = GetActivePlayer();
        if (activePlayer != null) {
            activePlayer.SetCombatMode(IsCombatMode);
        }
    }

    private void HandleCharacterSelected() {
        // Call ActivatePlayer when the selection button is clicked
        Debug.Log("Game Manager character selection handling");
        if (selectedCharacter != null) {
            Player playerComponent = selectedCharacter.GetComponent<Player>();
            if (playerComponent != null) {
                // Subscribe UIManager to player's health and mana events
                playerComponent.OnHealthChanged += UIManager.UpdateHealthIndicator;
                playerComponent.OnManaChanged += UIManager.UpdateManaIndicator;

                // Set the initial values for the health and mana sliders
                UIManager.UpdateHealthIndicator(playerComponent.Health);
                UIManager.UpdateManaIndicator(playerComponent.Mana);

                // Also set the max values for the sliders
                UIManager.SetHealthMaxValue(playerComponent.MaxHealth);
                UIManager.SetManaMaxValue(playerComponent.MaxMana);

                // Activate the selected player
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
        UIManager.ShowCharacterSelection(false);
        UIManager.ShowInspectionPanel(false);

        UIManager.SetHealthMaxValue(selectedPlayer.MaxHealth);
        UIManager.UpdateHealthIndicator(selectedPlayer.Health);

        UIManager.DisplayMana(selectedPlayer.usesMana);
        UIManager.SetManaMaxValue(selectedPlayer.MaxMana);
        UIManager.UpdateManaIndicator(selectedPlayer.Mana);
    }

    // Using LINQ (Language Integrated Query) Method Syntax
    public Player GetActivePlayer() {
        return players
            .Select(playerGO => playerGO.GetComponent<Player>())  // Access the Player component
            .FirstOrDefault(player => player != null && player.isActive);  // Check for active players
    }

    // Using LINQ (Language Integrated Query) Query Syntax
    // public Player GetActivePlayer() {
    //     var activePlayer = (from playerGO in players
    //                         let player = playerGO.GetComponent<Player>() // Get the Player component
    //                         where player != null && player.isActive      // Filter active players
    //                         select player).FirstOrDefault();            // Return the first active player
    //     return activePlayer;
    // }

    // Alternative without LINQ
    // public Player GetActivePlayer() {
    //     foreach (GameObject playerGO in players) {
    //         Player player = playerGO.GetComponent<Player>();
    //         if (player != null && player.isActive) {
    //             return player;
    //         }
    //     }
    //     return null; // No active player found
    // }

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
                    int spawnCount = Random.Range(Mathf.FloorToInt(maxEnemiesPerSpawnPoint / 2), maxEnemiesPerSpawnPoint + 1);

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

    public void HandleInteraction(GameObject target) {
        // Check if the target is interactable
        if (target.TryGetComponent<IInteractable>(out IInteractable interactable)) {
            interactable.Interact(); // Call the generic interaction
        } else {
            Debug.Log($"{target.name} is not interactable.");
        }
        
        // Normal assign, null-check, do construct:
        // NPC interactable = target.GetComponent<IInteractable>();
        // if (interactable != null) {
        //     interactable.Interact(); // Call the interactable's interaction logic
        // } else {
        //     Debug.Log($"{target.name} is not interactable.");
        // }

    }

}
