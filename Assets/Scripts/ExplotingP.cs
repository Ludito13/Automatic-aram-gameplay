using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ExplotingP : MonoBehaviour
{
    public Characters character;
    public float damage;
    public float relTimer;
    public float radius;
    public bool blueTeam;
    public float maxTimer;
    public float maxBombTimer;

    private float _timer;
    private float _timerBomb;
    private bool canMove;

    private void Start()
    {
        canMove = true;
    }

    void Update()
    {
        var col = GameManager.instance.GetNeightbour(transform, radius).Except(GameManager.instance.towers)
                                                                       .Except(GameManager.instance.nexus)
                                                                       .Where(x => x.blueTeam != blueTeam);

        _timer += Time.deltaTime;

        if(canMove) transform.position += transform.forward * 10 * Time.deltaTime;



        if (col.Any())
        {
            canMove = false;
            foreach (var item in col)
            {
                _timerBomb += Time.deltaTime;

                if (_timerBomb >= maxBombTimer)
                {
                    item.Damage(damage, false, 0, true, relTimer);
                    Destroy(gameObject);
                }

            }
        }
    }
}
