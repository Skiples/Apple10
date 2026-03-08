using System;
using System.Collections.Generic;
using UnityEngine;
namespace Utility.Theme
{
    public enum EnumTheme
    {
        BlackGreen,
        VisualStudioC,

    }
    [CreateAssetMenu(menuName = "Custom/UI/ThemeData")]
    public class ThemeData : ScriptableObject
    {
        public EnumTheme type;
        public Color primary;
        public Color secondary;
        public Color background;
        public Color textMain;
        public Color textSub;
        public Color number;
        public Color accent;
    }
}
