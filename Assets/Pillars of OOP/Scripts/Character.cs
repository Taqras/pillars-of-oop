using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Character : MonoBehaviour, IInspectable {

    // Speed is protected and initialized with a default value
    private float speed = 2f; // Backing field for speed, defaults to something sensible
    public virtual float Speed {
        get => speed;
        protected set => speed = value; // Correct syntax
    }

    // Encapsulated Name: ensure that all derived classes have a name
    private string characterName = "Unnamed"; // Backing field for name (name is reserved in unity, hence characterName)
    public virtual string CharacterName {
        get => characterName;
        protected set {
            if (!string.IsNullOrEmpty(value)) {
                characterName = value;
            } else {
                Debug.LogError("Name cannot be empty.");
            }
        }
    }

    public string Description { get; protected set; } = "No description available."; // Default description

    private int health = 0; // Backing field for health, defaults to dead
    public virtual int Health {
        get => health;
        protected set {
            if (value >= 0) {
                health = value;
            } else {
                Debug.LogError("Health cannot be negative.");
            }
        }
    }

    private int experience = 0; // Backing field for experience, defaults to 0
    public virtual int Experience {
        get => experience;
        protected set {
            if (value >= 0) {
                experience = value;
            } else {
                Debug.LogError("Experience cannot be negative.");
            }
        }
    }


    public virtual Dictionary<string, string> GetInfo() {
        var info = new Dictionary<string, string>
        {
            { InspectionKey.Name.ToString(), CharacterName },
            { InspectionKey.Description.ToString(), Description }
        };

        return info;
    }

    // Example method for managing health safely
    public abstract void Move();
    public abstract void Interact();
    public abstract void Attack();
    public abstract void Defend();

}
