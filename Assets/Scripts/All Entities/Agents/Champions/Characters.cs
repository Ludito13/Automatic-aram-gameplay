using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Characters : Entity
{
    public event Action<Characters> OnMoveC = delegate { };

    public int killCount;
    public int deathsCount;
    public float distanceFromTarget;
    public Entity target;
    [SerializeField] protected Animator anim;
    public GameObject shieldP;
    public float radius;
    public List<Node> _pathToFollow = new List<Node>();
    protected Pathfinding _pf = new Pathfinding();
    protected Vector3 _velocity;
    protected Movement _movement;
    protected FlockingAction _flocking;
    [SerializeField] protected Node goalNode;
    [SerializeField] protected Node startingNode;
    protected bool attack;
    [SerializeField] protected float maxTimer;
    protected float _timer;
    [SerializeField] protected float timeToAttack;
    [Space(10)]

    public float maxTimerQ;
    public float maxTimerW;
    public float maxTimerE;
    public float maxTimerR;

    [HideInInspector()] public float timerQ;
    [HideInInspector()] public float timerW;
    [HideInInspector()] public float timerE;
    [HideInInspector()] public float timerR;

    [Header("Flocking")]
    public float aliAndCoradius;
    public float sepRadius;

    public virtual void Awake()
    {
        _movement = new Movement(transform, maxSpeed, maxForce, 0, radius, _velocity, GetVelocity());
        _flocking = new FlockingAction(this, transform, velocity, sepRadius, aliAndCoradius, maxSpeed, maxForce);
    }

    public virtual void Start()
    {
        GameManager.instance.characters.Add(this);
        //GameManager.instance.allEntities.Add(this);

        _life = maxlife;
        StartCoroutine(AddToGrid());

        SetInitialsNodes();

        transform.position = startingNode.transform.position;
    }

    public virtual void Update()
    {
        OnMoveC(this);

    }

    public virtual void OnDestroy()
    {
        //GameManager.instance.allEntities.Remove(this);
        GameManager.instance.characters.Remove(this);
        GameManager.instance.UpdateTeams();
        grid.RemoveFromTheList(this);
    }

    public IEnumerator RestartPath()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            startingNode = GameManager.instance.ShortNode(transform);
            _movement.GetPath(_pf, startingNode, goalNode, _pathToFollow);
        }
    }

    public virtual IEnumerator Attack()
    {
        while (true)
        {
            while (attack)
            {
                yield return new WaitForSeconds(timeToAttack);
                Debug.Log("Minion ataca");
                AttackCharacter();

                if (!attack) break;
            }
            yield return null;

        }

    }

    #region Abilities
    public virtual void Q()
    {

    }

    public virtual void W()
    {

    }

    public virtual void E()
    {

    }

    public virtual void R()
    {

    }
    #endregion

    public virtual void FinishMinion()
    {

    }

    #region Cooldowns
    public virtual void CoolDownQ()
    {

    }
    public virtual void CoolDownW()
    {

    }
    public virtual void CoolDownE()
    {

    }
    public virtual void CoolDownR()
    {

    }
    #endregion

    public virtual void AttackCharacter()
    {

    }

    public void AddKillsCount(Entity c)
    {
        if (c._life == 0) killCount++;   
    }

    IEnumerator AddToGrid()
    {
        yield return new WaitForSeconds(0.4f);
        OnMoveC += grid.UpdateEntity;
        grid.UpdateEntity(this);
    }

    public virtual void Shield(float sum)
    {
        StartCoroutine(ShieldC(sum));
    }

    public IEnumerator ShieldC(float sum)
    {
        shieldP.SetActive(true);
        shield += sum;
        yield return new WaitForSeconds(2);
        shield = 0;
        shieldP.SetActive(false);

    }

    public virtual void SetInitialsNodes()
    {
        if (blueTeam)
        {
            startingNode = GameManager.instance.startingCNodeBlue;
            goalNode = GameManager.instance.goalCNodeBlue;
        }
        else
        {
            startingNode = GameManager.instance.startingCNodeRed;
            goalNode = GameManager.instance.goalCNodeRed;
        }
    }

    public virtual void DestroyCharacter()
    {
        //Destroy(gameObject);
    }
}
