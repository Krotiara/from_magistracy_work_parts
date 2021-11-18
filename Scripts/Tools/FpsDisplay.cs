using UnityEngine;

namespace CableWalker.Simulator.Tools
{
    public enum Corner
    {
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom
    }
    
    public class FpsDisplay : MonoBehaviour
    {
        public FpsEncounter FpsEncounter;
        public Corner Corner;
        public Color TextColor;
        public Color BackgroundColor;
        public Vector2 Indent;
        public float Scale;

        private float lastCheckTime;
        private bool wait;

        private void Update()
        {
            if (wait && Time.realtimeSinceStartup - lastCheckTime < 1)
                return;

            wait = false;
            
            if (FpsEncounter.Fps <= 5)
            {
                Debug.LogWarning($"Critical fps drop detected: {FpsEncounter.Fps}.");
                lastCheckTime = Time.realtimeSinceStartup;
                wait = true;
            }
            else if (FpsEncounter.Fps <= 15)
            {
                Debug.LogWarning($"Fps drop detected: {FpsEncounter.Fps}.");
                lastCheckTime = Time.realtimeSinceStartup;
                wait = true;
            }
        }
        
        private void OnGUI()
        {
            var width = Screen.width;
            var height = Screen.height;

            var style = new GUIStyle();
            style.alignment = TextAnchor.LowerRight;
            style.fontSize = (int) (Scale * height * 2 / 100);
            style.normal.textColor = TextColor;
            
            var text = $"{FpsEncounter.Fps:0.} fps";

            var size = style.CalcSize(new GUIContent(text));
            
            var x = Corner == Corner.LeftBottom || Corner == Corner.LeftTop
                ? Indent.x
                : width - size.x - Indent.x;
            var y = Corner == Corner.LeftTop || Corner == Corner.RightTop
                ? Indent.y
                : height - size.y - Indent.y;
            var rect = new Rect(x, y, size.x, size.y);
            
            var texture = new Texture2D((int)size.x, (int)size.y);
            var fillColorArray =  texture.GetPixels();
            for(var i = 0; i < fillColorArray.Length; i++)
                fillColorArray[i] = BackgroundColor;
            texture.SetPixels(fillColorArray);
            texture.Apply();
            
            GUI.Label(rect, texture, style);
            GUI.Label(rect, text, style);
        }
    }
}
