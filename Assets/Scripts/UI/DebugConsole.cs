﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour
{
    public PlayerController playerController;
    public ShipSpawner shipSpawner;
    public VerticalLayoutGroup verticalLayoutGroup;
    public GameObject content;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        shipSpawner = FindObjectOfType<ShipSpawner>();
    }

    private void Start()
    {
        content.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            content.SetActive(!content.activeSelf);
        }
    }

    public void GodMode()
    {
        playerController.controlledShip.invulnerable = !playerController.controlledShip.invulnerable;
    }

    public void SpawnDefaultShip()
    {
        shipSpawner.SpawnDefaultShip();
    }

    public void NoEnergyDrain()
    {
        playerController.controlledShip.hardpointSystem.EnableInfiniteEnergy();
    }

    public void PopulateMethodList()
    {

    }
}
