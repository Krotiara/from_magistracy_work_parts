using CableWalker.Simulator.Model;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator
{
    public class SpanWatchers : MonoBehaviour
    {
        public InformationHolder informationHolder;
        public static List<SpanWatcher> watchers;
        public GameObject Camera;
        public GameObject towerStringsLabels;
        public GameObject spansLabels;

        private GameObject holder;

        public void StartWatchers()
        {
            watchers = new List<SpanWatcher>();
            holder = new GameObject("WatchersHolder");
            Camera = GameObject.FindGameObjectWithTag("SpectatorCamera");
            towerStringsLabels = GameObject.FindGameObjectWithTag("TSLabels");
            spansLabels = GameObject.FindGameObjectWithTag("SpansLabels");
            foreach (Tower tower in informationHolder.GetList<Tower>())
                foreach (Tower nextTower in tower.NextTowers)
                {
                    GameObject objOnScene =
                        new GameObject($"Span {tower.Number}-{nextTower.Number} Watcher");
                    objOnScene.transform.position = (tower.Position + nextTower.Position) / 2;
                    objOnScene.transform.parent = holder.transform;
                    objOnScene.SetActive(false);
                    var watcher = objOnScene.AddComponent<SpanWatcher>();
                    watcher.StartWatching(
                        (tower,nextTower),
                        Camera,
                        (nextTower.Position - tower.Position).magnitude,
                        informationHolder, towerStringsLabels, spansLabels);
                }
        }

        public void Clear()
        {
            foreach (SpanWatcher s in watchers)
                s.labels = new List<GameObject>().ToArray();
            watchers = new List<SpanWatcher>();
            foreach (Transform t in holder.transform)
            {              
                GameObject.Destroy(t.gameObject);
            }
            GameObject.Destroy(holder);
        }

      

        private void OnApplicationQuit()
        {
            Destroy(holder);
        }
    }
}
