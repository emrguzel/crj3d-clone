using System;
using DG.Tweening;
using UnityEngine;

public class TargetBox : MonoBehaviour
{
  public event Action OnComplete;
  
  [SerializeField] Transform[] m_Positions;
  
  public Color Color { get; private set; }
  int          m_Count = 0;

  public void Initialize(Color color)
  {
    Color   = color;
    m_Count = 0;
    
    var color1 = Color;
    color1.a = 1;
    
    Color    = color1;

    GetComponent<MeshRenderer>().material.color = color;
  }

  public void Add(Roll roll)
  {
    var pos = m_Positions[m_Count].localPosition;

    pos.y += .23f;
    roll.transform.SetParent(transform);
    roll.transform.DOLocalJump(pos, .1f, 1, .23f).SetId(roll.transform);
    
    m_Count++;

    if (m_Count == m_Positions.Length)
    {
      OnComplete?.Invoke();
    }
  }
}
