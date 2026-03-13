using System.Drawing;  

namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Представляет игровой блок в игре Arkonoid.
    /// Содержит информацию о позиции, размерах, прочности и состоянии блока.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Координата X позиции блока
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Координата Y позиции блока
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Ширена блока в пикселях.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Высота блока в пикселях.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Прочность блока: количество ударов, которое он может выдержать.
        /// Значение 1, 2 и 3 соответствует разным визуальным стилям.
        /// </summary>
        public int Strength { get; set; }
        
        /// <summary>
        /// Флаг активности блока: true, если блок ещё не сломан, иначе false
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Возвращает прямоугольник блока для расчёта колизии.
        /// </summary>
        public RectangleF Rect => new RectangleF(X, Y, Width, Height);

        /// <summary>
        /// Инициализирует новый экземпляр блока с заданными параметрами.
        /// </summary>
        /// <param name="x">Горизонтальная позиция.</param>
        /// <param name="y">Вертикальная позиция.</param>
        /// <param name="w">Ширина блока в пикселях.</param>
        /// <param name="h">Высота блока в пикселях.</param>
        /// <param name="s">Прочность блока (количество ударов).</param>
        public Block(float x, float y, int w, int h, int s)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Strength = s;
            IsActive = true;
        }

        /// <summary>
        /// Обрабатывает удар по блоку: уменьшает прочность блока и деактивирует при разрушении. 
        /// </summary>
        public void Hit()
        {
            Strength--;
            if (Strength <= 0)
            {
                IsActive = false;
            }
        }
    }
}