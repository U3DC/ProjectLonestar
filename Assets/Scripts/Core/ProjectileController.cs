﻿using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float distanceTraveled = 0f;

    public Weapon weapon;
    public Vector3 target;
    public GameObject mainEffect;
    public GameObject impactEffect;

    private new Collider collider;
    private Rigidbody rb;
    private Vector3 startPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        gameObject.hideFlags = HideFlags.HideInHierarchy;
        distanceTraveled = 0f;
        target = Vector3.zero;
        startPosition = transform.position;
    }

    // Be sure to call this after instantiation
    public void Initialize(Ship owner, Weapon weapon)
    {
        this.weapon = weapon;

        Physics.IgnoreCollision(owner.GetComponentInChildren<Collider>(), collider);

        target = owner.AimPosition;

        transform.LookAt(target);
        rb.AddForce(transform.forward * weapon.thrust, ForceMode.Impulse);
    }

    private void LateUpdate()
    {
        distanceTraveled = Vector3.Distance(transform.position, startPosition);

        if(distanceTraveled > weapon.range) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(rb);
        Destroy(collider);

        WorldObject hitObject = collision.gameObject.GetComponent<WorldObject>();
        if (hitObject != null) hitObject.TakeDamage(weapon);

        mainEffect.SetActive(false);
        impactEffect.SetActive(true);

        Destroy(gameObject, 2);
    }
} 