using Arkanoid.Logic.Core;
using Arkanoid.Logic.Models;
using Arkanoid.WinForms.Properties;
using static Arkanoid.Logic.GameConstants;
using static Arkanoid.WinForms.FormConstants;

namespace Arkanoid.WinForms
{
    /// <summary>
    /// Основное окно игры. Отвечает за отрисовку графики, 
    /// обработку ввода пользователя и управление игровым циклом.
    /// </summary>
    public partial class Arkanoid : Form
    {
        private Bitmap bufferBitmap = null!;
        private Graphics bufferGraphics = null!;
        private System.Windows.Forms.Timer timer = null!;
        private GameEngine engine = null!;

        private Image ballImage = null!;
        private Image platformImage = null!;
        private Image heartImage = null!;
        private Image block1Image = null!;
        private Image block2Image = null!;
        private Image block3Image = null!;
        private Image powerUpWidePlatform = null!;
        private Image powerUpFireBall = null!;
        private Image powerUpFastBall = null!;

        /// <summary>
        /// Инициализирует новый экземпляр формы Arkanoid.
        /// Настраивает форму, таймер и обработчики событий.
        /// </summary>
        public Arkanoid()
        {
            InitializeComponent();
            SetupForm();
            SetupTimer();
            SetupEvents();
        }

        private void SetupForm()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.Opaque,
                true);
            UpdateStyles();

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(GameWidth, GameHeight);
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetupTimer()
        {
            timer = new System.Windows.Forms.Timer { Interval = TimerInterval };
            timer.Tick += OnTimerTick;
        }

        private void SetupEvents()
        {
            Paint += OnPaint;
            Load += OnLoad;
            MouseMove += OnMouseMove;
            KeyPreview = true;
            KeyDown += OnKeyDown;
        }

        private void OnLoad(object? sender, EventArgs e)
        {
            bufferBitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            bufferGraphics = Graphics.FromImage(bufferBitmap);

            engine = new GameEngine();

            ballImage = Resources.ball;
            platformImage = Resources.platform;
            heartImage = Resources.heart;
            block1Image = Resources.block1;
            block2Image = Resources.block2;
            block3Image = Resources.block3;
            powerUpWidePlatform = Resources.powerup_wide;
            powerUpFireBall = Resources.powerup_fire;
            powerUpFastBall = Resources.powerup_fast;

            ShowStartScreen();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            engine.Update(DeltaTime);
            DrawFrame();
            Refresh();
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bufferBitmap, 0, 0);
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!engine.IsGameStarted || engine.IsGameOver || engine.IsGameWon)
            {
                return;
            }

            engine.MovePlatform(e.X);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !engine.IsGameStarted && !engine.IsGameOver && !engine.IsGameWon)
            {
                engine.Start();
                timer.Start();
                return;
            }

            if (e.KeyCode == Keys.R && (engine.IsGameOver || engine.IsGameWon))
            {
                engine.RestartGame();
                timer.Start();
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

        private void DrawFrame()
        {
            bufferGraphics.Clear(Color.Black);

            bufferGraphics.DrawImage(
                ballImage,
                (int)engine.BallX,
                (int)engine.BallY,
                BallSize,
                BallSize);

            bufferGraphics.DrawImage(
                platformImage,
                (int)engine.PlatformX,
                engine.PlatformY,
                (int)engine.GetCurrentPlatformWidth(),
                PlatformSizeY);

            DrawBlocks();
            DrawPowerUps();
            DrawLives();

            if (engine.IsGameOver)
            {
                DrawGameOverOverlay();
            }
            else if (engine.IsGameWon)
            {
                DrawWinOverlay();
            }
        }

        private void DrawBlocks()
        {
            foreach (var block in engine.Blocks)
            {
                if (!block.IsActive)
                {
                    continue;
                }

                var blockImage = block.Strength switch
                {
                    3 => block3Image,
                    2 => block2Image,
                    _ => block1Image
                };

                bufferGraphics.DrawImage(
                    blockImage,
                    (int)block.X,
                    (int)block.Y,
                    block.Width,
                    block.Height);
            }
        }

        private void DrawLives()
        {
            using (var barBrush = new SolidBrush(LivesBarColor))
            {
                bufferGraphics.FillRectangle(barBrush, 0, 0,
                    ClientSize.Width, LivesBarHeight);
            }

            using (var linePen = new Pen(LivesLineColor, LivesLineThickness))
            {
                bufferGraphics.DrawLine(linePen, 0, LivesBarHeight,
                    ClientSize.Width, LivesBarHeight);
            }

            for (var i = 0; i < engine.Lives; i++)
            {
                var x = ClientSize.Width - LivesPaddingRight - (i + 1) *
                    HeartSize - i * HeartSpacing;
                var y = LivesPaddingTop;

                bufferGraphics.DrawImage(heartImage, x, y,
                    HeartSize, HeartSize);
            }
        }

        private void DrawPowerUps()
        {
            foreach (var powerUp in engine.ActivePowerUps)
            {
                if (!powerUp.IsActive)
                {
                    continue;
                }

                var powerUpImage = powerUp.Type switch
                {
                    PowerUpType.WidePlatform => powerUpWidePlatform,
                    PowerUpType.FireBall => powerUpFireBall,
                    PowerUpType.FastBall => powerUpFastBall,
                    _ => powerUpWidePlatform
                };

                bufferGraphics.DrawImage(
                    powerUpImage,
                    (int)powerUp.X,
                    (int)powerUp.Y,
                    powerUp.Size,
                    powerUp.Size);
            }
        }

        private void ShowStartScreen()
        {
            bufferGraphics.Clear(Color.Black);

            using (var titleFont = new Font("Arial", TitleFontSize, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(TextColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                bufferGraphics.DrawString(
                    "ARKANOID",
                    titleFont,
                    titleBrush,
                    ClientSize.Width / CenterDivisor,
                    ClientSize.Height / CenterDivisor,
                    sf);
            }

            using (var font = new Font("Arial", InfoFontSize))
            using (var brush = new SolidBrush(TextColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                bufferGraphics.DrawString(
                    "Нажми ПРОБЕЛ для старта\nESC для выхода",
                    font,
                    brush,
                    ClientSize.Width / CenterDivisor,
                    ClientSize.Height / CenterDivisor + StartScreenTextYOffset,
                    sf);
            }

            Refresh();
        }

        private void DrawGameOverOverlay()
        {
            using (var overlayBrush = new SolidBrush(OverlayColor))
            {
                bufferGraphics.FillRectangle(overlayBrush, 0, 0, ClientSize.Width, ClientSize.Height);
            }

            using (var font = new Font("Arial", SubtitleFontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(TextColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                bufferGraphics.DrawString(
                    "GAME OVER",
                    font,
                    brush,
                    ClientSize.Width / CenterDivisor,
                    ClientSize.Height / CenterDivisor + GameOverTextYOffset,
                    sf);
            }

            using (var font = new Font("Arial", InfoFontSize))
            using (var brush = new SolidBrush(TextColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                bufferGraphics.DrawString(
                    "Нажми R для рестарта или ESC для выхода",
                    font,
                    brush,
                    ClientSize.Width / CenterDivisor,
                    ClientSize.Height / CenterDivisor + InfoTextYOffset,
                    sf);
            }
        }

        private void DrawWinOverlay()
        {
            using (var overlayBrush = new SolidBrush(OverlayColor))
            {
                bufferGraphics.FillRectangle(overlayBrush, 0, 0, ClientSize.Width, ClientSize.Height);
            }

            using (var font = new Font("Arial", TitleFontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(TextColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                bufferGraphics.DrawString(
                    "YOU WIN!",
                    font,
                    brush,
                    ClientSize.Width / CenterDivisor,
                    ClientSize.Height / CenterDivisor + GameOverTextYOffset,
                    sf);
            }

            using (var font = new Font("Arial", InfoFontSize))
            using (var brush = new SolidBrush(TextColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                bufferGraphics.DrawString(
                    "Нажми R для рестарта или ESC для выхода",
                    font,
                    brush,
                    ClientSize.Width / CenterDivisor,
                    ClientSize.Height / CenterDivisor + InfoTextYOffset,
                    sf);
            }
        }
    }
}
