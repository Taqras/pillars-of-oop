using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    private int experience;
    
    public int Experience
    {
        get { return experience; }
        private set { experience = value; }
    }
    
    public void GainExperience(int amount)
    {
        Experience += amount;
    }

    public override void Move()
    {
        // Specific movement for Player
    }
    
    public override void Interact()
    {
        // Interaction logic for Player
    }
}
