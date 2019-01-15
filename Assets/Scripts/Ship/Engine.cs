﻿using UnityEngine;
using System.Collections;
using System;

public class Engine : ShipComponent 
{
    [Header("Main")]
    public Rigidbody rb;
    public new Transform transform;

    [Header("Model")]
    public float shipModelZModifier = 1;
    public Transform shipModel;
    public float lerpModifier = .1f;
    private Quaternion shipModelOrigRot;

    public float Speed
    {
        get
        {
            //return Vector3.Dot(rb.velocity, transform.forward);
            return Mathf.Abs(rb.velocity.z);
        }
    }

    [Header("Stats")]
    public EngineStats engineStats;

    private Cooldown sidestepCD;
    private Cooldown blinkCD;

    public float sidestepForce = 50000;
    public float sidestepDur = .2f;
    public float maxZVelocity = 10;
    public float blinkDistance = 20;

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

            if (value != 0) Drifting = false;

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
            if (drifting == value) return;

            if (value) 
            {
                Strafe = 0;
                Throttle = 0;
            }

            drifting = value;
            OnDriftingChange(value);
        }
    }

    public bool clampVelocity = true;
    private IEnumerator sidestepCR;

    public delegate void ThrottleChangedEventHandler(Engine sender, ThrottleChangeEventArgs e);
    public event ThrottleChangedEventHandler ThrottleChanged;

    public delegate void StrafeChangedEventHandler(Engine sender, float newStrafe, float oldStrafe);
    public event StrafeChangedEventHandler StrafeChanged;

    public event EventHandler DriftingChange;
    public delegate void EventHandler(bool drifting);

    public void SidestepRight()
    {
        if (sidestepCR != null || sidestepCD != null) return;

        sidestepCR = SidestepRoutine(new Vector3(sidestepForce, 0, 0), ForceMode.Force);
        StartCoroutine(sidestepCR);
    }

    public void SidestepLeft()
    {
        if (sidestepCR != null || sidestepCD != null) return;

        sidestepCR = SidestepRoutine(new Vector3(-sidestepForce, 0, 0), ForceMode.Force);
        StartCoroutine(sidestepCR);
    }

    public void Blink()
    {
        if (blinkCD) return;

        //var forwardsInt = forwards ? 1 : -1;
        var forwardsInt = 1;
        transform.position = transform.position + transform.forward * (blinkDistance * forwardsInt);
        blinkCD = Cooldown.Instantiate(this, 3);
    }

    private IEnumerator SidestepRoutine(Vector3 baseForce, ForceMode mode)
    {
        float elapsed = 0;

        while (elapsed < sidestepDur)
        {
            rb.AddRelativeForce(baseForce * (elapsed / sidestepDur), mode);
            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        sidestepCD = Cooldown.Instantiate(this, 3);
        sidestepCR = null;
    }

    public void SidestepHorizontal(bool usePositiveThrust)
    {
        if (sidestepCR != null || sidestepCD != null) return;
        var thrustBool = usePositiveThrust ? 1 : -1;

        sidestepCR = SidestepRoutine(new Vector3(sidestepForce * thrustBool, 0, 0), ForceMode.Force);
        StartCoroutine(sidestepCR);
    }

    private void Awake()
    {
        if (engineStats == null)
            engineStats = Instantiate(ScriptableObject.CreateInstance<EngineStats>());
        
        shipModelOrigRot = shipModel.localRotation;
    }

    private void OnDriftingChange(bool isDrifting)
    {
        if (DriftingChange != null) DriftingChange(isDrifting);
    }

    private void OnStrafeChange(float newStrafe, float oldStrafe)
    {
        if (StrafeChanged != null) StrafeChanged(this, newStrafe, oldStrafe);
    }

    private void OnThrottleChange(float newThrottle, float oldThrottle)
    {
        if (ThrottleChanged != null) ThrottleChanged(this, new ThrottleChangeEventArgs(newThrottle, oldThrottle));
    }

    private void HandleCruiseChange(CruiseEngine sender)
    {
        if (sender.State != CruiseState.Off)
            Drifting = false;
    }

    private void FixedUpdate()
    {
        if (!Drifting)
        {
            var forces = CalcStrafeForces() + CalcThrottleForces();
            rb.AddForce(forces);
        } 
    }

    private Vector3 GetClampedVelocity()
    {
        var newZ = Mathf.Clamp(rb.velocity.z, -maxZVelocity, maxZVelocity); 
        return new Vector3(rb.velocity.x, rb.velocity.y, newZ);
    }

    private Vector3 CalcStrafeForces()
    {
        return rb.transform.right * Strafe * engineStats.strafePower;
    }

    private Vector3 CalcThrottleForces()
    {
        return rb.transform.forward * Throttle * engineStats.enginePower;
    }

    public void ThrottleUp()
    {
        Drifting = false;
        if (Throttle == 1) return;
        Throttle = Mathf.MoveTowards(Throttle, 1, throttleChangeIncrement);
    }

    public void ThrottleDown()
    {
        Drifting = false;
        Throttle = Mathf.MoveTowards(Throttle, 0, throttleChangeIncrement);
    }

    public void ToggleDrifting()
    {
        Drifting = !Drifting;
    }

    public void LerpYawToNeutral()
    {
        Quaternion neutralRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        neutralRotation = Quaternion.Lerp(transform.rotation, neutralRotation, Time.deltaTime);
        transform.rotation = neutralRotation;

        neutralRotation = Quaternion.Lerp(shipModel.localRotation, shipModelOrigRot, lerpModifier);
        shipModel.localRotation = neutralRotation;
    }

    public void Pitch(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        transform.Rotate(Vector3.left * amount);
    }

    public void Yaw(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        transform.Rotate(Vector3.up * amount);
        VisualYawRotation(amount);
    }

    public void Roll(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        transform.Rotate(Vector3.forward * amount);
    }

    private void VisualYawRotation(float amount)
    {
        if (shipModel == null) return;

        // Rotate the model slightly based on yawOffset
        Vector3 turnRotation = shipModelOrigRot.eulerAngles + new Vector3(0, 0, -amount * shipModelZModifier);
        shipModel.localRotation = Quaternion.Euler(turnRotation);
    }
}

public class ThrottleChangeEventArgs
{
    public float newThrottle;
    public float oldThrottle;

    public ThrottleChangeEventArgs(float newThrottle, float oldThrottle)
    {
        this.newThrottle = newThrottle;
        this.oldThrottle = oldThrottle;
    }

    public bool IsAccelerating
    {
        get
        {
            return newThrottle > oldThrottle;
        }
    }
}
