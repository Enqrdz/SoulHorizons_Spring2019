﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Cards/HardenSoul")]
[RequireComponent(typeof(AudioSource))]

public class scr_HardenSoul : ActionData {

    private AudioSource PlayCardSFX;
    public AudioClip HardenSoulSFX;
    public int Shield_hp; //How much shield hp
    public override void Activate()

    {
        PlayCardSFX = GameObject.Find("ActionManager").GetComponent<AudioSource>();
        PlayCardSFX.clip = HardenSoulSFX;
        PlayCardSFX.Play();
        Entity player = GameObject.FindGameObjectWithTag("Player").GetComponent<Entity>();
        player._health.shield += Shield_hp;
    }
}
