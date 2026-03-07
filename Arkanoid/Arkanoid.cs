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
        private float ballVelocityX = 3f;
        private float ballVelocityY = -4f;
        private int platformStratPositionX;
        private int platformStratPositionY;
        private const int platformSizeX = 150;
        private const int platformSizeY = 50;
        private int lives = 3;
        private bool isGameOver = false;
        private bool isGameStarted = false;
        private const int LivesBarHeight = 30;
        private const int HeartSize= 24;
        private const int HeartSpacing =8;
        private const int  LivesPaddingRight = 20;
        private const int LivesPaddingTop = 3;

        private Random random = new();
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

            ShowStartScreen();
        }

        private void Arkanoid_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bufferBitmap, 0, 0);
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (!isGameStarted|| isGameOver)
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
            ballPositionX += ballVelocityX * frameMultiplier;
            ballPositionY += ballVelocityY * frameMultiplier;
        
            if (ballPositionX <= 0)
            {
                ballPositionX = 0;
                ballVelocityX = -ballVelocityX;
            }
            else if (ballPositionX >= Width - BallSize)
            {
                ballPositionX = ClientSize.Width - BallSize;
                ballVelocityX = -ballVelocityX;
            }

            if (ballPositionY <= LivesBarHeight)
            {
                ballPositionY = LivesBarHeight;
                ballVelocityY = -ballVelocityY;
            }
            
            if (ballPositionY >= this.ClientSize.Height)
            {
                lives--;
                if (lives <= 0)
                {
                    DrawFrame();
                    this.Refresh();
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

            DrawLives();
        }

        private void DrawLives()
        {
            using (SolidBrush barBrush = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                bufferGraphics.FillRectangle(barBrush, 0, 0, 
                    ClientSize.Width, LivesBarHeight);
            }

            using (Pen linePen = new Pen(Color.FromArgb(80, 80, 80), 2))
            {
                bufferGraphics.DrawLine(linePen, 0, LivesBarHeight,
                    ClientSize.Width, LivesBarHeight);
            }

            for (int i = 0; i < lives; i++)
            {
                int x = ClientSize.Width - LivesPaddingRight - (i + 1) *
                    HeartSize - i * HeartSpacing;
                int y = LivesPaddingTop;

                bufferGraphics.DrawImage(Properties.Resources.heart, x, y,
                    HeartSize, HeartSize);
            }
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
            var ballRect = new RectangleF(ballPositionX, ballPositionY,
                BallSize, BallSize);
            var platformRect = new RectangleF(
                platformStratPositionX,
                platformStratPositionY,
                platformSizeX,
                platformSizeY);

            if (ballRect.IntersectsWith(platformRect) && ballVelocityY > 0)
            {
                ballPositionY = platformStratPositionY - BallSize;

                float hitFactor = (ballPositionX + BallSize / 2f - 
                    platformStratPositionX) / platformSizeX;
                hitFactor = hitFactor * 2f - 1f;

                float maxAngelFactor = 0.8f;
                ballVelocityX = hitFactor * BallSpeed * maxAngelFactor;

                ballVelocityY = -MathF.Sqrt(MathF.Max(0, BallSpeed * BallSpeed - ballVelocityX * ballVelocityX));
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

        private void ShowStartScreen()
        {
            bufferGraphics.Clear(this.BackColor);

            using (Font titleFont = new Font("Arial", 36, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.Black))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                bufferGraphics.DrawString("ARKONOID", titleFont, titleBrush,
                    ClientSize.Width / 2, ClientSize.Height / 2, sf);
            }

            using (Font font = new Font("Arial", 12))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                bufferGraphics.DrawString("Нажми ПРОБЕЛ для старта\nESC для выхода", font, brush,
                    ClientSize.Width / 2, ClientSize.Height / 2 + 30, sf);
            }

            this.Refresh();
        }

        private void ResetBall()
        {
            ballPositionX = (this.ClientSize.Width / 2f) - (BallSize / 2f);
            ballPositionY = platformStratPositionY - BallSize - 5;

            float randomFactor = (float)(random.NextDouble() - 0.5) * 0.8f;

            ballVelocityX = randomFactor * BallSpeed;
            ballVelocityY = -MathF.Sqrt(MathF.Max(0, BallSpeed * BallSpeed - ballVelocityX * ballVelocityX));
        }

        private void Arkonoid_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !isGameStarted)
            {
                isGameStarted = true;
                LastFrameTime = DateTime.Now;
                animationTimer.Start();
                return;
            }

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