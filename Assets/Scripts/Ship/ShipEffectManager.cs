﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipEffectManager : ShipComponent
{
    public ParticleSystem[] engineEffects;
    public TrailRenderer[] trailEffects;
    public ParticleSystem chargeCruiseEffect;
    public ParticleSystem fullCruiseEffect;
    public ParticleSystem dustEffect;

    public float currentScale = 1;

    private Dictionary<ParticleSystem, Vector3> psDict = new Dictionary<ParticleSystem, Vector3>();
    private Vector3 originalScale;

    public override void Initialize(Ship sender)
    {
        base.Initialize(sender);
        sender.cruiseEngine.CruiseStateChanged += HandleCruiseChanged;
        sender.Possession += HandleOwnerPossession;
        sender.engine.DriftingChange += HandleDrifting;

        foreach (ParticleSystem ps in engineEffects)
        {
            psDict.Add(ps, ps.transform.localScale);
        }
    }

    private void FixedUpdate()
    {
        foreach (ParticleSystem ps in psDict.Keys)
        {
            Vector3 val;
            if (psDict.TryGetValue(ps, out val))
            {
                ps.transform.localScale = val * ship.engine.Throttle;
            }
        }
    }

    private void HandleDrifting(bool drifting)
    {
        foreach (var trail in trailEffects)
        {
            trail.enabled = !drifting;
        }
    }

    private void HandleOwnerPossession(PlayerController pc, Ship sender, bool possessed)
    {
        if (possessed)
        {
            dustEffect.Play();
        }

        else
        {
            dustEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void HandleCruiseChanged(CruiseEngine sender, CruiseState newState)
    {
        switch (sender.State)
        {
            case CruiseState.Charging:
                chargeCruiseEffect.Play();
                break;

            case CruiseState.On:
                chargeCruiseEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                fullCruiseEffect.Play();
                break;

            case CruiseState.Off:
            case CruiseState.Disrupted:
                chargeCruiseEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                fullCruiseEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                break;

            default:
                break;
        }
    }
}
