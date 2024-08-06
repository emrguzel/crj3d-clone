using DG.Tweening;
using UnityEngine;

public class UIManager : MonoBehaviour
{
  [SerializeField] CanvasGroup m_CompletedPanel;
  [SerializeField] CanvasGroup m_FailPanel;
    
  void OnEnable()
  {
    GameManager.GameStateChanged += OnGameStateChanged;
  }
  
  void OnDisable()
  {
    GameManager.GameStateChanged -= OnGameStateChanged;
  }

  void OnDestroy()
  {
    DOTween.Kill(m_CompletedPanel);
    DOTween.Kill(m_FailPanel);
  }

  public void OnButtonClicked()
  {
    GameManager.ChangeState.Invoke(GameState.Gameplay);
  }

  void OnGameStateChanged(GameState newState)
  {
    switch (newState)
    {
      case GameState.Completed:
        m_CompletedPanel.Open(delay: .5f);
        break;
      case GameState.Failed:
        m_FailPanel.Open(delay: .5f);
        break;
      case GameState.Gameplay:
        m_FailPanel.Close();
        m_CompletedPanel.Close();
        break;
    }
  }
}