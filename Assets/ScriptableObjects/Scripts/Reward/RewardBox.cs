using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RewardBox : MonoBehaviour
{
    public List<GameObject> itemPrefabs; // 아이템 프리팹 목록

    public void Open()
    {
        // 랜덤 인덱스 선택
        int randomIndex = Random.Range(0, itemPrefabs.Count);

        // 랜덤 아이템 생성
        GameObject itemObject = Instantiate(itemPrefabs[randomIndex], transform.position, Quaternion.identity);
    }
}