using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GreatSword : Characters
{
    private EventFSM<CharactersStates> _garen_FSM;
    public List<Action> abilities = new List<Action>();
    public MeleeWeapon weapon;
    public MeleeWeapon spellDamage;
    public MeleeWeapon qDamage;
    public GarenR prefab;
    public int abilitiesCount;
    //public float maxTimerQ;
    //public float maxTimerW;
    //public float maxTimerE;
    //public float maxTimerR;

    //[HideInInspector()] public float timerQ;
    //[HideInInspector()] public float timerW;
    //[HideInInspector()] public float timerE;
    //[HideInInspector()] public float timerR;

    private bool allAbilitiesComplete;
    private bool canDoAllAbilities;
    private bool canDoCombo1;
    private bool canDoCombo2;
    private bool canDoR;
    private bool canDoW;
    private bool canDoE;
    private bool canDoQ;
    private bool isMinion;

    private IEnumerator cor;

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
        #region FSM
        var path = new State<CharactersStates>("Path");
        var battle = new State<CharactersStates>("Battle");
        var death = new State<CharactersStates>("Death");
        var idle = new State<CharactersStates>("Idle");

        StateConfigurer.Create(idle).SetTransition(CharactersStates.Path, path)
                                    .SetTransition(CharactersStates.Battle, battle)
                                    .SetTransition(CharactersStates.Idle, idle)
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
            StopCoroutine(Attack());
            StartCoroutine(SetPath());
            //StartCoroutine(RestartPath());
        };

        path.OnUpdate += () =>
        {
            if (blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam))
            {
                SendInputToFSM(CharactersStates.Battle);

            }


            if (!blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam))
            {
                //target = GameManager.instance.DistanceFromTeamMember(GameManager.instance.blueTeam, transform);
                SendInputToFSM(CharactersStates.Battle);
            }

            if (_pathToFollow.Count > 0)
            {
                //Flocking();
                anim.SetBool("Run", true);
                _movement.FollowPath(_pathToFollow);
                //goalNode = GameManager.instance.GoalNode(transform, blueTeam);
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
            if (cor != null) StopCoroutine(cor);

            cor = Combos(0, 1, 2, 3);
            StartCoroutine(cor);
            //StartCoroutine(Combos(0, 1, 2, 3));
            //StartCoroutine(Combos(0, 1, 2, 3));
        };

        battle.OnUpdate += () =>
        {

            if (blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam))
            {
                target = GameManager.instance.DistanceFromTeamMember(GameManager.instance.redTeam, transform);
                //Teams(GameManager.instance.redTeam, target);              
            }


            if (!blueTeam && GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam))
            {
                target = GameManager.instance.DistanceFromTeamMember(GameManager.instance.blueTeam, transform);
                //Teams(GameManager.instance.blueTeam, target);
            }

            if (target != null)
                transform.LookAt(target.transform);

            #region Transitions
            if (_life <= 0)
            {
                SendInputToFSM(CharactersStates.Death);
                return;
            }

            if (blueTeam && !GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.redTeam) ||
                !blueTeam && !GameManager.instance.DistanceFromTarget(transform, distanceFromTarget, GameManager.instance.blueTeam))
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

            #region Combo 1 minions

            if (blueTeam && GameManager.instance.DistanceFromTargetMinions(transform, distanceFromTarget, GameManager.instance.allEntities.Where(x => !x.blueTeam).ToList()) || 
            !blueTeam && GameManager.instance.DistanceFromTargetMinion(transform, distanceFromTarget, GameManager.instance.allEntities.Where(x => x.blueTeam).ToList()))
            {
                E();
                //transform.position += Vector3.forward * 4f * Time.deltaTime;
            }

            #endregion

            #region Combo 1 character

            if (blueTeam && GameManager.instance.DistanceFromTargetCharacters(transform, distanceFromTarget, GameManager.instance.allEntities.Where(x => !x.blueTeam).ToList()) ||
                !blueTeam && GameManager.instance.DistanceFromTargetCharacters(transform, distanceFromTarget, GameManager.instance.allEntities.Where(x => x.blueTeam).ToList()))
            {
                canDoAllAbilities = allAbilitiesComplete;
            }

            #endregion

            #region E

            #endregion

            #region R
            R();
            #endregion
        };

        battle.OnExit += x =>
        {
            StopCoroutine(Combos(0, 1, 2, 3));
            _timer = 0;
            attack = false;
            canDoAllAbilities = false;
            //StopCoroutine(Attack());

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
        _garen_FSM = new EventFSM<CharactersStates>(idle);
        #endregion

        #endregion

    }

    public override void Update()
    {
        base.Update();

        _garen_FSM.Update();
        CoolDownQ();
        CoolDownE();
        CoolDownW();
        CoolDownR();

        if (canDoE) spellDamage.CauseDamage(false, 0, true, 1f);

        allAbilitiesComplete = timerQ == maxTimerQ && timerW == maxTimerW && timerE == maxTimerE && timerR == maxTimerR ? true : false;


    }

    public override void OnDestroy()
    {
        GameManager.instance.recal -= RecalculatePath;
        base.OnDestroy();
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
        if (blueTeam)
            transform.position = GameManager.instance.startingCNodeBlue.transform.position;
        else
            transform.position = GameManager.instance.startingCNodeRed.transform.position;

        anim.SetBool("Revive", false);
        yield return new WaitForSeconds(30);
        SendInputToFSM(CharactersStates.Idle);

    }

    public override void Shield(float sum)
    {
        base.Shield(sum);
    }

    public void RecalculatePath()
    {
        _movement.GetPath(_pf, GameManager.instance.ShortNode(transform), GameManager.instance.GoalNode(transform, blueTeam), _pathToFollow);
        SendInputToFSM(CharactersStates.Path);
    }

    IEnumerator Combos(int index1, int index2, int index3, int index4)
    {
        if (target == null || target != GetComponent<Characters>()) yield return null;

        //Debug.Log("HACE EL COMBO GAREEN 1");
        while (true)
        {
            if (canDoAllAbilities)
            {
                abilities[index1].Invoke();
                yield return new WaitForSeconds(2f);
                abilities[index2].Invoke();
                yield return new WaitForSeconds(1f);
                abilities[index3].Invoke();
                yield return new WaitForSeconds(7f);
                abilities[index4].Invoke();
            }

            yield return null;    
        }
    }


    public override void DestroyCharacter()
    {
        base.DestroyCharacter();
    }
    #region Abilities effect
    public void QEffect()
    {
        qDamage.CauseDamage(false, 0, true, 2f);
        StartCoroutine(QTime());
    }

    public void REffect()
    {
        if (target == null) return;

        GarenR obj = Instantiate(prefab, target.transform.position, target.transform.rotation);
        obj.blueTeam = blueTeam;
        obj.targetToFollow = target.transform;
        obj.character = this;
    }

    public void EEffect()
    {
        StartCoroutine(ETime());
    }
    #endregion

    #region Activation
    IEnumerator ETime()
    {
        canDoE = true;
        yield return new WaitForSeconds(1F);
        canDoE = false;
    }

    IEnumerator QTime()
    {
        canDoQ = true;
        yield return new WaitForSeconds(1F);
        canDoQ = false;
    }
    #endregion

    #region Abilites
    public override void Q()
    {
        if (timerQ >= maxTimerQ)
        {
            anim.SetTrigger("Q");
            timerQ = 0;
        }
    }

    public override void W()
    {
        if (timerW >= maxTimerW)
        {
            StartCoroutine(GenerateShield());
            timerW = 0;
        }
    }

    public override void E()
    {
        if (timerE >= maxTimerE)
        {
            EEffect();
            anim.SetTrigger("E");
            
            timerE = 0;
            //StartCoroutine(CoolDownE());
        }
    }

    public override void R()
    {
        if (target == null) return;

        if (target._life <= target.maxlife * 0.35f && timerR >= maxTimerR)
        {

            //if ()
            //{
            StartCoroutine(ChangeVelocity(10));
            anim.SetTrigger("R");
            timerR = 0;
            //    //StartCoroutine(CoolDownR());
            //}
        }
        else
        {
        }

    }
    #endregion

    #region CoolDowns
    public override void CoolDownQ()
    {
        //timerQ += Time.deltaTime;
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

    IEnumerator ChangeVelocity(float timer)
    {
        _speed = 0;
        yield return new WaitForSeconds(timer);
        _speed = maxSpeed;
    }

    IEnumerator GenerateShield()
    {
        shield = 20;
        yield return new WaitForSeconds(2f);
        shield = 0;
    }

    public override void Damage(float dmg, bool stun, float stunTime, bool rel, float relTime)
    {
        if (shield <= 0)
            base.Damage(dmg, stun, stunTime, rel, relTime);
        else shield -= dmg;

        if (_life == 0) deathsCount++;
    }

    public GreatSword SendInputToFSM(CharactersStates state)
    {
        _garen_FSM.SendInput(state);
        return this;
    }

    public IEnumerator SetPath()
    {
        yield return new WaitForSeconds(0.4f);
        _movement.GetPath(_pf, GameManager.instance.ShortNode(transform), GameManager.instance.GoalNode(transform, blueTeam), _pathToFollow);
    }

    public override void FinishMinion()
    {
        anim.SetTrigger("Death");
    }

    public override void AttackCharacter()
    {
        anim.SetTrigger("Attack");
    }

    public void SetDamage()
    {
        weapon.CauseDamage(false, 0, false, 0);
    }

    

    void Teams(List<Entity> team, Entity entity)
    {
        GameManager.instance.DistanceFromTeamMember(GameManager.instance.redTeam, transform);
        bool minion = GameManager.instance.DistanceFromTargetMinions(transform, distanceFromTarget, team);
        bool character = GameManager.instance.DistanceFromTargetCharacters(transform, distanceFromTarget, team);

        bool c = GameManager.instance.DistanceFromTargetChar(transform, distanceFromTarget, team);
        bool m = GameManager.instance.DistanceFromTargetMinion(transform, distanceFromTarget, team);

        if (!character)
        {
            if (minion)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanceFromTarget);
    }
}
