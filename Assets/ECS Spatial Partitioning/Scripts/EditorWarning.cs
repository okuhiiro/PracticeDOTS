#if UNITY_EDITOR
namespace ECS_Spatial_Partitioning.Scripts
{
    using UnityEngine;
    public class EditorWarning : MonoBehaviour
    {
        private string warningMessage = "Kindly note, the runtime performance for ECS in editor is significantly worse than the performance in a standalone build." +
                                        "\n" + "Please refer to the provided PDF for more information.";
        private void OnGUI()
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            var warningSize = GUI.skin.label.CalcSize(new GUIContent(warningMessage));
            var warningPosition = new Vector2((screenWidth - warningSize.x) / 2f, (screenHeight - warningSize.y));
            GUI.Label(new Rect(warningPosition.x, warningPosition.y - 100f, warningSize.x, warningSize.y), warningMessage);    
        }
    }
}
#endif