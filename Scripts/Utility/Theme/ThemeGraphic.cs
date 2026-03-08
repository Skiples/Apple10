using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.Theme
{
    [RequireComponent(typeof(Graphic))]
    public class ThemeGraphic : MonoBehaviour
    {
        public enum ColorType {
            Primary, Secondary, Background, TextMain, TextSub, Number, Accent
        }                                  
        public ColorType type;             
        Graphic graphic;

        void Start()
        {
            TryGetComponent(out graphic);
            ThemeManager.Instance.OnThemeChanged += Apply;
            Apply();
        }
        private void OnDestroy()
        {
            ThemeManager.Instance.OnThemeChanged -= Apply;
        }

        void Apply()
        {
            if(graphic)
                graphic.color = Convert(type);
        }
        Color Convert(ColorType _type)
        {
            var theme = ThemeManager.Instance.currentTheme;
            return _type switch
            {
                ColorType.Primary => theme.primary,
                ColorType.Secondary => theme.secondary,
                ColorType.Background => theme.background,
                ColorType.TextMain => theme.textMain,
                ColorType.TextSub => theme.textSub,
                ColorType.Number => theme.number,
                ColorType.Accent => theme.accent,
                _ => Color.white
            };
        }
    }
}