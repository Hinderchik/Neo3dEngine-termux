using System.Text;
using _3dEngine;
using _3dEngine.AbstractClass;
using Android.Graphics;
using Android.Text;
using Android.Widget;

namespace Neo3dEngine.Android;

public class AndroidScreen : Screen
{
    private readonly TextView _textView;
    private readonly StringBuilder _frameBuilder = new();
    private readonly char[] _charBuffer;
    private readonly Android.Graphics.Color[] _consoleColors;
    private readonly Android.Graphics.Color[] _colorBuffer;
    private readonly int[] _brightnessBuffer;

    private const string Gradient = " .:!/r(l1Z4H9W8$@";
    private readonly float _aspectRatio;

    public AndroidScreen(TextView textView, int width, int height) : base(width, height)
    {
        _textView = textView;
        _charBuffer = new char[width * height];
        _consoleColors = MapConsoleColors();
        _colorBuffer = new Android.Graphics.Color[width * height];
        _brightnessBuffer = new int[width * height];

        float windowAspect = (float)width / height;
        float pixelAspect = 11.0f / 24.0f;
        _aspectRatio = windowAspect * pixelAspect;

        _textView.SetTypeface(Typeface.Monospace, TypefaceStyle.Normal);
    }

    protected override Vector2 CalculateUV(int i, int j)
    {
        float x = (float)i / (Width - 1) * 2 - 1;
        float y = (float)j / (Height - 1) * 2 - 1;

        x *= _aspectRatio;
        y = -y;

        return new Vector2(x, y);
    }

    public override void RenderFrame(Scene scene)
    {
        Parallel.For(0, Height, j =>
        {
            for (int i = 0; i < Width; i++)
            {
                Vector2 uv = CalculateUV(i, j);
                var pixelData = scene.GetPixelData(uv);

                int index = j * Width + i;
                _brightnessBuffer[index] = pixelData.Brightness;
                _colorBuffer[index] = _consoleColors[(int)pixelData.Color];
            }
        });

        for (int i = 0; i < _brightnessBuffer.Length; i++)
        {
            int brightness = _brightnessBuffer[i];
            brightness = int.Clamp(brightness, 0, Gradient.Length - 1);
            _charBuffer[i] = Gradient[brightness];
        }

        var uiElements = scene.UI.GetElements();
        foreach (var element in uiElements)
        {
            DrawTextToBuffer(element.Text, element.Position, _consoleColors[(int)element.Color]);
        }

        Present();
    }

    protected override void Present()
    {
        _frameBuilder.Clear();

        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                int index = j * Width + i;
                _frameBuilder.Append(_charBuffer[index]);
            }

            if (j < Height - 1)
            {
                _frameBuilder.Append('\n');
            }
        }

        _textView.Post(() => _textView.Text = _frameBuilder.ToString());
    }

    public override void PrintText(string text, Vector2Int position)
    {
        DrawTextToBuffer(text, position, _consoleColors[(int)ConsoleColor.White]);
    }

    private void DrawTextToBuffer(string text, Vector2Int pos, Android.Graphics.Color color)
    {
        for (int i = 0; i < text.Length; i++)
        {
            int x = pos.X + i;
            int y = pos.Y;

            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                int index = y * Width + x;
                _charBuffer[index] = text[i];
                _colorBuffer[index] = color;
            }
        }
    }

    private static Android.Graphics.Color[] MapConsoleColors()
    {
        var map = new Android.Graphics.Color[16];

        map[(int)ConsoleColor.Black] = Android.Graphics.Color.Black;
        map[(int)ConsoleColor.DarkBlue] = Android.Graphics.Color.DarkBlue;
        map[(int)ConsoleColor.DarkGreen] = Android.Graphics.Color.DarkGreen;
        map[(int)ConsoleColor.DarkCyan] = Android.Graphics.Color.DarkCyan;
        map[(int)ConsoleColor.DarkRed] = Android.Graphics.Color.DarkRed;
        map[(int)ConsoleColor.DarkMagenta] = Android.Graphics.Color.DarkMagenta;
        map[(int)ConsoleColor.DarkYellow] = Android.Graphics.Color.DarkGoldenrod;
        map[(int)ConsoleColor.Gray] = Android.Graphics.Color.Gray;
        map[(int)ConsoleColor.DarkGray] = Android.Graphics.Color.DarkGray;
        map[(int)ConsoleColor.Blue] = Android.Graphics.Color.Blue;
        map[(int)ConsoleColor.Green] = Android.Graphics.Color.Green;
        map[(int)ConsoleColor.Cyan] = Android.Graphics.Color.Cyan;
        map[(int)ConsoleColor.Red] = Android.Graphics.Color.Red;
        map[(int)ConsoleColor.Magenta] = Android.Graphics.Color.Magenta;
        map[(int)ConsoleColor.Yellow] = Android.Graphics.Color.Yellow;
        map[(int)ConsoleColor.White] = Android.Graphics.Color.White;

        return map;
    }
}
