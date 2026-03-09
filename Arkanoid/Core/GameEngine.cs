using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Arkanoid.Models;

namespace Arkanoid.Core
{
    /// <summary>
    /// Бизнес-логика игры Arkanoid.
    /// Отвечает за физику мяча, колизии, управление уровнями и состоянием игры.
    /// </summary>
    public class GameEngine
    {
        public const float BallSpeed = 400f;
        public const int BallSize = 30;
        public const int PlatformSizeX = 100;
        public const int PlatformSizeY = 35;
        public const int LivesBarHeight = 30;
        public const int GameWidth = 800;
        public const int GameHeight = 600;

        private const int BlockWidth = 40;
        private const int BlockHeight = 25;
        private const int BlockPadding = 3;
        private const int BlocksStartX = 79;
        private const int BlocksStartY = 60;
        private const int BlocksPerRow = 15;
        private const int BlocksPerColumn = 6;

        private const float MaxAngleFactor = 0.8f;
        private const float RandomAngleFactor = 0.8f;
        private const float RandomDistributionCenter = 0.5f;
        private const int BallResetOffset = 5;
        private const float HitFactorMultiplier = 2f;
        private const float HitFactorOffset = 1f;

        private const int TotalLevels = 3;
        private const int TotalLives = 3;

        private const float PowerUpDropChance = 0.08f;
        private const float WidePlatformMultiplier = 1.5f;
        private const float FastBallMultiplier = 1.4f;
        private const float FireBallDuration = 5f;
        private const float WidePlatformDuration = 15f;
        private const float FastBallDuration = 10f;

        public List<PowerUp> ActivePowerUps { get; } = new();
        private float _currentPlatformWidth = PlatformSizeX;
        private bool _isFireBallActive = false;
        private float _fireBallTimer = 0f;
        private bool _isWidePlatformActive = false;
        private bool _isFastBallActive = false;
        private float _widePlatformTimer = 0f;
        private float _fastBallTimer = 0f;
        private const int PowerUpSize = 25;

        public float BallX { get; private set; }
        public float BallY { get; private set; }
        public float BallVelocityX { get; private set; }
        public float BallVelocityY { get; private set; }

        public float PlatformX { get; set; }
        public int PlatformY { get; }

        public int Lives { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsGameWon { get; private set; }
        public bool IsGameStarted { get; private set; }
        public int CurrentLevel { get; private set; }
        public List<Block> Blocks { get; } = new();

        private readonly int[,,] _levelLayouts = new int[,,]
        {
            {
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            },
            {
                {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            },
            {
                {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
                {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            }
        };

        private static readonly PowerUpType[] _powerUpTypes =
            (PowerUpType[])Enum.GetValues(typeof(PowerUpType));

        private readonly Random _random = new();

        /// <summary>
        /// Инициализирует новый экземпляр GameEngine.
        /// Устанавливает начальную позицию платформы и загружает первый уровень.
        /// </summary>
        public GameEngine()
        {
            PlatformY = GameHeight - PlatformSizeY;
            Lives = TotalLives;
            InitializeGame();
        }

        /// <summary>
        /// Запускает игру.
        /// </summary>
        public void Start() => IsGameStarted = true;

        /// <summary>
        /// Обновляет состояние игры за 1 кадр.
        /// </summary>
        /// <param name="deltaTime">Время в секундах с последнего обновления.</param>
        public void Update(float deltaTime)
        {
            if (!IsGameStarted || IsGameOver || IsGameWon) return;

            MoveBall(deltaTime);
            CheckCollisions();
            CheckLevelCompletion();
            UpdatePowerUps(deltaTime);
        }

        /// <summary>
        /// Обновляет позицию платформы на основе координаты мыши.
        /// </summary>
        /// <param name="mouseX">Горизонтальная позиция курсора мыши.</param>
        public void MovePlatform(float mouseX)
        {
            var targetX = mouseX - _currentPlatformWidth / 2;
            PlatformX = Math.Max(0, Math.Min(targetX, GameWidth - _currentPlatformWidth));
        }

        /// <summary>
        /// Перезапускает игру: сбрасывает состояние, жизни и загружает первый уровень.
        /// </summary>
        public void RestartGame()
        {
            IsGameOver = false;
            IsGameWon = false;
            IsGameStarted = false;
            Lives = TotalLives;
            CurrentLevel = 1;

            _currentPlatformWidth = PlatformSizeX;
            _isFireBallActive = false;
            _isWidePlatformActive = false;
            _isFastBallActive = false;
            _fireBallTimer = 0f;
            _widePlatformTimer = 0f;
            _fastBallTimer = 0f;
            ActivePowerUps.Clear();

            InitializeGame();
        }

        private void InitializeGame()
        {
            PlatformX = (GameWidth - PlatformSizeX) / 2;
            LoadLevel(1);
            ResetBall();
        }

        private void MoveBall(float frameMultiplier)
        {
            BallX += BallVelocityX * frameMultiplier;
            BallY += BallVelocityY * frameMultiplier;

            if (BallX <= 0)
            {
                BallX = 0;
                BallVelocityX = -BallVelocityX;
            }
            else if (BallX >= GameWidth - BallSize)
            {
                BallX = GameWidth - BallSize;
                BallVelocityX = -BallVelocityX;
            }

            if (BallY <= LivesBarHeight)
            {
                BallY = LivesBarHeight;
                BallVelocityY = -BallVelocityY;
            }

            if (BallY >= GameHeight)
            {
                Lives--;
                if (Lives <= 0)
                {
                    IsGameOver = true;
                }
                else
                {
                    ResetBall();
                    return;
                }
            }
        }

        private void CheckCollisions()
        {
            CheckPlatformCollision();
            CheckBlockCollisions();
        }

        private void CheckPlatformCollision()
        {
            var ballRect = new RectangleF(BallX, BallY, BallSize, BallSize);
            var platformRect = new RectangleF(PlatformX, PlatformY, _currentPlatformWidth, PlatformSizeY);

            if (ballRect.IntersectsWith(platformRect) && BallVelocityY > 0)
            {
                BallY = PlatformY - BallSize;

                float hitFactor = (BallX + BallSize / HitFactorMultiplier - PlatformX) / _currentPlatformWidth;
                hitFactor = hitFactor * HitFactorMultiplier - HitFactorOffset;

                BallVelocityX = hitFactor * BallSpeed * MaxAngleFactor;
                BallVelocityY = -MathF.Sqrt(MathF.Max(0, BallSpeed * BallSpeed - BallVelocityX * BallVelocityX));
            }
        }

        private void CheckBlockCollisions()
        {
            var ballRect = new RectangleF(BallX, BallY, BallSize, BallSize);

            foreach (var block in Blocks)
            {
                if (block.IsActive && ballRect.IntersectsWith(block.Rect))
                {
                    block.Hit();

                    if (!block.IsActive)
                    {
                        TrySpawnPowerUp(block.X, block.Y);
                    }

                    if (!_isFireBallActive)
                    {
                        float overlapLeft = ballRect.Right - block.Rect.Left;
                        float overlapRight = block.Rect.Right - ballRect.Left;
                        float overlapTop = ballRect.Bottom - block.Rect.Top;
                        float overlapBottom = block.Rect.Bottom - ballRect.Top;

                        float minHorizontal = Math.Min(overlapLeft, overlapRight);
                        float minVertical = Math.Min(overlapTop, overlapBottom);
                        float minOverlap = Math.Min(minHorizontal, minVertical);

                        if (minOverlap == overlapLeft || minOverlap == overlapRight)
                            BallVelocityX = -BallVelocityX;
                        else
                            BallVelocityY = -BallVelocityY;
                    }

                    break;
                }
            }
        }

        private void CheckLevelCompletion()
        {
            if (Blocks.All(b => !b.IsActive))
            {
                if (CurrentLevel < TotalLevels)
                {
                    CurrentLevel++;
                    LoadLevel(CurrentLevel);
                    ResetBall();
                }
                else
                {
                    IsGameWon = true;
                }
            }
        }

        /// <summary>
        /// Загружает блоки указанного уровня.
        /// </summary>
        /// <param name="level">Номер уровня.</param>
        public void LoadLevel(int level)
        {
            Blocks.Clear();
            CurrentLevel = level;
            int layoutIndex = Math.Min(level - 1, TotalLevels - 1);

            for (int row = 0; row < BlocksPerColumn; row++)
            {
                for (int col = 0; col < BlocksPerRow; col++)
                {
                    int strength = _levelLayouts[layoutIndex, row, col];
                    if (strength > 0)
                    {
                        float x = BlocksStartX + col * (BlockWidth + BlockPadding);
                        float y = BlocksStartY + row * (BlockHeight + BlockPadding);
                        Blocks.Add(new Block(x, y, BlockWidth, BlockHeight, strength));
                    }
                }
            }
        }

        private void ResetBall()
        {
            BallX = (GameWidth / HitFactorMultiplier) - (BallSize / HitFactorMultiplier);
            BallY = PlatformY - BallSize - BallResetOffset;

            float randomFactor = (float)(_random.NextDouble() - RandomDistributionCenter) * RandomAngleFactor;
            BallVelocityX = randomFactor * BallSpeed;
            BallVelocityY = -MathF.Sqrt(MathF.Max(0, BallSpeed * BallSpeed - BallVelocityX * BallVelocityX));
        }

        /// <summary>
        /// Обновляет состояние всех активных бонусов: перемещает их вниз, 
        /// проверяет выход за границы экрана и подбор платформой.
        /// Также обновляет таймеры временных эффектов.
        /// </summary>
        /// <param name="deltaTime">Время в секундах с последнего обновления.</param>
        public void UpdatePowerUps(float deltaTime)
        {
            foreach (var powerUp in ActivePowerUps)
            {
                if (!powerUp.IsActive) continue;

                powerUp.Update(deltaTime);

                if (powerUp.Y > GameHeight)
                {
                    powerUp.IsActive = false;
                }
                else if (CheckPowerUpCollection(powerUp))
                {
                    ApplyPowerUp(powerUp.Type);
                    powerUp.IsActive = false;
                }
            }

            ActivePowerUps.RemoveAll(p => !p.IsActive);

            UpdateEffectTimers(deltaTime);
        }

        private void UpdateEffectTimers(float deltaTime)
        {
            if (!_isFireBallActive && !_isWidePlatformActive && !_isFastBallActive)
            {
                return;
            }

            if (_isFireBallActive)
            {
                _fireBallTimer -= deltaTime;
                if (_fireBallTimer <= 0)
                {
                    _isFireBallActive = false;
                }
            }

            if (_isWidePlatformActive)
            {
                _widePlatformTimer -= deltaTime;
                if (_widePlatformTimer <= 0)
                {
                    _isWidePlatformActive = false;
                    _currentPlatformWidth = PlatformSizeX;  
                }
            }

            if (_isFastBallActive)
            {
                _fastBallTimer -= deltaTime;
                if (_fastBallTimer <= 0)
                {
                    _isFastBallActive = false;
                    BallVelocityX /= FastBallMultiplier;
                    BallVelocityY /= FastBallMultiplier;
                }
            }
        }

        private bool CheckPowerUpCollection(PowerUp powerUp)
        {
            var powerUpRect = powerUp.Rect;
            var platformRect = new RectangleF(PlatformX, PlatformY, _currentPlatformWidth, PlatformSizeY);
            return powerUpRect.IntersectsWith(platformRect);
        }

        private void ApplyPowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.WidePlatform:
                    _isWidePlatformActive = true;
                    _widePlatformTimer = WidePlatformDuration;
                    _currentPlatformWidth = PlatformSizeX * WidePlatformMultiplier;
                    break;

                case PowerUpType.FireBall:
                    _isFireBallActive = true;
                    _fireBallTimer = FireBallDuration;
                    break;

                case PowerUpType.FastBall:
                    _isFastBallActive = true;
                    _fastBallTimer = FastBallDuration;
                    BallVelocityX *= FastBallMultiplier;
                    BallVelocityY *= FastBallMultiplier;
                    break;
            }
        }

        private void TrySpawnPowerUp(float blockX, float blockY)
        {
            if (_random.NextDouble() < PowerUpDropChance)
            {
                var type = _powerUpTypes[_random.Next(_powerUpTypes.Length)];
                var powerUp = new PowerUp(blockX + BlockWidth / 2f - PowerUpSize / 2f, blockY, type);
                ActivePowerUps.Add(powerUp);
            }
        }

        /// <summary>
        ///Возвращает текущую ширину платформы с учётом активных бонусов.
        ///Используется для корректной отрисовки платформы и расчёта коллизий.
        /// </summary>
        /// <returns>Текущая ширина платформы в пикселях.</returns>
        public float GetCurrentPlatformWidth() => _currentPlatformWidth;
    }
}