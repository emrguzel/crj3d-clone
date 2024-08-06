using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static Action<GameState> ChangeState;
  public static event Action<GameState> GameStateChanged;

  GameState GameState { get; set; } = GameState.None;

  void Start()
  {
    SwitchState(GameState.Gameplay);
  }

  void OnEnable()
  {
    ChangeState += SwitchState;
  }
  
  void OnDisable()
  {
    ChangeState -= SwitchState;
  }

  void SwitchState(GameState newState)
  {
    if (GameState == newState)
      return;

    GameState = newState;
    GameStateChanged?.Invoke(GameState);
  }
}