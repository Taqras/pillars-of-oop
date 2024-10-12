using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {

    private CharacterController controller;
    private Animator animator;
    public bool isActive = false;  // Indicates if this player is currently active

    // Implement health in the derived class
    public override int Health { get; protected set; }
    private int experience = 0;
    public int Experience {
        get { return experience; }
        private set { experience = value; }
    }
    private bool isCombatReady = false;
    private Transform target;

    private void Start() {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        SetHealth(100);  // Initial health for the player

        // Optionally override speed programmatically if needed
        // Speed = 3f;
    }

    private void Update() {
        if (isActive) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                if (isCombatReady) {
                    ExitFocusMode();
                } else {
                    EnterFocusMode();
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                if (isCombatReady) {
                    Attack();
                } else {
                    // Select target for inspection
                    // Placeholder: raycast logic or click handling here
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                if (isCombatReady) {
                    Defend();
                } else {
                    // Interact with selected object
                    Interact();
                }
            }



            Move();
        }
    }
    
    public void GainExperience(int amount) {
        Experience += amount;
    }

    public override void Move() {

        // Specific movement for Player
        // Player movement logic using input
        float horizontal = Input.GetAxis("Horizontal");  // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical");      // W/S or Up/Down arrows

        // Rotate the player based on horizontal input (A/D)
        if (Mathf.Abs(horizontal) > 0.1f) {
            float rotationSpeed = 100f; // Adjust rotation speed as needed
            transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);
        }

        // Move the player forward/backward based on vertical input (W/S)
        Vector3 direction = transform.forward * vertical;
        float inputMagnitude = Mathf.Abs(vertical);

        // Set the Speed parameter in the Animator to trigger walking animations
        animator.SetFloat("Speed", inputMagnitude);

/*         if (inputMagnitude >= 0.1f) {

            // Rotate the player towards the movement direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);

            // Apply movement to the CharacterController to move the player forward
            controller.Move(direction * Speed * Time.deltaTime);  // Use Move() for full control of movement
        } */
        
        // Apply movement to the CharacterController to move the player forward/backward
        controller.Move(direction * Speed * Time.deltaTime);

    }
    
    // Encapsulated setter for isActive with additional logic
    public void Activation(bool active)
    {
        isActive = active;

        if (isActive)
        {
            // animator.SetTrigger("Activate"); // Trigger an activation animation
            Debug.Log($"{gameObject.name} is now active.");
        }
        else
        {
            Debug.Log($"{gameObject.name} is now inactive.");
        }
    }

        // Combat and exploration methods
    public void EnterFocusMode() {
        isCombatReady = true;
        // Placeholder for drawing weapons, setting combat state, etc.
    }

    public void ExitFocusMode() {
        isCombatReady = false;
        // Placeholder for sheathing weapons, switching to exploration state, etc.
    }

    public override void Attack() {
        if (isCombatReady && target != null) {
            // Placeholder for offensive action logic (melee, ranged, or magic)
            Debug.Log($"{CharacterName} attacks the target.");
        }
    }

    public override void Defend() {
        if (isCombatReady) {
            // Placeholder for defensive action logic (e.g., block, parry, aim)
            Debug.Log($"{CharacterName} is defending.");
        }
    }

    public override void Interact() {
        if (!isCombatReady && target != null) {
            // Placeholder for interaction logic (e.g., NPC dialogue, pickup item)
            Debug.Log($"{CharacterName} is interacting.");
        }
    }

    public void SelectTarget(Transform newTarget) {
        target = newTarget;
        // Placeholder for focusing on a target (e.g., enemy, NPC, or item)
    }

}
