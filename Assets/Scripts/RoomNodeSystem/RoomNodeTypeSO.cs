using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")] // 에디터 상에서 보여줄 RommNodeTypes만 표시해라
    #endregion Header
    public bool displayInNodeGraphEditor = true;
    #region Header
    [Header("One Type Should Be A Corridor")]  // 한 가지 타입은 복도가 되어야 한다.
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("One Type Should Be A CorridorNS ")]  // 한 가지 타입은 북쪽-남쪽 복도가 되어야 한다.
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("One Type Should Be A CorridorEW")]  // 한 가지 타입은 동쪽-서쪽 복도가 되어야 한다.
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("One Type Should Be An Entrance")]  // 입구가 되어야 한다.
    #endregion Header
    public bool isEntrance;
    #region Header
    [Header("One Type Should Be A Boss Room")]  // 보스방이 되어야 한다.
    #endregion Header
    public bool isBossRoom;
    #region Header
    [Header("One Type Should Be None (Unassigned)")]  // None(미지정)이 되어야 한다.
    #endregion Header
    public bool isNone;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
