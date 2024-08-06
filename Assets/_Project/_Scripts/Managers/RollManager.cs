using System;
using UnityEngine;
using UnityEngine.Pool;

public class RollManager : MonoBehaviour
{
  [SerializeField] Roll m_RollPrefab;

  Roll[]           m_RollsInLevel = Array.Empty<Roll>();
  ObjectPool<Roll> m_Pool;

  void Awake()
  {
    m_Pool = new ObjectPool<Roll>(
      () => Instantiate(m_RollPrefab, transform),
      o => o.gameObject.SetActive(true),
      o => o.gameObject.SetActive(false)
      ,collectionCheck: false);
  }

  void OnEnable()
  {
    LevelManager.OnLevelChanged += ConstructLevel;
  }
  
  void OnDisable()
  {
    LevelManager.OnLevelChanged -= ConstructLevel;
  }

  void ConstructLevel(LevelData levelData)
  {
    RecycleAll();
    
    m_RollsInLevel = new Roll[levelData.Data.Length];
    for (var i = 0; i < levelData.Data.Length; i++)
    {
      var rollData = levelData.Data[i];
      
      var spawned  = m_Pool.Get();
      spawned.transform.SetParent(null);
      spawned.transform.SetPositionAndRotation(rollData.Position, rollData.Rotation);
      spawned.transform.localScale = rollData.Scale;
      spawned.Initialize(rollData.ID, rollData.Parents, rollData.Color);

      m_RollsInLevel[i] = spawned;
    }
  }

  void RecycleAll()
  {
    foreach (var roll in m_RollsInLevel)
    {
      m_Pool.Release(roll);
    }
  }
}
