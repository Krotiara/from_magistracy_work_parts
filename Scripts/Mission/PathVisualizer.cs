using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Tools;
using UnityEngine;
using UnityEngine.Rendering;

namespace CableWalker.Simulator.Mission
{
    public class PathVisualizer : Singleton<PathVisualizer>
    {
        private int nextId;
        private List<LineRenderer> renderers;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        protected override void Awake()
        {
            base.Awake();
            renderers = new List<LineRenderer>();
        }

        public int Draw(IEnumerable<Vector3> path, Color color, float width)
        {
            var obj = new GameObject { name = nextId.ToString() };
            var lineRenderer = obj.AddComponent<LineRenderer>();
            var material = new Material(Shader.Find("Standard")) { color = color };
            material.EnableKeyword("_EMISSION");
            material.SetColor(EmissionColor, color);
            lineRenderer.material = material;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            obj.transform.SetParent(transform);
            
            var points = path.ToArray();
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);

            renderers.Add(lineRenderer);
            return nextId++;
        }

        public void Clear(int id)
        {
            if (id >= renderers.Count)
                return;

            var lineRenderer = renderers[id];
            renderers[id] = null;
            
            if (lineRenderer == null)
                return;

            Destroy(lineRenderer);
        }
        
        public void ClearAll()
        {
            for (var i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] == null)
                    continue;
                
                Destroy(renderers[i]);
                renderers[i] = null;
            }
        }
    }
}
