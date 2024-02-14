using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code

    // the following code should only be run in the Unity Editor
#if UNITY_EDITOR

    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    /// <summary>
    /// 노드 초기화
    /// </summary>
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        // 룸 노드 타입 리스트 로드
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// 노드 그리기
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        //  노드 박스를 BeginArea를 사용하여 그립니다.
        GUILayout.BeginArea(rect, nodeStyle);

        // 팝업 선택 변경 감지를 위한 영역 시작
        EditorGUI.BeginChangeCheck();

        // 룸 노드에 부모가 있거나 입구 타입이면 레이블을 표시하고, 그렇지 않으면 팝업을 표시합니다.
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // 변경할 수 없는 레이블 표시
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // 선택 가능한 RoomNodeType 이름 값으로 팝업 표시 (현재 설정된 roomNodeType을 기본값으로 함)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // 룸 타입 선택이 변경되어 자식 연결이 잠재적으로 유효하지 않게 될 경우
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // 룸 노드 타입이 변경되었고 이미 자식이 있는 경우 부모 자식 링크를 삭제해야 합니다.
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // 자식 룸 노드 가져오기
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // 자식 룸 노드가 null이 아니면
                        if (childRoomNode != null)
                        {
                            // 부모 룸 노드에서 자식ID 제거
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            // 자식 룸 노드에서 부모ID 제거
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    /// <summary>
    /// 선택 가능한 룸 노드 타입을 표시할 문자열 배열을 채웁니다.
    /// </summary>
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    /// <summary>
    /// 노드에 대한 이벤트 처리
    /// </summary>
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // 마우스 다운 이벤트 처리
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // 마우스 업 이벤트 처리
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // 마우스 드래그 이벤트 처리
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// 마우스 다운 이벤트 처리
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // 왼쪽 클릭 다운
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        // 오른쪽 클릭 다운
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    /// <summary>
    /// 왼쪽 클릭 다운 이벤트 처리
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        // 노드 선택 토글
        if (isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
    }

    /// <summary>
    /// 오른쪽 클릭 다운 이벤트 처리
    ///
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// 마우스 업 이벤트 처리
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // 왼쪽 클릭 업
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// 왼쪽 클릭 업 이벤트 처리
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// 마우스 드래그 이벤트 처리
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // 왼쪽 클릭 드래그 이벤트 처리
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    /// <summary>
    /// 왼쪽 마우스 드래그 이벤트 처리
    /// </summary>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// 노드 드래그
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// 노드에 자식ID 추가 (노드가 추가되었으면 true, 그렇지 않으면 false 반환)
    /// </summary>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // 부모에게 자식 노드를 유효하게 추가할 수 있는지 확인
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 부모 노드에 자식 노드를 유효하게 추가할 수 있는지 확인 - 가능하면 true, 그렇지 않으면 false 반환
    /// </summary>
    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        // 노드 그래프에 연결된 보스 룸이 이미 있는지 확인
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedBossNodeAlready = true;
        }

        // 자식 노드의 유형이 보스 룸이고 이미 연결된 보스 룸 노드가 있으면 false 반환
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;

        // 자식 노드의 유형이 None이면 false 반환
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        // 이미 이 자식 ID를 가진 자식이 있는 경우 false 반환
        if (childRoomNodeIDList.Contains(childID))
            return false;

        // 이 노드 ID와 자식 ID가 같으면 false 반환
        if (id == childID)
            return false;

        // 이 자식ID가 이미 부모ID 리스트에 있으면 false 반환
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        // 자식 노드에 이미 부모가 있으면 false 반환
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        // 자식이 복도이고 이 노드가 복도인 경우 false 반환
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        // 자식이 복도가 아니고 이 노드가 복도가 아닌 경우 false 반환
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        // 복도를 추가하는 경우 이 노드가 허용된 최대 자식 복도 수보다 작은지 확인
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        // 자식 룸이 입구인 경우 false 반환 - 입구는 항상 최상위 부모 노드여야 합니다.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        // 복도에 방을 추가하는 경우 이 복도 노드에 이미 방이 추가되지 않았는지 확인
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    /// <summary>
    /// 노드에 부모ID 추가 (노드가 추가되었으면 true, 그렇지 않으면 false 반환)
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// 노드에서 자식ID 제거 (노드가 제거되었으면 true, 그렇지 않으면 false 반환)
    /// </summary>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // 노드에 자식 ID가 포함되어 있으면 제거
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 노드에서 부모ID 제거 (노드가 제거되었으면 true, 그렇지 않으면 false 반환)
    /// </summary>
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        // if the node contains the parent ID then remove it
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }

#endif

    #endregion Editor Code
}