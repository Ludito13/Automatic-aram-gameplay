using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ThrowShield : MonoBehaviour
{
    public Characters target;
    public float shield;
    public float maxSpeed;
    public float maxForce;
    public float radius;
    public bool blueTeam;
    public float maxTimer;

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

        StartCoroutine(Shield());
    }

    void Update()
    {
        var col = GameManager.instance.GetNeightbour(transform, radius).Except(GameManager.instance.minions).OfType<Characters>();

        _timer += Time.deltaTime;

        if(_timer >= maxTimer)
        {
            _movement.AddForce(_movement.Pursuit(target.transform.position, GetVelocity()));
            _movement.MovementV();

            if (Vector3.Distance(transform.position, target.transform.position) <= 0.5) Destroy(gameObject);
        }
        else
        {
            transform.position += transform.forward * maxSpeed * Time.deltaTime;

        }

        //if (col.Any())
        //{
        //    foreach (var item in col)
        //    {
        //        item.Shield(shield);
        //    }
        //}
        
    }


    IEnumerator Shield()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.9f);
            var col = GameManager.instance.GetNeightbour(transform, radius).Except(GameManager.instance.minions).OfType<Characters>();
            if (col.Any())
            {
                foreach (var item in col)
                {
                    if (item.blueTeam != blueTeam) yield return null;

                    item.Shield(shield);
                }
            }
        }
    }

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
    //        path.Add(startingNode);
    //        path.Reverse();
    //        return path;
    //    }

    //    foreach (var next in current.GetNeighbors())
    //    {
    //        if (next.isBlocked) continue;
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
    //return new List<Node>();
}
