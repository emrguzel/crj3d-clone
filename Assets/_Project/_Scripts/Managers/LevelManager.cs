using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
  public static event Action<LevelData> OnLevelChanged;
  [SerializeField] Camera               m_Camera;

  [SerializeField] Roll m_RollObject;
  [SerializeField] LevelData[] m_Levels;

  RaycastHit m_Hit;

  int m_LevelIndex;

  bool m_CanRun = false;
  
  const string kLEVEL_KEY = "m_LevelIndex";

  void Start()
  {
    m_LevelIndex = PlayerPrefs.GetInt(kLEVEL_KEY, 0);
  }

  void OnEnable()
  {
    GameManager.GameStateChanged += OnGameStateChanged;
  }
  
  void OnDisable()
  {
    GameManager.GameStateChanged -= OnGameStateChanged;
  }

  void Update()
  {
    if (m_CanRun == false || !Input.GetMouseButtonDown(0))
      return;
    
    int mask = 0x1 << 31;
    
    Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
    
    if (Physics.Raycast(ray, out m_Hit,float.PositiveInfinity, mask))
    {
      if (m_Hit.transform.TryGetComponent(out Roll roll))
      {
        roll.OnClick();
      }
    }
  }

  void OnGameStateChanged(GameState newState)
  {
    switch (newState)
    {
      case GameState.Completed:
        m_LevelIndex++;
        PlayerPrefs.SetInt(kLEVEL_KEY, m_LevelIndex);
        m_CanRun = false;
        break;
      
      case GameState.Failed:
        m_CanRun = false;
        break;
      
      case GameState.Gameplay:
        LoadLevel(m_LevelIndex);
        m_CanRun = true;
        break;
    }
  }

  void LoadLevel(int index)
  {
    OnLevelChanged?.Invoke(m_Levels[index % m_Levels.Length]);
  }
}
