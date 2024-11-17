using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, Transform attacker);  // damage is the incoming damage, the target must decide how to deal with it
}
