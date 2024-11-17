using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public event Action OnCharacterSelected;
    [SerializeField] private GameObject characterSelectionPanel;              // Assign in Inspector
    [SerializeField] private Button characterSelectionButton;                 // Assign in Inspector
    [SerializeField] private GameObject inspectionPanel;                      // Assign in Inspector
    [SerializeField] private TMPro.TextMeshProUGUI inspectionNameText;        // Assign in Inspector
    [SerializeField] private TMPro.TextMeshProUGUI inspectionDescriptionText; // Assign in Inspector
    [SerializeField] private Slider healthSlider;                             // Assign in Inspector
    [SerializeField] private Slider manaSlider;                               // Assign in Inspector

    public void Start() {
        inspectionPanel.SetActive(false);
        // ShowCharacterSelection(false);
        ReadyToSelect(false);
    }

    public void ShowCharacterSelection(bool show) {
        characterSelectionPanel.SetActive(show);
    }

    public void ShowInspectionPanel(bool show) {
        inspectionPanel.SetActive(show);
    }

    public void ReadyToSelect(bool ready) {
            characterSelectionButton.interactable = ready;
    }

    public void DisplayInfo(Dictionary<string, string> info) {
        inspectionPanel.SetActive(true);
        // Update the appropriate UI elements based on the keys in the dictionary
        if (info.ContainsKey(InspectionKey.Name.ToString()) && inspectionNameText != null) {
            inspectionNameText.text = info[InspectionKey.Name.ToString()];
        }

        if (info.ContainsKey(InspectionKey.Description.ToString()) && inspectionDescriptionText != null) {
            inspectionDescriptionText.text = info[InspectionKey.Description.ToString()];
        }
    }

    public void OnClickInspectorButton() {
        inspectionPanel.SetActive(false);
    }

    public void OnClickCharacterSelectionButton() {
        Debug.Log("A character has been selected");
        OnCharacterSelected?.Invoke();  // Trigger the event to notify GameManager

    }

    public void SetHealthMaxValue(int maxHealth) {
        healthSlider.maxValue = maxHealth;
    }

    public void SetManaMaxValue(int maxMana) {
        manaSlider.maxValue = maxMana;
    }

    public void UpdateHealthIndicator(int health) {
        healthSlider.value = health;
    }

    public void UpdateManaIndicator(int mana) {
        manaSlider.value = mana;
    }

    public void DisplayMana(bool showMana) {
        manaSlider.gameObject.SetActive(showMana);
    }

}
