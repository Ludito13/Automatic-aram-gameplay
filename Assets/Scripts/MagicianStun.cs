using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MagicianStun : MonoBehaviour
{
    public Characters character;
    public float radius;
    public float damage;
    public float stunTime;
    public bool blueTeam;
    public float maxTimer;

    private float _timer;
    Vector3 _velocity;
    int count = 0;

    private void Start()
    {
        StartCoroutine(StunObjetive());
    }

    private void Update()
    {
        transform.position += transform.forward * 4f * Time.deltaTime;


        _timer += Time.deltaTime;

        if (_timer >= maxTimer) Destroy(gameObject);


    }

    IEnumerator StunObjetive()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            var col = GameManager.instance.GetNeightbour(transform, radius).Except(GameManager.instance.towers)
                                                               .Except(GameManager.instance.nexus).Take(2);
            if (col.Any())
            {

                int c = 0;
                foreach (var item in col)
                {
                    if (item.blueTeam == blueTeam) yield return null;

                    c++;
                    item.Damage(damage, true, stunTime, false, 0);

                    if (character != null) character.AddKillsCount(item);
                }
                if (c >= 2) Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
