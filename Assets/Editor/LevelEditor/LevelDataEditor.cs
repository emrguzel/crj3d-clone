using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
  int m_SelectedIndex = -1;

  void OnEnable() { SceneView.duringSceneGui += OnSceneGUI; }

  void OnDisable() { SceneView.duringSceneGui -= OnSceneGUI; }

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();
    LevelData levelData = (LevelData)target;
    
    if (GUILayout.Button("Scene Selection to Data"))
    {
      levelData.SelectionToData();
    }
    if (GUILayout.Button("Update IDs"))
    {
      levelData.UpdateParentIdReferences();
    }
  }

  void OnSceneGUI(SceneView sceneView)
  {
    if (EditorApplication.isPlaying)
    {
      return;
    }

    LevelData levelData = (LevelData)target;

    if (levelData.Data == null)
      return;

    DrawCubes(levelData);
    HandleInput(levelData);
  }
  
  void DrawCubes(LevelData levelData) {
    for (int i = 0; i < levelData.Data.Length; i++) {
      var data = levelData.Data[i];
      Handles.color = data.Color;

      Matrix4x4 matrix = Matrix4x4.TRS(data.Position, data.Rotation, data.Scale);

      Mesh cubeMesh = CreateCubeMesh();
      Graphics.DrawMesh(cubeMesh, matrix, GetCubeMaterial(data.Color), 0);

      if (HandleCubeSelection(data.Position, data.Rotation, data.Scale, i)) {
        m_SelectedIndex = i;
            
        LevelDataOverlay.Instance.SetData(i, levelData.Data[i].Color);
        LevelDataOverlay.Instance.LevelData = levelData;
        LevelDataOverlay.Instance.Index     = i;
        Repaint();
      }

      if (m_SelectedIndex == i)
      {
        DrawHandles(levelData, data, i);
      }
    }
  }
  
  void DrawHandles(LevelData levelData, RollData data, int i)
  {
    Vector3 newPosition = Handles.PositionHandle(data.Position, data.Rotation);
    if (newPosition != data.Position) {
      Undo.RecordObject(levelData, "Move RollData Position");
      levelData.Data[i].Position = newPosition;
      EditorUtility.SetDirty(levelData);
      Repaint();
    }
            
    Quaternion newRotation = Handles.RotationHandle(data.Rotation, data.Position);
    if (newRotation != data.Rotation) {
      Undo.RecordObject(levelData, "Move RollData Rotation");
      levelData.Data[i].Rotation = newRotation;
      EditorUtility.SetDirty(levelData);
      Repaint();
    }
  }

  void HandleInput(LevelData levelData) {
    Event e = Event.current;

    if (e.type == EventType.KeyDown && e.control) {
      if (e.keyCode == KeyCode.D) {
        DuplicateSelected(levelData, m_SelectedIndex);
        e.Use();
      } else if (e.keyCode == KeyCode.Delete) {
        DeleteSelected(levelData, m_SelectedIndex);
        e.Use();
      }
    }
  }

  void DeleteSelected(LevelData levelData, int index)
  {
    if (index < 0 || index >= levelData.Data.Length)
      return;

    Undo.RecordObject(levelData, "Delete RollData");
    
    var old = levelData.Data.ToList();
    old.RemoveAt(index);
    levelData.Data = old.ToArray();
    m_SelectedIndex = Mathf.Clamp(m_SelectedIndex, 0, levelData.Data.Length - 1);

    EditorUtility.SetDirty(levelData);
    Repaint();
  }

  bool HandleCubeSelection(Vector3 position, Quaternion rotation, Vector3 scale, int index)
  {
    // Create a matrix for transforming the cube
    Matrix4x4 matrix  = Matrix4x4.TRS(position, rotation, scale);
    Vector3[] corners = new Vector3[8];
    Vector3   center  = matrix.MultiplyPoint3x4(Vector3.zero);

    // Compute the 8 corners of the cube in world space
    corners[0] = matrix.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, -0.5f));
    corners[1] = matrix.MultiplyPoint3x4(new Vector3(0.5f, -0.5f, -0.5f));
    corners[2] = matrix.MultiplyPoint3x4(new Vector3(0.5f, 0.5f, -0.5f));
    corners[3] = matrix.MultiplyPoint3x4(new Vector3(-0.5f, 0.5f, -0.5f));
    corners[4] = matrix.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, 0.5f));
    corners[5] = matrix.MultiplyPoint3x4(new Vector3(0.5f, -0.5f, 0.5f));
    corners[6] = matrix.MultiplyPoint3x4(new Vector3(0.5f, 0.5f, 0.5f));
    corners[7] = matrix.MultiplyPoint3x4(new Vector3(-0.5f, 0.5f, 0.5f));

    // Convert world space corners to screen space
    Vector2[] screenCorners = corners.Select(c => HandleUtility.WorldToGUIPoint(c)).ToArray();

    // Check if the mouse click is within the bounds of the cube in screen space
    Rect bounds = new Rect(
      Mathf.Min(screenCorners[0].x, screenCorners[6].x),
      Mathf.Min(screenCorners[0].y, screenCorners[6].y),
      Mathf.Max(screenCorners[6].x, screenCorners[0].x) - Mathf.Min(screenCorners[0].x, screenCorners[6].x),
      Mathf.Max(screenCorners[6].y, screenCorners[0].y) - Mathf.Min(screenCorners[0].y, screenCorners[6].y)
    );

    // Handle mouse clicks
    Event e = Event.current;
    if (e.type == EventType.MouseDown && e.button == 0 && bounds.Contains(e.mousePosition))
    {
      return true;
    }

    return false;
  }

  void DuplicateSelected(LevelData levelData, int index)
  {
    if (index < 0 || index >= levelData.Data.Length)
      return;

    // Create a new RollData object as a copy of the selected one
    RollData selectedData = levelData.Data[index];
    RollData newData = new RollData(
      selectedData.Position + new Vector3(-.05f, 0f, 0), // Offset position for the duplicate
      selectedData.Rotation,
      selectedData.Scale,
      levelData.Data.Length
    )
    {
      Color = selectedData.Color
    };

    // Add the new RollData to the array
    Array.Resize(ref levelData.Data, levelData.Data.Length + 1);
    newData.ID           = levelData.Data.Length - 1;
    levelData.Data[^1] = newData;

    // Set the newly created object as the selected one
    m_SelectedIndex = levelData.Data.Length - 1;

    // Mark the LevelData as dirty to ensure changes are saved
    Undo.RecordObject(levelData, "Duplicate RollData");
    EditorUtility.SetDirty(levelData);
    Repaint();
  }

  Mesh CreateCubeMesh()
  {
    Mesh mesh = new Mesh();
    Vector3[] vertices =
    {
      new Vector3(-0.5f, -0.5f, -0.5f),
      new Vector3(0.5f, -0.5f, -0.5f),
      new Vector3(0.5f, 0.5f, -0.5f),
      new Vector3(-0.5f, 0.5f, -0.5f),
      new Vector3(-0.5f, -0.5f, 0.5f),
      new Vector3(0.5f, -0.5f, 0.5f),
      new Vector3(0.5f, 0.5f, 0.5f),
      new Vector3(-0.5f, 0.5f, 0.5f)
    };
    int[] triangles =
    {
      0, 2, 1, 0, 3, 2,
      4, 5, 6, 4, 6, 7,
      0, 1, 5, 0, 5, 4,
      1, 2, 6, 1, 6, 5,
      2, 3, 7, 2, 7, 6,
      3, 0, 4, 3, 4, 7
    };
    mesh.vertices  = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    return mesh;
  }

  Material GetCubeMaterial(Color color)
  {
    Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
    {
      color = color
    };
    return material;
  }
}