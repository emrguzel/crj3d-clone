using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Roll : MonoBehaviour
{
  public static event Action<int> OnRollIdCompleted;
  public static event Action<Roll> OnRollCompleted;

  [SerializeField] GameObject m_OutlineObject;

  public Color Color { get; private set; }
  int          m_ID;
  HashSet<int> m_ParentIDs = new();

  bool m_IsHardLocked;
  bool m_IsTweening;
  
  #if UNITY_EDITOR
  int[] m_DebugParentIDs;
  #endif

  static readonly Vector3 kSCALE_DOWN = new(0.06478706f, 0.00999999978f, 0.0608186382f);

  void OnEnable()
  {
    OnRollIdCompleted += RollIdOnRollIdCompleted;
  }
  
  void OnDisable()
  {
    OnRollIdCompleted -= RollIdOnRollIdCompleted;
    DOTween.Kill(transform);
  }

  void OnDestroy()
  {
    DOTween.Kill(transform);
  }

  public void Initialize(int rollDataID, int[] rollDataParents, Color color)
  {
    Color          = color;
    m_ID           = rollDataID;
    m_ParentIDs    = rollDataParents.ToHashSet();
    m_IsHardLocked = false;
    
    #if UNITY_EDITOR
    m_DebugParentIDs = m_ParentIDs.ToArray();
    #endif

    if (!IsLocked())
    {
      m_OutlineObject.SetActive(true);
    }
    
    GetComponent<MeshRenderer>().material.color = Color;
  }

  public void OnClick()
  {
    if (IsLocked())
    {
      ClickFailed();
      return;
    }
    
    ClickSuccess();
  }

  void RollIdOnRollIdCompleted(int removedID)
  {
    m_ParentIDs.Remove(removedID);

    #if UNITY_EDITOR
    m_DebugParentIDs = m_ParentIDs.ToArray();
    #endif

    if (!IsLocked())
    {
      m_OutlineObject.SetActive(true);
    }
  }

  bool IsLocked()
  {
    if (m_IsHardLocked)
    {
      return true;
    }
    return m_ParentIDs.Any();
  }

  void ClickSuccess()
  {
    m_IsHardLocked = true;
    m_OutlineObject.SetActive(false);
    
    ScaleDown();
  }

  void ClickFailed()
  {
    if (m_IsTweening)
    {
      return;
    }
    transform.DOShakeScale(.13f, .01f).SetId(transform);
  }
  
  void ScaleDown()
  {
    var     originalPosition = transform.position; 
    Vector3 newPosition      = originalPosition;

    Vector3 targetScale     = kSCALE_DOWN;
    Vector3 scaleDifference = targetScale - transform.localScale;
    Vector3 worldOffset     = transform.TransformDirection(Vector3.back) * scaleDifference.z / 2;
    
    newPosition -= worldOffset;

    Sequence seq = DOTween.Sequence().SetId(transform)
      .OnStart(()=> m_IsTweening = true)
      .OnComplete(RollCompleted);
    
    var scaleDown = transform.DOScale(targetScale, .5f);
    var move = transform.DOMove(newPosition, .5f);

    seq.Append(scaleDown);
    seq.Join(move);
  }

  void RollCompleted()
  {
    m_IsTweening = false;
    OnRollIdCompleted?.Invoke(m_ID);
    OnRollCompleted?.Invoke(this);
  }
}
