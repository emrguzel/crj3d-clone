using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "LevelData Overlay", true)]
public class LevelDataOverlay : Overlay
{
  public static LevelDataOverlay Instance;

  public LevelData LevelData;
  public int       Index;
  
  ColorField m_ColorField;
  Label      m_LabelID;

  public override void OnCreated()
  {
    base.OnCreated();
    Instance = this;
  }


  public override VisualElement CreatePanelContent()
  {
    var root = new VisualElement { name = "Level Data Editor" };
    m_ColorField = new ColorField("Color:");
    m_LabelID    = new Label("ID: Not selected.");
    root.Add(m_LabelID);
    root.Add(m_ColorField);
    
    m_ColorField.RegisterValueChangedCallback(OnColorChanged); 
    
    return root;
  }

  void OnColorChanged(ChangeEvent<Color> evt)
  {
    LevelData.Data[Index].Color = evt.newValue;
  }

  public void SetData(int id, Color color)
  {
    if (m_LabelID != null)
    {
      m_LabelID.text = $"ID: {id}";
    }
    if (m_ColorField != null)
    {
      m_ColorField.value = color;
    }
  }
}