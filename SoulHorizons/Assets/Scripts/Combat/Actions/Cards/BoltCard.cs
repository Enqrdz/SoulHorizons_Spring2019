﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Cards/Bolt")]
[RequireComponent(typeof(AudioSource))]

public class BoltCard : ActionData
{
    public AttackData boltAttack;
    private AudioSource PlayCardSFX;
    public AudioClip BoltSFX;

    public override void Activate()
    {
        PlayCardSFX = GameObject.Find("ActionManager").GetComponent<AudioSource>();
        PlayCardSFX.clip = BoltSFX;
        PlayCardSFX.Play();
        Entity player = GameObject.FindGameObjectWithTag("Player").GetComponent<Entity>();

        //add attack to attack controller script
        AttackController.Instance.AddNewAttack(boltAttack, player._gridPos.x, player._gridPos.y, player);
    }
}