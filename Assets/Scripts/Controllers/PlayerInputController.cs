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

    private bool _isDodging = false;
    private float _dodgeSpeed = 5f;
    private float _dodgeDuration = 0.5f;
    private float _dodgeCooldown = 3f;
    private Coroutine _dodgeCoroutine;
    public LayerMask layerMask;
    public float maxCheckDistance;

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
    
    public void OnDodge(InputValue value)
    {
        if (value.isPressed && !_isDodging)
        {
            _isDodging = true;
            _dodgeCoroutine = StartCoroutine(DodgeCoroutine());
        }
    }

    IEnumerator DodgeCoroutine()
    {
        float timer = 0f;
        Vector2 dodgeDirection = GetDodgeDirection();

        while (timer < _dodgeDuration)
        {
            transform.Translate(dodgeDirection * _dodgeSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(_dodgeCooldown);
        _isDodging = false;
    }

    private Vector2 GetDodgeDirection()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        return moveInput.normalized;
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("아이템 획득" + value.ToString());
            // Raycast 수행
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _camera.transform.forward, maxCheckDistance, layerMask);
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
