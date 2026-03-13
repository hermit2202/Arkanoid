namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Типы усилений, которые могут выпасть из блоков.
    /// </summary>
    public enum PowerUpType
    {
        /// <summary>
        /// Увеличивыет ширену платформы на 50%.
        /// </summary>
        WidePlatform,

        /// <summary>
        /// Делает мяц огненным: он пробивает блоки насквозь без отскока.
        /// </summary>
        FireBall,

        /// <summary>
        /// Увеличивает скорость мяча на 40%.
        /// </summary>
        FastBall
    }
}
