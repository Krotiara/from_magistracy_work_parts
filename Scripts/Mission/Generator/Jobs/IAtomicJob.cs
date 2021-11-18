using CableWalker.Simulator.Mission.Commands;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Generator.Jobs
{
    /// <summary>
    /// Интерфейс задания.
    /// </summary>
    /// <typeparam name="T">Тип объекта на сцене, над которым будет выполняться задание.</typeparam>
    public interface IAtomicJob<out T> where T : Model.Model
    {
        T Target { get; }
        List<Command> Commands { get; }
    }
}
