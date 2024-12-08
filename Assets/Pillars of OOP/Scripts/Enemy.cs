using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Character {

    private GameManager gameManager;
    private Animator animator;
    [SerializeField] private EnemyConfig enemyConfig;
    // Encapsulation: The health bar's visibility is managed entirely within the Enemy class,
    // ensuring external classes cannot directly manipulate the health bar state.
    [SerializeField] private GameObject healthBarPrefab; // Assign in Inspector
    private GameObject healthBarInstance; // Instance of the health bar
    private Slider healthSlider; // Slider for updating health
    private float verticalVelocity = 0f;  // Tracks the vertical velocity of the character
    private float gravity = 20f;          // Gravity force applied when the character is not grounded
    private Vector3 roamingDirection;
    private Vector3 aggressionStartPosition;
    private bool isAggressive = false;
    private Transform target;
    private float roamTimer;
    public float roamDirectionChangeMin = 2f; // minimum time to change roam direction
    public float roamDirectionChangeMax = 5f; // maximum time to change roam direction
    private bool roamPause = false;
    private float walkSpeed => enemyConfig.walkSpeed;
    private float runSpeed => enemyConfig.runSpeed;
    public int ExperienceReward => enemyConfig.ExperienceReward;
    public int HealthReward => enemyConfig.HealthReward;
    public int ManaReward => enemyConfig.ManaReward;
    public EnemyType myType => enemyConfig.enemyType;
    public float aggressionRange => enemyConfig.aggressionRange;
    public float pursueDistance => enemyConfig.pursueDistance;
    public float attackDelay => enemyConfig.attackDelay;
    public float attackDistance => enemyConfig.attackDistance;
    public int attackPower => enemyConfig.attackPower;
    private float lastAttackTime = 0f;
    private bool returnAfterAttack = false;


    new private void Awake() { // new because we're hiding the same-named method in the character class and calling explicitly

        // Debug.Log($"{gameObject.name} of type {enemyConfig.enemyType} ({myType}), is running Enemy.Awake()");
        // Ensure the base class's Awake is called
        base.Awake(); // Explicitly calling the base class Awke() to initialize characterController
        animator = GetComponent<Animator>();
        // animator.enabled = false; // Temporarily disable
    }

    private void Start() {
        MaxHealth = enemyConfig.MaxHealth;
        Health = MaxHealth;
        CharacterName = enemyConfig.characterName;
        Description = enemyConfig.description;
        // Initialize the health bar but keep it hidden
        if (healthBarPrefab != null) {
            healthBarInstance = Instantiate(healthBarPrefab, transform);

            // Position the health bar slightly above the enemy
            Vector3 healthBarPosition = transform.position + new Vector3(0, 0.8f, 0);
            healthBarInstance.transform.position = healthBarPosition;

            healthBarInstance.SetActive(false);
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
            if (healthSlider != null) {
                healthSlider.value = (float)Health / MaxHealth;
            } else {
                Debug.LogError("healthSlider is null");
            }
        } else {
            Debug.LogError("healthBarPrefab is null");
        }

        Debug.Log($"{gameObject.name} healthBarInstance: {healthBarInstance}");
        Debug.Log($"{gameObject.name} health bar active state after SetActive(false): {healthBarInstance.activeSelf}");

        ChangeRoamingDirection();
    }

    public void ShowHealthBar() {
        if (healthBarInstance != null) {
            healthBarInstance.SetActive(true);
            Debug.Log($"{gameObject.name}: Health bar shown.");
        } else{
            Debug.LogError($"{gameObject.name}: healthBarInstance is null");
        }
    }

    public void HideHealthBar() {
        if (healthBarInstance != null) {
            healthBarInstance.SetActive(false);
            Debug.Log($"{gameObject.name}: Health bar hidden.");
        } else{
            Debug.LogError($"{gameObject.name}: healthBarInstance is null!");
        }
    }

    private void Update() {

        // Update the health bar's rotation to face the camera
        if (healthBarInstance != null && healthBarInstance.activeSelf) {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) {
                healthBarInstance.transform.LookAt(mainCamera.transform);
                healthBarInstance.transform.Rotate(0, 180, 0); // Correct orientation
            }

            // Update the slider value
            if (healthSlider != null) {
                healthSlider.value = (float)Health / MaxHealth;
            }

        }

        if (isAggressive) {
            PursueAndAttack();
        } else {
            Move();
            CheckAggressionTrigger();
        }

    }

    private void BecomeAggressive(Transform newTarget) {
        isAggressive = true;
        returnAfterAttack = false;
        target = newTarget;
        aggressionStartPosition = transform.position;
        Debug.Log($"{CharacterName} has become aggressive towards {target.name}.");
    }

    private void CheckAggressionTrigger() {
        Player player = gameManager.GetActivePlayer();
        if (player != null) {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= aggressionRange && !returnAfterAttack) {
                Debug.Log("Aggression distance crossed");
                BecomeAggressive(player.transform);
            }
        }
    }

private void PursueAndAttack() {

    if (target == null) {
        Debug.LogError("PursueAndAttack null");
        isAggressive = false;
        return;
    }

    float distanceToStartPosition = Vector3.Distance(transform.position, aggressionStartPosition);
    float distanceToTarget = Vector3.Distance(transform.position, target.position);

    // Give up if the player is beyond the doubled aggression range
    if (distanceToStartPosition > aggressionRange * 2) {
        Debug.Log("Give up pursue");
        isAggressive = false;
        returnAfterAttack = true;
        StartCoroutine(ReturnToStartPosition());
        return;
    }

    // Attack if within range and delay has passed
    if (distanceToTarget <= attackDistance && Time.time >= lastAttackTime + attackDelay) {
        Attack();
        lastAttackTime = Time.time;
    }
    // Resume pursuing if the player is out of attack range but still within pursuit range
    else if (distanceToTarget > attackDistance && distanceToTarget <= aggressionRange * 2) {
        MoveTowards(target.position);
    } else {
        // Stop moving if within attack range
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
    }
}

    private IEnumerator ReturnToStartPosition() {
        while (Vector3.Distance(transform.position, aggressionStartPosition) > 0.1f) {
            MoveTowards(aggressionStartPosition);
            yield return null;
        }
    }

    private void MoveTowards(Vector3 destination) {
        animator.SetBool("isRunning", true);
        Vector3 direction = (destination - transform.position).normalized;
        characterController.Move(direction * runSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(direction);
    }


    public void SetGameManager(GameManager manager) {
        gameManager = manager;
    }


    public override void Move() {

        if (roamPause) {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        } else {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
        }

        if (characterController.isGrounded) {

            roamTimer -= Time.deltaTime;

            // Change direction every few seconds
            if (roamTimer <= 0) {
                ChangeRoamingDirection();
                roamTimer = Random.Range(roamDirectionChangeMin, roamDirectionChangeMax);
                roamPause = Random.value > 0.5f;
            }

            if (!roamPause) {
                if (!IsSteepSlope(transform.position + roamingDirection * Time.deltaTime)) {
                    characterController.Move(roamingDirection * walkSpeed * Time.deltaTime);
                    if (myType == EnemyType.Wolf) {
                        Vector3 forwardMovement = transform.forward * walkSpeed * Time.deltaTime;
                        characterController.Move(forwardMovement);
                    }
                } else {
                    // Debug.Log($"Steep slope detected; resetting roam timer for {myType}");
                    roamTimer = 0; // Reset timer to prompt a new direction in the next update
                }
            }

        }

        ApplyGravity();

    }

    private void ApplyGravity() {

        // if (characterController.isGrounded && verticalVelocity < 0) {
        //     verticalVelocity = -1f;
        // } else {
        //     verticalVelocity -= gravity * Time.deltaTime;
        // }

        // If not grounded, apply gravity
        if (!characterController.isGrounded) {
            verticalVelocity -= gravity * Time.deltaTime;
        } else {
            // Set a small downward force to keep the character grounded
            verticalVelocity = -1f;
        }

        Vector3 gravityMove = new Vector3(0, verticalVelocity, 0);
        characterController.Move(gravityMove * Time.deltaTime);

    }

    private void ChangeRoamingDirection() {

        float angle = Random.Range(0f, 360f);
        roamingDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;

        // Calculate the intended destination based on the next movement
        Vector3 nextPosition = transform.position + roamingDirection * Time.deltaTime;

        if (!IsSteepSlope(nextPosition)) {
            transform.rotation = Quaternion.LookRotation(roamingDirection, Vector3.up);
        } else {
            roamTimer = 0; // Reset timer to prompt a new direction in the next update
        }
    }
    
    public override void Interact() {
        Debug.Log($"{CharacterName}Interacting");
        // Logic when Enemy interacts with Player
        Player player = gameManager.GetActivePlayer();
        if (player != null) {
            BecomeAggressive(player.transform);
        }
    }

       public override void Attack() {

        if (target == null) {
            Debug.LogError("Attack null");
            return;
        }

        // Try to get the Player or IDamageable component on the target
        var damageableTarget = target.GetComponent<IDamageable>() as Player;

        if (damageableTarget != null && damageableTarget.isDead) {
            isAggressive = false;
            StartCoroutine(ReturnToStartPosition());
        } else {
            animator.SetTrigger("attackTrigger");
        }

    }

    // Called from the attack animation event
    public void ApplyAttackDamage() {
        if (target != null) {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null) {
                damageable.TakeDamage(attackPower, transform);
                Debug.Log($"{CharacterName} dealt {attackPower} damage to {target.name}");
            } else {
                Debug.Log("damageable is null");
            }
        }
    }

    public override void Defend() {
        // Enemy-specific defense logic
        Debug.Log($"{CharacterName} tries to defend.");
        // Placeholder: Play defense animation, reduce damage, etc.
    }

    public override void TakeDamage(int damage, Transform attacker) {
        // Reduce health or other damage-related logic
        Health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage from {attacker.name}. Health is now {Health}.");

        // Trigger the GetHit animation
        animator.SetTrigger("takeDamage");

        if (healthBarInstance != null && healthSlider != null) {
            healthSlider.value = (float)Health / MaxHealth;
        }

        if (Health <= 0) {
            Die();
        }

        if (!isAggressive) {
            BecomeAggressive(attacker);
        }

    }

    public override void Die() {
        // Notify GameManager
        gameManager.RemoveEnemyFromList(this.gameObject);

        // Give rewards to the player
        Player player = gameManager.GetActivePlayer();
        if (player != null) {
            player.GainExperience(ExperienceReward);
            player.GainHealth(HealthReward);
            player.GainMana(ManaReward);
        }

        // Trigger the Death animation
        animator.SetTrigger("isDead");

        // Destroy the enemy object
        Destroy(gameObject, 5f);
    }

    public override Dictionary<string, string> GetInfo() {
        // Start with the base class info
        var info = base.GetInfo();

        // Optionally, modify or add to the existing information
        // info[InspectionKey.CharacterName.ToString()] = CharacterName;
        // info[InspectionKey.Description.ToString()] = Description;

        return info;
    }
    
}
