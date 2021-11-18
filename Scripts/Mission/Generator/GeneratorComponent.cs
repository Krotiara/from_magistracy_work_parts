using System.Collections.Generic;
using CableWalker.Simulator.Mission.Generator.Jobs;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.PathFinding;
using CableWalker.Simulator.Solvers;
using CableWalker.Simulator.Tools;
using TMPro;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Generator
{
    public class GeneratorComponent : MonoBehaviour
    {
        private MissionGenerator generator;
        private float startTime;
        private PathFinder pathFinder;
        private PairFinder pairFinder;
        
        public TMP_InputField OutputField;
        public bool Visualize;

        private InformationHolder infoHolder;
        
        private void Awake()
        {
            pathFinder = new PathFinder(new AStar<BinaryHeap<Vertex>>());
            pairFinder = new PairFinder();
            generator = new MissionGenerator(pathFinder, pairFinder);
        }

        private void Start()
        {
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
        }

        private void Update()
        {
            if (generator.Result != null)
            {
                switch (generator.Result.Status)
                {
                    case GenerationStatus.Success:
                        Debug.Log($"Finished in {Time.realtimeSinceStartup - startTime}");
                        OutputField.text = generator.Result.CommandList.Format();
                        break;
                    case GenerationStatus.PathNotFound:
                        PathVisualizer.Instance.Draw(new [] { generator.Result.BadPath.Value.Item1, generator.Result.BadPath.Value.Item2 } , Color.red, 0.1f);
                        Debug.LogError("Path not found.");
                        break;
                    case GenerationStatus.CombinationNotFound:
                        Debug.LogError("Combination not found.");
                        break;
                }

                FindObjectOfType<MissionVisualizer>().Visualize();

                generator = new MissionGenerator(pathFinder, pairFinder);
            }
        }

        public void Run()
        {
            startTime = Time.realtimeSinceStartup;
            Debug.Log("Generation started.");

            var jobs = new List<IAtomicJob<Model.Model>>();

            for (var i = 115; i < 116; i++)
            {
                jobs.Add(new MagnetDiagnosticJob(infoHolder.Get<Cable>($"{i}-{i + 1}.A")));
                jobs.Add(new PhotoDiagnosticJob(infoHolder.Get<InsulatorString>($"{i + 1}.1"), 0, 0));
                jobs.Add(new PhotoDiagnosticJob(infoHolder.Get<InsulatorString>($"{i + 1}.2"), 0, 0));
                jobs.Add(new PhotoDiagnosticJob(infoHolder.Get<InsulatorString>($"{i + 1}.3"), 0, 0));
            }
            
            CoroutineBalancer.Instance.StartCoroutineBalance(generator.Generate(jobs, 5, 15, new Vector3(-745f, 34f, -698f)));
        }

        private void OnDrawGizmos()
        {
            if (!Visualize || pathFinder == null || pathFinder.Visited == null)
                return;

            Gizmos.color = Color.blue;

            foreach (var point in pathFinder.Visited)
                Gizmos.DrawSphere(point, 0.1f);
        }
    }
}
