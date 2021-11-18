using UnityEngine;

namespace CableWalker.Simulator
{
    public static class LodGroupInstaller
    {
        public static void InstallLodGroupRenderers(GameObject obj)
        {
            var lodGroup = obj.AddComponent<LODGroup>();
            var lods = new LOD[2];
            var childRenderers = obj.GetComponentsInChildren<Renderer>();

            lods[0] = new LOD(0.03f, childRenderers);
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
        }
    }
}
