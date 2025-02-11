using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public delegate void Recalculate();
    public Recalculate recal;

    public LayerMask wallMask;

    public List<Node> allnodes = new List<Node>();
    public List<Characters> characters = new List<Characters>();
    public List<Nexus> nexus = new List<Nexus>();
    public List<Minions> minions = new List<Minions>();
    public List<Towers> towers = new List<Towers>();
    public List<Entity> blueTeam = new List<Entity>();
    public List<Entity> redTeam = new List<Entity>();
    public List<Entity> allEntities = new List<Entity>();

    public SpatialGrid grid;
    public bool isGameOver;

    [Header("Characters")]
    public Characters[] redminionsCPrefab;
    public Characters[] blueminionsCPrefab;
    public Node startingCNodeBlue;
    public Node startingCNodeRed;
    public Node goalCNodeRed;
    public Node goalCNodeBlue;
    public Node finalGoalCRedNode;
    public Node finalGoalCBlueNode;

    [Header("Minions")]
    public Minions[] redminionsPrefab;
    public Minions[] blueminionsPrefab;
    public Node startingNodeBlue;
    public Node startingNodeRed;
    public Node goalNodeRed;
    public Node goalNodeBlue;
    public Node finalGoalRedNode;
    public Node finalGoalBlueNode;

    [HideInInspector()] public List<Node> blueGoalNodes = new List<Node>();
    [HideInInspector()] public List<Node> blueStartingNodes = new List<Node>();
    [HideInInspector()] public List<Node> redGoalNodes = new List<Node>();
    [HideInInspector()] public List<Node> redStartingNodes = new List<Node>();

    public bool DistanceFromTarget(Transform t, float distance, List<Entity> l) => l.Any(x => (x.transform.position - t.position).magnitude <= distance);
    public Entity DistanceFromTeamMember(List<Entity> e,Transform t) => e.OrderBy(x => x.transform.position.magnitude - t.position.magnitude).FirstOrDefault();

    public Minions DistanceFromTeamMemberMinion(List<Entity> e,Transform t) => e.OfType<Minions>().OrderBy(x => x.transform.position.magnitude - t.position.magnitude).FirstOrDefault();
    public Characters DistanceFromTeamMemberCharacter(List<Entity> e,Transform t) => e.OfType<Characters>().OrderBy(x => x.transform.position.magnitude - t.position.magnitude).FirstOrDefault();

    

    public bool DistanceFromTargetMinions(Transform t, float distance, List<Entity> l) => l.OfType<Minions>().Any(x => (x.transform.position - t.position).magnitude <= distance);
    public bool DistanceFromTargetCharacters(Transform t, float distance, List<Entity> l) => l.OfType<Characters>().Any(x => (x.transform.position - t.position).magnitude <= distance);
    
    
    public bool DistanceFromTargetChar(Transform t, float distance, List<Entity> l) => l.Except(minions).Except(towers).Except(nexus).Any(x => (x.transform.position - t.position).magnitude <= distance);
    public bool DistanceFromTargetMinion(Transform t, float distance, List<Entity> l) => l.Except(characters).Except(towers).Except(nexus).Any(x => (x.transform.position - t.position).magnitude <= distance);


    
    private List<Minions> redMinions = new List<Minions>();
    private List<Minions> blueMinions = new List<Minions>();
    private List<Towers> redTowers = new List<Towers>();
    private List<Towers> blueTowers = new List<Towers>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        
        StartCoroutine(Add());
        StartCoroutine(SpawnAllEnemies());

    }


    public void ChangeTime(float time)
    {
        Time.timeScale = time;
    }

    IEnumerator Add()
    {
        yield return new WaitForSeconds(2f);
        AddToTeam();



    }

    //public List<Node> SetPath(Pathfinding pf,Func<Pathfinding, Node, Node, List<Node>> funcion, Node start, Node goal)
    //{
    //    return funcion(pf, start, goal);
    //}


    public void AddToTeam()
    {
        blueTeam.Clear();
        redTeam.Clear();
        allEntities.Clear();
        allEntities = allEntities.Concat(characters).Concat(minions).Concat(towers).Concat(nexus).ToList();

        blueTeam = ClasifyTeam(allEntities.Where(x => x.blueTeam), blueTeam).Aggregate(new List<Entity>(), (x, y) =>
        {
            if(!x.Contains(y)) x.Add(y);

            return x;
        });

        redTeam = ClasifyTeam(allEntities.Where(x => !x.blueTeam), redTeam).Aggregate(new List<Entity>(), (x, y) =>
        {
            if (!x.Contains(y)) x.Add(y);
            
            return x;
        });

        blueMinions = ClasifyTeam(blueTeam.Select(x => x.GetComponent<Minions>()), blueMinions).Aggregate(new List<Minions>(), (x, y) =>
        {
            if (!x.Contains(y)) x.Add(y);

            return x;
        });

        redMinions = ClasifyTeam(redTeam.Select(x => x.GetComponent<Minions>()), redMinions).Aggregate(new List<Minions>(), (x, y) =>
        {
            if (!x.Contains(y)) x.Add(y);
            
            return x;
        });


        blueTowers = ClasifyTeam(blueTeam.Select(x => x.GetComponent<Towers>()), blueTowers).Aggregate(new List<Towers>(), (x, y) =>
        {
            x.Add(y);
            return x;
        });

        redTowers = ClasifyTeam(redTeam.Select(x => x.GetComponent<Towers>()), redTowers).Aggregate(new List<Towers>(), (x, y) =>
        {
            x.Add(y);
            return x;
        });
    }

    IEnumerable<T> ClasifyTeam<T>(IEnumerable<T> col, IEnumerable<T> col2) //Generator propio
    {
        foreach (var item in col)
        {
            if (!col2.Contains(item)) yield return item;
        }
    }

    public void RemoveToTeam()
    {
        foreach (var item in allEntities.Where(x => x.blueTeam))
        {
            blueTeam.Remove(item);
        }

        foreach (var item in allEntities.Where(x => !x.blueTeam))
        {
            redTeam.Remove(item);
        }
    }

    public void UpdateTeams()
    {

        AddToTeam();
    }

    public void NewNodes(bool team, Node oldstarting, Node oldgoalNode)
    {

        if(team)
        {
            if (blueStartingNodes.Contains(oldstarting)) blueStartingNodes.Remove(oldstarting);
            if (blueGoalNodes.Contains(oldgoalNode)) blueGoalNodes.Remove(oldstarting);


            oldstarting = blueStartingNodes.FirstOrDefault();
            oldgoalNode = blueGoalNodes.FirstOrDefault();
        }
        else
        {
            if (redStartingNodes.Contains(oldstarting)) redStartingNodes.Remove(oldstarting);
            if (redGoalNodes.Contains(oldgoalNode)) redGoalNodes.Remove(oldstarting);

            oldstarting = redStartingNodes.FirstOrDefault();
            oldgoalNode = redGoalNodes.FirstOrDefault();
        }
    }

    private void OnDestroy()
    {
        blueTeam.Clear();
        redTeam.Clear();
    }

    public void StartCoroutines(IEnumerator cor)
    {
        StartCoroutine(cor);
    }

    public Node ShortNode(Transform t) => GameManager.instance.allnodes.OrderBy(x => (t.position - x.transform.position).magnitude).FirstOrDefault();



    Towers NearTower(List<Towers> list, Transform t)
    {
        return list.OrderBy(x => (t.transform.position - x.transform.position).magnitude)
                   .FirstOrDefault();
    }



    IEnumerable<T> AddItems<T>(IEnumerable<T> seed, Func<T, bool> pred)
    {
        foreach (var item in seed)
        {
            if (pred(item)) yield return item;
        }

    }

    Transform NearTowerBlue(Transform t) => towers.Where(x => x.blueTeam).OfType<Transform>().OrderBy(x => (t.transform.position - x.transform.position).magnitude).FirstOrDefault();
    Transform NearTowerRed(Transform t) => towers.Where(x => !x.blueTeam).OfType<Transform>().OrderBy(x => (t.transform.position - x.transform.position).magnitude).FirstOrDefault();

    public Node GoalNode(Transform t, bool team)
    {
        Node goal = default;

        Towers newblueNode = towers.Where(x => x.blueTeam)
                                   .OrderBy(x => (t.transform.position - x.transform.position).magnitude)
                                   .FirstOrDefault();

        Towers newrNode = towers.Where(x => !x.blueTeam)
                                .OrderBy(x => (t.transform.position - x.transform.position).magnitude)
                                .FirstOrDefault();

        Towers newredNode = allEntities.OfType<Towers>().Where(x => !x.blueTeam)
                                       .OrderBy(x => (t.transform.position - x.transform.position).magnitude)
                                       .FirstOrDefault();



        if (team)
        {
            if (towers.Where(x => !x.blueTeam).Any()) 
                    goal = GameManager.instance.allnodes.OrderBy(x => (newrNode.transform.position - x.transform.position).magnitude).FirstOrDefault();
            else
            {
                goal = finalGoalRedNode;
            }
        }
        else
        {
            if (towers.Where(x => x.blueTeam).Any())
                    goal = GameManager.instance.allnodes.OrderBy(x => (newblueNode.transform.position - x.transform.position).magnitude).FirstOrDefault();
            else
            {
                goal = finalGoalBlueNode;
            }
        }

        return goal;
    }

    public IEnumerable<Entity> GetNeightbour(Transform t, float view)
    {
        return grid.Query(t.position + new Vector3(-view, 0, -view),
                          t.position + new Vector3(view, 0, view),
                          x =>
                          {
                              var pos = x - t.position;
                              pos.y = 0;
                              return pos.sqrMagnitude < view * view;
                          });
    }

    IEnumerator SpawnAllEnemies()
    {
        yield return new WaitForSeconds(30f);
        yield return new WaitForSeconds(5f);
        StartCoroutine(CreateObject(blueminionsPrefab, 0, 2, 1, GenerateEnemies, (x) => { }));
        StartCoroutine(CreateObject(redminionsPrefab, 0, 2, 1, GenerateEnemies, (x) => { }));
        yield return new WaitForSeconds(5f);
        StartCoroutine(CreateObject(blueminionsPrefab, 2, 1, 1.5f, GenerateEnemies, (x) => { }));
        StartCoroutine(CreateObject(redminionsPrefab, 2, 1, 1.5f, GenerateEnemies, (x) => { }));
        yield return new WaitForSeconds(3f);
        StartCoroutine(CreateObject(blueminionsPrefab, 1, 2, 1.5f, GenerateEnemies, (x) => { }));
        StartCoroutine(CreateObject(redminionsPrefab, 1, 2, 1.5f, GenerateEnemies, (x) => { }));
        yield return new WaitForSeconds(60f);
        StartCoroutine(SpawnAllEnemies());
    }

    public IEnumerator CreateObject(Minions[] mcol,int index, int count, float time, Func<Minions[],int, Minions> spawn, Action<List<Minions>> callback)
    {
        List<Minions> collection = new List<Minions>();

        while (count > 0)
        {
            count--;
            collection.Add(spawn(mcol,index));
            yield return new WaitForSeconds(time);
        }
        callback(collection);
    }

    public Minions GenerateEnemies(Minions[] mcol, int index)
    {
        Minions m = default;
        if(mcol.Any())
        {
            m = Instantiate(mcol[index], startingNodeBlue.transform.position, startingNodeBlue.transform.rotation);
        }
       
        return m;

    }
}
