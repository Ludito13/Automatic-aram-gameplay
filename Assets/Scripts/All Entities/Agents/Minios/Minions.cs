using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public abstract class Minions : Entity
{
    public event Action<Minions> OnMoveM = delegate { };

    public LayerMask ally;
    [Header("Flocking")]
    public float aliAndCoradius;
    public float sepRadius;
    public float radius;
    [Range(0f, 3f)]
    public float sepWidth = 1;
    [Range(0f, 3f)]
    public float aligWidth = 1;
    [Range(0f, 3f)]
    public float cohesionWidth = 1;

    [Space(10)]

    public float distanceFromTarget;
    public Entity target;
    public Animator anim;
    protected Node goalNode;
    protected Node startingNode;
    protected bool attack;
    protected Movement _movement;
    protected FlockingAction _flocking;
    protected Vector3 _velocity;
    [SerializeField] protected float timeToAttack;
    public float maxTimer;
    protected float _timer;
    public float angle;
    
    private EventFSM<MinionsStates> _m_FSM;
    public List<Node> _pathToFollow = new List<Node>();
    protected Pathfinding _pf = new Pathfinding();

    protected bool isCharacterNear;
    protected bool isTowerNear;

    public virtual void Awake()
    {
        _movement = new Movement(transform, maxSpeed, maxForce, 0, radius, _velocity, GetVelocity());
        _flocking = new FlockingAction(this, transform, velocity, sepRadius, aliAndCoradius, maxSpeed, maxForce);
    }

    public virtual void Start()
    {
        GameManager.instance.minions.Add(this);
        grid = GameManager.instance.grid;
        GameManager.instance.recal += RecalculatePath;

        _life = maxlife;
        StartCoroutine(AddToGrid());
        //GameManager.instance.allEntities.Add(this);
        GameManager.instance.UpdateTeams();

        SetInitialsNodes();

        transform.position = startingNode.transform.position;

        #region FSM
        var path = new State<MinionsStates>("Path");
        var battle = new State<MinionsStates>("Battle");
        var death = new State<MinionsStates>("Death");
        var avoid = new State<MinionsStates>("Avoid");

        StateConfigurer.Create(path).SetTransition(MinionsStates.Path, path)
                                    .SetTransition(MinionsStates.Attack, battle)
                                    .SetTransition(MinionsStates.Avoid, avoid)
                                    .SetTransition(MinionsStates.Death, death)
                                    .Done();

        StateConfigurer.Create(battle).SetTransition(MinionsStates.Attack, battle)
                                      .SetTransition(MinionsStates.Path, path)
                                      .SetTransition(MinionsStates.Avoid, avoid)
                                      .SetTransition(MinionsStates.Death, death)
                                      .Done();

        StateConfigurer.Create(death).SetTransition(MinionsStates.Attack, battle)
                                     .SetTransition(MinionsStates.Path, path)
                                     .SetTransition(MinionsStates.Avoid, avoid)
                                     .SetTransition(MinionsStates.Death, death)
                                     .Done();

        StateConfigurer.Create(avoid).SetTransition(MinionsStates.Attack, battle)
                                     .SetTransition(MinionsStates.Avoid, avoid)
                                     .SetTransition(MinionsStates.Path, path)
                                     .SetTransition(MinionsStates.Death, death)
                                     .Done();

        #region Path
        path.OnEnter += x =>
        {
            attack = false;
            StopCoroutine(Attack());
            StartCoroutine(SetPath());
        };

        path.OnUpdate += () =>
        {
            if (isStuned) return;

            if (blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam))
            {
                target = GameManager.instance.DistanceFromTeamMember(GameManager.instance.redTeam, transform);
                SendInputToFSM(MinionsStates.Attack);

            }


            if (!blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam))
            {
                target = GameManager.instance.DistanceFromTeamMember(GameManager.instance.blueTeam, transform);
                SendInputToFSM(MinionsStates.Attack);
            }

            if (_pathToFollow.Count > 0)
            {
                //Flocking();
                anim.SetBool("Run", true);
                _movement.FollowPath(_pathToFollow);
                goalNode = GameManager.instance.GoalNode(transform, blueTeam);

 
                return;
            }

            //Debug.Log(GameManager.instance.GoalNode(transform, blueTeam));

        };

        path.OnExit += x =>
        {
            anim.SetBool("Run", false);
        };
        #endregion

        #region Battle
        battle.OnEnter += x =>
        {
            //StartCoroutine(Attack());
        };

        battle.OnUpdate += () =>
        {
            if (isStuned) return;

            if (_life <= 0)
            {
                SendInputToFSM(MinionsStates.Death);
                return;
            }

            if (blueTeam && !GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam) ||
                !blueTeam && !GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam) ||
                target == null)
            {
                //if (target != null) return;

                SendInputToFSM(MinionsStates.Path);
                return;
            }

            //if (target == null)
            //{
            //    SendInputToFSM(MinionsStates.Path);
            //    return;
            //}
            if (Vector3.Distance(target.transform.position, transform.position) > attackDistance)
            {
                _movement.AddForce(_movement.Pursuit(target.transform.position, GetVelocity())); 
                _movement.AddForce(_movement.ObstaleAvoid(7));
                _movement.MovementV();
                //if (Physics.Raycast(transform.position, transform.forward, 20, 7))
                //{
                //    Debug.Log("Hay minion ahi, girar a la derecha");
                //    SendInputToFSM(MinionsStates.Avoid);
                //}
                anim.SetBool("Run", true);
                attack = false;
            }
            else
            {
                //Flocking();
                anim.SetBool("Run", false);
                attack = true;
            }


            if (attack)
            {
                _timer += Time.deltaTime;

                if (_timer >= maxTimer)
                {
                    AttackMinion();
                    _timer = 0;
                }
            }
            else _timer = 0;

        };

        battle.OnExit += x =>
        {

            _timer = 0;
            attack = false;
            //StopCoroutine(Attack());

        };

        #endregion

        #region Avoid 
        avoid.OnEnter += x =>
        {
            Debug.Log("Estado avoid");
        };

        avoid.OnUpdate += () =>
        {
            if(target == null)
            {
                SendInputToFSM(MinionsStates.Path);

            }

            if (Vector3.Distance(target.transform.position, transform.position) > attackDistance 
            /*!Physics.Raycast(transform.position, transform.forward, 20, 7)*/)
                SendInputToFSM(MinionsStates.Attack);

            _movement.AddForce(_movement.Pursuit(target.transform.position, GetVelocity()));
            _movement.AddForce(_movement.ObstaleAvoid(ally));

            _movement.MovementV();
            //AvoidObstacle();
        };

        avoid.OnExit += x =>
        {

        };
        #endregion

        #region Death
        death.OnEnter += x =>
        {
            FinishMinion();
        };

        death.OnUpdate += () =>
        {

        };

        death.OnExit += x =>
        {

        };

        #endregion
        _m_FSM = new EventFSM<MinionsStates>(path);
        #endregion

        //_pathToFollow = GameManager.instance.SetPath(_pf, _movement.GetPath, startingNode, goalNode);
    }

    public virtual void Update()
    {
        //OnMove += grid.UpdateEntity;
        //grid.UpdateEntity(this);
        OnMoveM(this);

        _m_FSM.Update();
        //_movement.MovementV();
    }

    public Minions SendInputToFSM(MinionsStates state)
    {
        _m_FSM.SendInput(state);
        return this;
    }

    public void RecalculatePath()
    {
        _movement.GetPath(_pf, GameManager.instance.ShortNode(transform), GameManager.instance.GoalNode(transform, blueTeam), _pathToFollow);
        SendInputToFSM(MinionsStates.Path);
    }

    public IEnumerator SetPath()
    {
        yield return new WaitForSeconds(0.4f);

        _movement.GetPath(_pf, GameManager.instance.ShortNode(transform), GameManager.instance.GoalNode(transform, blueTeam), _pathToFollow);

    }

    IEnumerator AddToGrid()
    {
        yield return new WaitForSeconds(0.4f);
        OnMoveM += grid.UpdateEntity;
        grid.UpdateEntity(this);
    }

    //public virtual void Target()
    //{
    //    if (blueTeam)
    //    {
    //        if (GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam))
    //            target = GameManager.instance.DistanceFromTeamMember(GameManager.instance.redTeam, transform);
    //    }
    //    else
    //    {
    //        if (GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam))
    //            target = GameManager.instance.DistanceFromTeamMember(GameManager.instance.blueTeam, transform);
    //    }
    //}

    public virtual void SetInitialsNodes()
    {
        if (blueTeam)
        {
            startingNode = GameManager.instance.startingNodeBlue;
            goalNode = GameManager.instance.goalNodeBlue;
        }
        else
        {
            startingNode = GameManager.instance.startingNodeRed;
            goalNode = GameManager.instance.goalNodeRed;
        }
    }

    public void AvoidObstacle()
    {
        var deltaPos = Vector3.zero;
        for (int i = 0; i < 10; i++)
        {
            var rot = Quaternion.AngleAxis((1 / (10 - 1)) * angle * 2 - angle, transform.position);
            var direction = transform.rotation * rot * Vector3.forward;

            RaycastHit hitInfo;
            if(Physics.Raycast(transform.position, direction, out hitInfo, 20))
            {
                deltaPos -= (1.0f / 10f) * _velocity + direction;

            }
            else
            {
                deltaPos += (1.0f / 10f) * _velocity + direction;

            }
        }
        transform.position += deltaPos * Time.deltaTime;
    }

    public virtual void FinishMinion()
    {

    }

    public virtual void AttackMinion()
    {

    }

    public virtual void Flocking()
    {
        _movement.AddForce(_flocking.Separatation() * sepWidth);
        //_movement.AddForce(_flocking.Alignment() * aligWidth);
        //_movement.AddForce(_flocking.Cohesion() * cohesionWidth);
    }

    public virtual IEnumerator Attack()
    {
        while (true)
        {
            while (attack)
            {
                yield return new WaitForSeconds(timeToAttack);
                Debug.Log("Minion ataca");
                AttackMinion();

                if (!attack) break;
            }
            yield return null;

        }

    }

    public IEnumerator RestartPath()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);
        }
    }

    public void DeathMinion()
    {
        Destroy(gameObject);
    }

    public virtual void OnDestroy()
    {
        GameManager.instance.recal -= RecalculatePath;
        grid.RemoveFromTheList(this);
        GameManager.instance.minions.Remove(this);
        //GameManager.instance.allEntities.Remove(this);
        GameManager.instance.UpdateTeams();
    }


    public virtual void SpawnObject()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sepRadius);        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aliAndCoradius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
