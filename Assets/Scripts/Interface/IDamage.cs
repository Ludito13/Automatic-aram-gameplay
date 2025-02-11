using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamage 
{
    public void Damage(float dmg, bool stun, float stunTime, bool rel, float relTime);
}
