using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


public class TopDownCharacterController : MonoBehaviour
{
    // event: 외부에서 호출 못하게 막는다
    public event Action<Vector2> OnMoveEvent;
    public event Action<Vector2> OnLookEvent;
    public event Action<AttackSO> OnAttackEvent;

    private float _timeSinceLastAttack = float.MaxValue;
    protected bool IsAttacking { get; set; }

    protected CharacterStatsHandler Stats { get; private set; }

    protected virtual void Awake()
    {
        Stats = GetComponent<CharacterStatsHandler>();
    }

    protected virtual void Update()
    {
        HandleAttackDelay();
    }

    private void HandleAttackDelay() //무기 딜레이
    {
        if (Stats.CurrentStates.attackSO == null)
            return;
        if (_timeSinceLastAttack <= Stats.CurrentStates.attackSO.delay)  
        {
            _timeSinceLastAttack += Time.deltaTime;
        }
        if (IsAttacking && _timeSinceLastAttack > Stats.CurrentStates.attackSO.delay)
        {
            _timeSinceLastAttack = 0;
            CallAttackEvent(Stats.CurrentStates.attackSO);
        }

    }

    public void CallMoveEvent(Vector2 direction)
    {
        OnMoveEvent?.Invoke(direction);  // ?. 앞이 null이 아닐 때 동작
    }

    public void CallLookEvent(Vector2 direction)
    {
        OnLookEvent?.Invoke(direction);
    }
    public void CallAttackEvent(AttackSO attackSO)  // 원거리 공격
    {
        OnAttackEvent?.Invoke(attackSO);
    }
}