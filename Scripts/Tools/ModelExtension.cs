using CableWalker.Simulator.Model;
using UnityEngine;

namespace CableWalker.Simulator.Tools
{
    public static class ModelExtension
    {
        /// <summary>
        /// Возвращает номер пролёта, к которому принадлежит элемент.
        /// </summary>
        /// <param name="model">Элемент линии</param>
        /// <returns></returns>
        public static string GetSpan(this Model.Model model)
        {
            switch (model)
            {
                case InsulatorString str:
                    if (str.IsIntermediate || str.Tower.Number == str.RelativeInsulatorString.Tower.Number)
                        return (int.Parse(str.Tower.Number) - 1).ToString();
                    return str.Tower.Number;
                case Cable cable:
                    return cable.Start.Tower.Number;
                case Tower tower:
                    return tower.Number;
                default:
                    Debug.LogError($"Определение пролёта для {model.GetType()} не поддерживается.");
                    return null;
            }
        }
    }
}
