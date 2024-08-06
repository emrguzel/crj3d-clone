using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Create LevelData", fileName = "LevelData", order = 0)]
public class LevelData : ScriptableObject
{
  public Color[]    ColorOrder;
  public RollData[] Data;

  void OnEnable()
  {
    if (Data == null)
    {
      Data = new[]
      {
        new RollData(Vector3.zero, Quaternion.identity, new Vector3(0.0647870451f, 0.00999999978f, 0.466620326f), 0)
      };
    }
  }

  #if UNITY_EDITOR
  
  public void SelectionToData()
  {
    var parent = Selection.activeTransform;
    var size   = parent.childCount;
    Data = new RollData[size];

    for (int i = 0; i < size; i++)
    {
      var child = parent.GetChild(i);

      Data[i] = new RollData(child.position, child.rotation, child.localScale, i);
    }

    UpdateParentIdReferences();
  }

  public void UpdateParentIdReferences()
  {
    var size = Data.Length;
    int mask = 0x1 << 31;

    var parent = new GameObject("EditorParent").transform;

    for (int i = 0; i < size; i++)
    {
      var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
      child.transform.SetParent(parent);
      
      child.transform.position = Data[i].Position;
      child.transform.rotation = Data[i].Rotation;
      child.transform.localScale = Data[i].Scale;
      
      child.layer = 31;
      
      child.name   = $"ID {i}";
      Data[i].ID = i;
    }

    // This section is pretty error-prone if level design is not correct, but enough for faster level generation/iteration. -eg 
    Physics.SyncTransforms();
    for (int i = 0; i < size; i++)
    {
      var child = parent.GetChild(i);

      var colliders = Physics.OverlapBox(child.position, child.localScale / 2, child.rotation, mask).ToList();

      colliders.RemoveAll(c => c.name == child.name);
      colliders.RemoveAll(c => c.transform.position.y <= child.transform.position.y);

      var parentIDs = new List<int>();
      
      for (int j = 0; j < size; j++)
      {
        if (parent.GetChild(j).TryGetComponent(out Collider collider))
        {
          if (colliders.Contains(collider))
          {
            parentIDs.Add(j);
          }
        }
      }

      Data[i].Parents = parentIDs.ToArray();
    }
    
    DestroyImmediate(parent.gameObject);
  }
  #endif
}