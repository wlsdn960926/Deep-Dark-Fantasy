using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    // Node layout values
    private const float nodeWidth = 160f;

    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // Connecting line values
    private const float connectingLineWidth = 3f;

    private const float connectingLineArrowSize = 6f;

    // Grid Spacing
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;


    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // 인스펙터 선택 변경 이벤트 구독
        Selection.selectionChanged += InspectorSelectionChanged;

        // 노드 레이아웃 스타일 정의
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // 선택된 노드 스타일 정의
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // 룸 노드 타입 로드
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // 인스펙터 선택 변경 이벤트 구독 해제
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// 인스펙터에서 룸 노드 그래프 스크립터블 오브젝트 에셋을 두 번 클릭하면 룸 노드 그래프 에디터 창을 엽니다.
    /// </summary>

    [OnOpenAsset(0)]  // UnityEditor.Callbacks 네임스페이스 필요
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    /// <summary>
    /// GUI Editor 그리기
    /// </summary>
    private void OnGUI()
    {
        // RoomNodeGraphSO 타입의 스크립터블 오브젝트가 선택되면 처리
        if (currentRoomNodeGraph != null)
        {
            // 배경 그리드 그리기
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // 드래그 중인 선 그리기
            DrawDraggedLine();

            // 이벤트 처리
            ProcessEvents(Event.current);

            // 룸 노드 간 연결 그리기
            DrawRoomConnections();

            // 룸 노드 그리기
            DrawRoomNodes();
        }

        if (GUI.changed)
            Repaint();
    }

    /// <summary>
    /// 룸 노드 그래프 에디터의 배경 그리드를 그립니다.
    /// </summary>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }

        Handles.color = Color.white;

    }


    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            // 현재 노드에서 라인 위치까지 선을 그립니다.
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        // 그래프 드래그를 리셋합니다.
        graphDrag = Vector2.zero;

        // 마우스가 현재 드래그 중이 아니거나 null인 룸 노드 위에 있는지 확인합니다.
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // 마우스가 룸 노드 위에 없거나 룸 노드에서 선을 드래그 중이라면 그래프 이벤트를 처리합니다.
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        // 그렇지 않다면 룸 노드 이벤트를 처리합니다.
        else
        {
            // 룸 노드 이벤트 처리
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    ///  마우스가 룸 노드 위에 있는지 확인합니다. 있으면 해당 룸 노드를 반환하고, 없으면 null을 반환합니다.
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 룸 노드 그래프 이벤트를 처리합니다.
    /// </summary>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
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

    /// <summary>
    /// 노드 위가 아닌 룸 노드 그래프에서 마우스 다운 이벤트를 처리합니다.
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // 그래프에서 마우스 오른쪽 클릭 다운 이벤트를 처리합니다 (컨텍스트 메뉴 표시).
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // 그래프에서 마우스 왼쪽 클릭 다운 이벤트를 처리합니다.
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// 컨텍스트 메뉴를 표시합니다.
    /// </summary>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    /// <summary>
    /// 마우스 위치에 룸 노드를 생성합니다.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        // 현재 노드 그래프가 비어 있으면 입구 룸 노드를 먼저 추가합니다.
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// 마우스 위치에 룸 노드를 생성합니다 - RoomNodeType도 전달하기 위해 오버로드됨.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // 룸 노드 스크립터블 오브젝트 에셋을 생성합니다.
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // 룸 노드를 현재 룸 노드 그래프 룸 노드 리스트에 추가합니다.
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // 룸 노드 값을 설정합니다.
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        // 룸 노드를 룸 노드 그래프 스크립터블 오브젝트 에셋 데이터베이스에 추가합니다.
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        // 그래프 노드 dictionary를 새로 고침 합니다.
        currentRoomNodeGraph.OnValidate();
    }

    /// <summary>
    /// 선택된 룸 노드를 삭제합니다.
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        // 모든 노드를 순회합니다.
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                // 자식 룸 노드 ID를 순회합니다.
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 자식 룸 노드를 검색합니다.
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        // 자식 룸 노드에서 부모ID를 제거합니다.
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // // 부모 룸 노드 ID를 순회합니다.
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    // 부모 노드를 검색합니다.
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // 부모 노드에서 자식ID를 제거합니다.
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // 대기열에 있는 룸 노드를 삭제합니다.
        while (roomNodeDeletionQueue.Count > 0)
        {
            // 대기열에서 룸 노드를 가져옵니다.
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // 딕셔너리에서 노드를 제거합니다.
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // 리스트에서 노드를 제거합니다.
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // 에셋 데이터베이스에서 노드를 제거합니다.
            DestroyImmediate(roomNodeToDelete, true);

            // 에셋 데이터베이스를 저장합니다.
            AssetDatabase.SaveAssets();

        }
    }

    /// <summary>
    /// 선택된 룸 노드 사이의 링크를 삭제합니다.
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // 모든 룸 노드를 순회합니다.
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    // 자식 룸 노드를 가져옵니다.
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    // 자식 룸 노드가 선택되어 있으면
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        // 부모 룸 노드에서 자식ID를 제거합니다.
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        // 자식 룸 노드에서 부모ID를 제거합니다.
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // 모든 선택된 룸 노드의 선택을 해제합니다.
        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// 모든 룸 노드의 선택을 해제합니다.
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// 모든 룸 노드를 선택합니다.
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    /// <summary>
    /// 마우스 업 이벤트를 처리합니다.
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // 오른쪽 마우스 버튼을 놓고 선을 드래그 중이라면
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // 룸 노드 위에 있는지 확인합니다.
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                // 가능하다면 부모 룸 노드의 자식으로 설정합니다.
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // 자식 룸 노드에 부모 ID를 설정합니다.
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// 마우스 드래그 이벤트를 처리합니다.
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // 오른쪽 클릭 드래그 이벤트 - 선을 그립니다.
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        // 왼쪽 클릭 드래그 이벤트 - 노드 그래프를 드래그합니다.
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// 오른쪽 마우스 드래그 이벤트를 처리합니다 - 선을 그립니다.
    /// </summary>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    /// <summary>
    /// 왼쪽 마우스 드래그 이벤트를 처리합니다 - 룸 노드 그래프를 드래그합니다.
    /// </summary>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed = true;
    }


    /// <summary>
    /// 룸 노드에서 연결 선을 드래그합니다.
    /// </summary>
    public void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// 룸 노드에서 선 드래그를 해제합니다.
    /// </summary>
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// 그래프 창에서 룸 노드 간의 연결을 그립니다.
    /// </summary>
    private void DrawRoomConnections()
    {
        // 모든 룸 노드를 순회합니다.
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                // 자식 룸 노드를 순회합니다.
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 딕셔너리에서 자식 룸 노드를 가져옵니다.
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 부모 룸 노드와 자식 룸 노드 사이의 연결 선을 그립니다.
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // 선의 시작점과 끝점을 가져옵니다.
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // 중간 지점을 계산합니다.
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        // 선의 시작점에서 끝점까지의 벡터를 계산합니다.
        Vector2 direction = endPosition - startPosition;

        // 중간 지점에서 수직으로 정규화된 위치를 계산합니다.
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        // 화살표 머리의 중간 지점 오프셋 위치를 계산합니다.
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        // 화살표를 그립니다.
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        // 선을 그립니다.
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// 그래프 창에서 룸 노드를 그립니다.
    /// </summary>
    private void DrawRoomNodes()
    {
        // 모든 룸 노드를 순회하며 그립니다.
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }

    /// <summary>
    /// 인스펙터에서 선택이 변경되었습니다.
    /// </summary>
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}