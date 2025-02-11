using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MagicianUltimate : MonoBehaviour
{
    public Characters character;
    public GameObject objectChild;
    public float damage;
    public float radius;
    public bool blueTeam;
    public float maxTimer;

    private float _timer;

    private void Start()
    {

    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= maxTimer) Destroy(gameObject);

        if (GameManager.instance.GetNeightbour(transform, radius).Any())
        {
            foreach (var item in GameManager.instance.GetNeightbour(transform, radius).Except(GameManager.instance.towers).Except(GameManager.instance.nexus).Where(x => x.blueTeam != blueTeam))
            {
                item.Damage(damage, false, 0, false, 0);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(objectChild.transform.position, radius);
    }
}
