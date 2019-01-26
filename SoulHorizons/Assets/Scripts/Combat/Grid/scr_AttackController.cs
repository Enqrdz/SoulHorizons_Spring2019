﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_AttackController : MonoBehaviour {

    public ActiveAttack[] activeAttacks = new ActiveAttack[10];     //Max number of active attacks is 10
    public int numberOfActiveAttacks = 0;                           //Number of attacks counter
    public static scr_AttackController attackController;            //Static reference for scr_AttackController
    public scr_Pause pauseReference;                                //scr_Pause reference

    private void Awake()
    {
        InitializeAttackController();
    }

    void Update()
    {
        for (int x = 0; x < numberOfActiveAttacks; x++)
        {
            if (activeAttacks[x].CanAttackContinue())
            {
                if (activeAttacks[x].currentIncrement > activeAttacks[x]._attack.maxIncrements)
                {
                    RemoveFromArray(x);
                    return;
                }
                else if (!activeAttacks[x]._attack.piercing && activeAttacks[x].isEntityHit)
                {
                    RemoveFromArray(x);
                    return;
                }
                else if (scr_Grid.GridController.LocationOnGrid(activeAttacks[x].position.x, activeAttacks[x].position.y) == false)
                {
                    //Debug.Log("location off grid " + activeAttacks[x]._attack.name); 
                    RemoveFromArray(x);
                    return;
                }

                if (activeAttacks[x].currentIncrement != 0)
                {
                    scr_Grid.GridController.DeactivateTile(activeAttacks[x].lastPosition.x, activeAttacks[x].lastPosition.y);
                }
                activeAttacks[x].lastPosition = activeAttacks[x].position;
                activeAttacks[x].Clone(scr_Grid.GridController.AttackPosition(activeAttacks[x]));
                activeAttacks[x].position = activeAttacks[x]._attack.ProgressAttack(activeAttacks[x].position.x, activeAttacks[x].position.y, activeAttacks[x]);
                activeAttacks[x].lastAttackTime = Time.time;
                activeAttacks[x].currentIncrement++;

            }
            //activeAttacks[x].particle.transform.position = Vector3.Lerp(activeAttacks[x].particle.transform.position, scr_Grid.GridController.GetWorldLocation(activeAttacks[x].lastPos.x,activeAttacks[x].lastPos.y) + activeAttacks[x]._attack.particlesOffset, (4.5f) * Time.deltaTime);
            //Replaced this lerp by passing the particle to progress attack above and letting the attack object determine particle behavior - Colin
            activeAttacks[x]._attack.ProgressEffects(activeAttacks[x]);
        }
    }

    private void InitializeAttackController()
    {
        attackController = this;
    }

    public void AddNewAttack(Attack _attack, int xPos, int yPos, scr_Entity ent)
    {
        activeAttacks[numberOfActiveAttacks] = new ActiveAttack(_attack, xPos, yPos, ent);
        activeAttacks[numberOfActiveAttacks]._attack.BeginAttack(xPos, yPos, activeAttacks[numberOfActiveAttacks]);
        activeAttacks[numberOfActiveAttacks].Clone(activeAttacks[numberOfActiveAttacks]._attack.BeginAttack(activeAttacks[numberOfActiveAttacks]));

        /*
        activeAttacks[numberOfActiveAttacks].particle = Instantiate(_attack.particles, scr_Grid.GridController.GetWorldLocation(xPos,yPos)+_attack.particlesOffset, Quaternion.identity);
        activeAttacks[numberOfActiveAttacks].particle.sortingOrder = -yPos; 
         */

        //Start effects for when the attack is created
        if (_attack == null)
        {
            Debug.Log("AttackController: attack is null");
        }
        if (activeAttacks[numberOfActiveAttacks] == null)
        {
            Debug.Log("AttackController: attack is null");
        }
        _attack.LaunchEffects(activeAttacks[numberOfActiveAttacks]);
        numberOfActiveAttacks++;

    }


    void RemoveFromArray(int index)
    {
        //Attack end effects
        activeAttacks[index]._attack.EndEffects(activeAttacks[index]);

        scr_Grid.GridController.DeactivateTile(activeAttacks[index].lastPosition.x, activeAttacks[index].lastPosition.y);
        scr_Grid.GridController.DeactivateTile(activeAttacks[index].position.x, activeAttacks[index].position.y);
        scr_Grid.GridController.DePrimeTile(activeAttacks[index].position.x, activeAttacks[index].position.y);
        Destroy(activeAttacks[index].particle);
        for (int x = index; x < numberOfActiveAttacks; x++)
        {
            if (x + 1 < activeAttacks.Length && activeAttacks[x + 1]._attack != null)
            {
                activeAttacks[x].Clone(activeAttacks[x + 1]);
            }
            else
            {
                activeAttacks[x] = new ActiveAttack();
            }
        }
        numberOfActiveAttacks--; 
    }

    public Attack AttackType(Vector2Int pos)
    {
        for (int x = 0; x < numberOfActiveAttacks; x++)
        {
            if (activeAttacks[x].position == pos)
            {
                return activeAttacks[x]._attack;
            }
        }
        
        return null; 
        
    }

    public Attack MoveIntoAttackCheck(Vector2Int pos, scr_Entity entity)
    {
        for (int x = 0; x < numberOfActiveAttacks; x++)
        {
            if (activeAttacks[x].lastPosition == pos)
            {
                if (activeAttacks[x].SourceEntity.type != entity.type)
                {
                    Attack atk = activeAttacks[x]._attack;
                    activeAttacks[x].isEntityHit = true;
                    return atk;
                }
            }
        }

        return null;
    }
}