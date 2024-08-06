using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class TargetManager : MonoBehaviour
{
  [SerializeField] TargetBox m_TargetBoxPrefab;
  [Space]
  [SerializeField] Transform   m_TargetBoxParent;
  [SerializeField] float       m_MoveOffsetX;
  [SerializeField] Transform[] m_WaitingPositions;
  
  [Space]
  
  TargetBox[] m_TargetBoxes = Array.Empty<TargetBox>();
  int                          m_TargetBoxIndex;

  readonly List<Roll> m_WaitingRolls = new();

  ObjectPool<TargetBox> m_Pool;

  Vector3 m_DefaultParentPosition;

  void Awake()
  {
    m_Pool = new ObjectPool<TargetBox>(
      () => Instantiate(m_TargetBoxPrefab, m_TargetBoxParent),
      o => o.gameObject.SetActive(true),
      o => o.gameObject.SetActive(false)
      ,collectionCheck: false);

    m_DefaultParentPosition = m_TargetBoxParent.position;
  }

  void OnEnable()
  {
    Roll.OnRollCompleted += ResolveRoll;
    LevelManager.OnLevelChanged += Initialize;
  }

  void OnDisable()
  {
    Roll.OnRollCompleted -= ResolveRoll;
    LevelManager.OnLevelChanged -= Initialize;
  }

  void OnDestroy()
  {
    DOTween.Kill(m_TargetBoxParent);
  }

  void Initialize(LevelData levelData)
  {
    RecycleAll();
    
    var colors = levelData.ColorOrder;
    m_TargetBoxIndex = 0;
    m_TargetBoxes    = new TargetBox[colors.Length];

    m_TargetBoxParent.position = m_DefaultParentPosition;
    
    float offset = 0f;
    for (var i = 0; i < colors.Length; i++)
    {
      var spawned = m_Pool.Get();

      spawned.Initialize(colors[i]);
      spawned.transform.localPosition = new Vector3(offset, 0, 0);

      m_TargetBoxes[i] = spawned;

      offset -= m_MoveOffsetX;
    }

    m_TargetBoxes[0].OnComplete += NextTarget;
  }

  void NextTarget()
  {
    m_TargetBoxes[m_TargetBoxIndex].OnComplete -= NextTarget;
    m_TargetBoxIndex++;

    if (m_TargetBoxIndex >= m_TargetBoxes.Length)
    {
      GameManager.ChangeState.Invoke(GameState.Completed);
      return;
    }

    m_TargetBoxParent.DOLocalMoveX(m_MoveOffsetX, .13f).SetRelative(true).SetDelay(.25f)
      .OnComplete(GetWaitingRolls).SetId(m_TargetBoxParent);
    m_TargetBoxes[m_TargetBoxIndex].OnComplete += NextTarget;
  }

  void ResolveRoll(Roll roll)
  {
    if (roll.Color == m_TargetBoxes[m_TargetBoxIndex].Color)
    {
      m_TargetBoxes[m_TargetBoxIndex].Add(roll);
    }
    else
    {
      m_WaitingRolls.Add(roll);

      UpdatePositions();
      
      if (m_WaitingRolls.Count >= 7 && CanResolve() == false)
      {
        GameManager.ChangeState.Invoke(GameState.Failed);
      }
    }
  }

  bool CanResolve()
  {
    var rolls = m_WaitingRolls.Where(r => r.Color == m_TargetBoxes[m_TargetBoxIndex].Color).Take(3).ToArray();

    if (rolls.Length <= 0)
    {
      return false;
    }

    return true;
  }

  void UpdatePositions()
  {
    for (int i = 0; i < m_WaitingRolls.Count; i++)
    {
      var waitingRoll = m_WaitingRolls[i];
        
      var jumpPos = m_WaitingPositions[i].position;
      jumpPos.y += .03f;

      if (waitingRoll.transform.position != jumpPos)
      {
        waitingRoll.transform.DOJump(jumpPos, .1f, 1, .23f).SetId(waitingRoll.transform);
      }
    }
  }

  void GetWaitingRolls()
  {
    var rolls = m_WaitingRolls.Where(r => r.Color == m_TargetBoxes[m_TargetBoxIndex].Color).Take(3).ToArray();
    foreach (var roll in rolls)
      if (roll)
      {
        m_TargetBoxes[m_TargetBoxIndex].Add(roll);
        m_WaitingRolls.Remove(roll);
      }
  }

  void RecycleAll()
  {
    m_WaitingRolls.Clear();
    foreach (var targetBox in m_TargetBoxes)
    {
      m_Pool.Release(targetBox);
    }
  }
}
