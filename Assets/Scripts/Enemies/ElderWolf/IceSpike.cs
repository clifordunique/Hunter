﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class IceSpike : MonoBehaviour
{
    [HideInInspector] public EnemySpell spell;
    [SerializeField] float lifeTime = 3f;
    [SerializeField] float prepareTime = 1f;
    float maxColSizeY;
    float maxColOffsetY;
    float curLifeTime;
    float curPrepareTime;
    
    int spellCastParamID;
    int spellEndParamID;

    bool damageDone;
    Animator anim;
    BoxCollider2D myCollider;
    PolygonCollider2D tipCollider;
    PlayerAtributes playerAtributes;
    SpellStates state = SpellStates.Prepare;

    // Start is called before the first frame update
    void Awake()
    {
        anim = transform.GetComponent<Animator>();
        myCollider = GetComponent<BoxCollider2D>();
        tipCollider = GetComponent<PolygonCollider2D>();
        maxColSizeY = myCollider.size.y;
        maxColOffsetY = myCollider.offset.y;
        myCollider.size = new Vector2(myCollider.size.x, .3f);
        myCollider.offset = new Vector2(myCollider.offset.x, .1f);
        spellCastParamID = Animator.StringToHash("spellCast");
        spellEndParamID = Animator.StringToHash("spellEnd");
        curLifeTime = lifeTime + prepareTime + Time.time;
        curPrepareTime = prepareTime + Time.time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch(state)
        {
            case SpellStates.Prepare:
                Prepare();
                break;
            case SpellStates.Cast:
                Cast();
                break;
            case SpellStates.End:
                End();
                break;
        }
    }

    void Prepare()
    {
        if (curPrepareTime <= Time.time)
        {
            anim.SetTrigger(spellCastParamID);
            state = SpellStates.Cast;
        }
    }

    void Cast()
    {
        if (myCollider.size.y < maxColSizeY)
            myCollider.size = new Vector2(myCollider.size.x, myCollider.size.y + maxColSizeY / .25f * Time.deltaTime); //.25f - cast animation duration

        if (myCollider.offset.y < maxColOffsetY)
            myCollider.offset = new Vector2(myCollider.offset.x, myCollider.offset.y + maxColOffsetY / .25f * Time.deltaTime);

        if (playerAtributes != null && !damageDone)
        {
            int _direction = playerAtributesDirection();
            float _repulseForceY = (1f - myCollider.size.y / maxColSizeY) * spell.repulseVector.y;
            playerAtributes.TakeDamage(spell.firstDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction, _repulseForceY), spell.dazedTime, spell.elementDamage);
            playerAtributes.TakeEffect(spell.effect);
            damageDone = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            playerAtributes = collision.GetComponent<PlayerAtributes>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            playerAtributes = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            //playerAtributes = collision.transform.GetComponent<PlayerAtributes>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            //playerAtributes = null;
        }
    }

    void Casted()
    {
        state = SpellStates.End;
    }

    void End()
    {
        myCollider.isTrigger = false;
        tipCollider.enabled = true;

        if (playerAtributes != null && !damageDone)
        {
            if (playerAtributes.rigidBody.velocity.y < 0)
            {
                int _direction = playerAtributesDirection();
                float _repulseForceY = spell.repulseVector.y * .3f;
                playerAtributes.TakeDamage(spell.firstDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction, _repulseForceY), spell.dazedTime, spell.elementDamage);
                playerAtributes.TakeEffect(spell.effect);
                damageDone = true;
            }
        }

        if (curLifeTime <= Time.time)
            Destroy(gameObject);
    }

    int playerAtributesDirection()
    {
        return (int) Mathf.Sign(playerAtributes.transform.position.x - transform.position.x);
    }
}
