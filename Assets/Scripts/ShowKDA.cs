using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 
using System.Linq;

public class ShowKDA : MonoBehaviour
{
    public Characters target;

    private void OnMouseDown()
    {
        UIManager.instance.ShowKDA(target);
    }
}
