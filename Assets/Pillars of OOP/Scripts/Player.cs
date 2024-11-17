using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class Player : Character {
    [SerializeField] private UIManager uiManager; // Reference to UIManager assigned in Inspector
    [SerializeField] private InspectionManager inspectionManager; // Reference to InspectionManager assigned in Inspector
    // private CharacterController characterController; defined in the character base class
    private Animator animator;
    public bool isActive = false;  // Indicates if this player is currently active
    private Transform target;
    private Vector3 mouseLeftDownPosition;   // Used to separate click from click-and-drag
    private Vector3 mouseRightDownPosition;  // Used to separate click from click-and-drag
    private float clickThreshold = 0.1f;

    private float jumpForce = 8f;         // The force applied when jumping (adjust as needed)
    private float gravity = 20f;          // Gravity force applied when the character is not grounded
    private float verticalVelocity = 0f;  // Tracks the vertical velocity of the character
    private bool isRunning = false;
    private float walkSpeed = 3f;
    private float runSpeed = 6f;
    private float rotationSpeed = 2f;
    private UnityEngine.AI.NavMeshPath path;
    private int currentPathIndex;
    private int attackPower;
    private int armour;
    private bool isManualControl = true;
    private bool isAttacking = false;
    public bool isDead = false;
    public bool usesMana = false;
    public CharacterConfig characterConfig; // Reference to the Scriptable Object
    // Encapsulated property for combat readiness
    private bool cameraLock;  // the camera should be locked on to the player
    public bool CameraLock {
        get => cameraLock;
        set {
            if (!cameraLock && value) {
                Debug.Log($"{gameObject.name} asked for camera lock.");
            } else if (cameraLock && !value) {
                Debug.Log($"{gameObject.name} asked the camera to stay.");
            }
            cameraLock = value;
        }
    }
    private bool isCombatReady;
    public bool IsCombatReady {
        get => isCombatReady;
        set {
            isCombatReady = value;
            if (isCombatReady) {
                Debug.Log($"{gameObject.name} entered focus mode. Click to attack.");
                // Additional logic for entering combat mode (e.g., draw weapon)
            } else {
                Debug.Log($"{gameObject.name} exited focus mode. Click to inspect.");
                // Additional logic for exiting combat mode (e.g., sheathe weapon)
            }
        }
    }

    new private void Awake() { // new because we're hiding the same-named method in the character class and calling explicitly

        // Debug.Log($"{gameObject.name} running Player.Awake()");
        // Ensure the base class's Awake is called
        base.Awake(); // Explicitly calling the base class Awke() to initialize characterController
        
        // characterController = GetComponent<CharacterController>(); defined in the character base class
        animator = GetComponent<Animator>();

        uiManager = FindObjectOfType<UIManager>(); // Dynamically assign UIManager
        if (uiManager != null) {
            OnHealthChanged += uiManager.UpdateHealthIndicator;
            OnManaChanged += uiManager.UpdateManaIndicator;
        } else {
            Debug.LogError("UIManager not found in the scene. Make sure there's a UIManager in the scene.");
        }

        inspectionManager = FindObjectOfType<InspectionManager>(); // Dynamically assign InspectionManager
        if (inspectionManager == null) {
            Debug.LogError("InspectionManager not found in the scene. Make sure there's an InspectionManager in the scene.");
        }
    }

    private void Start() {
        Health = 100; // needs to be persistent
        Mana = 100;  // needs to be persistent
        MaxHealth = 100; // needs to be persistent
        MaxMana = 100; // needs to be persistent
        
        Experience = 0; // needs to be persistent
        IsCombatReady = false;
        isRunning = false;
        isAttacking = false;
        SetCameraLock(false);
        Speed = 0;
        animator.SetFloat("Speed", Speed);
        // Initialize path and set up the pathfinding agent
        path = new UnityEngine.AI.NavMeshPath();

        // Use the value from CharacterConfig to set the animator parameter
        if (characterConfig != null)
        {
            Debug.Log($"{gameObject.name} is adjusting walk and run animation by {characterConfig.walkPlaybackSpeed} and {characterConfig.runPlaybackSpeed}");
            animator.SetFloat("WalkPlaybackSpeed", characterConfig.walkPlaybackSpeed);
            animator.SetFloat("RunPlaybackSpeed", characterConfig.runPlaybackSpeed);
            CharacterName = characterConfig.characterName;
            Description = characterConfig.description;
            armour = characterConfig.armour;
            attackPower = characterConfig.attackPower;
            usesMana = characterConfig.usesMana;
        }

    }

    private void FixedUpdate() {
        if (isActive) {
            Move();
        }
    }

    private void Update() {
        if (isActive) {

            // Using the base class's characterController
            if (characterController != null) {
                animator.SetBool("isGrounded", characterController.isGrounded);
            } else {
                Debug.LogError($"CharacterController is null in {gameObject.name}'s Update method.");
            }

            if (Input.GetKeyDown(KeyCode.Tab)) {
                ToggleFocusMode(); // Toggle combat readiness (focus mode) on Tab press
            }

            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                // Toggle the isRunning state only when the Shift key is pressed down
                isRunning = !isRunning;
            }

             // Detect when the left mouse button is pressed
            if (Input.GetMouseButtonDown(0)) {
                mouseLeftDownPosition = Input.mousePosition;
            }

            // Detect when the left mouse button is released (attack or inspect by clicking)
            if (Input.GetMouseButtonUp(0)) {
                // Check if the mouse has moved beyond the threshold
                if (Vector3.Distance(mouseLeftDownPosition, Input.mousePosition) < clickThreshold) {
                    Debug.Log($"{gameObject.name} is registering a click...");
                    GameObject clickedObject = ObjectSelector.GetClickedObject();
                    if (clickedObject != null) {
                        SelectTarget(clickedObject.transform);
                        if (IsCombatReady) {
                            Debug.Log($"{gameObject.name} is attacking");
                            Attack();
                        } else {
                            Debug.Log($"{gameObject.name} is inspecting");
                            inspectionManager.Inspect(clickedObject);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                // Register initial position of right-click for drag detection
                mouseRightDownPosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1)) {
                // Check if the right-click action is a click or a drag
                if (Vector3.Distance(mouseRightDownPosition, Input.mousePosition) < clickThreshold) {
                    // Minimal movement, trigger defense or interaction
                    if (IsCombatReady) {
                        Debug.Log("Right mouse button up - trigger defense");
                        Defend(); // Trigger defense if in combat mode
                    } else {
                        Interact(); // Interact if not in combat mode
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded) {
                Debug.Log($"{gameObject.name} jumping");
                animator.SetTrigger("jumpTrigger");
                verticalVelocity = jumpForce;  // Apply jump force when the jump starts
            }
        }
    }

    private void ToggleFocusMode() {
    IsCombatReady = !IsCombatReady; // Toggle the combat readiness flag
        if (IsCombatReady) {
            Debug.Log($"{gameObject.name} entered focus mode. Click to attack.");
            SetCameraLock(false);
        } else {
            Debug.Log($"{gameObject.name} exited focus mode. Click to inspect.");
            SetCameraLock(true);
        }
    }

    public void SetCameraLock(bool lockState) {
        CameraLock = lockState;
        // Debug.Log($"{gameObject.name} set CameraLock to {lockState}");
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
    
    public void GainExperience(int amount) {
        Experience += amount;
        Debug.Log($"Gained {amount} experience. Total experience: {Experience}");
    }

    public void GainHealth(int amount) {
        Health = Mathf.Min(MaxHealth, Health + amount); // Cap at max health
        Debug.Log($"Gained {amount} health. Current health: {Health}");
    }

    public void GainMana(int amount) {
        Mana = Mathf.Min(MaxMana, Mana + amount); // Cap at max mana
        Debug.Log($"Gained {amount} mana. Current mana: {Mana}");
    }

    public override void Move() {

        // Specific movement for Player
        float horizontal = Input.GetAxis("Horizontal");  // A/D for strafing
        float vertical = Input.GetAxis("Vertical");      // W/S for forward/backward
        bool isRotating = false;
        float mouseX = 0;

        // Check left-click rotation
        if (Input.GetMouseButton(0)) {
            mouseX = Input.GetAxis("Mouse X");
            Debug.Log($"Mathf.Abs(mouseX) = {Mathf.Abs(mouseX)}");
        }

        // Detect manual input for movement
        // if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f || Input.GetMouseButton(0)) {
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f || Mathf.Abs(mouseX) > 0.1f) {
            
            Debug.Log("Activating manual control");
            isManualControl = true; // Player has assumed control
            isAttacking = false;

            // Stop NavMesh Agent path following if in manual control
            if (isRunning) {
                StopCoroutine(MoveAndAttackCoroutine()); // Stop AI movement coroutine if active
                // isRunning = false;
                Debug.Log("Player has taken manual control.");
            }
        }

        if (!isAttacking) {
            if (Mathf.Abs(mouseX) > 0.1f) {
                Debug.Log("Left-click rotation");
                isRotating = true;
                SetCameraLock(true);
                float rotationSpeed = 100f; // Adjust rotation speed as needed
                transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
            } else {
                isRotating = false;
                if (IsCombatReady) {
                    SetCameraLock(false);
                }
            }

            // Set Speed based on running or walking
            Speed = isRunning ? runSpeed : walkSpeed;

            // If no movement input, stop the character
            if (Mathf.Abs(vertical) < 0.01f && Mathf.Abs(horizontal) < 0.01f) {
                Speed = 0;
                if (IsCombatReady) {
                    if (!isRotating) {
                        SetCameraLock(false);
                    }
                }
            } else {
                Debug.Log("Player moving");
                SetCameraLock(true);
            }

            // Calculate strafe direction (left/right movement)
            Vector3 strafeDirection = transform.right * horizontal;

            // Calculate forward/backward direction
            Vector3 forwardDirection = transform.forward * vertical;

            // Combine strafing and forward/backward movement
            Vector3 moveDirection = (strafeDirection + forwardDirection).normalized * Speed;

            // Calculate intended position based on the current position and move direction
            Vector3 intendedPosition = transform.position + moveDirection * Time.deltaTime;

            // Apply slope detection: Prevent downhill movement on steep slopes
            if (IsSteepSlope(intendedPosition)) {
                return; // Cancel movement if on a steep slope
            }
            
            // Apply gravity when character is not grounded
            if (characterController.isGrounded && verticalVelocity < 0) {
                // If grounded and falling, reset vertical velocity to a small value to stay grounded
                verticalVelocity = -1f;  // A small negative value to ensure the character stays grounded
            } else {
                // Apply gravity if character is in the air
                verticalVelocity -= gravity * Time.deltaTime;
            }

            // Add vertical velocity to the movement direction (gravity or jump)
            moveDirection.y = verticalVelocity;

            // Apply movement to the Character Controller to move the player
            characterController.Move(moveDirection * Time.deltaTime);

            // Ensure the player remains upright by locking rotation on the x- and z-axes
            Vector3 fixedRotation = transform.rotation.eulerAngles;
            fixedRotation.x = 0;
            fixedRotation.z = 0;
            transform.rotation = Quaternion.Euler(fixedRotation);


            // Set animator parameters for movement
            animator.SetFloat("Speed", vertical * (Speed / runSpeed));  // Forward/backward
            animator.SetFloat("Strafe", horizontal);  // Left/right strafe
        }
    }
    
    // Encapsulated setter for isActive with additional logic
    public void Activation(bool active)
    {
        isActive = active;

        if (isActive) {
            Debug.Log($"{gameObject.name} is now active.");
            SetCameraLock(true);
        }
        else {
            Debug.Log($"{gameObject.name} is now inactive.");
            SetCameraLock(false);
        }
    }

    public void EnterFocusMode() {
        IsCombatReady = true;
        // Placeholder for drawing weapons, setting combat state, etc.
    }

    public void ExitFocusMode() {
        IsCombatReady = false;
        // Placeholder for sheathing weapons, switching to exploration state, etc.
    }

    public override void Attack() {
        if (IsCombatReady && target != null && !isAttacking) {

            isAttacking = true;
            isManualControl = false;
            // Start the coroutine to handle moving and attacking
            StartCoroutine(MoveAndAttackCoroutine());

        }
    }

    private IEnumerator MoveAndAttackCoroutine() {

        bool wasRunning = isRunning;
        float pathRecalculateInterval = 0.5f; // Interval for path recalculations
        float timeSinceLastPathUpdate = 0;

        while (Vector3.Distance(transform.position, target.position) > characterConfig.attackDistance + 0.5f) {

            if (isManualControl) {
                Debug.Log("Manual control activated; stopping AI movement.");
                isAttacking = false;
                isRunning = wasRunning;
                yield break; // Exit coroutine if manual control starts
            }

            timeSinceLastPathUpdate += Time.deltaTime;

            // Recalculate path if the interval has passed or if no valid path exists
            if (timeSinceLastPathUpdate >= pathRecalculateInterval || path == null || currentPathIndex >= path.corners.Length) {

                // First we must get into combat distance

                // Initialize and calculate path to the target
                if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, target.position, UnityEngine.AI.NavMesh.AllAreas, path)) {
                    // Debug.Log("1: Calculate path");
                    currentPathIndex = 0;
                    isRunning = true;
                    Speed = runSpeed;
                    animator.SetFloat("Speed", runSpeed); // Start move animation
                    isManualControl = false; // Ensure AI movement starts without manual control
                    timeSinceLastPathUpdate = 0; // Reset path update timer
                } else {
                    // Debug.Log("Path calculation failed.");
                    isAttacking = false;
                    yield break;
                }
            }

            // Move towards the next waypoint
            if (currentPathIndex < path.corners.Length) {
                Vector3 direction = (path.corners[currentPathIndex] - transform.position).normalized;
                float distanceToCorner = Vector3.Distance(transform.position, path.corners[currentPathIndex]);

                //if (direction.magnitude > 0.1f) {
                if (distanceToCorner > 0.4f) {  // Adjust threshold for smoother movement
                    // Debug.Log($"3: Distance to path corner: {distanceToCorner}");
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * (rotationSpeed));
                    // characterController.Move(direction * runSpeed * Time.deltaTime);
                    characterController.Move(new Vector3(direction.x * runSpeed, verticalVelocity, direction.z * runSpeed) * Time.deltaTime);
                } else {
                    currentPathIndex++; // Move to the next corner if close enough
                }
            }

            if (characterController.isGrounded) {
                // If grounded and falling, reset vertical velocity to a small value to stay grounded
                verticalVelocity = -1f;  // A small negative value to ensure the character stays grounded
            } else {
                // Apply gravity if character is in the air
                verticalVelocity -= gravity * Time.deltaTime;
            }

            // Add a slight delay between iterations to prevent jitter
            // yield return new WaitForSeconds(0.05f);
            yield return null;
        }

        // Stop running and initiate the attack if within range
        // if (!isManualControl) {
        if (Vector3.Distance(transform.position, target.position) <= characterConfig.attackDistance + 0.5f) {
            isRunning = wasRunning;
            Speed = 0;
            animator.SetFloat("Speed", Speed); // Stop running animation when reaching the target

            // Rotate towards the target until facing it
            Vector3 lookDirection = (target.position - transform.position).normalized;
            // Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            characterController.enabled = false;
            transform.rotation = Quaternion.LookRotation(lookDirection);
            characterController.enabled = true;


            // Trigger the attack once facing the target
            // Debug.Log($"{CharacterName} attacks the target: {target.name}");

            animator.SetTrigger("attackTrigger");
            
        }

        isAttacking = false;

    }


    public override void Defend() {
        if (IsCombatReady) {
            // trigger defend animation and increase defence variable to reduce health loss on hit
            // Placeholder for defensive action logic (e.g., block, parry, aim)
            Debug.Log($"{CharacterName} is defending!");
            animator.SetTrigger("defendTrigger");
        } else {
            Debug.LogError("Defend called without being combat ready");
        }
    }

    public void ApplyDefensiveAction() {
        // increasing armour and such

        // DefenceEffect is a particle effect stored in the magic users bones
        ParticleSystem defenseEffect = GetComponentsInChildren<ParticleSystem>(true).FirstOrDefault(ps => ps.name == "DefenseEffect");

        if (usesMana) {
            if (ConsumeMana(10)) {
                if (defenseEffect != null) {
                    defenseEffect.Play();
                } else {
                    Debug.Log("defenceEffect is null");                
                }
                Debug.Log("Magic defense");
                BoostArmourTemporarily(5, 5);
            } else {
                Debug.Log("Not enough mana for magic defense");
            }
        } else {
            Debug.Log("Melee defense");
            BoostArmourTemporarily(5, 8);
        }
    }

    public void BoostArmourTemporarily(int boostAmount, float duration) {
        StartCoroutine(TemporaryArmourBoost(boostAmount, duration));
    }

    private IEnumerator TemporaryArmourBoost(int boostAmount, float duration) {
        // Apply the armour boost
        int originalArmour = armour;
        armour += boostAmount;
        Debug.Log($"Armour boosted to {armour} for {duration} seconds.");

        // Wait for the duration
        yield return new WaitForSeconds(duration);

        // Revert the armour boost
        armour = originalArmour;
        Debug.Log("Armour reverted to original value.");
    }

    public override void Interact() {
        if (!IsCombatReady && target != null) {
            // Placeholder for interaction logic (e.g., NPC dialogue, pickup item)
            Debug.Log($"{CharacterName} is interacting.");
        }
    }

    // Called from the attack animation event
    public void ApplyAttackDamage() {
        if (target != null) {

            TriggerAttackEffect();

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null) {
                // Deal damage
                if (usesMana) {
                    if (ConsumeMana(5)) {
                        damageable.TakeDamage(attackPower, transform);
                    }
                } else {
                    damageable.TakeDamage(attackPower, transform);
                }
            }
        }
    }

    public override void TakeDamage(int damage, Transform attacker) {

        if (!isDead) {

            int adjustedDamage = Mathf.Max(0, damage - armour); // Ensure damage isn't negative

            // Reduce health or other damage-related logic
            Health -= adjustedDamage;
            Debug.Log($"{CharacterName} took {adjustedDamage} damage. Remaining health: {Health}");

            // Trigger the GetHit animation
            animator.SetTrigger("hitTrigger");

            // Optional: Check for death or other post-hit conditions
            if (Health <= 0) {
                // Handle death logic if needed
                Die();
            }
        } else {
            Debug.Log($"{CharacterName} is dead and is still being attacked");
        }

    }

    public override void Die() {
        if (!isDead) {
            isDead = true;
            animator.SetTrigger("isDead");
            Debug.Log($"{CharacterName} died");
        }
    }

    // Call TriggerAttackEffect from an attack animation event to ensure good visual sync
    public void TriggerAttackEffect() {

        if (target == null) return;

        if (characterConfig.attackEffect != null) {

            Debug.Log("We have an attack effect, it should go off at " + target.position);

            Vector3 forwardOffset = target.forward * 0.1f; //0.5f; // 0.5 meters in front of target
            float effectHeightOffset = target.GetComponent<CharacterController>()?.height * 0.75f ?? 1.5f; // Fallback to 1.5f if no CharacterController
            Vector3 effectPosition = new Vector3(target.position.x + forwardOffset.x, target.position.y + effectHeightOffset, target.position.z + forwardOffset.z);
/* 
            // Calculate effect position based on caster's height and an offset in front of the target
            float casterHeight = transform.position.y + 1.2f; // Approximate caster height, adjust as needed

            Vector3 effectPosition = new Vector3(target.position.x + forwardOffset.x, casterHeight, target.position.z + forwardOffset.z);
 */
            GameObject effectInstance = Instantiate(characterConfig.attackEffect, effectPosition, Quaternion.identity);

            var particleSystem = effectInstance.GetComponent<ParticleSystem>();

            if (particleSystem != null && !characterConfig.autoDestroy) {
                // Only destroy manually if autoDestroy is false
                Destroy(effectInstance, characterConfig.effectDuration);
            }
        }

        Mana -= 10; // put this in config
    }

    public void SelectTarget(Transform newTarget) {
        target = newTarget;
        Debug.Log($"{gameObject.name} has selected target: {target.name}");
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
