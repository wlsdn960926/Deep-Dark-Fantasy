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
        // �ν����� ���� ���� �̺�Ʈ ����
        Selection.selectionChanged += InspectorSelectionChanged;

        // ��� ���̾ƿ� ��Ÿ�� ����
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // ���õ� ��� ��Ÿ�� ����
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // �� ��� Ÿ�� �ε�
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // �ν����� ���� ���� �̺�Ʈ ���� ����
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// �ν����Ϳ��� �� ��� �׷��� ��ũ���ͺ� ������Ʈ ������ �� �� Ŭ���ϸ� �� ��� �׷��� ������ â�� ���ϴ�.
    /// </summary>

    [OnOpenAsset(0)]  // UnityEditor.Callbacks ���ӽ����̽� �ʿ�
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
    /// GUI Editor �׸���
    /// </summary>
    private void OnGUI()
    {
        // RoomNodeGraphSO Ÿ���� ��ũ���ͺ� ������Ʈ�� ���õǸ� ó��
        if (currentRoomNodeGraph != null)
        {
            // ��� �׸��� �׸���
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // �巡�� ���� �� �׸���
            DrawDraggedLine();

            // �̺�Ʈ ó��
            ProcessEvents(Event.current);

            // �� ��� �� ���� �׸���
            DrawRoomConnections();

            // �� ��� �׸���
            DrawRoomNodes();
        }

        if (GUI.changed)
            Repaint();
    }

    /// <summary>
    /// �� ��� �׷��� �������� ��� �׸��带 �׸��ϴ�.
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
            // ���� ��忡�� ���� ��ġ���� ���� �׸��ϴ�.
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        // �׷��� �巡�׸� �����մϴ�.
        graphDrag = Vector2.zero;

        // ���콺�� ���� �巡�� ���� �ƴϰų� null�� �� ��� ���� �ִ��� Ȯ���մϴ�.
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // ���콺�� �� ��� ���� ���ų� �� ��忡�� ���� �巡�� ���̶�� �׷��� �̺�Ʈ�� ó���մϴ�.
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        // �׷��� �ʴٸ� �� ��� �̺�Ʈ�� ó���մϴ�.
        else
        {
            // �� ��� �̺�Ʈ ó��
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    ///  ���콺�� �� ��� ���� �ִ��� Ȯ���մϴ�. ������ �ش� �� ��带 ��ȯ�ϰ�, ������ null�� ��ȯ�մϴ�.
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
    /// �� ��� �׷��� �̺�Ʈ�� ó���մϴ�.
    /// </summary>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // ���콺 �ٿ� �̺�Ʈ ó��
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // ���콺 �� �̺�Ʈ ó��
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // ���콺 �巡�� �̺�Ʈ ó��
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// ��� ���� �ƴ� �� ��� �׷������� ���콺 �ٿ� �̺�Ʈ�� ó���մϴ�.
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // �׷������� ���콺 ������ Ŭ�� �ٿ� �̺�Ʈ�� ó���մϴ� (���ؽ�Ʈ �޴� ǥ��).
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // �׷������� ���콺 ���� Ŭ�� �ٿ� �̺�Ʈ�� ó���մϴ�.
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// ���ؽ�Ʈ �޴��� ǥ���մϴ�.
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
    /// ���콺 ��ġ�� �� ��带 �����մϴ�.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        // ���� ��� �׷����� ��� ������ �Ա� �� ��带 ���� �߰��մϴ�.
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// ���콺 ��ġ�� �� ��带 �����մϴ� - RoomNodeType�� �����ϱ� ���� �����ε��.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // �� ��� ��ũ���ͺ� ������Ʈ ������ �����մϴ�.
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // �� ��带 ���� �� ��� �׷��� �� ��� ����Ʈ�� �߰��մϴ�.
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // �� ��� ���� �����մϴ�.
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        // �� ��带 �� ��� �׷��� ��ũ���ͺ� ������Ʈ ���� �����ͺ��̽��� �߰��մϴ�.
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        // �׷��� ��� dictionary�� ���� ��ħ �մϴ�.
        currentRoomNodeGraph.OnValidate();
    }

    /// <summary>
    /// ���õ� �� ��带 �����մϴ�.
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        // ��� ��带 ��ȸ�մϴ�.
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                // �ڽ� �� ��� ID�� ��ȸ�մϴ�.
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // �ڽ� �� ��带 �˻��մϴ�.
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        // �ڽ� �� ��忡�� �θ�ID�� �����մϴ�.
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // // �θ� �� ��� ID�� ��ȸ�մϴ�.
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    // �θ� ��带 �˻��մϴ�.
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // �θ� ��忡�� �ڽ�ID�� �����մϴ�.
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // ��⿭�� �ִ� �� ��带 �����մϴ�.
        while (roomNodeDeletionQueue.Count > 0)
        {
            // ��⿭���� �� ��带 �����ɴϴ�.
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // ��ųʸ����� ��带 �����մϴ�.
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // ����Ʈ���� ��带 �����մϴ�.
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // ���� �����ͺ��̽����� ��带 �����մϴ�.
            DestroyImmediate(roomNodeToDelete, true);

            // ���� �����ͺ��̽��� �����մϴ�.
            AssetDatabase.SaveAssets();

        }
    }

    /// <summary>
    /// ���õ� �� ��� ������ ��ũ�� �����մϴ�.
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // ��� �� ��带 ��ȸ�մϴ�.
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    // �ڽ� �� ��带 �����ɴϴ�.
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    // �ڽ� �� ��尡 ���õǾ� ������
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        // �θ� �� ��忡�� �ڽ�ID�� �����մϴ�.
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        // �ڽ� �� ��忡�� �θ�ID�� �����մϴ�.
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // ��� ���õ� �� ����� ������ �����մϴ�.
        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// ��� �� ����� ������ �����մϴ�.
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
    /// ��� �� ��带 �����մϴ�.
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
    /// ���콺 �� �̺�Ʈ�� ó���մϴ�.
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // ������ ���콺 ��ư�� ���� ���� �巡�� ���̶��
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // �� ��� ���� �ִ��� Ȯ���մϴ�.
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                // �����ϴٸ� �θ� �� ����� �ڽ����� �����մϴ�.
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // �ڽ� �� ��忡 �θ� ID�� �����մϴ�.
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// ���콺 �巡�� �̺�Ʈ�� ó���մϴ�.
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // ������ Ŭ�� �巡�� �̺�Ʈ - ���� �׸��ϴ�.
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        // ���� Ŭ�� �巡�� �̺�Ʈ - ��� �׷����� �巡���մϴ�.
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// ������ ���콺 �巡�� �̺�Ʈ�� ó���մϴ� - ���� �׸��ϴ�.
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
    /// ���� ���콺 �巡�� �̺�Ʈ�� ó���մϴ� - �� ��� �׷����� �巡���մϴ�.
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
    /// �� ��忡�� ���� ���� �巡���մϴ�.
    /// </summary>
    public void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// �� ��忡�� �� �巡�׸� �����մϴ�.
    /// </summary>
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// �׷��� â���� �� ��� ���� ������ �׸��ϴ�.
    /// </summary>
    private void DrawRoomConnections()
    {
        // ��� �� ��带 ��ȸ�մϴ�.
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                // �ڽ� �� ��带 ��ȸ�մϴ�.
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // ��ųʸ����� �ڽ� �� ��带 �����ɴϴ�.
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
    /// �θ� �� ���� �ڽ� �� ��� ������ ���� ���� �׸��ϴ�.
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // ���� �������� ������ �����ɴϴ�.
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // �߰� ������ ����մϴ�.
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        // ���� ���������� ���������� ���͸� ����մϴ�.
        Vector2 direction = endPosition - startPosition;

        // �߰� �������� �������� ����ȭ�� ��ġ�� ����մϴ�.
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        // ȭ��ǥ �Ӹ��� �߰� ���� ������ ��ġ�� ����մϴ�.
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        // ȭ��ǥ�� �׸��ϴ�.
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        // ���� �׸��ϴ�.
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// �׷��� â���� �� ��带 �׸��ϴ�.
    /// </summary>
    private void DrawRoomNodes()
    {
        // ��� �� ��带 ��ȸ�ϸ� �׸��ϴ�.
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
    /// �ν����Ϳ��� ������ ����Ǿ����ϴ�.
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