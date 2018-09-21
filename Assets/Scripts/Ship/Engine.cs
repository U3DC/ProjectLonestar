﻿using UnityEngine;
using System.Collections;
using System;

public class Engine : ShipComponent 
{
    public Rigidbody rb;

    public float Speed
    {
        get
        {
            return Vector3.Dot(rb.velocity, transform.forward);
        }
    }

    public float turnSpeed = 1;
    public float throttlePower = 1;
    public float strafePower = 1;
    public float mass = 5;
    public float drag = 5;
    public float driftMass = 5;
    public float driftDrag = .05f;

    public EngineStats engineStats;

    private float throttle;
    public float Throttle
    {
        get
        {
            return throttle;
        }

        set
        {
            if (value == throttle) return;
            var oldThrottle = throttle;
            throttle = Mathf.Clamp(value, 0, 1);
            OnThrottleChange(value, oldThrottle);
        }
    }

    private float strafe;
    public float Strafe
    {
        get
        {
            return strafe;
        }

        set
        {
            if (value == strafe) return;
            var oldStrafe = strafe;
            strafe = Mathf.Clamp(value, -1, 1);
            OnStrafeChange(value, oldStrafe);
        }
    }
    public float throttleChangeIncrement = .1f;

    public bool IsStrafing { get { return Strafe != 0; } }

    private bool drifting;
    public bool Drifting
    {
        get
        {
            return drifting;
        }

        set
        {
            if (value == drifting) return;
            drifting = value;
            OnDriftingChange(value);
        }
    }

    public delegate void ThrottleChangedEventHandler(Engine sender, float newThrottle, float oldThrottle);
    public event ThrottleChangedEventHandler ThrottleChanged;

    public delegate void StrafeChangedEventHandler(Engine sender, float newStrafe, float oldStrafe);
    public event StrafeChangedEventHandler StrafeChanged;

    public event EventHandler DriftingChange;
    public delegate void EventHandler(bool drifting);

    private void Awake()
    {
        if (engineStats == null)
            engineStats = Instantiate(ScriptableObject.CreateInstance<EngineStats>());
    }

    private void OnDriftingChange(bool isDrifting)
    {
        if (isDrifting)
        {
            rb.drag = driftDrag;
            rb.mass = driftMass;
        }

        else
        {
            rb.drag = drag;
            rb.mass = mass;
        }

        if (DriftingChange != null) DriftingChange(isDrifting);
    }

    private void OnStrafeChange(float newStrafe, float oldStrafe)
    {
        if (StrafeChanged != null) StrafeChanged(this, newStrafe, oldStrafe);
    }

    private void OnThrottleChange(float newThrottle, float oldThrottle)
    {
        Drifting = false;
        if (ThrottleChanged != null) ThrottleChanged(this, newThrottle, oldThrottle);
    }

    private void HandleCruiseChange(CruiseEngine sender)
    {
        if (sender.State != CruiseState.Off)
        {
            Drifting = false;
        }
    }

    private void FixedUpdate()
    {
        ApplyStrafeForces();
        ApplyThrottleForces();
    }

    private void ApplyStrafeForces()
    {
        if (Strafe != 0)
        {
            rb.AddForce(rb.transform.right * Strafe * strafePower);
        }
    }

    private void ApplyThrottleForces()
    {
        if (Throttle > 0 && Drifting == false)
        {
            rb.AddForce(rb.transform.forward * Throttle * throttlePower);
        }
    }

    public void ThrottleUp()
    {
        Drifting = false;
        Throttle = Mathf.MoveTowards(Throttle, 1, throttleChangeIncrement);
    }

    public void ThrottleDown()
    {
        Drifting = false;
        Throttle = Mathf.MoveTowards(Throttle, 0, throttleChangeIncrement);
    }

    public void LerpYawToNeutral()
    {
        Quaternion neutralRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        neutralRotation = Quaternion.Lerp(transform.rotation, neutralRotation, Time.deltaTime);
        transform.rotation = neutralRotation;
    }

    public void Pitch(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        transform.Rotate(new Vector3(turnSpeed * -amount, 0, 0));
    }

    public void Yaw(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        transform.Rotate(new Vector3(0, turnSpeed * amount, 0));
    }

    public void Roll(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        transform.Rotate(new Vector3(0, 0, turnSpeed * amount));
    }
}
