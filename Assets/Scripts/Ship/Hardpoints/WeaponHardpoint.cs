﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class WeaponHardpoint : Hardpoint
{
    public AudioSource audioSource;
    public Vector3 aimPosition;

    public Projectile projectilePrefab;

    private bool _active = true;
    public bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            _active = value;
            if (value)
            {
                OnActivated();
            }
            else
            {
                OnDeactivated();
            }
        }
    }

    public event EventHandler<EventArgs> Fired;
    public event EventHandler<EventArgs> Activated;
    public event EventHandler<EventArgs> Deactivated;

    private void OnFired()
    {
        if (Fired != null)
            Fired(this, EventArgs.Empty);
    }

    private void OnActivated()
    {
        if (Activated != null)
            Activated(this, EventArgs.Empty);
    }

    private void OnDeactivated()
    {
        if (Deactivated != null)
            Deactivated(this, EventArgs.Empty);
    }

    public void Toggle()
    {
        Active = !Active;
    }

    public bool Fire(Vector3 target, Collider[] collidersToIgnore = null)
    {
        if (projectilePrefab == null || IsOnCooldown) return false;

        var newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        newProjectile.Initialize(target, collidersToIgnore);

        //audioSource.PlayOneShot(weapon.clip);

        StartCooldown(projectilePrefab.weaponStats.cooldownDuration);

        return true;
    }
}
