using System.Drawing;

namespace Arkanoid.Models
{
    public enum PowerUpType
    {
        WidePlatform,
        FireBall,
        FastBall
    }

    public class PowerUp
    {
        public float X { get; set; }
        public float Y { get; set; }
        public PowerUpType Type { get; }
        public bool IsActive { get; set; }
        public float Speed { get; } = 150f;
        public int Size { get; } = 25;

        public RectangleF Rect => new RectangleF(X, Y, Size, Size);

        public PowerUp(float x, float y, PowerUpType type)
        {
            X = x;
            Y = y;
            Type = type;
            IsActive = true;
        }

        public void Update(float deltaTime)
        {
            Y += Speed * deltaTime;
        }
    }
}