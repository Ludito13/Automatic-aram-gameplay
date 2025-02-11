using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MagicMinion : Minions
{
    private EventFSM<MinionsStates> _m_FSM;

    public GarenR ray;
    public float rayRadius;
    public float rayDamage;
    public FollowingBullet bullet;
    public Transform spawn;

    public override void Awake()
    {
        base.Awake();

    }

    public override void Start()
    {
        base.Start();
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
    }

    public override void Update()
    {
        base.Update();
        //_m_FSM.Update();
    }


    //public MagicMinion SendInputToFSM(MinionsStates state)
    //{
    //    _m_FSM.SendInput(state);
    //    return this;
    //}

    public void RayToEnemies()
    {
        if (GameManager.instance.GetNeightbour(transform, rayRadius).Any(x => x.blueTeam != blueTeam))
        {
            foreach (var item in GameManager.instance.GetNeightbour(transform, rayRadius).Where(x => x.blueTeam != blueTeam).Take(2))
            {
                GarenR obj = Instantiate(ray, item.transform.position, item.transform.rotation);
                obj.blueTeam = blueTeam;
                obj.damage = rayDamage;
                //item.Damage(rayDamage, false, 0, false, 0);
            }
        }
    }

    public override void SpawnObject()
    {
        FollowingBullet obj = Instantiate(bullet, spawn.position, spawn.rotation);
        obj.target = target;
        obj.blueTeam = blueTeam;
    }

    public override void AttackMinion() //Dispara proyectiles que hacen daño a los enemigos
    {
        anim.SetTrigger("Attack");
    }

    public override void FinishMinion() //Al morir lanza un rayo cae sobre el jugador mas cercano
    {
        anim.SetTrigger("Death");
        //Destroy(this.gameObject);
    }
}
