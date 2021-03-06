using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class ElderWolf : Enemy
{
    /*
    combos = new EnemyComboDictionary()
    {
        { "One Attack", new EnemyCombo(1, WeaponAttackType.Melee, 30, new int[] { 3 }, new float[] { 0f }, new float[] { .2f }, 2f, new Vector2[] { new Vector2(5f, 0f) }) },
        { "Two Fast Attack", new EnemyCombo(2, WeaponAttackType.Melee, 25, new int[] { 2, 2 }, new float[] { .2f, 0f }, new float[] { .2f, .2f }, 2f, new Vector2[] { new Vector2(1f, 0f), new Vector2(1f, 0f) }) },
        { "Two Slow Attack", new EnemyCombo(2, WeaponAttackType.Melee, 25, new int[] { 3, 3 }, new float[] { .4f, 0f }, new float[] { .3f, .3f }, 3f, new Vector2[] { new Vector2(1f, 0f), new Vector2(3f, 0f) }) },
        { "Three Attack", new EnemyCombo(3, WeaponAttackType.Melee, 20, new int[] { 2, 2, 4 }, new float[] { .2f, .3f, 0f }, new float[] { .2f, .2f, .4f }, 4f, new Vector2[] { new Vector2(1f, 0f), new Vector2(2f, 0f), new Vector2(4f, 0f) }) },
    };*/

    [Header("Spell Properties")]
    [SerializeField] EnemySpellDictionary mySpells = new EnemySpellDictionary()
    {
        { "Long Jump",  new EnemySpell(10f, new Vector2(10f, 5f), 1, 10f, 3f, 1.5f, .75f, new Vector2(2f, 2f), new Vector2(5f, 5f), 1.5f, 5, 7) },
        { "Back Jump",  new EnemySpell(3f, new Vector2(7f, 4f), -1, 7f, 3f, .5f, .5f, new Vector2(2f, 2f), new Vector2(3f, 0f), 1f, 2, 5) },
        { "Ice Breath", new EnemySpell(7f, 8f, 2f, 1f, .5f, new Vector2(10f, 5f), new Vector2(.5f, 0f), .3f, 2, .5f, new Element("Ice", Elements.Ice, 1), new Effect(Effects.Freeze, 1)) },
        { "Swing Tail", new EnemySpell(5f, 8f, 2f, 1f, .5f, new Vector2(2f, 2f), new Vector2(5f, 2f), 1.5f, 10) },
        { "Ice Spikes", new EnemySpell(11f, 15f, 2f, 1f, 2f, new Vector2(2f, 5f), new Vector2(4f, 2f), 2f, 15, new Element("Ice", Elements.Ice, 10), new Effect(Effects.Freeze, 2)) },
        { "Howl",       new EnemySpell(15f, 15f, 3f, 1.5f, 3f, new Vector2(0f, 0f), new Vector2(4f, 2f), .5f, 5, .5f) },
        { "Knockback",  new EnemySpell(2.5f, new Vector2(10f, 5f), 1, 5f, .5f, .5f, 1f, new Vector2(6f, 2f), new Vector2(6f, 6f), 1.5f, 2, 5) },
    };
    [SerializeField] GameObject iceCloudPrefab = null;

    Dictionary<string, float> spellsCurCooldown = new Dictionary<string, float>();

    bool isSpellCasted;

    [SerializeField] Transform frontLegs = null;
    [SerializeField] Transform head = null;
    [SerializeField] Transform tail = null;
    [SerializeField] Transform iceBreathTransform = null;
    IceBreath iceBreath;
    BoxCollider2D tailCol;
    Collider2D objectToDamage;
    [SerializeField] GameObject wolfPrefab = null;

    float maxDurationWithoutDamage = 15f;       //For Howl and other spells which used if Wolf can't give damage a long time    

    private void OnGUI()
    {
        float[] cd = new float[10];
        string[] name = new string[10];
        int a = 0;
        foreach(KeyValuePair<string, float> spell in spellsCurCooldown)
        {
            name[a] = spell.Key + ": ";
            cd[a] = spell.Value > 0f ? spell.Value - Time.time : 0f;
            a++;
        }
        //GUI.TextField(new Rect(10, 10, 150, 200), name[0] + cd[0].ToString() + "\n" + name[1] + cd[1].ToString() + "\n" + name[2] + cd[2].ToString() + "\n"
        //    + name[3] + cd[3].ToString() + "\n" + name[4] + cd[4].ToString() + "\n" + name[5] + cd[5].ToString() + "\n" + name[6] + cd[6].ToString() + "\n"
        //     + name[7] + cd[7].ToString() + "\n" + name[8] + cd[8].ToString() + "\n");
    }

    new void Awake()
    {
        base.Awake();

        iceBreath = iceBreathTransform.GetComponent<IceBreath>();
        iceBreath.colliderSize = mySpells["Ice Breath"].damageRange;

        foreach (Transform child in myTransform)
        {
            if (child.name == "Tail")
                tailCol = child.GetComponent<BoxCollider2D>();
        }

        foreach (KeyValuePair<string, EnemySpell> _spell in mySpells)
        {
            spellsCurCooldown.Add(_spell.Key, 0f);
        }
    }

    new void Update() 
    {
        base.Update();
        Debug.Log("spell: " + spell);

        //Sets two of three spells on cooldown
        if (!target)
        {
            int a = Random.Range(0, 3);

            switch (a)
            {
                case 0:
                    spellsCurCooldown["Long Jump"] = mySpells["Long Jump"].cooldown + Time.time;
                    spellsCurCooldown["Ice Spikes"] = mySpells["Ice Spikes"].cooldown + Time.time;
                    spellsCurCooldown["Ice Breath"] = 0f;
                    break;
                case 1:
                    spellsCurCooldown["Long Jump"] = 0f;
                    spellsCurCooldown["Ice Spikes"] = mySpells["Ice Spikes"].cooldown + Time.time;
                    spellsCurCooldown["Ice Breath"] = mySpells["Ice Breath"].cooldown + Time.time;
                    break;
                case 2:
                    spellsCurCooldown["Long Jump"] = mySpells["Long Jump"].cooldown + Time.time;
                    spellsCurCooldown["Ice Spikes"] = 0f;
                    spellsCurCooldown["Ice Breath"] = mySpells["Ice Breath"].cooldown + Time.time;
                    break;
            }

            spellsCurCooldown["Howl"] = mySpells["Howl"].cooldown / 2f + Time.time;
        }

        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    GameObject _spawner = Resources.Load<GameObject>("Enemies/Elder Wolf/Ice Spike Spawner");
        //    GameObject _inst = Instantiate(_spawner, myTransform);
        //    IceSpikesSpawner _iss = _inst.GetComponent<IceSpikesSpawner>();
        //    _iss.spell = mySpells["Ice Spikes"];
        //    _iss.player = playerMovement == null ? GameObject.Find("Player").GetComponent<PlayerMovement>() : playerMovement;
        //}
    }

    override protected void SwitchSpell()
    {
        //Cast spell if it's possible
        if (curGlobalSpellCD <= Time.time && target != null && !isAttack && !isCast)
        {
            bool spellSelected = false;

            if ((spellsCurCooldown["Back Jump"] <= Time.time || spellsCurCooldown["Knockback"] <= Time.time) && DistanceToPlayer().x < mySpells["Knockback"].castRange && DistanceToPlayer().y < 2f)
            {
                int _rand = Random.Range(0, 2);

                if (_rand == 0 && spellsCurCooldown["Back Jump"] <= Time.time)
                {
                    spell = "Back Jump";
                    spellSelected = true;
                }
                else if(_rand == 1 && spellsCurCooldown["Knockback"] <= Time.time)
                {
                    spell = "Knockback";
                    spellSelected = true;
                }
            }
            else
            { 
                if (IsPlayerBehind())
                {
                    //Swing Tail:
                    //  Player behind wolf in cast range
                    if (!spellSelected && spellsCurCooldown["Swing Tail"] <= Time.time)
                    {
                        if (DistanceToPlayer().x <= mySpells["Swing Tail"].castRange && DistanceToPlayer().y < 4f)
                        {
                            spell = "Swing Tail";
                            spellSelected = true;
                        }
                    }
                    else return;
                }
                else
                {
                    //Back Jump:
                    //  After take many damage in last seconds and player in front of the wolf Back Jump -> Howl (50% chance)
                    //  Two fast attack combo if one of them hit the player then Back Jump and Ice Breath
                    if (!spellSelected && spellsCurCooldown["Back Jump"] <= Time.time)
                    {
                        if ((lastAttack == "Two Fast Attack" && isHitPlayer) ||
                            (damageTakenDPS >= maxHealth / 100 * 3 && Random.Range(0, 100) > 50))
                        {
                            spell = "Back Jump";
                            spellSelected = true;
                        }

                    }
                    //Ice Breath:
                    //  Two fast attack combo if one of them hit the player then Back Jump and Ice Breath
                    //  If one slow attack hit player (and repulse him from wolf)
                    //  After Long Jump if player get caught (50% chance)
                    //  Player in cast range (30% chance)
                    if (!spellSelected && spellsCurCooldown["Ice Breath"] <= Time.time)
                    {
                        if ((lastAttack == "Two Fast Attack" && lastSpell == "Back Jump") ||
                             lastAttack == "One Attack" ||
                            (lastSpell  == "Long Jump" && Random.Range(0, 100) > 50) ||
                             DistanceToPlayer().x <= mySpells["Ice Breath"].castRange && DistanceToPlayer().x >= colliderWidth && Random.Range(0, 100) > 30 ||
                             lastSpell == "Knockback")
                        {
                            spell = "Ice Breath";
                            spellSelected = true;
                        }
                    }
                    //Long Jump:
                    //  If player on cast range between Long Jump and Ice Spikes
                    //  When player jump (50% chance)
                    if (!spellSelected && spellsCurCooldown["Long Jump"] <= Time.time)
                    {
                        if (DistanceToPlayer().x <= mySpells["Long Jump"].castRange ||
                           ((!playerMovement.isOnGround ||
                            playerMovement.isJumping) && Random.Range(0, 100) > 50))
                        {
                            spell = "Long Jump";
                            spellSelected = true;
                        }
                    }
                    //Ice Spikes:
                    //  After Long Jump if player get caught (50% chance)
                    //  Player out in cast range
                    //  Three-attack combo ends with Ice Spikes
                    if (!spellSelected && spellsCurCooldown["Ice Spikes"] <= Time.time)
                    {
                        if ((lastSpell == "Long Jump" && playerMovement.hurtType == HurtType.Catch /*&& Random.Range(0, 100) > 50*/) ||
                            DistanceToPlayer().x >= mySpells["Ice Spikes"].castRange ||
                            lastAttack == "Three Attack")
                        {
                            spell = "Ice Spikes";
                            spellSelected = true;
                        }
                    }
                    //Howl:
                    //  After take many damage in last seconds and player in front of the wolf Back Jump -> Howl (50% chance)
                    //  If wolf dont hit player more few seconds
                    if (!spellSelected && spellsCurCooldown["Howl"] <= Time.time)
                    {
                        if ((lastSpell == "Back Jump" && damageTakenDPS >= maxHealth / 100 * 3 && Random.Range(0, 100) > 50) ||
                           (playerAttributes.timeOfLastTakenDamage + maxDurationWithoutDamage <= Time.time && DistanceToPlayer().x <= mySpells["Howl"].castRange) ||
                           (damageTakenDPS >= maxHealth / 100 * 3 && Random.Range(0, 100) > 50))
                        {
                            spell = "Howl";
                            spellSelected = true;
                        }
                    }
                }

                if(!spellSelected) return;
            }

            if (spell != "None")
            {
                lastSpell = spell;
                curSpellPrepareTime = mySpells[spell].prepareTime + Time.time;
                SwitchState(State.CastSpell);
                spellState = EnemySpellStates.Prepare;
            }
        }
    }

    override protected void CastSpell()
    {
        if (spellState == EnemySpellStates.Prepare && curSpellPrepareTime <= Time.time)
        {
            spellState = EnemySpellStates.Cast;
            curSpellCastTime = mySpells[spell].castTime + Time.time;
        }

        if (spellState == EnemySpellStates.Cast && curSpellCastTime <= Time.time)
            spellState = EnemySpellStates.End;

        switch (spell)
        {
            case "None":
                Debug.LogError("Enemy try cast spell with none value. Enemy ID: " + gameObject.GetInstanceID());
                break;
            case "Long Jump":
                LongJump();
                break;
            case "Back Jump":
                BackJump();
                break;
            case "Swing Tail":
                SwingTail();
                break;
            case "Ice Breath":
                IceBreath();
                break;
            case "Ice Spikes":
                IceSpikes();
                break;
            case "Howl":
                Howl();
                break;
            case "Knockback":
                Knockback();
                break;
        }
    }

    void SpellCasted()
    {
        objectToDamage = null;
        isSpellCasted = false;
        spellsCurCooldown[spell] = mySpells[spell].cooldown + Time.time;
        curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
        curAttackCD = mySpells[spell].globalCD + Time.time;
        spell = "None";
        SwitchState(State.Attack);
    }
    
    void LongJump()
    {
        spellNumber = 1;

        if (isSpellCasted)
        {
            SpellCasted();
        }
        else if (!frontLegs.gameObject.activeSelf && spellState == EnemySpellStates.Cast)
        {
            frontLegs.gameObject.SetActive(true);
            frontLegs.GetComponent<EWLongJump>().spell = mySpells[spell];
        }
    }

    void BackJump()
    {
        spellNumber = 2;

        if (isSpellCasted)
        {
            SpellCasted();
        }
    }

    void SwingTail()
    {
        spellNumber = 3;

        if (isSpellCasted)
        {
            SpellCasted();
        }
    }

    void IceBreath()
    {
        spellNumber = 4;

        if (isSpellCasted)
        {
            SpellCasted();
        }
    }

    void IceSpikes()
    {
        spellNumber = 5;

        if (isSpellCasted)
        {
            SpellCasted();
        }
    }

    void Howl()
    {
        spellNumber = 6;

        if (isSpellCasted)
        {
            SpellCasted();
        }
    }

    void Knockback()
    {
        spellNumber = 7;

        if(isSpellCasted)
        {
            SpellCasted();
        }
    }

    void StartJump()
    {
        rigidBody.AddForce(new Vector2(mySpells[spell].jumpDistance.x * mySpells[spell].jumpDirection * direction, mySpells[spell].jumpDistance.y), ForceMode2D.Impulse);
        PlayJumpAudio(mySpells[spell].jumpClip);
        SpawnIceCloud();
        //if (spell == "Knockback")
        //    SpellOnceDamage();
    }

    new private void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        /*
        if (frontLegsCol != null)
            Gizmos.DrawWireCube(frontLegs.position, new Vector3(frontLegsCol.size.x * 2f, frontLegsCol.size.y, 0f));*/

        if (spell == "Knockback" || spell == "Long Jump" || spell == "Back Jump")
            Gizmos.DrawWireCube(myTransform.position, mySpells[spell].damageRange);
    }

    void Landing()
    {
        frontLegs.gameObject.SetActive(false);

        if (spell != "Knockback")
        {
            rigidBody.velocity = Vector2.zero;
            isSpellCasted = true;
            Instantiate(iceCloudPrefab, myTransform.position, Quaternion.identity);
        }
        else
        {
            rigidBody.AddForce(new Vector2(mySpells["Knockback"].jumpDistance.x / 4f * direction, 0f), ForceMode2D.Impulse);
            GameObject _iceTracks = Resources.Load<GameObject>("Enemies/Elder Wolf/Particles/IceTracks");
            Instantiate(_iceTracks, new Vector2(myTransform.position.x + 2.8f * -direction, myTransform.position.y + 0.2f), myTransform.rotation, myTransform);
        }

        //GameObject _landingEffect = Resources.Load<GameObject>("Enemies/Elder Wolf/Elder Wolf Landing Effect");
        //Instantiate(_landingEffect, myTransform.position, Quaternion.identity);
        PlayLandingAudio(mySpells[spell].landingClip);
        

        objectToDamage = Physics2D.OverlapBox(myTransform.position, mySpells[spell].damageRange, 0f, playerLayer);

        if (objectToDamage != null)
        {
            int _repulseDirection = IsPlayerBehind() ? -1 : 1;
            objectToDamage.GetComponent<PlayerAttributes>().TakeDamage(mySpells[spell].lastDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * _repulseDirection, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime, mySpells[spell].elementDamage);
        }
    }

    void SpellOnceDamage()
    {
        switch (spell)
        {
            case "Ice Spikes":
                GameObject _spawner = Resources.Load<GameObject>("Enemies/Elder Wolf/Ice Spike Spawner");
                GameObject _inst = Instantiate(_spawner, myTransform);
                IceSpikesSpawner _iss = _inst.GetComponent<IceSpikesSpawner>();
                _iss.spell = mySpells[spell];
                _iss.player = playerMovement == null ? GameObject.Find("Player").GetComponent<PlayerMovement>() : playerMovement;
                break;
            case "Swing Tail":
                objectToDamage = Physics2D.OverlapBox(tailCol.transform.position, new Vector2(tailCol.size.x, tailCol.size.y), 0, playerLayer);

                if (objectToDamage != null)
                    objectToDamage.GetComponent<PlayerAttributes>().TakeDamage(mySpells[spell].firstDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * -direction, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime, mySpells[spell].elementDamage);

                audioSource.PlayOneShot(mySpells[spell].spellClip);

                break;
            case "Howl":
                HowlCirclePulse _hcp = head.GetComponent<HowlCirclePulse>();
                head.gameObject.SetActive(true);
                _hcp.spell = mySpells[spell];
                _hcp.StartCoroutine(_hcp.CirclePulse());

                for (int i = 0; i <= 1; i++)
                {
                    int _xPos = i % 2 == 0 ? 1 : -1;
                    Instantiate(wolfPrefab, new Vector2(myTransform.position.x + 20 * _xPos, myTransform.position.y), myTransform.rotation);
                }
                break;
            case "Knockback":
                objectToDamage = Physics2D.OverlapBox(myTransform.position, mySpells[spell].damageRange, 0, playerLayer);

                if (objectToDamage != null)
                {
                    int _repulseDirection = IsPlayerBehind() ? -1 : 1;
                    objectToDamage.GetComponent<PlayerAttributes>().TakeDamage(mySpells[spell].firstDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * _repulseDirection, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime / 3f, mySpells[spell].elementDamage);
                }
                break;
            case "Ice Breath":
                if (iceBreath.gameObject.activeSelf)
                    break;
                else
                {
                    iceBreath.gameObject.SetActive(true);
                    iceBreath.spell = mySpells[spell];
                    iceBreath.StartCoroutine(iceBreath.IceBreathCast());
                    break;
                }
        }
    }

    void SpawnIceCloud()
    {
        Instantiate(iceCloudPrefab, myTransform.position, myTransform.rotation);
    }

    void SpellEnded()
    {
        isSpellCasted = true;
    }

    override protected void FlipOtherTransforms()
    {
        tail.transform.localPosition = new Vector2(-tail.transform.localPosition.x, tail.transform.localPosition.y);
        frontLegs.transform.localPosition = new Vector2(-frontLegs.transform.localPosition.x, frontLegs.transform.localPosition.y);
        head.transform.localPosition = new Vector2(-head.transform.localPosition.x, head.transform.localPosition.y);
        iceBreath.FlipObject();
    }

    void PlayJumpAudio(AudioClip clip)
    {
        if (audioSource == null)
            return;

        audioSource.pitch = Random.Range(.6f, .9f);
        audioSource.PlayOneShot(clip);
    }

    void PlayLandingAudio(AudioClip clip)
    {
        if (audioSource == null)
            return;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(clip, 2f);
    }
}