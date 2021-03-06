﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Loadout")]
public class Loadout : ScriptableObject 
{
    public List<Equipment> equipment = new List<Equipment>();
    public Dictionary<int, Gun> guns = new Dictionary<int, Gun>();
    public Projectile[] projectiles;
}
