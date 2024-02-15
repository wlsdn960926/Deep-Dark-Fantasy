using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DungeonMgr : SingletonMonoBehaviour<DungeonMgr> // 수정된 싱글톤 상속
{
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;
    [SerializeField] private int currentDungeonLevelListIndex = 0;

    [HideInInspector] public GameState gameState;

    private bool dungeonInitialized = false; // 던전이 초기화되었는지 추적하는 새 변수

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
                if (!dungeonInitialized) // 던전이 아직 초기화되지 않았다면
                {
                    PlayDungeonLevel(currentDungeonLevelListIndex);
                    dungeonInitialized = true; // 던전이 초기화되었음을 표시
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

    // 게임 상태를 재설정하고 던전을 다시 생성해야 할 경우 사용할 메소드
    public void ResetDungeon()
    {
        dungeonInitialized = false;
        gameState = GameState.gameStarted;
    }
}
