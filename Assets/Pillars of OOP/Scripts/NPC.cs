using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Character {

    [SerializeField] private NPCConfig npcConfig;
    private Animation npcAnimation;
    private GameManager gameManager;

    new private void Awake() {

        base.Awake(); // Explicitly calling the base class Awke() to initialize characterController

        npcAnimation = GetComponent<Animation>();

        gameManager = FindObjectOfType<GameManager>(); // Dynamically assign InspectionManager
        if (gameManager == null) {
            Debug.LogError("GameManager not found in the scene. Make sure there's a GameManager in the scene.");
        }

    }
    public void Start() {

        CharacterName = npcConfig.characterName;
        Description = npcConfig.description;

        npcAnimation.Play("Idle");
    }

    public override void Move() {
        // Specific movement for Enemy
    }

    public override void Interact() {
        // Polymorphism: The Interact() method is overridden to provide NPC-specific interaction logic
        // while still maintaining a shared interface with other character types.
        Player player = gameManager.GetActivePlayer();

        if (player != null) {
            Debug.Log($"Interacting with {CharacterName}");

            // Replenish health and mana through Player's control
            player.GainHealth(player.MaxHealth);
            player.GainMana(player.MaxMana);

            // Play an interaction animation
            npcAnimation.Play("Attack2");
            npcAnimation.PlayQueued("Idle");

            // Notify the player with a UI text box
            gameManager.UIManager.ShowInteractionPanel($"You can feel {CharacterName} deep in your mind; \"beware the wolves to the north! I will heal you, but beware! Beware!\"");
        } else {
            Debug.LogWarning("No active player found for interaction.");
        }
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

    public override void TakeDamage(int damage, Transform attacker) {
        // Reduce health or other damage-related logic
        // Health -= damage;
        Debug.Log($"{gameObject.name} took no damage. Health is still {Health}.");

        // Trigger the GetHit animation
        // animator.SetTrigger("hitTrigger");

        // Optional: Check for death or other post-hit conditions
        if (Health <= 0) {
            // Handle death logic if needed
        }
    }

    public override void Die() {
        // Notify GameManager
        // gameManager.RemoveEnemyFromList(this.gameObject);

        // Trigger the Death animation
        // animator.SetTrigger("isDead");

        // Destroy the enemy object
        // Destroy(gameObject, 5f);
    }

    public override Dictionary<string, string> GetInfo() {
        // Start with the base class info
        var info = base.GetInfo();
        npcAnimation.Play("Attack1");
        npcAnimation.PlayQueued("Idle");
        // Optionally, modify or add to the existing information
        // info[InspectionKey.Name.ToString()] = CharacterName;
        // info[InspectionKey.Description.ToString()] = Description;

        return info;
    }

}
