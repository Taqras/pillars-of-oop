using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    [SerializeField] private UIManager uiManager; // Reference to UIManager assigned in Inspector
    [SerializeField] private InspectionManager inspectionManager; // Reference to InspectionManager assigned in Inspector
    public CameraController cameraScript;
    public GameObject[] players; // Array of potential player characters
    public bool isCombatMode = false; // A global state for managing combat mode
    private bool haveCharacter = false;
    GameObject selectedCharacter;

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

/*     public void OnPlayerClicked(GameObject clickedPlayer) {
        Player playerComponent = clickedPlayer.GetComponent<Player>();
        if (playerComponent != null) {
            Debug.Log("Selected a player: " + playerComponent.name);
            selectedCharacter = clickedPlayer;
            inspectionManager.Inspect(selectedCharacter);
            uiManager.ReadyToSelect(true);  // Enable the character selection button
        } else {
            Debug.Log("The clicked object is not a player.");
            uiManager.ReadyToSelect(false);
        }
    } */

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

        // Hide the character selection UI after activation
        uiManager.ShowCharacterSelection(false);
    }

    public void ToggleCombatMode()
    {
        isCombatMode = !isCombatMode;
        foreach (GameObject player in players) {
            Player playerComponent = player.GetComponent<Player>();
            playerComponent.IsCombatReady = isCombatMode; // Set combat mode via encapsulated property

        }
    }

}
