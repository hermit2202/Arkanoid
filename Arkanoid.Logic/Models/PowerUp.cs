using System.Drawing;

namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Представляет падающий бонус, который можно поймать платформой.
    /// Содержит позицию, тип, скорость и состояние активности.
    /// </summary>
    public class PowerUp
    {
        /// <summary>
        /// Горизонтальная позиция усиления.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Вертикальная позиция усиления.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Тип бонуса, определяющий его эффект.
        /// </summary>
        public PowerUpType Type { get; }

        /// <summary>
        /// Флаг активности: true, если бонус ещё не подобран или не упал за экран.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Скорость падения бонуса.
        /// </summary>
        private float Speed { get; } = 150f;

        /// <summary>
        /// Размер бонуса в пикселях.
        /// </summary>
        public int Size { get; } = 25;

        /// <summary>
        /// Возвращает прямоугольную область бонуса для расчёта колизий.
        /// </summary>
        public RectangleF Rect => new RectangleF(X, Y, Size, Size);

        /// <summary>
        /// Инициализирует новый экземпляр бонуса.
        /// </summary>
        /// <param name="x">Горизонтальная позиция спавна.</param>
        /// <param name="y">Вертикальная позиция спавна.</param>
        /// <param name="type">Тип блонуса, определяющий его эффект.</param>
        public PowerUp(float x, float y, PowerUpType type)
        {
            X = x;
            Y = y;
            Type = type;
            IsActive = true;
        }

        /// <summary>
        /// Обнавляет позицию бонуса: перемещает его вниз с учётом времени.
        /// </summary>
        /// <param name="deltaTime">Время в секундах с последнего обнавления.</param>
        public void Update(float deltaTime)
        {
            Y += Speed * deltaTime;
        }
    }
}