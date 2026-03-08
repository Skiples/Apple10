using System;
using UnityEngine;

namespace Utility.Theme
{
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance;

        public ThemeData currentTheme;

        public event Action OnThemeChanged;

        private void Awake()
        {
            Instance = this;
        }

        public void SetTheme(ThemeData theme)
        {
            currentTheme = theme;
            OnThemeChanged?.Invoke();
        }
    }
}
