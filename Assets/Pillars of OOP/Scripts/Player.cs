using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : Character {
    [SerializeField] private UIManager uiManager; // Reference to UIManager assigned in Inspector
    [SerializeField] private InspectionManager inspectionManager; // Reference to InspectionManager assigned in Inspector
    private CharacterController characterController;
    private Animator animator;
    public bool isActive = false;  // Indicates if this player is currently active
    private Transform target;

    private float jumpForce = 8f;         // The force applied when jumping (adjust as needed)
    private float gravity = 20f;          // Gravity force applied when the character is not grounded
    private float verticalVelocity = 0f;  // Tracks the vertical velocity of the character

    public CharacterConfig characterConfig; // Reference to the Scriptable Object
        // Encapsulated property for combat readiness
    private bool isCombatReady;
    public bool IsCombatReady {
        get => isCombatReady;
        set {
            isCombatReady = value;
            if (isCombatReady) {
                Debug.Log("Entered focus mode. Click to attack.");
                // Additional logic for entering combat mode (e.g., draw weapon)
            } else {
                Debug.Log("Exited focus mode. Click to inspect.");
                // Additional logic for exiting combat mode (e.g., sheathe weapon)
            }
        }
    }


    private void Awake() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        uiManager = FindObjectOfType<UIManager>(); // Dynamically assign UIManager
        if (uiManager == null) {
            Debug.LogError("UIManager not found in the scene. Make sure there's a UIManager in the scene.");
        }
        inspectionManager = FindObjectOfType<InspectionManager>(); // Dynamically assign InspectionManager
        if (inspectionManager == null) {
            Debug.LogError("InspectionManager not found in the scene. Make sure there's an InspectionManager in the scene.");
        }
    }

    private void Start() {

        Health = 100; // needs to be persistent
        Experience = 100; // needs to be persistent
        IsCombatReady = false;

        // Use the value from CharacterConfig to set the animator parameter
            if (characterConfig != null)
            {
                Debug.Log($"{gameObject.name} is adjusting walk and run animation by {characterConfig.walkPlaybackSpeed} and {characterConfig.runPlaybackSpeed}");
                animator.SetFloat("WalkPlaybackSpeed", characterConfig.walkPlaybackSpeed);
                animator.SetFloat("RunPlaybackSpeed", characterConfig.runPlaybackSpeed);
                CharacterName = characterConfig.characterName;
                Description = characterConfig.description;
            }

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
                ToggleFocusMode(); // Toggle combat readiness (focus mode) on Tab press
            }

            if (Input.GetMouseButtonDown(0)) {
                Debug.Log($"{gameObject.name} is registering a click...");
                GameObject target = ObjectSelector.GetClickedObject();
                if (target != null) {
                    if (isCombatReady) {
                        Debug.Log($"{gameObject.name} is attacking");
                        Attack(target);
                    } else {
                        Debug.Log($"{gameObject.name} is inspecting");
                        inspectionManager.Inspect(target);
                    }
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


            if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded) {
                Debug.Log("Jumping");
                animator.SetTrigger("jumpTrigger");
                verticalVelocity = jumpForce;  // Apply jump force when the jump starts
            }
        }
    }

    private void ToggleFocusMode() {
    isCombatReady = !isCombatReady; // Toggle the combat readiness flag
        if (isCombatReady) {
            Debug.Log("Entered focus mode. Click to attack.");
        } else {
            Debug.Log("Exited focus mode. Click to inspect.");
        }
    }

    private GameObject GetTarget() {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.Log("Raycating...");
        if (Physics.Raycast(ray, out hit)) {
            Debug.Log("...hit!");
            return hit.collider.gameObject; // Return the GameObject that was clicked on
        } else {
            Debug.Log("...missed");
        }
        return null; // If nothing was hit, return null
    }


    private void InspectObject(GameObject target) {
        if (target != null) {
            IInspectable inspectable = target.GetComponent<IInspectable>();
            if (inspectable != null) {
                // Retrieve information from the model (target object)
                Dictionary<string, string> info = inspectable.GetInfo();

                // Pass the data to the UIManager to display it
                if (uiManager != null) {
                    uiManager.DisplayInfo(info);
                } else {
                    Debug.Log("uiManager is null");
                }
            } else {
                Debug.Log("Target has no IInspectable component");
            }
        } else {
            Debug.Log("Target is null");
        }
    }

    private void Attack(GameObject target) {
        if (target != null) {
            IInspectable inspectable = target.GetComponent<IInspectable>();
            if (inspectable != null) {
                // Implement attack logic here
                Debug.Log($"Attacking target: {inspectable.GetInfo()}");
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
 
    public override Dictionary<string, string> GetInfo() {
        // Start with the base class info
        var info = base.GetInfo();

        // Optionally, modify or add to the existing information
        info[InspectionKey.Name.ToString()] = CharacterName;
        info[InspectionKey.Description.ToString()] = Description;

        return info;
    }

}
