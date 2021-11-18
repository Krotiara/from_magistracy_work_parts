namespace CableWalker.Simulator.Tools
{
    /// <summary>
    /// Утилитарный класс для работы с углами.
    /// </summary>
    public static class AngleHelper
    {
        /// <summary>
        /// Нормализует угол в диапазоне от -180 до 180.
        /// </summary>
        /// <param name="angle">Исходный угол в градусах</param>
        /// <returns></returns>
        public static float NormalizeSigned(float angle)
        {
            angle = (angle + 180) % 360;
            
            if (angle < 0)
                angle += 360;
            
            angle = angle - 180;

            if (angle == -180)
                return 180;

            return angle;
        }
        
        /// <summary>
        /// Нормализует угол в диапазоне от 0 до 360.
        /// </summary>
        /// <param name="angle">Исходный угол в градусах</param>
        /// <returns></returns>
        public static float NormalizeUnsigned(float angle)
        {
            angle %= 360;
            
            if (angle < 0)
                angle += 360;
            
            return angle;
        }
    }
}
