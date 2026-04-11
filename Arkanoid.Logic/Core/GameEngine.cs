using System.Drawing;
using Arkanoid.Logic.Models;
using static Arkanoid.Logic.GameConstants;

namespace Arkanoid.Logic.Core
{
    /// <summary>
    /// Бизнес-логика игры Arkanoid.
    /// Отвечает за физику мяча, колизии, управление уровнями и состоянием игры.
    /// </summary>
    public class GameEngine
    {
        /// <summary>
        /// Список всех активных бонусов, которые в данный омент падают на игровом поле.
        /// </summary>
        public List<PowerUp> ActivePowerUps { get; } = new();
        private float currentPlatformWidth = GameConstants.PlatformSizeX;
        private bool isFireBallActive = false;
        private float fireBallTimer = 0f;
        private bool isWidePlatformActive = false;
        private bool isFastBallActive = false;
        private float widePlatformTimer = 0f;
        private float fastBallTimer = 0f;
        private const int powerUpSize = 25;

        /// <summary>
        /// Горизонтальная позиция мяча в пикселях. 
        /// Используется для отрисовки и расчёта коллизий. 
        /// </summary>
        public float BallX { get; private set; }

        /// <summary>
        /// Вертикальная позиция мяча в пикселях. 
        /// Используется для отрисовки и расчёта коллизий. 
        /// </summary>
        public float BallY { get; private set; }

        /// <summary>
        /// Горизонтальная скорость мяча (пикселей в секунду).
        /// Отрицательное значение — движение влево, положительное — вправо.
        /// </summary>
        public float BallVelocityX { get; private set; }

        /// <summary>
        /// Вертикальная скорость мяча (пикселей в секунду).
        /// Отрицательное значение — движение вверх, положительное — вниз.
        /// </summary>
        public float BallVelocityY { get; private set; }

        /// <summary>
        /// Вертикальная позиция платформы в пикселях.
        /// </summary>

        public float PlatformX { get; set; }

        /// <summary>
        /// Горизонтальная позиция платформы в пикселях.
        /// </summary>
        public int PlatformY { get; }

        /// <summary>
        /// Количество оставщихся жизний  игрка.
        /// </summary>
        public int Lives { get; private set; }

        /// <summary>
        /// Флаг завершения игры поражением.
        /// </summary>
        public bool IsGameOver { get; private set; }

        /// <summary>
        /// Флаг завершения игры победой.
        /// </summary>
        public bool IsGameWon { get; private set; }

        /// <summary>
        /// Флаг запуска игры.
        /// </summary>
        public bool IsGameStarted { get; private set; }

        /// <summary>
        /// Номер текущего уровня.
        /// </summary>
        public int CurrentLevel { get; private set; }

        /// <summary>
        /// Список всех блоков текущего уровня
        /// </summary>
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
            if (!IsGameStarted || IsGameOver || IsGameWon)
            {
                return;
            }

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
            var targetX = mouseX - currentPlatformWidth / 2;
            PlatformX = Math.Max(0, Math.Min(targetX, GameWidth - currentPlatformWidth));
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

            currentPlatformWidth = PlatformSizeX;
            isFireBallActive = false;
            isWidePlatformActive = false;
            isFastBallActive = false;
            fireBallTimer = 0f;
            widePlatformTimer = 0f;
            fastBallTimer = 0f;
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
            var platformRect = new RectangleF(PlatformX, PlatformY, currentPlatformWidth, PlatformSizeY);

            if (ballRect.IntersectsWith(platformRect) && BallVelocityY > 0)
            {
                BallY = PlatformY - BallSize;

                var hitFactor = (BallX + BallSize / HitFactorMultiplier - PlatformX) / currentPlatformWidth;
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

                    if (!isFireBallActive)
                    {
                        var overlapLeft = ballRect.Right - block.Rect.Left;
                        var overlapRight = block.Rect.Right - ballRect.Left;
                        var overlapTop = ballRect.Bottom - block.Rect.Top;
                        var overlapBottom = block.Rect.Bottom - ballRect.Top;

                        var minHorizontal = Math.Min(overlapLeft, overlapRight);
                        var minVertical = Math.Min(overlapTop, overlapBottom);
                        var minOverlap = Math.Min(minHorizontal, minVertical);

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

        private void LoadLevel(int level)
        {
            Blocks.Clear();
            CurrentLevel = level;
            var layoutIndex = Math.Min(level - 1, TotalLevels - 1);

            for (var row = 0; row < BlocksPerColumn; row++)
            {
                for (var col = 0; col < BlocksPerRow; col++)
                {
                    var strength = _levelLayouts[layoutIndex, row, col];
                    if (strength > 0)
                    {
                        var x = BlocksStartX + col * (BlockWidth + BlockPadding);
                        var y = BlocksStartY + row * (BlockHeight + BlockPadding);
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
            if (!isFireBallActive && !isWidePlatformActive && !isFastBallActive)
            {
                return;
            }

            if (isFireBallActive)
            {
                fireBallTimer -= deltaTime;
                if (fireBallTimer <= 0)
                {
                    isFireBallActive = false;
                }
            }

            if (isWidePlatformActive)
            {
                widePlatformTimer -= deltaTime;
                if (widePlatformTimer <= 0)
                {
                    isWidePlatformActive = false;
                    currentPlatformWidth = PlatformSizeX;  
                }
            }

            if (isFastBallActive)
            {
                fastBallTimer -= deltaTime;
                if (fastBallTimer <= 0)
                {
                    isFastBallActive = false;
                    BallVelocityX /= FastBallMultiplier;
                    BallVelocityY /= FastBallMultiplier;
                }
            }
        }

        private bool CheckPowerUpCollection(PowerUp powerUp)
        {
            var powerUpRect = powerUp.Rect;
            var platformRect = new RectangleF(PlatformX, PlatformY, currentPlatformWidth, PlatformSizeY);
            return powerUpRect.IntersectsWith(platformRect);
        }

        private void ApplyPowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.WidePlatform:
                    isWidePlatformActive = true;
                    widePlatformTimer = WidePlatformDuration;
                    currentPlatformWidth = PlatformSizeX * WidePlatformMultiplier;
                    break;

                case PowerUpType.FireBall:
                    isFireBallActive = true;
                    fireBallTimer = FireBallDuration;
                    break;

                case PowerUpType.FastBall:
                    isFastBallActive = true;
                    fastBallTimer = FastBallDuration;
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
                var powerUp = new PowerUp(blockX + BlockWidth / 2f - powerUpSize / 2f, blockY, type);
                ActivePowerUps.Add(powerUp);
            }
        }

        /// <summary>
        ///Возвращает текущую ширину платформы с учётом активных бонусов.
        ///Используется для корректной отрисовки платформы и расчёта коллизий.
        /// </summary>
        /// <returns>Текущая ширина платформы в пикселях.</returns>
        public float GetCurrentPlatformWidth() => currentPlatformWidth;
    }
}