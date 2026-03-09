using System.Drawing;

namespace Arkanoid.Models
{
    public class Block
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Strength { get; set; }
        public bool IsActive { get; set; }

        public RectangleF Rect => new RectangleF(X, Y, Width, Height);

        public Block(float x, float y, int w, int h, int s)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Strength = s;
            IsActive = true;
        }

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