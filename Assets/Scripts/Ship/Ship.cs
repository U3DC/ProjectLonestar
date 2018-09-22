﻿using CommandTerminal;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour, ITargetable
{
    [Header("Stats")]
    public PilotDetails pilotDetails;
    public EngineStats engineStats;
    public ShipDetails shipDetails;
    //public ShipStats stats;

    [Header("Ship Components")]
    public HardpointSystem hardpointSystem;
    public Vector3 aimPosition;
    public Engine engine;
    public CruiseEngine cruiseEngine;
    public ShipCamera shipCam;
    public Rigidbody rb;
    public Hull hull;

    public delegate void PossessionEventHandler(PlayerController pc, Ship sender, bool possessed);
    public event PossessionEventHandler Possession;
    public event TargetEventHandler BecameTargetable;
    public event TargetEventHandler BecameUntargetable;

    private void Awake()
    {
        hardpointSystem = GetComponentInChildren<HardpointSystem>();
        cruiseEngine = GetComponentInChildren<CruiseEngine>();
        shipCam = GetComponentInChildren<ShipCamera>();
        engine = GetComponentInChildren<Engine>();
        rb = GetComponentInChildren<Rigidbody>();
        hull = GetComponentInChildren<Hull>();

        hull.HealthDepleted += HandleHullHealthDepleted;
    }

    private void HandleHullHealthDepleted(object sender, DeathEventArgs e)
    {
        if (BecameUntargetable != null) BecameUntargetable(this);
        Destroy(gameObject);
    }

    public void SetPossessed(PlayerController pc, bool possessed)
    {
        name = possessed ? "PLAYER SHIP" : pilotDetails.FullName;

        tag = possessed ? "Player" : "Untagged";

        foreach(Transform transform in transform)
        {
            var newLayer = possessed ? LayerMask.NameToLayer("Player") : 0;

            transform.gameObject.layer = newLayer;
        }

        // Enable Ship Camera
        pc.MouseStateChanged += HandleMouseStateChanged;
        HandleMouseStateChanged(pc.MouseState);
        shipCam.enabled = true;

        OnPossession(pc, possessed);
    }

    private void HandleMouseStateChanged(MouseState state)
    {
        if (shipCam == null) return;

        switch (state)
        {
            case MouseState.Off:
                shipCam.isFollowingShip = false;
                break;

            case MouseState.Toggled:
            case MouseState.Held:
                shipCam.isFollowingShip = true;
                break;
        }
    }

    protected void OnPossession(PlayerController pc, bool possessed)
    {
        if (Possession != null) Possession(pc, this, possessed);
    }

    private void FixedUpdate()
    {
        ClampSpeeds();
    }

    public void FireAllWeapons()
    {
        // change
        hardpointSystem.FireActiveWeapons(ShipCamera.GetMousePositionInWorld(shipCam.camera));
    }

    public void SetupTargetIndicator(TargetIndicator indicator)
    {
        //hull.TookDamage += indicator.HandleTargetTookDamage;
    }

    public bool IsTargetable()
    {
        return true;
    }

    private void ClampSpeeds()
    {
        if (cruiseEngine == null) return;

        float currentMaxSpeed = 0;

        switch (cruiseEngine.State)
        {
            case CruiseState.Off:
                currentMaxSpeed = engineStats.maxAfterburnSpeed;
                break;

            case CruiseState.Charging:
                currentMaxSpeed = engineStats.maxNormalSpeed;
                break;

            case CruiseState.On:
                currentMaxSpeed = engineStats.maxCruiseSpeed;
                break;

            case CruiseState.Disrupted:
                currentMaxSpeed = engineStats.maxAfterburnSpeed;
                break;

            default:
                break;
        }

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, currentMaxSpeed);
    }
}