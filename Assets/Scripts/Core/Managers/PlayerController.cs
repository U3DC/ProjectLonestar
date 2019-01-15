﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(Camera), typeof(AudioListener), typeof(ShipCamera))]
public class PlayerController : MonoBehaviour
{
    [Header("--- Input ---")]
    public bool canPause = true;
    public bool inputAllowed = true;
    public float doubleTapDuration = 1;

    private MouseState mouseState;
    public MouseState MouseState
    {
        get
        {
            return mouseState;
        }

        set
        {
            mouseState = value;
            UpdateShipCameraState();
            if (MouseStateChanged != null) MouseStateChanged(MouseState);
        }
    }



    // The amount of time that the controller waits until it 
    // determines the player is trying to switch to manual mouse flight
    public float mouseHoldDelay = .1f;

    public Ship ship;
    public Ship lastShip;
    public ShipCamera shipCamera;
    public Flycam flycam;
    public Camera cam;
    public PostProcessLayer ppl;
    public AudioListener listener;

    public delegate void PossessionEventHandler(PlayerController sender, PossessionEventArgs args);
    public event PossessionEventHandler PossessedNewShip;
    public event PossessionEventHandler ReleasedShip;

    public delegate void MouseStateEventHandler(MouseState state);
    public event MouseStateEventHandler MouseStateChanged;

    protected void Awake()
    {
        if (FindObjectsOfType<PlayerController>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        cam = Utilities.CheckComponent<Camera>(gameObject);
        shipCamera = Utilities.CheckComponent<ShipCamera>(gameObject);
        flycam = Utilities.CheckComponent<Flycam>(gameObject);
        listener = Utilities.CheckComponent<AudioListener>(gameObject);

        // cam = GetComponent<Camera>();
        // shipCamera = GetComponent<ShipCamera>();
        // flycam = CheckComponent<Flycam>();
        // listener = CheckComponent<AudioListener>();
        //ppl = CheckComponent<PostProcessLayer>();
    }

    // private T CheckComponent<T>() where T : Component
    // {
    //     var obj = GetComponent<T>();

    //     if (obj == null)
    //     {
    //         obj = gameObject.AddComponent<T>();
    //     }
        
    //     return obj;
    // }

    protected virtual void OnPossessedNewShip(PossessionEventArgs args)
    {
        if (PossessedNewShip != null) PossessedNewShip(this, args);
    }

    protected virtual void OnReleasedShip(Ship releasedShip)
    {
        if (ReleasedShip != null) ReleasedShip(this, new PossessionEventArgs(null, releasedShip));
    }

    public void Possess(Ship newShip)
    {
        if (newShip == null)
        {
            Release();
            return;
        }

        var oldShip = ship;

        if (oldShip != null)
        {
            oldShip.SetPossessed(this, false);
        }

        newShip.SetPossessed(this, true);

        foreach (var coll in newShip.GetComponentsInChildren<Collider>())
        {
            coll.tag = "Player";                    
        }

        flycam.enabled = false;
        shipCamera.SetTarget(newShip.cameraPosition);

        newShip.GetComponent<StateController>().currentState = null;
        newShip.Died += Release;

        ship = newShip;
        lastShip = null;
        enabled = true;
        OnPossessedNewShip(new PossessionEventArgs(ship, null));
    }

    public void Release()
    {
        ship.SetPossessed(this, false);
        shipCamera.ClearTarget();

        foreach (var coll in ship.colliders)
        {
            coll.tag = "Untagged";
        }

        ship.Died -= Release;

        MouseState = MouseState.Off;
        flycam.enabled = true;
        enabled = false;

        lastShip = ship;
        ship = null;
        OnReleasedShip(ship);
    }

    public void Repossess()
    {
        if (lastShip != null)
        {
            Possess(lastShip);
        }
    }

    private void Update()
    {
        Vector3 target = ShipCamera.GetMousePositionInWorld(cam);

        if(Input.GetKeyDown(InputManager.PauseGameKey) && canPause)
        {
            GameStateUtils.TogglePause();
        }

        if(!inputAllowed || ship == null) return;

        #region movement
        if(Input.GetKey(InputManager.ThrottleUpKey) || Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            ship.engine.ThrottleUp();
        }

        else if(Input.GetKeyUp(InputManager.ThrottleUpKey))
        {
            Action action = ship.engine.Blink;
            StartCoroutine(DoubleTap(InputManager.ThrottleUpKey, action));
        }

        if(Input.GetKey(InputManager.ThrottleDownKey) || Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            ship.engine.ThrottleDown();
        }

        if(Input.GetKey(InputManager.StrafeLeftKey))
        {
            ship.engine.Strafe = -1; 
        }
        else if (Input.GetKeyUp(InputManager.StrafeLeftKey))
        {
            Action action = ship.engine.SidestepLeft;
            StartCoroutine(DoubleTap(InputManager.StrafeLeftKey, action));
        }

        if(Input.GetKey(InputManager.StrafeRightKey))
        {
            ship.engine.Strafe = 1; 
        }
        else if (Input.GetKeyUp(InputManager.StrafeRightKey))
        {
            Action action = ship.engine.SidestepRight;
            StartCoroutine(DoubleTap(InputManager.StrafeRightKey, action));
        }

        // If neither strafe key is pressed, reset the ship's strafing
        if(!Input.GetKey(InputManager.StrafeRightKey) && !Input.GetKey(InputManager.StrafeLeftKey))
        {
            ship.engine.Strafe = 0; 
        }         

        if(Input.GetKeyDown(InputManager.ToggleMouseFlightKey))
        {
            ToggleMouseFlight();
        }

        if(Input.GetKeyDown(InputManager.AfterburnerKey))
        {
            ship.hardpointSystem.ToggleAfterburner(true);
        }
        
        else if(Input.GetKeyUp(InputManager.AfterburnerKey))
        {
            ship.hardpointSystem.ToggleAfterburner(false);
        }

        if(Input.GetKeyDown(InputManager.ManualMouseFlightKey))
        {
            if(MouseState == MouseState.Off) StartCoroutine("ManualMouseFlightCoroutine");
        }

        if(Input.GetKeyUp(InputManager.ManualMouseFlightKey))
        {
            StopAllCoroutines();

            if (MouseState == MouseState.Held) MouseState = MouseState.Off;
        }

        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(InputManager.ThrottleUpKey))
        {
            ship.cruiseEngine.ToggleCruiseEngines();
        }

        if(Input.GetKeyDown(InputManager.KillEnginesKey))
        {
            ship.engine.Drifting = !ship.engine.Drifting;
        }

        #endregion

        #region hardpoints
        if(Input.GetKey(InputManager.Hardpoint1Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(1, target);
        }

        if(Input.GetKey(InputManager.Hardpoint2Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(2, target);
        }

        if(Input.GetKey(InputManager.Hardpoint3Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(3, target);
        }

        if(Input.GetKey(InputManager.Hardpoint4Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(4, target);
        }

        if(Input.GetKey(InputManager.Hardpoint5Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(5, target);
        }

        if(Input.GetKey(InputManager.Hardpoint6Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(6, target);
        }

        if(Input.GetKey(InputManager.Hardpoint7Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(7, target);
        }

        if(Input.GetKey(InputManager.Hardpoint8Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(8, target);
        }

        if(Input.GetKey(InputManager.Hardpoint9Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(9, target);
        }

        if(Input.GetKey(InputManager.Hardpoint10Key))
        {
            ship.hardpointSystem.FireWeaponHardpoint(10, target);
        }

        if(Input.GetKey(InputManager.FireKey))
        {
            ship.hardpointSystem.FireActiveWeapons(target);
        }

        if (Input.GetKeyDown(InputManager.LootAllKey))
        {
            ship.hardpointSystem.tractorBeam.TractorAllLoot();
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (ship == null || ship.engine == null) return;

        var engine = ship.engine;

        switch (mouseState)
        {
            case MouseState.Off:
                engine.LerpYawToNeutral();
                break;

            case MouseState.Toggled:
            case MouseState.Held:
                engine.Pitch(GameStateUtils.GetMousePositionOnScreen().y);
                engine.Yaw(GameStateUtils.GetMousePositionOnScreen().x);
                break;

            default:
                Debug.LogWarning("MouseState has incorrect value...");
                break;
        }
    }

    // Maybe make those ship cam changes events
    public void ToggleMouseFlight()
    {
        switch(MouseState)
        {
            case MouseState.Off:
                MouseState = MouseState.Toggled;
                break;

            case MouseState.Toggled:
                MouseState = MouseState.Off;
                break;

            case MouseState.Held:
                MouseState = MouseState.Toggled;
                break;
        }
    }

    private void UpdateShipCameraState()
    {
        if (shipCamera == null) return;

        switch (MouseState)
        {
            case MouseState.Off:
                shipCamera.calculateRotationOffsets = false;
                break;

            case MouseState.Toggled:
            case MouseState.Held:
                shipCamera.calculateRotationOffsets = true;
                break;
        }
    }

    IEnumerator ManualMouseFlightCoroutine()
    {
        yield return new WaitForSeconds(mouseHoldDelay);

        MouseState = MouseState.Held;
    }

    IEnumerator DoubleTap(KeyCode pressedKey, Action action)
    {
        float elapsed = 0;

        while (elapsed < doubleTapDuration)
        {
            if (Input.GetKey(pressedKey))
            {
                action();
                yield break;
            }
        
            elapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}

public class PossessionEventArgs : EventArgs
{
    public Ship newShip;
    public Ship oldShip;

    public bool PossessingNewShip
    {
        get
        {
            return newShip != null;
        }
    }

    public PossessionEventArgs(Ship newShip, Ship oldShip)
    {
        this.newShip = newShip;
        this.oldShip = oldShip;
    }
}




