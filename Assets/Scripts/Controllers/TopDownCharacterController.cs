using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;


public class TopDownCharacterController : MonoBehaviour
{
    // event: �ܺο��� ȣ�� ���ϰ� ���´�
    public event Action<Vector2> OnMoveEvent;
    public event Action<Vector2> OnLookEvent;
    public event Action<AttackSO> OnAttackEvent;
    public event Action<InteractionManager> OnInteractionEvent;
    

    private float _timeSinceLastAttack = float.MaxValue;
    protected bool IsAttacking { get; set; }
    protected bool IsInteraction { get; set; }

    protected CharacterStatsHandler Stats { get; private set; }

    protected virtual void Awake()
    {
        Stats = GetComponent<CharacterStatsHandler>();
    }

    protected virtual void Update()
    {
        HandleAttackDelay();
    }

    private void HandleAttackDelay() //���� �����
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
        OnMoveEvent?.Invoke(direction);  // ?. ���� null�� �ƴ� �� ����
    }

    public void CallLookEvent(Vector2 direction)
    {
        OnLookEvent?.Invoke(direction);
    }
    public void CallAttackEvent(AttackSO attackSO)  // ���Ÿ� ����
    {
        OnAttackEvent?.Invoke(attackSO);
    }
    public void CallInteractionEvent(InteractionManager interaction)
    {
        OnInteractionEvent?.Invoke(interaction); 
    }
}