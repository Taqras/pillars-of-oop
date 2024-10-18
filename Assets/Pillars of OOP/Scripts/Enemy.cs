using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character {

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

    public override Dictionary<string, string> GetInfo() {
        // Start with the base class info
        var info = base.GetInfo();

        // Optionally, modify or add to the existing information
        // info[InspectionKey.CharacterName.ToString()] = CharacterName;
        // info[InspectionKey.Description.ToString()] = Description;

        return info;
    }
    
}
