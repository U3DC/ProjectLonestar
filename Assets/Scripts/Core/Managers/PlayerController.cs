﻿using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool HasPawn
    {
        get
        {
            return ship != null;
        }
    }

    [Header("--- Input ---")]
    public bool canPause = true;
    public bool inputAllowed = true;

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
            if (MouseStateChanged != null) MouseStateChanged(MouseState);
        }
    }

    // The amount of time that the controller waits until it 
    // determines the player is trying to switch to manual mouse flight
    public float mouseHoldDelay = .1f;

    public Ship ship;
    public GameObject hudPrefab;

    public delegate void PossessionEventHandler(PlayerController sender, PossessionEventArgs args);
    public event PossessionEventHandler PossessedNewShip;
    public event PossessionEventHandler ReleasedShip;

    public delegate void MouseStateEventHandler(MouseState state);
    public event MouseStateEventHandler MouseStateChanged;

    private void Awake()
    {
        name = "PLAYER CONTROLLER";

        if (FindObjectsOfType<PlayerController>().Length > 1)
        {
            print("Trying to create PlayerController when one already exists in the scene...");
            Destroy(gameObject);
        }
    }

    // This should be eleswhere
    public Ship SpawnPlayer(GameObject shipPrefab, Loadout loadout)
    {
        ship = ShipSpawner.SpawnShip(shipPrefab, Vector3.zero, loadout);
        return ship;
    }

    protected virtual void OnPossessedNewShip(PossessionEventArgs args)
    {
        if (PossessedNewShip != null) PossessedNewShip(this, args);
    }

    protected virtual void OnReleasedShip(Ship releasedShip)
    {
        if (ReleasedShip != null) ReleasedShip(this, new PossessionEventArgs(null, releasedShip));
    }

    public HUDManager SpawnHUD()
    {
        var hud = FindObjectOfType<HUDManager>();

        if (hud == null)
        {
            hud = Instantiate(GameSettings.Instance.HUDPrefab).GetComponent<HUDManager>();
        }

        hud.SetPlayerController(this);

        return hud;
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

        ship = newShip;

        newShip.SetPossessed(this, true);

        OnPossessedNewShip(new PossessionEventArgs(ship, oldShip));

        enabled = true;
    }

    public void Release()
    {
        ship.SetPossessed(this, false);
        MouseState = MouseState.Off;
        enabled = false;

        Instantiate(GameSettings.Instance.flycamPrefab);
        OnReleasedShip(ship);

        ship = null;
    }

    private void Update()
    {
        if(inputAllowed && ship != null)
        {
            #region movement
            if(Input.GetKey(InputManager.ThrottleUpKey) || Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                ship.engine.ThrottleUp();
            }

            if(Input.GetKey(InputManager.ThrottleDownKey) || Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                ship.engine.ThrottleDown();
            }

            if(Input.GetKey(InputManager.StrafeLeftKey))
            {
                ship.engine.Strafe = -1; 
            }

            if(Input.GetKey(InputManager.StrafeRightKey))
            {
                ship.engine.Strafe = 1; 
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
                ship.hardpointSystem.FireWeaponHardpoint(1, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint2Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(2, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint3Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(3, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint4Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(4, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint5Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(5, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint6Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(6, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint7Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(7, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint8Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(8, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint9Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(9, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.Hardpoint10Key))
            {
                ship.hardpointSystem.FireWeaponHardpoint(10, ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if(Input.GetKey(InputManager.FireKey))
            {
                ship.hardpointSystem.FireActiveWeapons(ShipCamera.GetMousePositionInWorld(ship.shipCam.camera));
            }

            if (Input.GetKeyDown(InputManager.LootAllKey))
            {
                ship.hardpointSystem.tractorBeam.TractorAllLoot();
            }

            #endregion
        }

        if(Input.GetKeyDown(InputManager.PauseGameKey) && canPause)
        {
            GameStateUtils.TogglePause();
        }
    }

    private void FixedUpdate()
    {
        var engine = ship.engine;

        if (engine == null) return;

        switch (mouseState)
        {
            case MouseState.Off:
                engine.LerpYawToNeutral();
                break;

            case MouseState.Toggled:
            case MouseState.Held:
                engine.Pitch(GetMousePositionOnScreen().y);
                engine.Yaw(GetMousePositionOnScreen().x);
                break;

            default:
                break;
        }
    }

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

    public static Vector2 GetMousePositionOnScreen()
    {
        int width = Screen.width;
        int height = Screen.height;

        Vector2 center = new Vector2(width / 2, height / 2);
        Vector3 mousePosition = Input.mousePosition;

        // Shifts the origin of the screen to be in the middle instead of the bottom left corner

        var mouseX = mousePosition.x - center.x;
        var mouseY = mousePosition.y - center.y;

        mouseX = Mathf.Clamp(mouseX / center.x, -1, 1);
        mouseY = Mathf.Clamp(mouseY / center.y, -1, 1);

        return new Vector2(mouseX, mouseY);
    }

    IEnumerator ManualMouseFlightCoroutine()
    {
        yield return new WaitForSeconds(mouseHoldDelay);

        MouseState = MouseState.Held;
    }

    public void SpawnFlyCam(Vector3 pos)
    {
        RemoveFlycamFromScene();
        var flyCam = Instantiate(GameSettings.Instance.flycamPrefab);
        flyCam.transform.position = pos + new Vector3(0, 10, 0);
    }

    // Can only be one fly cam in the scene

    public void RemoveFlycamFromScene()
    {
        var flyCam = FindObjectOfType<Flycam>();
        if (flyCam != null) Destroy(flyCam.gameObject);
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




