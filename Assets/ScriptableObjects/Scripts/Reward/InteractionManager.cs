using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public interface IInteractable
{
    string GetInteractPrompt();
    void OnInteract();
}

public class InteractionManager : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            // 상호작용 로직 구현 (예: 아이템 획득, 메시지 출력 등)
            Debug.Log("아이템과 상호작용!");
        }
    }
}