using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GarenR : MonoBehaviour
{
    public Characters character;
    public float damage;
    public bool blueTeam;
    public float weaponRadius;
    public float maxTimer = 1f;
    [HideInInspector] public Transform targetToFollow;
    Entity _target;

    private float _timer;

    private void Start()
    {
        CauseDamage();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= maxTimer) Destroy(gameObject);
    }

    public void CauseDamage()
    {
        _target = GameManager.instance.GetNeightbour(transform, weaponRadius)
                                      .Except(GameManager.instance.towers)
                                      .Except(GameManager.instance.nexus)
                                      .Except(GameManager.instance.minions)
                                      .OfType<Characters>()
                                      .Where(x => x.blueTeam != blueTeam)
                                      .FirstOrDefault();

        if (_target != null)
        {
            Debug.Log("DEFINITIVA DE GAREN");
            _target.Damage(damage, true, 2, false, 0);
            if (character != null) character.AddKillsCount(_target);

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
