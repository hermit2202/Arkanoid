namespace Arkanoid.WinForms
{
    /// <summary>
    /// Константы связанные с отображением формы.
    /// </summary>
    public static class FormConstants
    {
        public const int TimerInterval = 16;
        public const float DeltaTime = 0.016f;

        public const int HeartSize = 24;
        public const int HeartSpacing = 8;
        public const int LivesPaddingRight = 20;
        public const int LivesPaddingTop = 3;
        public const int LivesLineThickness = 2;

        public static readonly Color OverlayColor = Color.FromArgb(128, Color.Black);
        public static readonly Color LivesBarColor = Color.FromArgb(40, 40, 40);
        public static readonly Color LivesLineColor = Color.FromArgb(80, 80, 80);
        public static readonly Color TextColor = Color.White;

        public const float TitleFontSize = 36f;
        public const float SubtitleFontSize = 24f;
        public const float InfoFontSize = 12f;

        public const int CenterDivisor = 2;
        public const int StartScreenTextYOffset = 30;
        public const int GameOverTextYOffset = -30;
        public const int InfoTextYOffset = 20;
    }
}
