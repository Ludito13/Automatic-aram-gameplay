using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nexus : Entity
{
    public GameObject teamRed;
    public GameObject teamBlue;

    private void Start()
    {
        _life = maxlife;
        GameManager.instance.nexus.Add(this);
        GameManager.instance.UpdateTeams();
        OnMove += grid.UpdateEntity;
        grid.UpdateEntity(this);

    }

    void Update()
    {
        if(_life <= 0)
        {
            GameManager.instance.isGameOver = true;

            if (blueTeam) teamRed.SetActive(true) ;
            else teamBlue.SetActive(true);

        }
    }

    public void OnDestroy()
    {
        grid.RemoveFromTheList(this);
        GameManager.instance.nexus.Remove(this);
        GameManager.instance.UpdateTeams();
    }

    IEnumerator AddToGrid()
    {
        yield return new WaitForSeconds(0.4f);

    }
}
