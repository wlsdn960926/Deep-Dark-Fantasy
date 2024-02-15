using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

public interface IInteractable
{
    string GetInteractPrompt();
    void OnInteract();
}

public class PlayerInputController : TopDownCharacterController
{
    private Camera _camera;
    private float maxInteractDistance = 0.5f;
    private LayerMask layerMask;

    protected override void Awake()
    {
        base.Awake();
        _camera = Camera.main;
    }

    public void OnMove(InputValue value)
    {
        // Debug.Log("OnMove" + value.ToString());
        Vector2 moveInput = value.Get<Vector2>().normalized;
        CallMoveEvent(moveInput);
    }

    public void OnLook(InputValue value)
    {
        // Debug.Log("OnLook" + value.ToString());
        Vector2 newAim = value.Get<Vector2>();
        Vector2 worldPos = _camera.ScreenToWorldPoint(newAim);
        newAim = (worldPos - (Vector2)transform.position).normalized;

        if (newAim.magnitude >= .9f)
        // Vector ���� �Ǽ��� ��ȯ
        {
            CallLookEvent(newAim);
        }
    }

    public void OnFire(InputValue value)
    {
        IsAttacking = value.isPressed;
    }
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("아이템 획득" + value.ToString());
            // Raycast 수행
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _camera.transform.forward, maxInteractDistance, layerMask);

            if (hit.collider != null)
            {
                Debug.Log(hit.rigidbody);
                // 아이템과 상호작용
                ItemObj item = hit.collider.gameObject.GetComponent<ItemObj>();
                if (item != null)
                {
                    item.OnInteract();
                }
            }
        }
    }
}