using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {

    private CharacterController characterController;
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

    private float jumpForce = 8f;         // The force applied when jumping (adjust as needed)
    private float gravity = 20f;          // Gravity force applied when the character is not grounded
    private float verticalVelocity = 0f;  // Tracks the vertical velocity of the character

    public CharacterConfig characterConfig; // Reference to the Scriptable Object

    private void Start() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Use the value from CharacterConfig to set the animator parameter
            if (characterConfig != null)
            {
                Debug.Log($"{gameObject.name} is adjusting walk and run animation by {characterConfig.walkPlaybackSpeed} and {characterConfig.runPlaybackSpeed}");
                animator.SetFloat("WalkPlaybackSpeed", characterConfig.walkPlaybackSpeed);
                animator.SetFloat("RunPlaybackSpeed", characterConfig.runPlaybackSpeed);
            }

        SetHealth(100);  // Initial health for the player

    }

    private void FixedUpdate() {
        if (isActive) {
            Move();
        }
    }

    private void Update() {
        if (isActive) {

            animator.SetBool("isGrounded", characterController.isGrounded);

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

            if (Input.GetKeyDown(KeyCode.Tab)) {
                if (isCombatReady) {
                    ExitFocusMode();
                } else {
                    EnterFocusMode();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded) {
                Debug.Log("Jumping");
                animator.SetTrigger("jumpTrigger");
                verticalVelocity = jumpForce;  // Apply jump force when the jump starts
            }
        }
    }
    
    public void GainExperience(int amount) {
        Experience += amount;
    }

    public override void Move() {

        // Specific movement for Player
        float horizontal = Input.GetAxis("Horizontal");  // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical");      // W/S or Up/Down arrows

        // Rotate the player based on horizontal input (A/D)
        if (Mathf.Abs(horizontal) > 0.1f) {
            float rotationSpeed = 100f; // Adjust rotation speed as needed
            transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);
        }

        // Calculate the movement direction
        Vector3 direction = transform.forward * vertical;
        direction = direction.normalized * Speed;

        // Apply gravity when character is not grounded
        if (characterController.isGrounded && verticalVelocity < 0) {
            // If grounded and falling, reset vertical velocity to a small value to stay grounded
            verticalVelocity = -1f;  // A small negative value to ensure the character stays grounded
        } else {
            // Apply gravity if character is in the air
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Add vertical velocity to the movement direction (gravity or jump)
        direction.y = verticalVelocity;

        // Apply movement to the Character Controller to move the player
        characterController.Move(direction * Time.deltaTime);

        // Calculate input magnitude based on vertical movement (used for animations)
        float inputMagnitude = Mathf.Abs(vertical);  // The value will be between 0 (idle) and 1 (full speed)

        // Set the Speed parameter in the Animator to control walking/running animations
        animator.SetFloat("Speed", inputMagnitude);

    }
    
    // Encapsulated setter for isActive with additional logic
    public void Activation(bool active)
    {
        isActive = active;

        if (isActive) {
            Debug.Log($"{gameObject.name} is now active.");
        }
        else {
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

    void OnDrawGizmosSelected() {
    CharacterController characterController = GetComponent<CharacterController>();
    if (characterController != null) {
        // Calculate bottom center position
        float bottomCenterY = transform.position.y + characterController.center.y - (characterController.height / 2) + characterController.radius;
        Vector3 bottomCenterPosition = new Vector3(transform.position.x, bottomCenterY, transform.position.z);

        // Draw a red sphere to visualize the bottom center
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(bottomCenterPosition, 0.05f);
    }
    }

}
