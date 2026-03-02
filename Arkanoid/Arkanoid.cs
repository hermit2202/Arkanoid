using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.Automation;

namespace Arkanoid
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Arkanoid : Form
    {
        private const int IntervalTimer = 40;
        private const float BallSpeed = 5f;
        private const int BallSize = 40;
        private float ballPositionX;
        private float ballPositionY;
        private int ballDirectionsX = 1;
        private int ballDirectionsY = 1;
        private int platformStratPositionX;
        private int platformStratPositionY;
        private const int platformSizeX = 150;
        private const int platformSizeY = 50;
        private int lives = 3;
        private bool isGameOver = false;

        private Bitmap bufferBitmap = null!;
        private Graphics bufferGraphics = null!;
        private float deltaTime;
        private DateTime LastFrameTime;
        private System.Windows.Forms.Timer animationTimer;

        /// <summary>
        /// 
        /// </summary>
        public Arkanoid()
        {
            InitializeComponent();

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.Opaque,
                true);
            this.UpdateStyles();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = IntervalTimer;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            this.Paint += Arkanoid_Paint;
            this.Load += Arkanoid_Load;
            this.MouseMove += Arkanoid_MouseMove;
            this.KeyPreview = true;
            this.KeyDown += Arkonoid_KeyDown;
        }

        private void Arkanoid_Load(object? sender, EventArgs e)
        {
            bufferBitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            bufferGraphics = Graphics.FromImage(bufferBitmap);

            ballPositionX = (this.ClientSize.Width / 2f) - BallSize;
            ballPositionY = this.ClientSize.Height - BallSize - platformSizeY;

            platformStratPositionX = (this.ClientSize.Width - platformSizeX) / 2;
            platformStratPositionY = this.ClientSize.Height - platformSizeY;

            ballDirectionsX = 1;
            ballDirectionsY = -1;
        }

        private void Arkanoid_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bufferBitmap, 0, 0);
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (isGameOver)
            {
                return;
            }

            var currentTime = DateTime.Now;
            deltaTime = (float)(currentTime - LastFrameTime).TotalSeconds;
            LastFrameTime = currentTime;

            if (deltaTime > 0.1f)
            {
                deltaTime = 0.1f;
            }

            var targetFrameRate = 60f;
            float frameMultiplier = deltaTime * targetFrameRate;

            MoveBall(frameMultiplier);
            DrawFrame();

            this.Refresh();
        }

        private void MoveBall(float frameMultiplier)
        {
            ballPositionX += BallSpeed * frameMultiplier * ballDirectionsX;
            ballPositionY += BallSpeed * frameMultiplier * ballDirectionsY;
        
            if (ballPositionX <= 0)
            {
                ballPositionX = -ballPositionX;
                ballDirectionsX = 1;
            }
            else if (ballPositionX >= Width - BallSize)
            {
                ballPositionX = 2 * (Width - BallSize) - ballPositionX;
                ballDirectionsX = -1;
            }

            if (ballPositionY <= 0)
            {
                ballPositionY = -ballPositionY;
                ballDirectionsY = 1;
            }
            
            if (ballPositionY >= this.ClientSize.Height)
            {
                lives--;
                if (lives <= 0)
                {
                    GameOver();
                }
                else
                {
                    ResetBall();
                }
                return;
            }

           CheckPlatformCollision();
        }
        
        private void DrawFrame()
        {
            if (isGameOver)
            {
                return;
            }

            bufferGraphics.Clear(this.BackColor);

            bufferGraphics.DrawImage(Properties.Resources.ball,
                (int)ballPositionX, (int)ballPositionY, BallSize, BallSize);

            bufferGraphics.DrawImage(Properties.Resources.platform, 
                platformStratPositionX, platformStratPositionY, 
                platformSizeX, platformSizeY);
        }

        private void Arkanoid_MouseMove(object? sender, MouseEventArgs e)
        {
            var targetX = e.X - platformSizeX / 2;

            platformStratPositionX = Math.Max(0, Math.Min(
                targetX, ClientSize.Width - platformSizeX));

            platformStratPositionX += (int)((targetX - platformStratPositionX) * 0.2f);
        }

        private void CheckPlatformCollision()
        {
            if (platformStratPositionY <= 0)
            {
                return;
            }

            var ballRect = new RectangleF(ballPositionX, ballPositionY,
                BallSize, BallSize);
            var platformRect = new RectangleF(
                platformStratPositionX,
                platformStratPositionY,
                platformSizeX,
                platformSizeY);

            if (ballRect.IntersectsWith(platformRect))
            {
                ballPositionY = platformStratPositionY - BallSize;
                ballDirectionsY = -1;

                float hitPosition = (ballPositionX + BallSize / 2f - 
                    platformStratPositionX) / platformSizeX;

                ballDirectionsX = hitPosition > 0.5f ? 1 : -1;

                float angel = (hitPosition - 0.5f) * 2f;
                ballDirectionsX = Math.Sign(angel);
            }
        }

        private void GameOver ()
        {
            isGameOver = true;
            animationTimer.Stop();

            bufferGraphics.Clear(Color.FromArgb(128, Color.Black));

            using (Font font = new Font("Arial", 24, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, 
                LineAlignment = StringAlignment.Center })
            {
                bufferGraphics.DrawString("GAME OVER", font, brush,
                    ClientSize.Width / 2, ClientSize.Height / 2 - 30, sf);
            }
            using (Font font = new Font("Arial", 12))
            using (SolidBrush brush = new SolidBrush(Color.White))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                bufferGraphics.DrawString("Нажми R для рестарта или ESC для выхода", font, brush,
                    ClientSize.Width / 2, ClientSize.Height / 2 + 20, sf);
            }

            this.Refresh();
        }

        private void ResetBall()
        {
            ballPositionX = (this.ClientSize.Width / 2f) - (BallSize / 2f);
            ballPositionY = platformStratPositionY - BallSize - 5;

            ballDirectionsX = (new Random().Next(0, 2) == 0) ? -1 : 1;
            ballDirectionsY = -1;
        }

        private void Arkonoid_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.R && lives <= 0 &&
                !animationTimer.Enabled)
            {
                isGameOver = false;
                lives = 3;
                ResetBall();
                platformStratPositionX = (this.ClientSize.Width - platformSizeX) / 2;
                LastFrameTime = DateTime.Now;
                animationTimer.Start();
            }

            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }
    }
}