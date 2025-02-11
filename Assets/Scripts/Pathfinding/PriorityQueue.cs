using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PriorityQueue<T>
{
    private Dictionary<T, float> _allNodes = new Dictionary<T, float>();

    public int count { get => _allNodes.Count; }

    public void Enqueue(T elem, float cost)
    {
        if (!_allNodes.ContainsKey(elem)) _allNodes.Add(elem, cost);
        else _allNodes[elem] = cost;
    }

    public T Dequeue()
    {
        T min = default;
        float currentValue = Mathf.Infinity;

        min = _allNodes.Aggregate(min, (x, y) =>
        {
            if(y.Value < currentValue)
            {
                x = y.Key;
                currentValue = y.Value;
            }
            return x;
        });

        //foreach (var item in _allNodes)
        //{
        //    if (item.Value < currentValue)
        //    {
        //        min = item.Key;
        //        currentValue = item.Value;
        //    }
        //}
        _allNodes.Remove(min);
        return min;
    }
}
