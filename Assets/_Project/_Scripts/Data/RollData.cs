using System;
using UnityEngine;

[Serializable]
public struct RollData
{
  public Vector3    Position;
  public Quaternion Rotation;
  public Vector3    Scale;

  public int   ID;
  public int[] Parents;

  public Color Color;

  public RollData(Vector3 position, Quaternion rotation, Vector3 scale, int id)
  {
    Position = position;
    Rotation = rotation;
    Scale    = scale;

    ID      = id;
    Color   = Color.white;
    Parents = Array.Empty<int>();
  }
}