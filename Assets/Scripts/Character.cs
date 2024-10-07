using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Character
{
    public string Name { get; set; }
    public int Health { get; set; }

    public abstract void Move();
    public abstract void Interact();
}
