using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Mission.Generator.Jobs;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.PathFinding;
using CableWalker.Simulator.Solvers;
using CableWalker.Simulator.Tools;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Generator
{
    /// <summary>
    /// Класс, отвечающий за генерацию оптимальной миссии.
    /// </summary>
    public class MissionGenerator
    {
        private readonly PathFinder pathFinder;
        private readonly PairFinder pairFinder;

        private readonly InformationHolder infoHolder;
        
        public GenerationResult Result { get; private set; }

        public MissionGenerator(PathFinder pathFinder, PairFinder pairFinder)
        {
            this.pathFinder = pathFinder;
            this.pairFinder = pairFinder;
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
        }

       

        /// <summary>
        /// Сгенерировать миссию.
        /// </summary>
        /// <param name="jobs">Задание</param>
        /// <param name="cableStep">Шаг разбиения кабеля на точки</param>
        /// <param name="angleLimit">Предел угла для определения точки посадки</param>
        /// <param name="startPoint">Заданная точка начала миссии, где спавнится канатоход</param>
        /// <returns></returns>
        public IEnumerator Generate(IEnumerable<IAtomicJob<Model.Model>> jobs, float cableStep, float angleLimit, Vector3 startPoint)
        {
            var commands = new List<Command>();
            var objectsInfo = infoHolder;
            
            // Разбиваем задания по пролётам, для которых они предназначаются.
            var jobsBySpans = jobs
                .GroupBy(s => s.Target.GetSpan())
                .OrderBy(g => g.Key)
                .Where(g => g.Key != null)
                .ToList();
            
            startPoint = new Vector3(startPoint.x, TerrainUtils.GetSampleHeight(startPoint), startPoint.z);
            
            var currentPosition = Vector3.zero;
            
            for (var i = 0; i < jobsBySpans.Count; i++)
            {
                var currentJobsSpan = jobsBySpans[i];
                var currentCable = objectsInfo.Get<Cable>($"{currentJobsSpan.Key}-{int.Parse(currentJobsSpan.Key) + 1}.A");
                if (currentCable == null)
                {
                    Debug.LogError($"Can't find cable {currentJobsSpan.Key}-{int.Parse(currentJobsSpan.Key) + 1}.A");
                    yield break;
                }

                var tStep = cableStep / currentCable.Length;
                
                var photoTargetList = new List<Model.Model>();
                var magnetTargetList = new List<Cable>();
                foreach (var subJob in currentJobsSpan)
                {
                    if (subJob is PhotoDiagnosticJob photoDiagnosticJob)
                        photoTargetList.Add(photoDiagnosticJob.Target);
                    else if (subJob is MagnetDiagnosticJob magnetDiagnosticJob)
                        magnetTargetList.Add(magnetDiagnosticJob.Target);
                }
                var photoTargets = photoTargetList.ToArray();
                var magnetTargets = magnetTargetList.ToArray();
                
                var pointList = new List<Vector3>();
                for (var t = 0.0f; t <= 1.0f; t += tStep)
                    pointList.Add(currentCable.GetPoint(t));
                var points = pointList.ToArray();

                var weights = new float[points.Length, photoTargets.Length];
                for (var targetIdx = 0; targetIdx < photoTargets.Length; targetIdx++)
                    for (var pointIdx = 0; pointIdx < points.Length; pointIdx++)
                    {
                        var target = photoTargets[targetIdx];
                        var point = points[pointIdx];

                        var distance = Vector3.Distance(target.Position, point);

                        // TODO-nexusasx10: Использовать Spherecast вместо Raycast.
                        // if (Physics.Raycast(point, target.Position - point, out var hitInfo, distance) && hitInfo.transform == target.ObjectOnScene.transform)
                        weights[pointIdx, targetIdx] = distance; // TODO-nexusasx10: Учитывать ракурсы.
                                                                 // else
                                                                 //     weights[pointIdx, targetIdx] = float.PositiveInfinity;
                    }
                
                yield return pairFinder.Solve(weights, 100);

                if (pairFinder.Solution == null)
                {
                    Result = new GenerationResult(GenerationStatus.CombinationNotFound, null, null);
                    yield break;
                }
                
                var cableStart = currentCable.GetPoint(0);
                var cableEnd = currentCable.GetPoint(1);
                
                // TODO-nexusasx10: Учитывать ограничения на углы посадки и взлёта.
                var firstPoint = cableStart;
                var firstPointT = 0.0f;
                var lastPoint = cableEnd;
                
                if (i == 0)
                {
                    if (!magnetTargets.Contains(currentCable))
                    {
                        for (var t = 0.0f; t <= 1.0f; t += tStep)
                        {
                            var point = currentCable.GetPoint(t);

                            var a = t > 0.01f ? t - 0.01f : 0;
                            var b = t < 0.99f ? t + 0.01f : 1;
                            var direction = currentCable.GetPoint(b) - currentCable.GetPoint(a);
                            var angle = Vector3.Angle(Vector3.up, direction);

                            if (Vector3.Distance(point, startPoint) < Vector3.Distance(firstPoint, startPoint) &&
                                angle > 90 - angleLimit && angle < 90 + angleLimit)
                            {
                                firstPoint = point;
                                firstPointT = t;
                            }
                        }
                    }

                    commands.Add(new SetStart(new Vector2(startPoint.x, startPoint.z)));
                    currentPosition = startPoint;
                }

                if (i == jobsBySpans.Count - 1)
                {
                    if (!magnetTargets.Contains(currentCable))
                    {
                        lastPoint = cableStart;
                        for (var targetIdx = 0; targetIdx < photoTargets.Length; targetIdx++)
                        for (var pointIdx = 0; pointIdx < points.Length; pointIdx++)
                        {
                            if (pairFinder.Solution[pointIdx, targetIdx])
                            {
                                var point = points[pointIdx];
                                var t = pointIdx * tStep;

                                var a = t > 0.01f ? t - 0.01f : 0;
                                var b = t < 0.99f ? t + 0.01f : 1;
                                var direction = currentCable.GetPoint(b) - currentCable.GetPoint(a);
                                var angle = Vector3.Angle(Vector3.up, direction);

                                if (Vector3.Distance(point, cableEnd) < Vector3.Distance(lastPoint, cableEnd) &&
                                    angle > 90 - angleLimit && angle < 90 + angleLimit)
                                    lastPoint = point;
                            }
                        }
                    }
                }
                
                var upperCount = 0;
                while (Physics.OverlapSphere(currentPosition + Vector3.up * upperCount, 0.5f).Length > 0)
                    upperCount++;
                
                commands.Add(new StartEngines());
                commands.Add(new TakeOffGround());
                
                currentPosition += Vector3.up * upperCount;
                
                var flyTarget = firstPoint;
                while (Physics.OverlapSphere(flyTarget, 0.5f).Length > 0)
                    flyTarget += Vector3.up;
                    
                yield return pathFinder.FindPath(currentPosition, flyTarget);
                if (pathFinder.Solution == null)
                {
                    Result = new GenerationResult(GenerationStatus.PathNotFound, null, (currentPosition, flyTarget));
                    yield break;
                }
                commands.AddRange(pathFinder.Solution.Select(n => new FlyTo(n.x, n.y, n.z)));
                
                // todo-nexusasx10: Нужно дать возможность указать, на какие кабели можно сесть, а если их несколько, рассматривать точки на всех.
               // commands.Add(new SitOnCable(int.Parse(currentCable.Start.Tower.Number), int.Parse(currentCable.End.Tower.Number), "A", firstPointT));
                commands.Add(new StopEngines());

                currentPosition = flyTarget;
                
                if (magnetTargets.Contains(currentCable))
                    commands.Add(new StartMagnetScanning());
                
                for (var pointIdx = 0; pointIdx < points.Length; pointIdx++)
                for (var targetIdx = 0; targetIdx < photoTargets.Length; targetIdx++)
                {
                    if (pairFinder.Solution[pointIdx, targetIdx])
                    {
                        var target = photoTargets[targetIdx];
                        var point = points[pointIdx];

                        var distanceDiff = Vector3.Distance(currentPosition, point);

                        var speed = 2;
                        if (Vector3.Distance(currentPosition, cableStart) > Vector3.Distance(point, cableStart))
                            speed *= -1;
                        
                        if (distanceDiff > 0.001)
                        {
                            //commands.Add(new WheelModuleMove(distanceDiff, speed)); 26.02
                            currentPosition = point;
                        }

                        commands.Add(new VideoCameraLookAtInsulator(target.Number));
                        commands.Add(new VideoCameraTakePhoto());
                        // У камеры есть задержка между операциями фотографирования. Пока что она симулируется здесь.
                        commands.Add(new Wait(2));
                    }
                }
                
                if (magnetTargets.Contains(currentCable))
                    commands.Add(new StopMagnetScanning());

                currentPosition = lastPoint;

                if (i == jobsBySpans.Count - 1)
                {
                    upperCount = 0;
                    while (Physics.OverlapSphere(currentPosition + Vector3.up * upperCount, 0.5f).Length > 0)
                        upperCount++;

                    commands.Add(new StartEngines());
                    commands.Add(new TakeOffGround());
                    
                    currentPosition += new Vector3(0, upperCount, 0);
                    flyTarget = new Vector3(currentPosition.x, TerrainUtils.GetSampleHeight(currentPosition) + 1, currentPosition.z);
                
                    yield return pathFinder.FindPath(currentPosition, flyTarget);
                    if (pathFinder.Solution == null)
                    {
                        Result = new GenerationResult(GenerationStatus.PathNotFound, null, (currentPosition, flyTarget));
                        yield break;
                    }
                    commands.AddRange(pathFinder.Solution.Select(n => new FlyTo(n.x, n.y, n.z)));
                    
                    commands.Add(new StopEngines());
                }

                yield return null;
            }

            Result = new GenerationResult(GenerationStatus.Success, commands, null);
        }
    }
}
