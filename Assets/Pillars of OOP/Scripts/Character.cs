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

    protected void Awake() {
        Debug.Log($"{gameObject.name} running Character.Awake()");
        characterController = GetComponent<CharacterController>();
        if (characterController == null) {
            Debug.LogError("CharacterController is missing!");
        } else {
            Debug.Log("CharacterController is assigned successfully.");
        }
    }

    // Example method for managing health safely
    public abstract void Move();
    public abstract void Interact();
    public abstract void Attack();
    public abstract void Defend();
    public abstract void TakeDamage(int damage);

    protected bool IsSteepSlope(Vector3 moveDirection) {
        if (moveDirection == null) {
            Debug.Log("moveDirection is null!");
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f)) {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
          
            // Check if moving downhill (we don't care about uphill)
            bool isMovingDownhill = Vector3.Dot(moveDirection, hit.normal) > 0;

            // Debugging to understand what's happening
            // Debug.Log($"Slope Angle: {slopeAngle}, Move Direction: {moveDirection}, Hit Normal: {hit.normal}, Downhill: {isMovingDownhill}");


            // Only block downhill movement on steep slopes
            if (slopeAngle > characterController.slopeLimit && isMovingDownhill) {
                // Debug.Log($"Slope Angle: {slopeAngle}, Move Direction: {moveDirection}, Hit Normal: {hit.normal}, Downhill: {isMovingDownhill}");
                return true;
            }
        }
        return false;
    }

}
