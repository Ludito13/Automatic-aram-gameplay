using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject details;

    public TMP_Text entitiesText;
    public TMP_Text charactersText;
    public TMP_Text minionsText;

    public TMP_Text kda;
    public TMP_Text qCoolDown;
    public TMP_Text wCoolDown;
    public TMP_Text eCoolDown;
    public TMP_Text rCoolDown;

    private Characters characters;
    private string w;
    private bool sawDetails;

    private void Awake()
    {
        if (instance == null) instance = this;
        else DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        sawDetails = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && !sawDetails)
        {
            details.SetActive(true);
            StartCoroutine(WaitToChangeBool(true));
        }
        
        if(Input.GetKeyDown(KeyCode.Tab) && sawDetails)
        {
            details.SetActive(false);
            StartCoroutine(WaitToChangeBool(false));
        }

        entitiesText.text = w;
        var allEntities = GameManager.instance.allEntities.Except(GameManager.instance.towers).Except(GameManager.instance.nexus).ToList();
        var allCharacters = GameManager.instance.allEntities.OfType<Characters>().ToList();
        var allMinions = GameManager.instance.allEntities.OfType<Minions>().ToList();

        entitiesText.text = "Cantidad de entidades en el campo " + allEntities.Count;

        charactersText.text = "Cantidad de jugadores maximos " + allCharacters.Count;
        minionsText.text = "Cantidad de jugadores subditos " + allMinions.Count;

        if (characters == null) return;

        var qCoold = GameManager.instance.characters.Where(x => x == characters).Select(x => x.timerQ).Zip(GameManager.instance.characters.Where(x => x == characters).Select(x => x.maxTimerQ), (x, y) => "Q: " + x + "/" + y).ToList().SingleOrDefault();
        var wCoold = GameManager.instance.characters.Where(x => x == characters).Select(x => x.timerW).Zip(GameManager.instance.characters.Where(x => x == characters).Select(x => x.maxTimerW), (x, y) => "W: " + x + "/" + y).ToList().SingleOrDefault();
        var eCoold = GameManager.instance.characters.Where(x => x == characters).Select(x => x.timerE).Zip(GameManager.instance.characters.Where(x => x == characters).Select(x => x.maxTimerE), (x, y) => "E: " + x + "/" + y).ToList().SingleOrDefault();
        var rCoold = GameManager.instance.characters.Where(x => x == characters).Select(x => x.timerR).Zip(GameManager.instance.characters.Where(x => x == characters).Select(x => x.maxTimerR), (x, y) => "R: " + x + "/" + y).ToList().SingleOrDefault();

        qCoolDown.text = qCoold;
        wCoolDown.text = wCoold;
        eCoolDown.text = eCoold;
        rCoolDown.text = rCoold;
    }

    IEnumerator WaitToChangeBool(bool c)
    {
        yield return null;
        sawDetails = c;
    }

    public void ShowKDA(Characters entity)
    {
        characters = entity;
        Debug.Log(characters);
        var deathCount = GameManager.instance.characters.Where(x => x == entity).Select(x => x.name).Zip(GameManager.instance.characters.Where(x => x == entity).Select(x => x._life), (x, y) => x + " vida actual: " + y).ToList().SingleOrDefault();


        kda.text = deathCount;

    }

}
