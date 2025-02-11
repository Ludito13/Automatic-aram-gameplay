using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FlockingAction
{
    float _sepRadius;
    float _aliAndCoradius;
    float _maxSpeed;
    float _maxForce;
    Transform _newTransform;
    Entity _entity;
    Vector3 _velocity;
    bool _foodRange;

    public FlockingAction(Entity e,Transform t, Vector3 vel, float sep, float aliAndCo, float mSpeed, float mForce)
    {
        _entity = e;
        _newTransform = t;
        _velocity = vel;
        _sepRadius = sep;
        _aliAndCoradius = aliAndCo;
        _maxSpeed = mSpeed;
        _maxForce = mForce;
    }

    public Vector3 CalculateSteering(Vector3 desired)
    {
        return Vector3.ClampMagnitude((desired.normalized * 0.1F) - _velocity, 0.05F);
    }

    public Vector3 Separatation()
    {
        Debug.Log(GameManager.instance.GetNeightbour(_newTransform, _sepRadius).Count());
        var desired = GameManager.instance.GetNeightbour(_newTransform, _sepRadius).Aggregate(new Vector3(), (x, y) =>
        {
            x.y = 0;

            var distance = y.transform.position - _newTransform.position;
            distance.y = 0;
            if (distance.magnitude <= _sepRadius) x += distance;

            return x;
        });

        if (desired == Vector3.zero) return desired;

        desired = -desired;
        desired.y = 0;

        return CalculateSteering(desired);
    }

    public Vector3 Alignment()
    {
        int count = 0;

        var desired = GameManager.instance.GetNeightbour(_newTransform, _aliAndCoradius)
                                          .OrderByDescending(x => _entity)
                                          .SkipWhile(x => x == _entity)
                                          .Aggregate(new Vector3(), (x, y) =>
                                          {
                                              x.y = 0;

                                              var dir = y.transform.position - _newTransform.position;
                                              dir.y = 0;
                                              if (dir.magnitude <= _aliAndCoradius)
                                              {
                                                  x += y.GetVelocity();
                                                  count++;
                                              }

                                              return x;
                                          });

        if (count == 0) return desired;
        desired /= count;
        desired.y = 0;

        return CalculateSteering(desired);
    }

    public Vector3 Cohesion()
    {
        //IA2-P1
        int count = 0;
        var desired = GameManager.instance.GetNeightbour(_newTransform, _aliAndCoradius)
                                          .OrderByDescending(x => _entity)
                                          .SkipWhile(x => x == _entity)
                                          .Aggregate(new Vector3(), (x, y) =>
                                          {
                                              x.y = 0;

                                              var dist = y.transform.position - _newTransform.position;
                                              dist.y = 0;
                                              if (dist.magnitude <= _aliAndCoradius)
                                              {
                                                  x += y.transform.position;
                                                  count++;
                                              }

                                              return x;
                                          });

        if (count == 0) return desired;

        desired /= count;
        desired -= _newTransform.position;
        desired.y = 0;
        return CalculateSteering(desired);
    }
}
