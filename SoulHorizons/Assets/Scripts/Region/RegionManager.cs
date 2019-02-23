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
    [SerializeField]
    private RegionState currentRegion;
    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject nodeConnectionPrefab;

    private List<Button> buttons;
    private GameObject encounterMap;

    void Start()
    {
        currentRegion = SaveManager.currentGame.GetRegion();
        encounterMap = new GameObject("Map");

        GenerateButtons();
        CreateNodeConnections();

        EventSystem eventSystem = EventSystem.current;
    }

    public void GoToEncounter(EncounterState encounter)
    {
        if(encounter.isAccessible)
        {
            SaveManager.currentGame.SetCurrentEncounterState(encounter);
            scr_SceneManager.globalSceneManager.ChangeScene(encounter.GetEncounterData().sceneName);
        }
    }

    public void GenerateButtons()
    {
        buttons = new List<Button>();

        for(int i = 0; i < currentRegion.map.rings.Count; i++)
        {
            foreach(Node node in currentRegion.map.rings[i])
            {
                GameObject newButton = Instantiate(
                    buttonPrefab, 
                    node.position,
                    Quaternion.identity,
                    encounterMap.transform);

                EncounterButtonManager encounterButtonManager = newButton.GetComponent<EncounterButtonManager>();
                encounterButtonManager.SetEncounterState(node.GetEncounterState());

                Button button = newButton.GetComponent<Button>();
                button.onClick.AddListener(delegate {GoToEncounter(node.encounter);});

                buttons.Add(newButton.GetComponent<Button>());
            }
        }

        GameObject eventSystem = GameObject.Find("/EventSystem");
        eventSystem.GetComponent<EventSystem>().firstSelectedGameObject = buttons[0].gameObject;      
    }

    private void CreateNodeConnections()
    {
        for(int i = 0; i < currentRegion.map.rings.Count; i++)
        {
            foreach(Node node in currentRegion.map.rings[i])
            {
                foreach(Node connectedNode in node.nextNodes)
                {
                    GameObject newConnection = Instantiate(
                        nodeConnectionPrefab, 
                        node.position,
                        Quaternion.identity,
                        encounterMap.transform);

                    Vector3[] points = new Vector3[2];
                    points[0] = node.position;
                    points[1] = connectedNode.position;

                    LineRenderer lr = newConnection.GetComponent<LineRenderer>();
                    lr.positionCount = points.Length;
                    lr.SetPositions(points);
                }
            }
        }
    }
}