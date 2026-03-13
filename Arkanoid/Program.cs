namespace Arkanoid
{
    /// <summary>
    /// Точка входа в приложение Arkonoid.
    /// Инициализирует среду для выполнения Windows Forms.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа в приложение.
        /// Инициализирует визуальные стили в Windows Forms и запускает главное окно игры.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Arkanoid());
        }
    }
}