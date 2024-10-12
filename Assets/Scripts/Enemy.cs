using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character {
    // Implement health in the derived class
    public override int Health { get; protected set; }

    private void Start() {
        SetHealth(100);
    }

    public override void Move() {
        // Specific movement for Enemy
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

}
