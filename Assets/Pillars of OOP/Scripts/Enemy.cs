using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character {

    private GameManager gameManager;
    private Animator animator;
    [SerializeField] private EnemyConfig enemyConfig;
    private float verticalVelocity = 0f;  // Tracks the vertical velocity of the character
    private float gravity = 20f;          // Gravity force applied when the character is not grounded
    private Vector3 roamingDirection;
    private float roamTimer;
    public float roamDirectionChangeMin = 2f; // minimum time to change roam direction
    public float roamDirectionChangeMax = 5f; // maximum time to change roam direction
    private bool roamPause = false;


    new private void Awake() { // new because we're hiding the same-named method in the character class and calling explicitly

        Debug.Log($"{gameObject.name} running Enemy.Awake()");
        // Ensure the base class's Awake is called
        base.Awake(); // Explicitly calling the base class Awke() to initialize characterController
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ChangeRoamingDirection();
    }

    private void Update() {
        Move();
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
                    characterController.Move(roamingDirection * Time.deltaTime);
                } else {
                    roamTimer = 0; // Reset timer to prompt a new direction in the next update
                }
            }

            ApplyGravity();
        }
    }

    private void ApplyGravity() {
        if (characterController.isGrounded && verticalVelocity < 0) {
            verticalVelocity = -1f;
        } else {
            verticalVelocity -= gravity * Time.deltaTime;
        }
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
        // Logic when Enemy interacts with Player
    }

       public override void Attack() {
        // Enemy-specific attack logic
        Debug.Log($"{CharacterName} attacks viciously!");
        // Placeholder: Play enemy attack animation, apply damage, etc.
    }

    public override void Defend() {
        // Enemy-specific defense logic
        Debug.Log($"{CharacterName} tries to defend.");
        // Placeholder: Play defense animation, reduce damage, etc.
    }

    public override void TakeDamage(int damage) {
        // Reduce health or other damage-related logic
        Health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health is now {Health}.");

        // Trigger the GetHit animation
        animator.SetTrigger("takeDamage");

        // Optional: Check for death or other post-hit conditions
        if (Health <= 0) {
            Die();
        }
    }

    public override void Die() {
        // Notify GameManager
        gameManager.RemoveEnemyFromList(this.gameObject);

        // Give rewards to the player
        Player player = FindObjectOfType<Player>();
        if (player != null) {
            player.GainExperience(enemyConfig.ExperienceReward);
            player.GainHealth(enemyConfig.HealthReward);
            player.GainMana(enemyConfig.ManaReward);
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
