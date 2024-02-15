using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DungeonMgr : SingletonMonoBehaviour<DungeonMgr> // ������ �̱��� ���
{
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;
    [SerializeField] private int currentDungeonLevelListIndex = 0;

    [HideInInspector] public GameState gameState;

    private bool dungeonInitialized = false; // ������ �ʱ�ȭ�Ǿ����� �����ϴ� �� ����

    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    private void Update()
    {
        HandleGameState();
    }

    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                if (!dungeonInitialized) // ������ ���� �ʱ�ȭ���� �ʾҴٸ�
                {
                    PlayDungeonLevel(currentDungeonLevelListIndex);
                    dungeonInitialized = true; // ������ �ʱ�ȭ�Ǿ����� ǥ��
                }
                break;
        }
    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        bool dungeonBuiltSuccessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);
        if (!dungeonBuiltSuccessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
        }
    }

    // ���� ���¸� �缳���ϰ� ������ �ٽ� �����ؾ� �� ��� ����� �޼ҵ�
    public void ResetDungeon()
    {
        dungeonInitialized = false;
        gameState = GameState.gameStarted;
    }
}
