using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    private bool _inv;
    public bool Invulnerable
    {
        get
        {
            return _inv;
        }

        set
        {
            if (InvulnerableToggled != null) InvulnerableToggled(this, value);
            _inv = value;
        }
    }
    public HealthStats stats;

    public float health = 100;
    public float shield = 100;
    public float armor;

    public event EventHandler TookDamage;
    public event EventHandler HealthDepleted;
    public event HealthEventHandler InvulnerableToggled;

    public delegate void EventHandler();
    public delegate void HealthEventHandler(Health sender, bool invulnerable);

    private void Awake() 
    {
        Init();
    }

    public void Init()
    {
        stats = Utilities.CheckScriptableObject<HealthStats>(stats);

        health = stats.startingHealth;
        shield = stats.startingShield;
        armor = stats.startingArmor;
    } 

    private void OnTookDamage(WeaponStats weapon)
    {
        if (TookDamage != null) TookDamage();
    }

    private void OnHealthDepleted()
    {
        if (HealthDepleted != null) HealthDepleted();
    }

    public virtual void TakeDamage(WeaponStats weapon)
    {
        if (_inv) return;

        if (shield >= stats.minShield)
        {
            shield -= weapon.shieldDamage;
        }

        else if (armor >= stats.minArmor)
        {
            // Do some more calculations here
            armor -= weapon.hullDamage;
        }

        else if (health >= stats.minHealth)
        {
            health -= weapon.hullDamage;
        }

        OnTookDamage(weapon);

        if (health <= stats.minHealth) OnHealthDepleted();
    }
}