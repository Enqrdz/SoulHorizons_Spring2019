﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamingRonin : scr_EntityAI
{
    //Roaming Ronin
    //Two Attacks: a long range attack and an upclose melee when player is close
    //AI: Two Phases: Changes phase when health is 40%
    // Phase 1: Moves every 3 seconds, tries to be in the same row as the player
    // Phase 2: Moves every 1 second, melee attack also strikes twice

    public AttackData rangedAttack;
    public AttackData rangedAttack2; //stronger version of ranged attack
    public AttackData meleeAttack;
    public AttackData meleeAttack2; //stronger version of melee

    int maxHealth = 0;
    int currHealth = 0;

    int attackPhase = 0; //0 for normal phase, 1 for broken armor phase
    public int damageThreshold = 40; //when Ronin reaches 40% health, switch to next phase
    bool gonnaMelee = false;

    public float movementIntervalLower;
    public float movementIntervalUpper;
    int state = 0;
    bool completedTask = true;

    AudioSource[] SFX_Sources;
    AudioSource Attack_SFX;
    AudioSource Footsteps_SFX;
    public AudioClip[] movements_SFX;
    private AudioClip movement_SFX;
    public AudioClip[] attacks_SFX;
    private AudioClip attack_SFX;

    void Start()
    {
        AudioSource[] SFX_Sources = GetComponents<AudioSource>();
        Footsteps_SFX = SFX_Sources[0];
        Attack_SFX = SFX_Sources[0];
        anim = gameObject.GetComponentInChildren<Animator>();
        maxHealth = entity._health.hp;
        Debug.Log("Well Met!");
    }

    public override void UpdateAI()
    {
        scr_Grid.GridController.SetTileOccupied(true, entity._gridPos.x, entity._gridPos.y, this.entity);
        currHealth = entity._health.hp;
        PhaseManager();
        if (completedTask)
        {
            StartCoroutine(Brain());
        }

    }

    public override void Move()
    {
        int xPos = entity._gridPos.x;
        int yPos = entity._gridPos.y;
        int tries = 0;

        while (tries < 10)
        {
            if (gonnaMelee)
            {
                xPos = PickXCoord(xPos);
            }
            else
            {
                yPos = PickYCoord(yPos);
            }

            if (!scr_Grid.GridController.CheckIfOccupied(xPos, yPos) && (scr_Grid.GridController.ReturnTerritory(xPos, yPos).name == entity.entityTerritory.name))
            {
                entity.SetTransform(xPos, yPos);
                return;
            }
            else
            {
                tries++;
                if (tries >= 10)
                {
                    xPos = PickXCoord(xPos);
                    if (!scr_Grid.GridController.CheckIfOccupied(xPos, yPos) && (scr_Grid.GridController.ReturnTerritory(xPos, yPos).name == entity.entityTerritory.name))
                    {
                        entity.SetTransform(xPos, yPos);
                        return;
                    }
                }

            }
        }

    }

    public override void Die()
    {
        entity.Death();
    }

    public int PickXCoord(int xPos)
    {

        if (!gonnaMelee)
        {
            xPos++;
        }
        else
        {
            int xRange = scr_Grid.GridController.columnSizeMax;
            int tempX = xPos;
            for (int i = 0; i < xRange; i++)
            {
                tempX--;
                if (scr_Grid.GridController.grid[tempX, entity._gridPos.y].territory.name != TerrName.Player)
                {
                    xPos = tempX;
                }
                else
                {
                    return xPos;
                }
            }
        }
        return xPos;
    }

    public int PickYCoord(int yPos)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        int playerYPos = player.GetComponent<Entity>()._gridPos.y;
        if (!gonnaMelee)
        {
            if (yPos != playerYPos)
            {
                yPos = playerYPos;
            }
            return yPos;
        }
        else
        {
            return yPos;
        }
    }

    public void MoveBack()
    {
        int index = Random.Range(0, movements_SFX.Length);
        movement_SFX = movements_SFX[index];
        Footsteps_SFX.clip = movement_SFX;
        Footsteps_SFX.Play();

        int randomDir;
        randomDir = Random.Range(0, 2);
        int xPos = entity._gridPos.x;
        int yPos = entity._gridPos.y;

        if (randomDir == 0)
        {
            xPos++;
            yPos--;
        }
        else
        {
            xPos++;
            yPos++;
        }
        try
        {
            if (!scr_Grid.GridController.CheckIfOccupied(xPos, yPos) && (scr_Grid.GridController.ReturnTerritory(xPos, yPos).name == entity.entityTerritory.name))
            {
                entity.SetTransform(xPos, yPos);
                return;
            }
        }
        catch
        {
            MoveBack();
        }
    }

    private void PhaseManager()
    {
        int healthWhenArmorBreaks = (maxHealth * damageThreshold) / 100;
        if (currHealth <= healthWhenArmorBreaks)
        {
            attackPhase = 1;
        }
        else
        {
            attackPhase = 0;
        }
    }

    void StartRangedAttack()
    {
        //insert animation here
        anim.SetBool("RoninRanged", true);
        if (attackPhase == 0)
        {
            AttackController.Instance.AddNewAttack(rangedAttack, entity._gridPos.x, entity._gridPos.y, entity);
        }
        else
        {
            AttackController.Instance.AddNewAttack(rangedAttack2, entity._gridPos.x, entity._gridPos.y, entity);
        }

    }

    void RangedAttack()
    {
        int index = Random.Range(0, attacks_SFX.Length);
        attack_SFX = attacks_SFX[index];
        Attack_SFX.clip = attack_SFX;
        Attack_SFX.Play();

        Debug.Log("Air Slash");
        if (attackPhase == 0)
        {
            AttackController.Instance.AddNewAttack(rangedAttack, entity._gridPos.x, entity._gridPos.y, entity);
        }
        else
        {
            AttackController.Instance.AddNewAttack(rangedAttack2, entity._gridPos.x, entity._gridPos.y, entity);
        }
    }

    void StartMeleeAttack()
    {
        //insert animation here
        anim.SetBool("RoninMelee", true);
        Move();
        if (attackPhase == 0)
        {
            AttackController.Instance.AddNewAttack(meleeAttack, entity._gridPos.x, entity._gridPos.y, entity);
        }
        else
        {
            AttackController.Instance.AddNewAttack(meleeAttack2, entity._gridPos.x, entity._gridPos.y, entity);
        }

    }

    void MeleeAttack()
    {
        int index = Random.Range(0, attacks_SFX.Length);
        attack_SFX = attacks_SFX[index];
        Attack_SFX.clip = attack_SFX;
        Attack_SFX.Play();

        Debug.Log("BACK SLASH");
        if (attackPhase == 0)
        {
            AttackController.Instance.AddNewAttack(meleeAttack, entity._gridPos.x, entity._gridPos.y, entity);
        }
        else
        {
            AttackController.Instance.AddNewAttack(meleeAttack2, entity._gridPos.x, entity._gridPos.y, entity);
        }
    }


    private IEnumerator Brain()
    {
        switch (state)
        {
            case 0:
                completedTask = false;
                float _movementInterval = Random.Range(movementIntervalLower, movementIntervalUpper);
                yield return new WaitForSecondsRealtime(_movementInterval);
                Move();
                state = 1;
                completedTask = true;
                break;
            case 1:
                completedTask = false;
                gonnaMelee = true;
                yield return new WaitForSecondsRealtime(0.75f);
                state = 2;
                completedTask = true;
                break;
            case 2:
                completedTask = false;
                StartMeleeAttack();
                state = 3;
                gonnaMelee = false;
                completedTask = true;
                break;
            case 3:
                completedTask = false;
                float moveInterval = Random.Range(movementIntervalLower, movementIntervalUpper);
                yield return new WaitForSecondsRealtime(moveInterval);
                MoveBack();
                state = 4;
                completedTask = true;
                break;
            case 4:
                completedTask = false;
                yield return new WaitForSecondsRealtime(.75f);
                StartRangedAttack();
                yield return new WaitForSecondsRealtime(2);
                state = 0;
                completedTask = true;
                break;

        }
        yield return null;
    }
}
