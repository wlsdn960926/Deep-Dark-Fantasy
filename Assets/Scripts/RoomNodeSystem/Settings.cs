using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
	#region DUNGEON BUILD SETTINGS
	public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
	public const int maxDungeonBuildAttempts = 10;

	#endregion

	#region ROOM SETTINGS

	public const int maxChildCorridors = 3;  // 한 방에서 이어지는 자식 복도의 최대 수, 3이상으로 설정 시 공간적 제약 땜에 던전 구축 실패 가능성 있음.
    #endregion
}
