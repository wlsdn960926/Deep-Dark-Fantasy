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
    /// ��� �ʱ�ȭ
    /// </summary>
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        // �� ��� Ÿ�� ����Ʈ �ε�
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// ��� �׸���
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        //  ��� �ڽ��� BeginArea�� ����Ͽ� �׸��ϴ�.
        GUILayout.BeginArea(rect, nodeStyle);

        // �˾� ���� ���� ������ ���� ���� ����
        EditorGUI.BeginChangeCheck();

        // �� ��忡 �θ� �ְų� �Ա� Ÿ���̸� ���̺��� ǥ���ϰ�, �׷��� ������ �˾��� ǥ���մϴ�.
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // ������ �� ���� ���̺� ǥ��
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // ���� ������ RoomNodeType �̸� ������ �˾� ǥ�� (���� ������ roomNodeType�� �⺻������ ��)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // �� Ÿ�� ������ ����Ǿ� �ڽ� ������ ���������� ��ȿ���� �ʰ� �� ���
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // �� ��� Ÿ���� ����Ǿ��� �̹� �ڽ��� �ִ� ��� �θ� �ڽ� ��ũ�� �����ؾ� �մϴ�.
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // �ڽ� �� ��� ��������
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // �ڽ� �� ��尡 null�� �ƴϸ�
                        if (childRoomNode != null)
                        {
                            // �θ� �� ��忡�� �ڽ�ID ����
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            // �ڽ� �� ��忡�� �θ�ID ����
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
    /// ���� ������ �� ��� Ÿ���� ǥ���� ���ڿ� �迭�� ä��ϴ�.
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
    /// ��忡 ���� �̺�Ʈ ó��
    /// </summary>
    public void ProcessEvents(Event currentEvent)
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

    /// ���콺 �ٿ� �̺�Ʈ ó��
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // ���� Ŭ�� �ٿ�
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        // ������ Ŭ�� �ٿ�
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    /// <summary>
    /// ���� Ŭ�� �ٿ� �̺�Ʈ ó��
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        // ��� ���� ���
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
    /// ������ Ŭ�� �ٿ� �̺�Ʈ ó��
    ///
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// ���콺 �� �̺�Ʈ ó��
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // ���� Ŭ�� ��
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// ���� Ŭ�� �� �̺�Ʈ ó��
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// ���콺 �巡�� �̺�Ʈ ó��
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // ���� Ŭ�� �巡�� �̺�Ʈ ó��
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    /// <summary>
    /// ���� ���콺 �巡�� �̺�Ʈ ó��
    /// </summary>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// ��� �巡��
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// ��忡 �ڽ�ID �߰� (��尡 �߰��Ǿ����� true, �׷��� ������ false ��ȯ)
    /// </summary>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // �θ𿡰� �ڽ� ��带 ��ȿ�ϰ� �߰��� �� �ִ��� Ȯ��
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// �θ� ��忡 �ڽ� ��带 ��ȿ�ϰ� �߰��� �� �ִ��� Ȯ�� - �����ϸ� true, �׷��� ������ false ��ȯ
    /// </summary>
    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        // ��� �׷����� ����� ���� ���� �̹� �ִ��� Ȯ��
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedBossNodeAlready = true;
        }

        // �ڽ� ����� ������ ���� ���̰� �̹� ����� ���� �� ��尡 ������ false ��ȯ
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;

        // �ڽ� ����� ������ None�̸� false ��ȯ
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        // �̹� �� �ڽ� ID�� ���� �ڽ��� �ִ� ��� false ��ȯ
        if (childRoomNodeIDList.Contains(childID))
            return false;

        // �� ��� ID�� �ڽ� ID�� ������ false ��ȯ
        if (id == childID)
            return false;

        // �� �ڽ�ID�� �̹� �θ�ID ����Ʈ�� ������ false ��ȯ
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        // �ڽ� ��忡 �̹� �θ� ������ false ��ȯ
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        // �ڽ��� �����̰� �� ��尡 ������ ��� false ��ȯ
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        // �ڽ��� ������ �ƴϰ� �� ��尡 ������ �ƴ� ��� false ��ȯ
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        // ������ �߰��ϴ� ��� �� ��尡 ���� �ִ� �ڽ� ���� ������ ������ Ȯ��
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        // �ڽ� ���� �Ա��� ��� false ��ȯ - �Ա��� �׻� �ֻ��� �θ� ��忩�� �մϴ�.
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        // ������ ���� �߰��ϴ� ��� �� ���� ��忡 �̹� ���� �߰����� �ʾҴ��� Ȯ��
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    /// <summary>
    /// ��忡 �θ�ID �߰� (��尡 �߰��Ǿ����� true, �׷��� ������ false ��ȯ)
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// ��忡�� �ڽ�ID ���� (��尡 ���ŵǾ����� true, �׷��� ������ false ��ȯ)
    /// </summary>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // ��忡 �ڽ� ID�� ���ԵǾ� ������ ����
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    /// <summary>
    /// ��忡�� �θ�ID ���� (��尡 ���ŵǾ����� true, �׷��� ������ false ��ȯ)
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