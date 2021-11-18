using CableWalker.Simulator.Model;
using CableWalker.Simulator.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator
{
    public class SpanWatcher : MonoBehaviour
    {
        public List<Cable> Cables { get; private set; }
        public GameObject MainCamera { get; private set; }
        public float ActivationDistance { get; private set; }
        public Vector3 Position { get; private set; }
        public GameObject[] labels;
        CableDisplayMode mode;
        bool displayMode = true;
        float currentDistance;
        
        public void StartWatching((Tower,Tower) span, GameObject camera, float spanDistance, InformationHolder infoHolder, GameObject towerStringsLabels, GameObject spansLabels)
        {
            MainCamera = camera;
            Cables = infoHolder.GetCablesBetweenTowers(span.Item1, span.Item2);
            labels = GetLabelsInSpan(span, towerStringsLabels, spansLabels);
            ActivationDistance = spanDistance;
            gameObject.SetActive(true);
            Position = transform.position;
            
            //StartCoroutine("CheckDisplayModeActivation");
        }

        private GameObject[] GetLabelsInSpan((Tower,Tower) span, GameObject towerStringsLabels, GameObject spansLabels)
        {
            var result = new List<GameObject>();
            result.Add(towerStringsLabels.transform.Find(span.Item1.Number).gameObject);
            result.Add(towerStringsLabels.transform.Find(span.Item2.Number).gameObject);
            result.Add(spansLabels.transform.Find($"{span.Item1.Number}-{span.Item2.Number}").gameObject);
            //foreach (Transform child in towerStringsLabels.transform.Find(span.Item1.Number))
            //    result.Add(child.gameObject);
            //foreach (Transform child in towerStringsLabels.transform.Find(span.Item2.Number))
            //    result.Add(child.gameObject);
            //foreach (Transform child in spansLabels.transform.Find($"{span.Item1.Number}-{span.Item2.Number}"))
            //    result.Add(child.gameObject);
            return result.ToArray();
        }

        private void Update()
        {
            currentDistance = (MainCamera.transform.position - Position).magnitude;
            mode = currentDistance < ActivationDistance ?
                CableDisplayMode.Primitive : CableDisplayMode.LineRenderer;
            displayMode = currentDistance < ActivationDistance;
            //if (mode != Cables[0].DisplayMode) //14.02.2020 пока что отключил
            //{
            //    foreach (var cable in Cables)
            //            cable.Regenerate(mode);
            //}
           
            for (var i = 0; i < labels.Length; i++)
                labels[i].SetActive(displayMode);
                    //labels[i].GetComponent<LookAtCamera>().enabled = displayMode;               
            
        }

        //IEnumerator CheckDisplayModeActivation()
        //{
        //    CableDisplayMode mode;
        //    mode = (MainCamera.transform.position - this.transform.position).magnitude < ActivationDistance ?
        //        CableDisplayMode.Primitive : CableDisplayMode.LineRenderer;
        //    if (mode != Cables[0].CableComponents.DisplayMode)
        //    {
        //        for (int i = 0; i < Cables.Count; i++)
        //        {
        //            Cables[i].CableComponents.Regenerate(mode);
        //        }
        //    }
        //    yield return new WaitForSeconds(1f);
        //    StartCoroutine("CheckDisplayModeActivation");
        //}
    }
}
