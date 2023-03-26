using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class RocketBehaviour : MonoBehaviour
{
    private Transform _target;
    private const float speed = 15.0f;
    private bool _homing;

    private const float rocketStrength = 15.0f;
    private const float aliveTimer = 5.0f;

    public void Fire([NotNull] Transform target)
    {
        _target = target != null ? target : throw new ArgumentNullException(nameof(target));
        _homing = true;
        Destroy(gameObject, aliveTimer);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_homing || _target == null) return;
        var direction = (_target.position - transform.position).normalized;
        transform.position += Time.deltaTime * speed * direction;
        transform.LookAt(_target);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_target == null) return;
        Destroy(gameObject);
        if (!collision.gameObject.CompareTag(_target.tag)) return;
        var targetRb = collision.gameObject.GetComponent<Rigidbody>();
        var awayFromTarget = -collision.contacts[0].normal;
        targetRb.AddForce(awayFromTarget * rocketStrength, ForceMode.Impulse);
    }
}