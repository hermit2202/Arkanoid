namespace Arkanoid.Logic
{
    /// <summary>
    /// Глобальные константы игры.
    /// </summary>
    public static class GameConstants
    {
        public const int GameWidth = 800;
        public const int GameHeight = 600;

        public const float BallSpeed = 400f;
        public const int BallSize = 30;

        public const int PlatformSizeX = 100;
        public const int PlatformSizeY = 35;

        public const int LivesBarHeight = 30;

        public const int BlockWidth = 40;
        public const int BlockHeight = 25;
        public const int BlockPadding = 3;
        public const int BlocksStartX = 79;
        public const int BlocksStartY = 60;
        public const int BlocksPerRow = 15;
        public const int BlocksPerColumn = 6;

        public const float MaxAngleFactor = 0.8f;
        public const float RandomAngleFactor = 0.8f;
        public const float RandomDistributionCenter = 0.5f;
        public const int BallResetOffset = 5;
        public const float HitFactorMultiplier = 2f;
        public const float HitFactorOffset = 1f;

        public const int TotalLevels = 3;
        public const int TotalLives = 3;

        public const float PowerUpDropChance = 0.08f;
        public const float WidePlatformMultiplier = 1.5f;
        public const float FastBallMultiplier = 1.4f;
    }
}
