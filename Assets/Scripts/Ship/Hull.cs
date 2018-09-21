﻿using UnityEngine;
using System.Collections;
using System;

public class Hull : MonoBehaviour, IDamageable
{
    public bool invulnerable = false;
    public HullType hullType = HullType.Light;

    public event EventHandler<DamageEventArgs> TookDamage;
    public event EventHandler<DeathEventArgs> HealthDepleted;

    [SerializeField] private float _health; 
    public float Health
    {
        get
        {
            return _health;
        }

        set
        {
            _health = value;
        }
    }

    [SerializeField] private float _maxHealth;
    public float MaxHealth
    {
        get
        {
            return _maxHealth;
        }

        set
        {
            _maxHealth = value;
        }
    }

    protected void OnHealthDepleted(Weapon weapon)
    {
        if (HealthDepleted != null) HealthDepleted(this, new DeathEventArgs(weapon, gameObject.transform.position));
    }

    protected void OnTookDamage(Weapon weapon)
    {
        if (TookDamage != null) TookDamage(this, new DamageEventArgs(weapon));
    }

    public void TakeDamage(Weapon weapon)
    {
        if (invulnerable) return;

        Health -= weapon.hullDamage;

        if (Health <= 0)
        {
            OnHealthDepleted(weapon);
        }

        OnTookDamage(weapon);
    }
}
