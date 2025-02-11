using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Magician : Characters
{
    private EventFSM<CharactersStates> _lux_FSM;
    public List<Action> abilities = new List<Action>();
    public Transform basicSpawn;
    public Transform qSpawn;
    public MagicianStun stun;
    public ThrowShield shieldW;
    public ExplotingP eProyectile;
    public MagicianUltimate ultimate;
    public Transform spawnR;
    public FollowingBullet basicProyectil;
    public float maxTimerRandom;
    //public float maxTimerQ;
    //public float maxTimerW;
    //public float maxTimerE;
    //public float maxTimerR;

    //private float timerQ;
    //private float timerW;
    //private float timerE;
    //private float timerR;
    private float timerRandom;
    private bool isMinion;
    private int randomNumber;



    public override void Awake()
    {
        base.Awake();
        abilities.Add(Q);
        abilities.Add(W);
        abilities.Add(E);
        abilities.Add(R);

        timerQ = maxTimerQ;
        timerW = maxTimerW;
        timerE = maxTimerE;
        timerR = maxTimerR;

    }

    public override void Start()
    {
        base.Start();
        grid = GameManager.instance.grid;
        GameManager.instance.recal += RecalculatePath;
        var idle = new State<CharactersStates>("Idle");
        var path = new State<CharactersStates>("Path");
        var battle = new State<CharactersStates>("Battle");
        var death = new State<CharactersStates>("Death");

        StateConfigurer.Create(idle).SetTransition(CharactersStates.Idle, idle)
                                    .SetTransition(CharactersStates.Path, path)
                                    .SetTransition(CharactersStates.Battle, battle)
                                    .SetTransition(CharactersStates.Death, death)
                                    .Done();

        StateConfigurer.Create(path).SetTransition(CharactersStates.Path, path)
                                    .SetTransition(CharactersStates.Battle, battle)
                                    .SetTransition(CharactersStates.Death, death)
                                    .SetTransition(CharactersStates.Idle, idle)
                                    .Done();

        StateConfigurer.Create(battle).SetTransition(CharactersStates.Battle, battle)
                                      .SetTransition(CharactersStates.Path, path)
                                      .SetTransition(CharactersStates.Death, death)
                                      .SetTransition(CharactersStates.Idle, idle)
                                      .Done();

        StateConfigurer.Create(death).SetTransition(CharactersStates.Battle, battle)
                                     .SetTransition(CharactersStates.Path, path)
                                     .SetTransition(CharactersStates.Death, death)
                                     .SetTransition(CharactersStates.Idle, idle)
                                     .Done();

        #region Idle

        idle.OnEnter += x =>
        {
            StartCoroutine(StartPath());
        };

        idle.OnUpdate += () =>
        {

        };

        idle.OnExit += x =>
        {

        };

        #endregion

        #region Path
        path.OnEnter += x =>
        {
            attack = false;
            StartCoroutine(SetPath());
            //StartCoroutine(RestartPath());
        };

        path.OnUpdate += () =>
        {
            if (blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam))
            {
                Teams(GameManager.instance.redTeam, target);
                SendInputToFSM(CharactersStates.Battle);

            }

            if (!blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam))
            {
                Teams(GameManager.instance.blueTeam, target);
                SendInputToFSM(CharactersStates.Battle);
            }

            if (_pathToFollow.Count > 0)
            {
                //Flocking();
                anim.SetBool("Run", true);
                _movement.FollowPath(_pathToFollow);
                //goalNode = GameManager.instance.GoalNode(transform, blueTeam);

                if (_life <= 0) SendInputToFSM(CharactersStates.Death);
                return;
            }

            if (Input.GetKeyDown(KeyCode.A)) Q();
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

        };

        battle.OnUpdate += () =>
        {
            if (isStuned) return;

            if (blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam))
            {
                Teams(GameManager.instance.redTeam, target);
            }

            if (!blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam))
            {
                Teams(GameManager.instance.blueTeam, target);
            }

            if(target != null)
            {
                basicSpawn.LookAt(target.transform);

                transform.LookAt(target.transform);
            }


            #region Transitions
            if (_life <= 0)
            {
                SendInputToFSM(CharactersStates.Death);
                return;
            }

            if (blueTeam && !GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam) ||
                !blueTeam && !GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam) ||
                target == null)
            {
                SendInputToFSM(CharactersStates.Path);
                return;
            }
            #endregion

            #region BASIC ATTACK
            if (Vector3.Distance(target.transform.position, transform.position) > attackDistance)
            {
                _movement.AddForce(_movement.Pursuit(target.transform.position, GetVelocity())); _movement.MovementV();
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
                    AttackCharacter();
                    _timer = 0;
                }
            }
            else _timer = 0;
            #endregion

            if (Input.GetKeyDown(KeyCode.A)) Q();
            if (Input.GetKeyDown(KeyCode.S)) W();
            //if (Input.GetKeyDown(KeyCode.D)) E();
            //if (Input.GetKeyDown(KeyCode.F)) R();

            

            timerRandom += Time.deltaTime;

            if (timerRandom >= maxTimerRandom)
            {
                randomNumber = UnityEngine.Random.Range(0, abilities.Count);
                abilities[randomNumber].Invoke();
                timerRandom = 0;
            }

        };

        battle.OnExit += x =>
        {


        };
        #endregion

        #region Death
        death.OnEnter += x =>
        {
            FinishMinion();
            StartCoroutine(Respawn());
        };

        death.OnUpdate += () =>
        {

        };

        death.OnExit += x =>
        {

        };
        #endregion

        _lux_FSM = new EventFSM<CharactersStates>(idle);
    }

    IEnumerator StartPath()
    {
        yield return new WaitForSeconds(5f);
        SendInputToFSM(CharactersStates.Path);
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);
        anim.SetBool("Revive", true);
        _life = maxlife;
        SetInitialsNodes();

        transform.position = startingNode.transform.position;

        anim.SetBool("Revive", false);
        yield return new WaitForSeconds(30f);
        SendInputToFSM(CharactersStates.Idle);

    }


    public void RecalculatePath()
    {
        _movement.GetPath(_pf, GameManager.instance.ShortNode(transform), GameManager.instance.GoalNode(transform, blueTeam), _pathToFollow);
        SendInputToFSM(CharactersStates.Path);
    }

    public void SpawnBasciProyectil()
    {
        FollowingBullet obj = Instantiate(basicProyectil, basicSpawn.transform.position, basicSpawn.transform.rotation);
        obj.target = target;
        obj.blueTeam = blueTeam;
    }


    public override void Update()
    {
        _lux_FSM.Update();

        CoolDownQ();
        CoolDownW();
        CoolDownE();
        CoolDownR();
    }

    public Magician SendInputToFSM(CharactersStates state)
    {
        _lux_FSM.SendInput(state);
        return this;
    }


    public IEnumerator SetPath()
    {
        yield return new WaitForSeconds(0.4f);
         _movement.GetPath(_pf, GameManager.instance.ShortNode(transform), GameManager.instance.GoalNode(transform, blueTeam), _pathToFollow);
    }

    public override void OnDestroy()
    {
        GameManager.instance.recal -= RecalculatePath;

        base.OnDestroy();
    }

    #region Abilites

    public void QEffect()
    {
        MagicianStun obj = Instantiate(stun, qSpawn.transform.position, qSpawn.transform.rotation);
        obj.blueTeam = blueTeam;
        obj.character = this;
    }

    public void WEffect()
    {
        ThrowShield obj = Instantiate(shieldW, qSpawn.transform.position, qSpawn.transform.rotation);
        obj.target = this;
        obj.blueTeam = blueTeam;
    }

    public void Effect()
    {
        ExplotingP obj = Instantiate(eProyectile, spawnR.transform.position, spawnR.transform.rotation);
        obj.blueTeam = blueTeam;
        obj.character = this;
    }

    public void REffect()
    {
        MagicianUltimate obj = Instantiate(ultimate, spawnR.transform.position, spawnR.transform.rotation);
        obj.blueTeam = blueTeam;
        obj.character = this;
    }

    public override void Q()
    {
        if(timerQ >= maxTimerQ)
        {
            timerQ = 0;
            anim.SetTrigger("Q");
        }

    }

    public override void W()
    {
        if (timerW >= maxTimerW)
        {
            timerW = 0;
            anim.SetTrigger("W");
        }
    }

    public override void E()
    {
        if (timerE >= maxTimerE)
        {
            timerE = 0;
            anim.SetTrigger("E");
        }
    }

    public override void R()
    {
        if (timerR >= maxTimerR)
        {
            timerR = 0;
            anim.SetTrigger("R");
        }
    }

    #endregion

    #region Cooldown

    public override void CoolDownQ()
    {
        timerQ += Time.deltaTime;

        if (timerQ >= maxTimerQ) timerQ = maxTimerQ;
    }

    public override void CoolDownW()
    {
        timerW += Time.deltaTime;

        if (timerW >= maxTimerW) timerW = maxTimerW;
    }
    public override void CoolDownE()
    {
        timerE += Time.deltaTime;

        if (timerE >= maxTimerE) timerE = maxTimerE;
    }
    public override void CoolDownR()
    {
        timerR += Time.deltaTime;

        if (timerR >= maxTimerR) timerR = maxTimerR;
    }
    #endregion 

    void Teams(List<Entity> team, Entity entity)
    {
        GameManager.instance.DistanceFromTeamMember(GameManager.instance.redTeam, transform);
        bool minion = GameManager.instance.DistanceFromTargetMinions(transform, distanceFromTarget, team);
        bool character = GameManager.instance.DistanceFromTargetCharacters(transform, distanceFromTarget, team);


        bool c = GameManager.instance.DistanceFromTargetChar(transform, distanceFromTarget, team);
        bool m = GameManager.instance.DistanceFromTargetMinion(transform, distanceFromTarget, team);


        if (!c)
        {
            if (m)
            {
                target = GameManager.instance.DistanceFromTeamMemberMinion(team, transform);
                isMinion = true;
            }
            else isMinion = false;
        }
        else
        {
            target = GameManager.instance.DistanceFromTeamMemberCharacter(team, transform);
        }
    }

    public override void FinishMinion()
    {
        anim.SetTrigger("Death");
    }

    public override void AttackCharacter()
    {
        anim.SetTrigger("Attack");
    }

    public override void SetInitialsNodes()
    {
        base.SetInitialsNodes();
    }

    public override void DestroyCharacter()
    {
        base.DestroyCharacter();
    }

    public override IEnumerator Attack()
    {
        return base.Attack();
    }

    public override void Shield(float sum)
    {
        base.Shield(sum);
    }

    public override void Damage(float dmg, bool stun, float stunTime, bool rel, float relTime)
    {
        if (shield <= 0)
            base.Damage(dmg, stun, stunTime, rel, relTime);
        else if (shield > 0) shield -= dmg;

        if(_life <= 0) deathsCount++;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanceFromTarget);

    }
}
