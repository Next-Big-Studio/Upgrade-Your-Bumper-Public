using Destructors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarlosDestructor : Destructor
{

    // Carlos doesn't deform! He just takes damage.

    public float bodyStrength;
    public CarlosBoss carlosScript;

    public override void TakeDamageAtPoint(Vector3 point, Vector3 direction, float damage)
    {
        carlosScript.hp -= (damage/bodyStrength);
    }
}
