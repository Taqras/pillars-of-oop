using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Character : MonoBehaviour, IInspectable, IDamageable, IInteractable {

    protected CharacterController characterController;

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
                health = 0;
            }
        }
    }

    private int maxHealth = 0; // Backing field for maxHealth, defaults to none
    public virtual int MaxHealth {
        get => maxHealth;
        protected set {
            if (value >= 0) {
                maxHealth = value;
            } else {
                maxHealth = 0;
            }
        }
    }


    private int mana = 0; // Backing field for mana, defaults to none
    public virtual int Mana {
        get => mana;
        protected set {
            if (value >= 0) {
                mana = value;
            } else {
                mana = 0;
            }
        }
    }

    private int maxMana = 0; // Backing field for maxMana, defaults to none
    public virtual int MaxMana {
        get => maxMana;
        protected set {
            if (value >= 0) {
                maxMana = value;
            } else {
                maxMana = 0;
            }
        }
    }

    private int experience = 0; // Backing field for experience, defaults to none
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

    protected void Awake() {
        Debug.Log($"{gameObject.name} running Character.Awake()");
        characterController = GetComponent<CharacterController>();
        if (characterController == null) {
            Debug.LogError("CharacterController is missing!");
        } else {
            Debug.Log("CharacterController is assigned successfully.");
        }
    }

    public abstract void Move();
    public abstract void Interact();
    public abstract void Attack();
    public abstract void Defend();
    public abstract void TakeDamage(int damage);
    public abstract void Die();

    public bool IsSteepSlope(Vector3 position) {
        // Cast a ray downward from the given position to detect the ground surface
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity)) {
            // Calculate the slope angle based on the ground normal
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            
            // Check if the slope angle exceeds the character's slope limit
            return slopeAngle > characterController.slopeLimit;
        }
        return false; // Return false if no ground was detected
    }

    public bool ConsumeMana(int amount) {
        if (Mana >= amount) {
            Mana -= amount;
            Debug.Log($"{CharacterName} consumed {amount} of mana. Remaining mana is {Mana}");
            return true;
        } else {
            Debug.Log($"{CharacterName} does not have enough mana!");
            return false;
        }
    }

}
