using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CanonMinion : Minions
{
    //private EventFSM<MinionsStates> _m_FSM;

    public float explotionDamage;
    public float explotionRadius;
    public FollowingBullet bullet;
    public Transform spawn;

    public override void Awake()
    {
        base.Awake();
        

    }

    public override void Start()
    {
        base.Start();
        
    }

    public override void Update()
    {
        base.Update();
    }

    public void Explote()
    {
        if (GameManager.instance.GetNeightbour(transform, explotionRadius).Any(x => x.blueTeam != blueTeam))
        {
            foreach (var item in GameManager.instance.GetNeightbour(transform, explotionRadius).Where(x => x.blueTeam != blueTeam))
            {
                item.Damage(explotionDamage, false, 0, false, 0);
            }
        }
    }

    //public CanonMinion SendInputToFSM(MinionsStates state)
    //{
    //    _m_FSM.SendInput(state);
    //    return this;
    //}

    public override void AttackMinion() //Dispara un proyectil que explota y hace daño en area
    {
        anim.SetTrigger("Attack");
    }

    public void CauseDamage()
    {

    }

    public override void SpawnObject()
    {
        FollowingBullet obj = Instantiate(bullet, spawn.position, spawn.rotation);
        obj.target = target;
        obj.blueTeam = blueTeam;
    }

    public override void FinishMinion() //Cuando muere explota y genera daño en area
    {
        anim.SetTrigger("Death");

    }
}
