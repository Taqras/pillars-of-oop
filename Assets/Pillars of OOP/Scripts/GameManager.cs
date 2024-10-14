using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public CameraController cameraScript;
    public GameObject[] players; // Array of potential player characters

    private void Start() {
        // Initial camera settings for intro
        cameraScript.DisableFollowing();

        // Example intro sequence logic here...

        // After intro, start the game and assign camera to a specific player
        StartGame();
    }

    private void StartGame() {
        int selectedPlayerIndex = 1; // Assuming player selection logic here

        SetActivePlayer(selectedPlayerIndex);

        // Tell the camera to start following the selected player
        GameObject selectedPlayer = players[selectedPlayerIndex];
        cameraScript.AttachToPlayer(selectedPlayer.transform);
    }

    public void SetActivePlayer(int index) {
        for (int i = 0; i < players.Length; i++) {
            players[i].GetComponent<Player>().Activation(i == index);
        }
    }

}
