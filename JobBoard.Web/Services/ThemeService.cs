namespace JobBoard.Web.Services
{
    public class ThemeService
    {
        private bool _isDarkMode = true;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            private set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    NotifyStateChanged();
                }
            }
        }

        public event Action? OnChange;

        public void SetTheme(bool isDark)
        {
            IsDarkMode = isDark;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}