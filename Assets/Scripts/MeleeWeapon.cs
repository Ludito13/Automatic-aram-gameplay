using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MeleeWeapon : MonoBehaviour
{
    public Characters character;

    public float damage;
    public bool blueTeam;
    public float weaponRadius;
    public float maxTimer = 1f;
    Entity _target;
    private float timer;

    //void Update()
    //{
    //    _target = GameManager.instance.GetNeightbour(transform, weaponRadius)
    //                            .Where(x => x.blueTeam != blueTeam)
    //                            .FirstOrDefault();

    //    if (GameManager.instance.GetNeightbour(transform, weaponRadius).Any(x => x.blueTeam != blueTeam))
    //    {
    //        Debug.Log("EL ARMA HACE DANIO");
    //        _target.Damage(damage);

    //        //timer += Time.deltaTime;

    //        //if (timer >= maxTimer)
    //        //{
    //        //    _target.Damage(damage);
    //        //    timer = 0;
    //        //}

    //    }
    //}

    public void CauseDamage(bool stun, float stunTime, bool rel, float relTimer)
    {
        _target = GameManager.instance.GetNeightbour(transform, weaponRadius)
                                        .Where(x => x.blueTeam != blueTeam)
                                        .FirstOrDefault();

        if (GameManager.instance.GetNeightbour(transform, weaponRadius).Any(x => x.blueTeam != blueTeam))
        {
            _target.Damage(damage, stun, stunTime, rel, relTimer);


            if(character != null)
            {
                character.AddKillsCount(_target);
            }

            //timer += Time.deltaTime;

            //if (timer >= maxTimer)
            //{
            //    _target.Damage(damage);
            //    timer = 0;
            //}

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, weaponRadius);
    }
}
