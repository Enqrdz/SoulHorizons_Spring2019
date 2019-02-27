﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Cards/Mend")]
[RequireComponent(typeof(AudioSource))]

public class scr_Mend : ActionData
{

    public int Mend_hp;
    private AudioSource PlayCardSFX;
    public AudioClip MendSFX;
    public override void Activate()
    {
        PlayCardSFX = GameObject.Find("ActionManager").GetComponent<AudioSource>();
        PlayCardSFX.clip = MendSFX;
        PlayCardSFX.Play();
        Entity player = GameObject.FindGameObjectWithTag("Player").GetComponent<Entity>();
        player._health.hp += Mend_hp;
        if(player._health.hp > player._health.max_hp)
        {
            player._health.hp = player._health.max_hp;
        }

    }
}