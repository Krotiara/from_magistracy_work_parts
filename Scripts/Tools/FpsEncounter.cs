using System.Collections;
using UnityEngine;

namespace CableWalker.Simulator.Tools
{
    public class FpsEncounter : MonoBehaviour
    {
        private bool needUpdate;
    
        public float UpdateDelay;
        public float Fps { get; private set; }

        private void Awake()
        {
            needUpdate = true;
        }

        private void Update()
        {
            if (!needUpdate)
                return;
            
            Fps = 1.0f / Time.unscaledDeltaTime;
            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            needUpdate = false;
            yield return new WaitForSeconds(UpdateDelay);
            needUpdate = true;
        }
    }
}
