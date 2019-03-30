﻿using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(AudioSource))]

public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance { get; private set; }
	public ActionUI[] abilityUI;
    public ActionUI[] mantraUI;
    public GameObject handPanel;
    public Animator playerAnimator;
    public float globalCooldown;


    [SerializeField]
    private AudioSource CardChange_SFX;
    [SerializeField]
	private Deck currentDeck;
    [SerializeField]
    private Mantras currentMantras;
    [SerializeField]
    private scr_SoulManager soulManager;

    private bool readyToCastAction = true;
    private bool readyToCastAbility = true;
    private bool readyToCastMantra = true;
    private bool deckIsEnabled = true;

    void Awake()
    {
        currentDeck = GetComponent<Deck>();
        currentMantras = GetComponent<Mantras>();
    }

	void Start ()
    {
        AudioSource SFX_Source = GetComponent<AudioSource>();
        playerAnimator = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Animator>();
        CardChange_SFX = SFX_Source;
        UpdateGUI();
        SetMantraGraphics();
    }

    void Update()
    {
        if (deckIsEnabled == true)
        {
            GetUserInput();
            UpdateGUI();
        }
    }

    void GetUserInput()
    {
        //decisionNumber will be either: -1,0,1,2,3
        int decisionNumber = InputManager.ActionNumber();

        if (decisionNumber != -1 && readyToCastAction)
        {
            bool decisionNumberIsACard = decisionNumber == 0 || decisionNumber == 1;
            bool decisionNumberIsAMantra = decisionNumber == 2 || decisionNumber == 3;

            if (decisionNumberIsACard && readyToCastAbility)
            {
                ActivateCard(decisionNumber);
            }
            if (decisionNumberIsAMantra && readyToCastMantra)
            {
                ActivateMantra(decisionNumber);
            }
        }
    }


    private void ActivateMantra(int decisionNumber)
    {
        //Decision number comes from input manager and will by 2 or 3
        //Index must be 0 or 1
        int index = decisionNumber - 2;

        currentMantras.Activate(index);

        StartCoroutine(MantraCooldown(currentMantras.activeMantras[index].cooldown));
        StartCoroutine(ActionCooldown(globalCooldown));

        soulManager.ChargeSoulTransform(currentMantras.activeMantras[index].element, currentMantras.activeMantras[index].transformChargeAmount);

        playerAnimator.SetBool("Cast", true);

        for (int i = 0; i < mantraUI.Length; i++)
        {
            mantraUI[i].StartCooldown(currentMantras.activeMantras[index].cooldown);
            if (readyToCastAbility)
            {
                abilityUI[i].StartCooldown(globalCooldown);
            }
        }
    }

    private void ActivateCard(int index)
    {
        currentDeck.Activate(index);

        StartCoroutine(AbilityCooldown(currentDeck.hand[index].cooldown));
        StartCoroutine(ActionCooldown(globalCooldown));

        soulManager.ChargeSoulTransform(currentDeck.hand[index].element, currentDeck.hand[index].transformChargeAmount);

        playerAnimator.SetBool("Cast", true);

        for (int i = 0; i < abilityUI.Length; i++)
        {
            abilityUI[i].StartCooldown(currentDeck.hand[index].cooldown);

            if (readyToCastMantra)
            {
                mantraUI[i].StartCooldown(globalCooldown);
            }
        }
    }

    void UpdateGUI()
    {
        SetCardGraphics();
    }

    void SetCardGraphics()
    {
        for (int i = 0; i < abilityUI.Length; i++)
        {
            if (currentDeck.hand[i] != null) //the slot in the hand may not have been refilled if the cooldown is not finished
            {
                abilityUI[i].SetName(currentDeck.hand[i].actionName);
                abilityUI[i].SetArt(currentDeck.hand[i].art);
                abilityUI[i].SetElement(currentDeck.hand[i].element);
            }

            //update the backup hand slot
            if (currentDeck.backupHand[i] != null)
            {
                abilityUI[i].SetBackupName(currentDeck.backupHand[i].actionName);
            }
            else
            {
                abilityUI[i].SetBackupName(null);
            }
        }
    }

    private void SetMantraGraphics()
    {
        for (int i = 0; i < mantraUI.Length; i++)
        {
            if(currentMantras.activeMantras[i] != null)
            {
                mantraUI[i].SetName(currentMantras.activeMantras[i].actionName);
                mantraUI[i].SetArt(currentMantras.activeMantras[i].art);
                mantraUI[i].SetElement(currentMantras.activeMantras[i].element);
            }
            else
            {
                Debug.Log("No Active Mantra UI set!");
            }
        }
    }

    private IEnumerator ActionCooldown(float cooldown)
    {
        if (cooldown != 0)
        {
            readyToCastAction = false;
            yield return new WaitForSeconds(cooldown);
            readyToCastAction = true;
        }
    }

    private IEnumerator AbilityCooldown(float cooldown)
	{
        if(cooldown != 0)
        {
            readyToCastAbility = false;
            yield return new WaitForSeconds(cooldown);
            readyToCastAbility = true;
        }
	}

    private IEnumerator MantraCooldown(float cooldown)
    {
        if (cooldown != 0)
        {
            readyToCastMantra = false;
            yield return new WaitForSeconds(cooldown);
            readyToCastMantra = true;
        }
    }

    public void DisableBasicActions(bool actionsAreDisabled)
    {
        if (actionsAreDisabled)
        {
            deckIsEnabled = false;
            handPanel.SetActive(false);
        }
        else
        {
            deckIsEnabled = true;
            handPanel.SetActive(true);
        }
    }
}
