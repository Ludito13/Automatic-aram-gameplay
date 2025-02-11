using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    private List<Node> _neighbors = new List<Node>();
    public LayerMask obstacles;

    public bool isBlocked = false;

    public int cost = 1;
    public float viewRaycast;
    public float viewRadius;

    void Start()
    {
        StartCoroutine(AddToManager());
        cost = Random.Range(0, 100);
    }

    bool RayCastForward() => Physics.Raycast(transform.position, transform.forward, viewRaycast, obstacles);
    bool RayCastBack() => Physics.Raycast(transform.position, -transform.forward, viewRaycast, obstacles);
    bool RayCastRight() => Physics.Raycast(transform.position, transform.right, viewRaycast, obstacles);
    bool RayCastLeft() => Physics.Raycast(transform.position, -transform.right, viewRaycast, obstacles);


    private void Update()
    {
        if (RayCastForward() || RayCastBack() || RayCastRight() || RayCastLeft()) isBlocked = true;
        else isBlocked = false;
    }                                           

    public List<Node> GetNeighbors()
    {

        if (_neighbors.Count > 0) return _neighbors;

        foreach (var node in GameManager.instance.allnodes)
        {
            if (node == this || _neighbors.Contains(node)) continue;

            Vector3 dir = node.transform.position - transform.position;

            if (Physics.Raycast(transform.position, dir, dir.magnitude, GameManager.instance.wallMask)) continue;
            if (dir.magnitude > viewRadius) continue;

            _neighbors.Add(node);
        }
        return _neighbors;
    }

    IEnumerator AddToManager()
    {
        if(!GameManager.instance.allnodes.Contains(this)) GameManager.instance.allnodes.Add(this);
        yield return new WaitForSeconds(0.1f);
        GetNeighbors();
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}
