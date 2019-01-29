﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Stats")]
    public PilotDetails pilotDetails;
    public EngineStats engineStats;
    public ShipDetails shipDetails;
    public ShipPhysicsStats physicsStats;

    // TODO: Make some of these into properties
    [Header("Ship Components")]
    public Health health;
    public HardpointSystem hpSys;
    public Vector3 aimPosition;
    public Engine engine;
    public CruiseEngine cruiseEngine;
    public Rigidbody rb;
    public List<Collider> colliders;
    public TargetingInfo targetInfo;

    private ShipBase _shipBase;
    public ShipBase ShipBase { get { return _shipBase; } }

    public ParticleSystem deathFX;

    [Header("Other")]
    private bool _possessed;
    public bool Possessed { get { return _possessed; } }
    public Transform cameraPosition;
    public Transform firstPersonCameraPosition;

    public delegate void PossessionEventHandler(PlayerController pc, Ship sender, bool possessed);
    public delegate void ShipEventHandler(Ship sender);

    public static event ShipEventHandler Spawned;
    public event ShipEventHandler Died;

    public event PossessionEventHandler Possession;

    protected void OnPossession(PlayerController pc, bool possessed) { if (Possession != null) Possession(pc, this, possessed); }

    public void Init(ShipBase shipBase)
    {
        if (_shipBase) DestroyImmediate(_shipBase.gameObject);

        _shipBase = Instantiate(shipBase, transform);
        _shipBase.transform.localPosition = Vector3.zero;

        GetComponentsInChildren<Collider>(true, colliders);
        foreach (Collider coll in colliders)
        {
            var newLayer = _possessed ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("Default");
            coll.gameObject.layer = newLayer;
            coll.tag = _possessed ? "Player" : "Untagged";
        }
    }

    void Awake()
    {
        targetInfo = Utilities.CheckComponent<TargetingInfo>(gameObject);
        var header = shipDetails.shipName + " - PILOTNAMEHERE";
        targetInfo.Init(header, health);
    }

    void Start()
    {
        engine.DriftingChange += HandleDriftingChange;
        engine.ThrottleChanged += HandleThrottleChange;
        cruiseEngine.CruiseStateChanged += HandleCruiseChange;
        health.HealthDepleted += HandleHealthDepleted;

        if (ShipBase == null) Init(GameSettings.Instance.defaultShipBase);

        var components = GetComponentsInChildren<ShipComponent>();
        components.ToList().ForEach(x => x.Initialize(this));

        name = ShipBase.name;

        if (Spawned != null) Spawned(this);
    }

    private void HandleCruiseChange(CruiseEngine sender, CruiseState newState)
    {
        if (newState == CruiseState.Charging) 
        {
            engine.Drifting = false;
        }
    }

    private void HandleThrottleChange(Engine sender, ThrottleChangeEventArgs e)
    {
        if (e.IsAccelerating == false)
        {
            cruiseEngine.StopAnyCruise();
        }
    }

    private void HandleDriftingChange(bool drifting)
    {
        if (cruiseEngine != null && drifting)
        {
            cruiseEngine.StopAnyCruise();
        }

        ShipPhysicsStats.HandleDrifting(rb, physicsStats, drifting);
    }

    private void HandleHealthDepleted()
    {
        Die();
    }

    public void SetPossessed(PlayerController pc, bool possessed)
    {
        // Add the random pilot's name
        name = possessed ? "PLAYER SHIP" : "NPC SHIP"; 
        tag = possessed ? "Player" : "Ship";

        if (possessed)
        {
            transform.SetSiblingIndex(0);
        }

        foreach (Collider coll in colliders)
        {
            var newLayer = possessed ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("Default");
            coll.gameObject.layer = newLayer;
            coll.tag = possessed ? "Player" : "Untagged";
        }

        targetInfo.targetable = !possessed;

        _possessed = possessed;
        OnPossession(pc, possessed);
    }

    public void Die()
    {
        // EVENT CALL HERE (ON DYING)

        if (deathFX != null)
        {
            Instantiate(deathFX, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Dying ship has no deathFX...");
        }

        if (Died != null) Died(this);

        Destroy(gameObject);
        // EVENT CALL ALSO? (ON DEATH)
    }
}
