using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//아직 안쓸겁니다. 예비용

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 10; // 최대 체력
    private int currentHealth; // 현재 체력

    public Image heartImagePrefab; // 하트 이미지 프리팹
    public Transform heartsParent; // 하트 이미지의 부모 Transform
    public Sprite fullHeartSprite; // 체력이 가득 찬 하트 스프라이트
    public Sprite halfHeartSprite;
    public Sprite emptyHeartSprite; // 체력이 비어 있는 하트 스프라이트

    private Image[] heartImages; // 하트 이미지 배열

    private void Start()
    {
        // 현재 체력을 최대 체력으로 초기화합니다.
        currentHealth = maxHealth;

        // 하트 이미지 배열을 초기화합니다.
        heartImages = new Image[maxHealth];

        // 하트 이미지를 생성하고 초기화합니다.
        for (int i = 0; i < maxHealth; i++)
        {
            Image heartImage = Instantiate(heartImagePrefab, heartsParent);
            heartImages[i] = heartImage;
            heartImage.sprite = fullHeartSprite; // 초기에는 모든 하트가 가득 찬 상태입니다.
        }

        // 체력을 업데이트합니다.
        UpdateHealthUI();
    }

    // 체력을 감소시키는 메서드
    public void Demage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;

        // 체력이 변할 때마다 UI를 업데이트합니다.
        UpdateHealthUI();
    }

    // 체력을 증가시키는 메서드
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        // 체력이 변할 때마다 UI를 업데이트합니다.
        UpdateHealthUI();
    }

    // UI 업데이트 메서드
    private void UpdateHealthUI()
    {
        // 현재 체력에 맞게 하트 이미지의 상태를 업데이트합니다.
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < currentHealth)
                heartImages[i].sprite = fullHeartSprite; // 체력이 남은 하트는 가득 찬 상태입니다.
            else
                heartImages[i].sprite = emptyHeartSprite; // 체력이 없는 하트는 비어 있는 상태입니다.
        }
    }
}

