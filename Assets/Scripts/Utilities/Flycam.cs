﻿/*
EXTENDED FLYCAM
    Desi Quintans (CowfaceGames.com), 17 August 2012.
    Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
    Updated 5 Sept 2018 by Taylor Snyder

LICENSE
    Free as in speech, and free as in beer.

FEATURES

WASD/Arrows:    Movement
Q:              Climb
E:              Drop
Shift:          Move faster
Control:        Move slower
F:              Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).

*/
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera), typeof(AudioListener))]
public class Flycam : MonoBehaviour
{
    [Header("Rotation")]
    public float pitchSensitivity = 2;
    public float yawSensitivity = 2;
    public float minPitch = -90;
    public float maxPitch = 90;

    [Range(0, 1)]
    public float rotSmoothFactor = .85f;

    [Header("Movement")]
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;

    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    Vector3 currRot;
    Vector3 targetRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Movement();
        if (Cursor.lockState == CursorLockMode.Locked) Rotate();
    }

    void Movement()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        }


        if (Input.GetKey(KeyCode.Q)) { transform.position += transform.up * climbSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.E)) { transform.position -= transform.up * climbSpeed * Time.deltaTime; }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    void Rotate()
    {
        targetRot = new Vector3(
            Mathf.Clamp(targetRot.x + -Input.GetAxis("Mouse Y") * pitchSensitivity, minPitch, maxPitch),
            targetRot.y + Input.GetAxis("Mouse X") * yawSensitivity
            );

        currRot = Vector3.Lerp(currRot, targetRot, rotSmoothFactor);
        transform.rotation = Quaternion.Euler(currRot);
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}