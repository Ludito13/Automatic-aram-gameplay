using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Pathfinding
{
    private List<Node> pathToFollow = new List<Node>();

    public static IEnumerable<T> Generate<T>(T seed, Func<T, T> modify)
    {
        T acum = seed;
        while (true)
        {
            yield return acum;
            acum = modify(acum);
        }

    }

    public List<Node> Path(Node startingNode, Node goalNode)
    {
        PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        frontier.Enqueue(startingNode, 0);

        return AStarPath(startingNode, goalNode);
    }

    void Cookie(List<Node> n)
    {
        pathToFollow = n;
    }

    public List<Node> AStarPath(Node startingNode, Node goalNode)
    {
        #region Nodes

        if (startingNode == null || goalNode == null)
        {
            return new List<Node>();
        }

        PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        frontier.Enqueue(startingNode, 0);

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        cameFrom.Add(startingNode, null);

        Dictionary<Node, int> costSoFar = new Dictionary<Node, int>();
        costSoFar.Add(startingNode, 0);


        GameManager.instance.StartCoroutine(PathfindingDo(frontier.count, startingNode, goalNode, CalculatePath, Cookie));

        return CalculatePath(startingNode, goalNode);


        #endregion

        #region Matrix
        //PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        //frontier.Enqueue(startingNode, 0);

        //Dictionary<Node, Node> cameForm = new Dictionary<Node, Node>();
        //cameForm.Add(startingNode, null);

        //Dictionary<Node, int> costSoFar = new Dictionary<Node, int>();
        //costSoFar.Add(startingNode, 0);

        //while (frontier.count > 0)
        //{
        //    Node current = frontier.Dequeue();

        //    if (current == goalNode)
        //    {
        //        List<Node> path = new List<Node>();
        //        while (current != startingNode)
        //        {
        //            path.Add(current);
        //            current = cameForm[current];
        //        }
        //        path.Reverse();
        //        return path;
        //    }


        //    current.GetNeighbors().Aggregate(new List<Node>(), (x, y) =>
        //    {
        //        if (!y.isBlocked) return x;
        //        int newCost = costSoFar[current] + y.cost;

        //        if(!costSoFar.ContainsKey(y))
        //        {
        //            float priority = newCost + Vector3.Distance(y.transform.position, goalNode.transform.position);
        //            costSoFar.Add(y, newCost);
        //            frontier.Enqueue(y, priority);
        //            cameForm.Add(y, current);
        //        }
        //        else if(newCost < costSoFar[y])
        //        {
        //            float priority = newCost + Vector3.Distance(y.transform.position, goalNode.transform.position);
        //            costSoFar[y] = newCost;
        //            cameForm[y] = current;
        //            frontier.Enqueue(y, priority);
        //        }

        //        return x;
        //    });
        //}

        //return new List<Node>();
        #endregion
    }

    public List<Node> CalculatePath(Node startingNode, Node goalNode)
    {

        PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        frontier.Enqueue(startingNode, 0);

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        cameFrom.Add(startingNode, null);

        Dictionary<Node, int> costSoFar = new Dictionary<Node, int>();
        costSoFar.Add(startingNode, 0);

        Node current = frontier.Dequeue();

        if (current == goalNode)
        {
            List<Node> path = new List<Node>();

            while (current != startingNode)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Reverse();
            return path;
        }

        return current.GetNeighbors().Aggregate(new List<Node>(), (x, y) =>
        {
            int newCost = costSoFar[current] + y.cost;

            if (!costSoFar.ContainsKey(y))
            {
                float priority = newCost + Vector3.Distance(y.transform.position, goalNode.transform.position);
                costSoFar.Add(y, newCost);
                frontier.Enqueue(y, priority);
                cameFrom.Add(y, current);
            }
            else if (newCost < costSoFar[y])
            {
                float priority = newCost + Vector3.Distance(y.transform.position, goalNode.transform.position);
                costSoFar[y] = newCost;
                cameFrom[y] = current;
                frontier.Enqueue(y, priority);
            }
            x.Add(y);
            return x;
        });
    }

    public IEnumerator PathfindingDo(int num, Node starting, Node goal,Func<Node, Node, List<Node>> method, Action<List<Node>> callback)
    {
        List<Node> collection = new List<Node>();

        while (num > 0)
        {
            foreach (var item in method(starting, goal))
            {
                Debug.Log("Calculo");
                collection.Add(item);
                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(0.05f);
        }
        callback(collection);
    }

    public IEnumerable AStar(Node start, Node goalNode)
    {
        PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        frontier.Enqueue(start, 0);

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        cameFrom.Add(start, null);

        Dictionary<Node, int> costSoFar = new Dictionary<Node, int>();
        costSoFar.Add(start, 0);



        //Dictionary<Node, Node> previous = new Dictionary<Node, Node>();
        //Dictionary<Node, float> actualDistances = new Dictionary<Node, float>();
        //actualDistances.Add(start, 0f);

        HashSet<Node> visited = new HashSet<Node>();
        List<Node> pending = new List<Node>();
        pending.Add(start);




        while (frontier.count > 0)
        {
            Node current = frontier.Dequeue(); 

            pending.Remove(current);
            visited.Add(current);

            if (current == goalNode)
            {
                return Generate(current, x => cameFrom[x])
                            .TakeWhile(x => cameFrom.ContainsKey(x))
                            .Reverse();
            }
            else
            {
                foreach (var elem in current.GetNeighbors().Where(x => !visited.Contains(goalNode)))
                {
                    var altDist = costSoFar[current];
                    costSoFar[elem] = costSoFar[current];
                    cameFrom[elem] = current;
                    pending.Add(elem);
                }
            }
        }
        return Enumerable.Empty<Node>();
    }


    public IEnumerator GenericSearch(Node start, Node goal,Action<IEnumerable<Node>> callback)
    {
        PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        frontier.Enqueue(start, 0);

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        //cameFrom.Add(start, null);

        Dictionary<Node, int> costSoFar = new Dictionary<Node, int>();
        costSoFar.Add(start, 0);

        bool isPathChecked = false;

        HashSet<Node> visited = new HashSet<Node>();
        IEnumerable<Node> pending = new Node[1] { start };
        Dictionary<Node, Node> parent = new Dictionary<Node, Node>();

        var wait = new WaitForSeconds(0.05f);
        while (frontier.count > 0)
        {
            Node current = frontier.Dequeue();

            visited.Add(current);

            if (current == goal)
            {
                isPathChecked = true;
                var path = Generate(current, x => cameFrom[x])
                        .TakeWhile(x => cameFrom.ContainsKey(x))
                        .Reverse();

                callback(path);
            }
            else
            {
                var n = current.GetNeighbors().Where(x => !visited.Contains(x)).ToList();

                foreach (var elem in n)
                {
                    int newCost = costSoFar[current] + elem.cost;

                    if (!costSoFar.ContainsKey(elem))
                    {
                        float priority = newCost + Vector3.Distance(elem.transform.position, goal.transform.position);
                        costSoFar.Add(elem, newCost);
                        frontier.Enqueue(elem, 0);
                        cameFrom.Add(elem, current);

                    }
                    else if (newCost < costSoFar[elem])
                    {
                        float priority = newCost + Vector3.Distance(elem.transform.position, goal.transform.position);
                        costSoFar[elem] = newCost;
                        cameFrom[elem] = current;
                        frontier.Enqueue(elem, priority);
                    }
                }

                yield return wait;
            }
        }

        if(!isPathChecked) callback(Enumerable.Empty<Node>());
    }
}
