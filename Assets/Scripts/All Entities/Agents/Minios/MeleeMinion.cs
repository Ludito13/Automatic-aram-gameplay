using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class MeleeMinion : Minions
{
    private EventFSM<MinionsStates> _m_FSM;

    public MeleeWeapon weapon;
    //public List<Node> _pathToFollow = new List<Node>();
    //protected Pathfinding _pf = new Pathfinding();

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
        //SetInitialsNodes();

        //transform.position = startingNode.transform.position;

        #region FSM
        /*var path = new State<MinionsStates>("Path");
        var battle = new State<MinionsStates>("Battle");
        var death = new State<MinionsStates>("Death");

        StateConfigurer.Create(path).SetTransition(MinionsStates.Path, path)
                                    .SetTransition(MinionsStates.Attack, battle)
                                    .SetTransition(MinionsStates.Death, death)
                                    .Done();

        StateConfigurer.Create(battle).SetTransition(MinionsStates.Attack, battle)
                                      .SetTransition(MinionsStates.Path, path)
                                      .SetTransition(MinionsStates.Death, death)
                                      .Done();

        StateConfigurer.Create(death).SetTransition(MinionsStates.Attack, battle)
                                     .SetTransition(MinionsStates.Path, path)
                                     .SetTransition(MinionsStates.Death, death)
                                     .Done();


        path.OnEnter += x =>
        {
            attack = false;
            StopCoroutine(Attack());
            StartCoroutine(SetPath());
            StartCoroutine(RestartPath());
        };

        path.OnUpdate += () =>
        {
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

        battle.OnEnter += x =>
        {
            Debug.Log("Battle");
            StartCoroutine(Attack());
        };

        battle.OnUpdate += () =>
        {
            if (_life <= 0)
            {
                SendInputToFSM(MinionsStates.Death);
                return;
            }

            if (blueTeam && !GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam) ||
                !blueTeam && !GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam))
            {
                if (target != null) return;

                SendInputToFSM(MinionsStates.Path);
                return;
            }

            if (target == null) return;


            if (Vector3.Distance(target.transform.position, transform.position) > attackDistance)
            {
                _movement.AddForce(_movement.Pursuit(target.transform.position, GetVelocity()));
                anim.SetBool("Run", true);
                attack = false;
            }
            else
            {
                anim.SetBool("Run", false);
                attack = true;
            }
        };

        battle.OnExit += x =>
        {


            attack = false;
            StopCoroutine(Attack());

        };

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
        _m_FSM = new EventFSM<MinionsStates>(path);*/
        #endregion
        //_m_FSM = new EventFSM<MinionsStates>(path);
    }

    public override void AttackMinion() //Pega golpes a Melee
    {
        anim.SetTrigger("Attack");
    }

    public override void FinishMinion() //Muere de forma normal
    {
        anim.SetTrigger("Death");
        //Destroy(this.gameObject);
    }

    //IEnumerator SetPath()
    //{
    //    yield return new WaitForSeconds(0.4f);
    //    _pathToFollow = _movement.GetPath(_pf, startingNode, goalNode);
    //}

    //private MeleeMinion SendInputToFSM(MinionsStates state)
    //{
    //    _m_FSM.SendInput(state);
    //    return this;
    //}

    public override void Update()
    {
        base.Update();
        //_m_FSM.Update();
    }

    //public override void Target()
    //{
    //    base.Target();
    //}

    public override void SetInitialsNodes()
    {
        base.SetInitialsNodes();
    }

    public void CauseDamage()
    {
        weapon.CauseDamage(false, 0, false, 0);
    }

    //public MeleeMinion SendInputToFSM(MinionsStates state)
    //{
    //    _m_FSM.SendInput(state);
    //    return this;
    //}

    //public List<Node> GetPath(Pathfinding p, Node start, Node goal)
    //{
    //    List<Node> path = new List<Node>();

    //    return path = p.AStarPath(start, goal);
    //}

    //public void FollowPath(List<Node> path)
    //{
    //    Vector3 pos = path[0].transform.position;
    //    Vector3 dir = pos - transform.position;
    //    transform.forward = dir;

    //    transform.position += transform.forward * maxSpeed * Time.deltaTime;

    //    if (dir.magnitude < 0.1f)
    //        path.RemoveAt(0);
    //}

    //public Vector3 Seek(Vector3 target)
    //{
    //    Vector3 desired = target - transform.position;
    //    desired.Normalize();
    //    desired *= _speed;

    //    Vector3 steering = desired - _velocity;
    //    steering = Vector3.ClampMagnitude(steering, maxForce);

    //    return steering;
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireSphere(transform.position, distanceFromTarget);
    //    Gizmos.DrawWireSphere(transform.position, attackDistance);
    //}
}
