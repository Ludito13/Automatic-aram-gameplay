using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Towers : Entity
{
    public LineRenderer line;
    public Transform lineSpawn;
    public FollowingBullet prefab;
    public Transform spawn;
    public float distance;
    public float maxTimer;
    public Entity red;
    public Entity blue;
    public Node oldGoalNode;
    public Node oldStartingNode;

    private IEnumerator spawnBullet;
    private float _timer;
    private bool _minionInArea;
    
    private void Start()
    {
        GameManager.instance.towers.Add(this);
        grid = GameManager.instance.grid;
        OnMove += grid.UpdateEntity;
        grid.UpdateEntity(this);
        //GameManager.instance.allEntities.Add(this);
        GameManager.instance.UpdateTeams();
        _timer = maxTimer;
        _life = maxlife;


        if(blueTeam)
        {
            oldStartingNode = GameManager.instance.startingNodeBlue;
            oldGoalNode = GameManager.instance.goalNodeBlue;
        }
        else
        {
            oldStartingNode = GameManager.instance.startingNodeRed;
            oldGoalNode = GameManager.instance.goalNodeRed;
        }
    }

    public void Update()
    {
        if (_life <= 0)
        {
            //GameManager.instance.NewNodes(blueTeam, oldGoalNode, oldStartingNode);
            GameManager.instance.recal();
            Destroy(gameObject);
        }

        if (prefab == null) return;

        if (blueTeam)
        {
            if (!GameManager.instance.redTeam.Any())
            {
                red = null;
            }
            else
            {

                Teams(GameManager.instance.redTeam.ToList(), red);
                if (red == null) return;
                Spawn(red);
            }
        }
        else
        {
            if (!GameManager.instance.blueTeam.Any())
            {
                blue = null;
            }
            else
            {

                Teams(GameManager.instance.blueTeam.ToList(), blue);

                if (blue == null) return; 
                Spawn(blue);
            }
        }

        //if (redTeam)
        //{

        //}
    }

    bool Timer()
    {
        return _timer <= 0 ? true : false;
    }

    void Spawn(Entity target)
    {
        if (prefab == null) return;
        if (target == null) return;

        _timer += Time.deltaTime;

        if(_timer >= maxTimer)
        {

            FollowingBullet bullet = Instantiate(prefab, spawn.position, spawn.rotation);
            bullet.target = target;
            bullet.blueTeam = blueTeam;
            _timer = 0;
        }
    }

    void Teams(List<Entity> team, Entity entity)
    {
        bool minion = team.OfType<Minions>().Any(x => (x.transform.position - transform.position).magnitude <= distance);
        bool character = team.OfType<Characters>().Any(x => (x.transform.position - transform.position).magnitude <= distance);

        //if (entity == null)
        //{
        //    line.gameObject.SetActive(false);
        //}
        //else line.gameObject.SetActive(true);

        if (minion)
        {
            var a = team.OfType<Minions>().OrderBy(x => (x.transform.position - transform.position).magnitude).FirstOrDefault();
            entity = a;

            _minionInArea = true;
            if (entity != null) Spawn(entity);

            //StartCoroutine(SpawnPoryectil());
        }
        else _minionInArea = false;

        if (character && !_minionInArea)
        {
            var a = team.OfType<Characters>().OrderBy(x => (x.transform.position - transform.position).magnitude).FirstOrDefault();
            entity = a;

            if (entity != null) Spawn(entity);

            //StartCoroutine(SpawnPoryectil());
        }


        
    }


    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;

        //Gizmos.DrawWireSphere(transform.position, distance);
    }

    private void OnDestroy()
    {
        GameManager.instance.towers.Remove(this);
        grid.RemoveFromTheList(this);
        //GameManager.instance.allEntities.Remove(this);
        GameManager.instance.UpdateTeams();
    }
}
