using System;
using System.ComponentModel;

namespace ConsolePanel
{
    public enum Color
    {
        Black = 0,
        Gray = 8,
        Blue = 1,
        LightBlue = 9,
        Green = 2,
        LightGreen = 0xA,
        Aqua = 3,
        LightAqua = 0xB,
        Red = 4,
        LightRed = 0xC,
        Purple = 5,
        LightPurple = 0xD,
        Yellow = 6,
        LightYellow = 0xE,
        White = 7,
        BrightWhite = 0xF
    }

    [Serializable]
    class Settings
    {
        private Color background = Color.Black;
        private Color foreground = Color.White;

        [DisplayName("Background Color"), DefaultValue(Color.Black)]
        public Color BackgroundColor
        {
            get
            {
                return this.background;
            }
            set
            {
                this.background = value;
            }
        }

        [DisplayName("Foreground Color"), DefaultValue(Color.White)]
        public Color ForegroundColor
        {
            get
            {
                return this.foreground;
            }
            set
            {
                this.foreground = value;
            }
        }
    }
}
