using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Character {

    // Implement health in the derived class
    public override int Health { get; protected set; }
    public override void Move() {
        // Specific movement for Enemy
    }

    public override void Interact() {
        // NPC talks to the player
    }
}
