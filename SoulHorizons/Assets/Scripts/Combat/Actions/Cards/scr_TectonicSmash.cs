﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Tectonic Smash")]
[RequireComponent(typeof(AudioSource))]

public class scr_TectonicSmash : ActionData
{
    private AudioSource PlayCardSFX;
    public AudioClip SmashSFX;
    public AttackData Smash;

    public override void Activate()
    {
        PlayCardSFX = ObjectReference.Instance.ActionManager;
        PlayCardSFX.clip = SmashSFX;
        PlayCardSFX.Play();
        Entity player = ObjectReference.Instance.PlayerEntity;

        //add attack to attack controller script
        AttackController.Instance.AddNewAttack(Smash, player._gridPos.x, player._gridPos.y, player);
    }

    public override void Project()
    {
        throw new System.NotImplementedException();
    }

    public override void DeProject()
    {
        throw new System.NotImplementedException();
    }
}
