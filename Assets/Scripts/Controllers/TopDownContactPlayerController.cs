using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownContactPlayerController : TopDownCharacterController
{
    [SerializeField][Range(0f, 100f)] private float nothing;
    [SerializeField] private string targetTag = "Enemy";
    private bool _isCollidingWithTarget;

    [SerializeField] private SpriteRenderer characterRenderer;

    private HealthSystem healthSystem;
    private HealthSystem _collidingTargetHealthSystem;
    private TopDownMovement _collidingMovement;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.OnDamage += OnDamage;
    }

    public void FixdeUpdate()
    {
        if (_isCollidingWithTarget)
        {
            ApplyHealthChange();
        }

    }

    private void OnDamage()
    {
        nothing = 100f;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject receiver = collision.gameObject;

        if (!receiver.CompareTag(targetTag))
        {
            return;
        }

        _collidingTargetHealthSystem = receiver.GetComponent<HealthSystem>();
        if (_collidingTargetHealthSystem != null)
        {
            _isCollidingWithTarget = true;
        }

        _collidingMovement = receiver.GetComponent<TopDownMovement>();
    }
   

    private void ApplyHealthChange()
    {
        AttackSO attackSO = Stats.CurrentStates.attackSO;
        bool hasBeenChanged = _collidingTargetHealthSystem.ChangeHealth(-attackSO.power);
        if (attackSO.isOnKnockBack && _collidingMovement != null)
        {
            _collidingMovement.ApplyKnockback(transform, attackSO.knockbackPower, attackSO.knockbackTime);
        }
    }

}

