﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(AudioSource))]

public class scr_PlayerMovement : EntityAI
{
    AudioSource Footsteps_SFX;
    public AudioClip[] movements_SFX;
    public GameObject dashEffect; //the particle effect that plays when the player dashes
    private AudioClip movement_SFX;
    public GameObject dashMeter;
    public GameObject dashIcon;
    public Sprite dashOn;
    public Sprite dashOff;
    List<GameObject> dashes = new List<GameObject>(); //array holding dash icons to update

    [SerializeField] private int staminaChargesMax = 2;
    [SerializeField] private int staminaCharges = 2; //the number of times that the player can dash without recharging
    [SerializeField] private float staminaRechargeTime = 1.5f; //how long it takes to recharge 1 stamina

    public int GetStaminaCharges()
    {
        return staminaCharges;
    }
  
    public override void Move()
    {

    }

    public override void UpdateAI()
    {
        MovementCheck();
        UpdateDash();
    }
    public override void Die()
    {
    }

    public void UpdateDash()
    {
        for(int i = 0; i < staminaChargesMax; i++)
        {
            dashes[i].GetComponent<SpriteRenderer>().sprite = dashOn;
        }
        for(int i = staminaChargesMax-1; i > staminaCharges-1; i--)
        {
            dashes[i].GetComponent<SpriteRenderer>().sprite = dashOff;
        }
    }
    void Start()
    {
        float tempX = dashMeter.transform.position.x;
        float tempY = dashMeter.transform.position.y;
        for (int i = 0; i < staminaChargesMax; i++)
        {
            Debug.Log("Making a dash: " + i);
            GameObject dashSprite = Instantiate(dashIcon, new Vector3(tempX, tempY, 0), Quaternion.identity);
            dashSprite.transform.SetParent(dashMeter.transform);
            dashes.Add(dashSprite);
            tempY -= .1f;
        }
    }

    int inputX;
    int inputY;
    bool axisPressed = false; //used to get "OnJoystickDown"
    void MovementCheck()
    {
        int _x = entity._gridPos.x;
        int _y = entity._gridPos.y;

        if(inputX != scr_InputManager.MainHorizontal())
        {
            inputX = scr_InputManager.MainHorizontal();
            if (inputX != 0 && scr_InputManager.IsDashPressed() && staminaCharges > 0)
            {
                staminaCharges--; //spend a stamina charge
                Instantiate(dashEffect, transform.position, dashEffect.transform.rotation);
                StartCoroutine(StaminaRecharge()); //recharge the stamina after waiting the recharge time

                //dash in the direction of the movement
                if (inputX < 0)
                {
                    //move to the left end of the grid
                    _x = 0;
                }
                else
                {
                    //move to the rightmost player space
                    //get the rightmost player tile on this row
                    for (int x = _x; x  < Grid.Instance.columnSizeMax; x++)
                    {
                        if (Grid.Instance.ReturnTerritory(x,_y).name == entity.entityTerritory.name)
                        {
                            _x = x; //if we found a valid space then this becomes the new space to move to
                        }
                        else
                        {
                            break; //stop searching once we hit enemy territory
                        }
                    }
                }
            }
            else //perform normal movement
            {
                _x += scr_InputManager.MainHorizontal();
            }
            axisPressed = true;
            if (scr_InputManager.MainHorizontal() == 0)
            {
                Debug.Log("FOOTSTEP SOUNDS");
                AudioSource[] SFX_Sources = GetComponents<AudioSource>();
                Footsteps_SFX = SFX_Sources[0];
                int index = Random.Range(0, movements_SFX.Length);
                movement_SFX = movements_SFX[index];
                Footsteps_SFX.clip = movement_SFX;
                Footsteps_SFX.Play();
            }
        }

        if (inputY != scr_InputManager.MainVertical())
        {
            inputY = scr_InputManager.MainVertical();
            if (inputY != 0 && scr_InputManager.IsDashPressed() && staminaCharges > 0)
            {
                staminaCharges--; //spend a charge
                Instantiate(dashEffect, transform.position, dashEffect.transform.rotation);
                StartCoroutine(StaminaRecharge()); //recharge the stamina after waiting the recharge time

                //dash in the direction of the movement
                if (inputY < 0)
                {
                    //move to the left end of the grid
                    _y = 0;
                }
                else
                {
                    //move to the rightmost player space
                    //get the rightmost player tile on this row
                    for (int y = _y; y  < Grid.Instance.rowSizeMax; y++)
                    {
                        if (Grid.Instance.ReturnTerritory(_x,y).name == entity.entityTerritory.name)
                        {
                            _y = y; //if we found a valid space then this becomes the new space to move to
                        }
                        else
                        {
                            break; //stop searching once we hit enemy territory
                        }
                    }
                }
            }
            else //perform normal movement
            {
                _y += scr_InputManager.MainVertical();
            }
            axisPressed = true;
            if (scr_InputManager.MainVertical() == 0)
            {
                AudioSource[] SFX_Sources = GetComponents<AudioSource>();
                Footsteps_SFX = SFX_Sources[0];
                int index = Random.Range(0, movements_SFX.Length);
                movement_SFX = movements_SFX[index];
                Footsteps_SFX.clip = movement_SFX;
                Footsteps_SFX.Play();
            }
        }

        if (scr_InputManager.MainHorizontal() == 0 && scr_InputManager.MainVertical() == 0)
        {
            //joystick is not pressed
            axisPressed = false;
        }

        if (Grid.Instance.LocationOnGrid(_x, _y) &&  Grid.Instance.ReturnTerritory(_x,_y).name == entity.entityTerritory.name)
        {
           
            entity.SetTransform(_x, _y);
        }

    }

    ///<summary>
    ///Recharge 1 stamina after waiting the recharge time
    ///</summary>
    private IEnumerator StaminaRecharge()
    {
        yield return new WaitForSeconds(staminaRechargeTime);
        staminaCharges++;
    }

}

