using System;
using System.Drawing;
using System.Windows.Forms;
using Arkanoid.Core;

namespace Arkanoid.UI
{
    /// <summary>
    /// Основное окно игры. Отвечат за отрисовку графики, 
    /// обработку ввода пользователя и управление игровым циклом.
    /// </summary>
    public partial class Arkanoid : Form
    {
        private const int TimerInterval = 16;
        private const float DeltaTime = 0.016f;

        private const int LivesBarHeight = 30;
        private const int HeartSize = 24;
        private const int HeartSpacing = 8;
        private const int LivesPaddingRight = 20;
        private const int LivesPaddingTop = 3;
        private const int LivesLineThickness = 2;

        private static readonly Color OverlayColor = Color.FromArgb(128, Color.Black);
        private static readonly Color LivesBarColor = Color.FromArgb(40, 40, 40);
        private static readonly Color LivesLineColor = Color.FromArgb(80, 80, 80);
        private static readonly Color TextColor = Color.White;


        private const float TitleFontSize = 36f;
        private const float SubtitleFontSize = 24f;
        private const float InfoFontSize = 12f;

        private const int CenterDivisor = 2;
        private const int StartScreenTextYOffset = 30;
        private const int GameOverTextYOffset = -30;
        private const int InfoTextYOffset = 20;

        private Bitmap bufferBitmap = null!;
        private Graphics bufferGraphics = null!;
        private System.Windows.Forms.Timer _timer = null!;
        private GameEngine _engine = null!;

        private Image _ballImage = null!;
        private Image _platformImage = null!;
        private Image _heartImage = null!;
        private Image _block1Image = null!;
        private Image _block2Image = null!;
        private Image _block3Image = null!;

        /// <summary>
        /// Инициализирует новый экземпляр формы Arkonoid.
        /// Настраивает форму, таймер и обработчик событий.
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
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.Opaque,
                true);
            this.UpdateStyles();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ClientSize = new Size(GameEngine.GameWidth, GameEngine.GameHeight);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetupTimer()
        {
            _timer = new System.Windows.Forms.Timer { Interval = TimerInterval };
            _timer.Tick += OnTimerTick;
        }

        private void SetupEvents()
        {
            this.Paint += OnPaint;
            this.Load += OnLoad;
            this.MouseMove += OnMouseMove;
            this.KeyPreview = true;
            this.KeyDown += OnKeyDown;
        }

        private void OnLoad(object? sender, EventArgs e)
        {
            bufferBitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            bufferGraphics = Graphics.FromImage(bufferBitmap);

            _engine = new GameEngine();

            _ballImage = Properties.Resources.ball;
            _platformImage = Properties.Resources.platform;
            _heartImage = Properties.Resources.heart;
            _block1Image = Properties.Resources.block1;
            _block2Image = Properties.Resources.block2;
            _block3Image = Properties.Resources.block3;

            ShowStartScreen();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _engine.Update(DeltaTime);
            DrawFrame();
            this.Refresh();
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bufferBitmap, 0, 0);
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_engine.IsGameStarted || _engine.IsGameOver || _engine.IsGameWon)
            {
                return; 
            }

            _engine.MovePlatform(e.X);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !_engine.IsGameStarted && !_engine.IsGameOver && !_engine.IsGameWon)
            {
                _engine.Start();
                _timer.Start();
                return;
            }

            if (e.KeyCode == Keys.R && (_engine.IsGameOver || _engine.IsGameWon))
            {
                _engine.RestartGame();
                _timer.Start();
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

        private void DrawFrame()
        {
            bufferGraphics.Clear(this.BackColor);

            bufferGraphics.DrawImage(
                _ballImage,
                (int)_engine.BallX,
                (int)_engine.BallY,
                GameEngine.BallSize,
                GameEngine.BallSize);

            bufferGraphics.DrawImage(
                _platformImage,
                (int)_engine.PlatformX,
                _engine.PlatformY,
                GameEngine.PlatformSizeX,
                GameEngine.PlatformSizeY);

            DrawBlocks();
            DrawLives();

            if (_engine.IsGameOver)
            {
                DrawGameOverOverlay();
            }
            else if (_engine.IsGameWon)
            {
                DrawWinOverlay();
            }
        }

        private void DrawBlocks()
        {
            foreach (var block in _engine.Blocks)
            {
                if (!block.IsActive) continue;

                var blockImage = block.Strength switch
                {
                    3 => _block3Image,
                    2 => _block2Image,
                    _ => _block1Image
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

            for (var i = 0; i < _engine.Lives; i++)
            {
                var x = ClientSize.Width - LivesPaddingRight - (i + 1) *
                    HeartSize - i * HeartSpacing;
                var y = LivesPaddingTop;

                bufferGraphics.DrawImage(_heartImage, x, y,
                    HeartSize, HeartSize);
            }
        }

        private void ShowStartScreen()
        {
            bufferGraphics.Clear(this.BackColor);

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

            this.Refresh();
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