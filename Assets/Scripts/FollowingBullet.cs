using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FollowingBullet : MonoBehaviour
{
    public Entity target;
    public float damage;
    public float maxSpeed;
    public float maxForce;
    public bool blueTeam;
    public float maxTimer = 10;

    private float _timer;
    private Vector3 _velocity;
    private Movement _movement;


    public Vector3 GetVelocity()
    {
        return _velocity;
    }

    private void Start()
    {
        _movement = new Movement(transform, maxSpeed, maxForce, 0, 0, _velocity, GetVelocity());   
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= maxTimer) Destroy(gameObject);



        if (target == null) return;

        _movement.AddForce(_movement.Pursuit(target.transform.position, GetVelocity()));
        _movement.MovementV();
        transform.LookAt(target.transform);
    }



    private void OnTriggerEnter(Collider other)
    {
        var e = other.gameObject.GetComponent<IDamage>();
        var a = other.gameObject.GetComponent<Entity>();

        if(e != null && a.blueTeam != blueTeam)
        {
            e.Damage(damage, false, 0, false,0);
            Destroy(gameObject);
        }
    }
}
