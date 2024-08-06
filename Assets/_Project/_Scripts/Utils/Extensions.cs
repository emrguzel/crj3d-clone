using System;
using DG.Tweening;
using UnityEngine;

public static class Extensions
{
  public static void Open(this CanvasGroup cg, float delay = 0, Action onComplete = null)
  {
    cg.DOFade(1, .23f).SetId(cg).SetDelay(delay)
      .OnComplete(() =>
      {
        cg.interactable   = true;
        cg.blocksRaycasts = true;
        onComplete?.Invoke();
      });
  }
  
  public static void Close(this CanvasGroup cg, float delay = 0, Action onComplete = null)
  {
    cg.DOFade(0, .13f).SetId(cg).SetDelay(delay)
      .OnComplete(() =>
      {
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        onComplete?.Invoke();
      });
  }
}