using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Character : MonoBehaviour {

    // Speed is protected and initialized with a default value
    protected float Speed { get; private set; } = 3f;  // Default speed, can be overridden in derived classes

    // Encapsulated Name: Only allow setting the name once
    private string characterName;
    public string CharacterName {
        get { return characterName; }
        protected set {
            // Only allow setting the name if it hasn't been set yet
            if (string.IsNullOrEmpty(characterName)) {
                characterName = value;
            } else {
                Debug.LogError("Name has already been set and cannot be changed.");
            }
        }
    }

    // Abstract property for health; derived classes must define it
    public abstract int Health { get; protected set; }

    // Controlled way to set health
    protected void SetHealth(int newHealth) {
        if (newHealth >= 0) {
            Health = newHealth;
        } else {
            Debug.LogError("Health cannot be negative.");
        }
    }

    // Example method for managing health safely
    public void TakeDamage(int damage) {
        if (damage >= 0) {
            SetHealth(Mathf.Max(Health - damage, 0));  // Ensure health doesn't go below 0
        }
    }

    public abstract void Move();
    public abstract void Interact();
    public abstract void Attack();
    public abstract void Defend();
}
