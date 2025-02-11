using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Movement 
{
    //Node current = frontier.Dequeue();

    //if (current == goalNode)
    //{
    //    List<Node> path = new List<Node>();

    //    while (current != startingNode)
    //    {
    //        path.Add(current);
    //        current = cameFrom[current];
    //    }
    //    path.Reverse();
    //    return path;
    //}
    //return current.GetNeighbors().Aggregate(new List<Node>(), (x, y) =>
    //{
    //    int newCost = costSoFar[current] + y.cost;

    //    if (!costSoFar.ContainsKey(y))
    //    {
    //        float priority = newCost + Vector3.Distance(y.transform.position, goalNode.transform.position);
    //        costSoFar.Add(y, newCost);
    //        frontier.Enqueue(y, priority);
    //        cameFrom.Add(y, current);
    //    }
    //    else if (newCost < costSoFar[y])
    //    {
    //        float priority = newCost + Vector3.Distance(y.transform.position, goalNode.transform.position);
    //        costSoFar[y] = newCost;
    //        cameFrom[y] = current;
    //        frontier.Enqueue(y, priority);
    //    }
    //    x.Add(y);
    //    return x;
    //});

    //while (frontier.count > 0)
    //{
    //    Node current = frontier.Dequeue();

    //    if (current == goalNode)
    //    {
    //        List<Node> path = new List<Node>();

    //        while (current != startingNode)
    //        {
    //            path.Add(current);
    //            current = cameFrom[current];
    //        }
    //        path.Reverse();
    //        return path;
    //    }

    //    foreach (var next in current.GetNeighbors())
    //    {
    //        int newCost = costSoFar[current] + next.cost;

    //        if (!costSoFar.ContainsKey(next))
    //        {
    //            float priority = newCost + Vector3.Distance(next.transform.position, goalNode.transform.position);
    //            costSoFar.Add(next, newCost);
    //            frontier.Enqueue(next, priority);
    //            cameFrom.Add(next, current);
    //        }
    //        else if (newCost < costSoFar[next])
    //        {
    //            float priority = newCost + Vector3.Distance(next.transform.position, goalNode.transform.position);
    //            costSoFar[next] = newCost;
    //            cameFrom[next] = current;
    //            frontier.Enqueue(next, priority);
    //        }
    //    }
    //}

    Transform _newTransfom;
    Vector3 _getVelocity;
    Vector3 _velocity;
    float _speed;
    float _force;
    float _viewAngle;
    float _viewRadius;
    float _sepRadius;

    public Movement(Transform mov, float s, float f, float vA, float vR, Vector3 vel, Vector3 getVel)
    {
        _newTransfom = mov;
        _speed = s;
        _force = f;
        _viewAngle = vA;
        _viewRadius = vR;
        _velocity = vel;
        _getVelocity = getVel;
    }

    //public bool LineOfSigth(Node goalNode)
    //{
    //    Vector3 direction = goalNode.transfom.position - _newTransfom.position;
    //    return Physics.Raycast(_newTransfom.position, direction, direction.magnitude);
    //}

    public void GetPath(Pathfinding p, Node start, Node goal, List<Node> path)
    {
        GameManager.instance.StartCoroutine(p.GenericSearch(start, goal, (x) =>
        {
            foreach (var item in x)
            {
                if(!path.Contains(item)) path.Add(item);
            }
        }));
        //return p.AStarPath(start, goal);
    }

    public void FollowPath(List<Node> path)
    {
        Vector3 pos = path[0].transform.position;
        Vector3 dir = pos - _newTransfom.position;
        _newTransfom.forward = dir;

        _newTransfom.position += _newTransfom.forward * _speed * Time.deltaTime;

        if (dir.magnitude < 0.1f)
            path.RemoveAt(0);
    }

    public Vector3 Seek(Vector3 target)
    {
        Vector3 desired = target - _newTransfom.position;
        desired.Normalize();
        desired *= _speed;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _force);

        return steering;
    }

    public Vector3 Pursuit(Vector3 target, Vector3 velocity)
    {
        Vector3 futurePos = target + velocity * Time.deltaTime;

        return Seek(futurePos);
    }

    public bool InFildOfView(GameObject target)
    {
        if (Vector3.Distance(_newTransfom.position, target.transform.position) > _viewRadius)
            return false;

        Vector3 dir = target.transform.position - _newTransfom.position;
        float angle = Vector3.Angle(_newTransfom.position, dir);
        return angle <= _viewAngle / 2;
    }

    public void MovementV()
    {
        _newTransfom.position += _velocity * Time.deltaTime;
        _newTransfom.forward = _velocity;
        _velocity.y = 0;
    }

    public void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _speed);

    }

    public Vector3 ObstaleAvoid(LayerMask layer)
    {
        Collider[] objs = Physics.OverlapSphere(_newTransfom.position, _viewRadius, layer);
        Vector3 desired = Vector3.zero;

        foreach (var item in objs/*GameManager.instance.GetNeightbour(_newTransfom, _viewRadius).Where(x => x.gameObject.layer == layer)*/)
        {
            var a = Vector3.Angle(_newTransfom.forward, item.transform.position - _newTransfom.position);
            var b = Vector3.Angle(_newTransfom.forward, item.transform.position + _newTransfom.position);
            //Debug.Log("A es " + a);
            //Debug.Log("B es " + b);

            if (Vector3.Angle(_newTransfom.forward, item.transform.position - _newTransfom.position) < 30)
            {
                return CalculateSteering(_newTransfom.right);

            }
            //else if(Vector3.Angle(_newTransfom.forward, item.transform.position - _newTransfom.position) > 30)
            //{
            //    Debug.Log("Izquierda");

            //    return CalculateSteering(-_newTransfom.right);   // Vector3.AngleAxis()
            //}

        }

        return CalculateSteering(-desired);
    }

    Vector3 CalculateSteering(Vector3 desired)
    {
        return desired == Vector3.zero ? desired :
            Vector3.ClampMagnitude((desired.normalized * _speed) - _velocity, _force);
    }

    Vector3 GetVectorFromAngle(float angle)
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    
}
