using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum CharactersStates
{
    Path,
    Death, 
    Idle,
    Battle,
    Q,
    W,
    E,
    R
}

public enum MinionsStates
{
    Path,
    Attack,
    Death, 
    Avoid
}

public abstract class Entity : MonoBehaviour, IDamage
{
    public event Action<Entity> OnMove = delegate { };
    public SpatialGrid grid;
    [Header("Entity")]
    public bool blueTeam;
    //public bool redTeam;
    public float maxSpeed;
    public float maxForce;
    public float maxlife;
    public float attackDistance;
    protected Vector3 velocity;
    public float _life { get; protected set; }
    public float shield;
    protected float _speed;
    public bool onGrid { protected get; set; }
    public bool isStuned;
    public bool isRel;

    public Vector3 GetVelocity() => velocity;

    private void Start()
    {
        grid = GameManager.instance.grid;
        OnMove += grid.UpdateEntity;
        grid.UpdateEntity(this);


        //GameManager.instance.allEntities.Add(this);
        GameManager.instance.UpdateTeams();
        grid.UpdateEntity(this);
    }

    private void Update()
    {
        OnMove(this);
        grid.UpdateEntity(this);

    }



    public virtual void Damage(float dmg, bool stun, float stunTime, bool rel, float relTime)
    {
        _life -= dmg;

        if (stun) StartCoroutine(Stun(stunTime));

        if (rel) StartCoroutine(Rel(relTime));

        //if (_life <= 0) Destroy(gameObject);
    }

    IEnumerator Rel(float time)
    {
        isRel = true;
        _speed = maxSpeed * 0.40f;
        yield return new WaitForSeconds(time);
        _speed = maxSpeed;
        isRel = false;
    }


    IEnumerator Stun(float time)
    {
        isStuned = true;
        _speed = 0;
        yield return new WaitForSeconds(time);
        _speed = maxSpeed;
        isStuned = false;
    }
}
