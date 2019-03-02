﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;

public class RegionManager : MonoBehaviour
{
    RegionState currentRegion;

    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject nodeConnectionPrefab;
    [SerializeField]
    private GameObject inventoryButton;

    private List<Button> buttons;

    void Start()
    {
        currentRegion = SaveManager.currentGame.GetRegion();

        GenerateButtons();
    }

    void Update()
    {
        CheckForInventoryInput();
    }

    public void GoToEncounter(EncounterState encounter)
    {
        SaveManager.currentGame.SetCurrentEncounterIndex(index);
        scr_SceneManager.globalSceneManager.ChangeScene(encounter.sceneName);
    }

    public void GenerateButtons()
    {
        buttons = new List<Button>();

        GameObject encounterCanvas = GameObject.FindWithTag("EncounterCanvas");

        foreach(EncounterState encounterState in currentRegion.encounters)
        {   
            GameObject newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(encounterCanvas.GetComponent<RectTransform>());

            EncounterData newEncounterData;
            newEncounterData = encounterState.GetEncounterData();

            EncounterButtonManager encounterButtonManager = newButton.GetComponent<EncounterButtonManager>();
            encounterButtonManager.SetStateAndEncounter(encounterState, newEncounterData);

            Button button = newButton.GetComponent<Button>();
            button.onClick.AddListener(delegate {GoToEncounter(newEncounterData, currentRegion.encounters.IndexOf(encounterState));});

            buttons.Add(newButton.GetComponent<Button>());
        }

        GameObject eventSystem = GameObject.Find("/EventSystem");
        eventSystem.GetComponent<EventSystem>().firstSelectedGameObject = buttons[0].gameObject;
    }

    public void CheckForInventoryInput()
    {
        if(Input.GetButtonDown("PlayCard2_Button") || Input.GetKeyDown("i"))
        {
            inventoryButton.GetComponent<Button>().onClick.Invoke();
        }
    }
}