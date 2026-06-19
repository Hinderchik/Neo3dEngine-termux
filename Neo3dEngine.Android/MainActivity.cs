using _3dEngine;
using _3dEngine.Inputs;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.IO;

namespace Neo3dEngine.Android;

[Activity(Label = "Neo3dEngine", Theme = "@style/Maui.MainTheme.NoActionBar", MainLauncher = true)]
public class MainActivity : AppCompatActivity
{
    private AndroidScreen? _screen;
    private AndroidInputProvider? _inputProvider;
    private Thread? _gameThread;
    private CancellationTokenSource? _cts;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var layout = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        var renderText = new TextView(this)
        {
            TextSize = 8,
            Typeface = Android.Graphics.Typeface.Monospace
        };
        layout.AddView(renderText, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            0,
            4.0f));

        var controls = CreateControlsLayout();
        layout.AddView(controls, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            0,
            1.0f));

        SetContentView(layout);

        _inputProvider = new AndroidInputProvider();
        Input.SetProvider(_inputProvider);

        _cts = new CancellationTokenSource();
        _gameThread = new Thread(() => RunGame(renderText, _cts.Token))
        {
            IsBackground = true
        };
        _gameThread.Start();
    }

    private void RunGame(TextView renderText, CancellationToken token)
    {
        CopyAssetIfNeeded("monkey.obj", "monkey.obj");

        int width = 60;
        int height = 40;

        _screen = new AndroidScreen(renderText, width, height);
        AndroidSceneLauncher.RunPreviewScene(_screen);
    }

    private LinearLayout CreateControlsLayout()
    {
        var root = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal
        };

        var dpad = CreateDpad();
        root.AddView(dpad, new LinearLayout.LayoutParams(
            0,
            ViewGroup.LayoutParams.MatchParent,
            1.0f));

        var actions = CreateActionButtons();
        root.AddView(actions, new LinearLayout.LayoutParams(
            0,
            ViewGroup.LayoutParams.MatchParent,
            1.0f));

        return root;
    }

    private LinearLayout CreateDpad()
    {
        var layout = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        var up = CreateButton("↑", ConsoleKey.UpArrow);
        var middle = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal
        };
        var left = CreateButton("←", ConsoleKey.LeftArrow);
        var right = CreateButton("→", ConsoleKey.RightArrow);
        middle.AddView(left, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f));
        middle.AddView(right, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f));
        var down = CreateButton("↓", ConsoleKey.DownArrow);

        layout.AddView(up);
        layout.AddView(middle);
        layout.AddView(down);

        return layout;
    }

    private LinearLayout CreateActionButtons()
    {
        var layout = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        var w = CreateButton("W", ConsoleKey.W);
        var a = CreateButton("A", ConsoleKey.A);
        var s = CreateButton("S", ConsoleKey.S);
        var d = CreateButton("D", ConsoleKey.D);
        var shift = CreateButton("Shift", ConsoleKey.LeftShift);
        var ctrl = CreateButton("Ctrl", ConsoleKey.LeftCtrl);
        var space = CreateButton("Space", ConsoleKey.Spacebar);

        var row1 = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        row1.AddView(a, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f));
        row1.AddView(d, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f));

        var row2 = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        row2.AddView(shift, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f));
        row2.AddView(ctrl, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f));

        layout.AddView(w);
        layout.AddView(row1);
        layout.AddView(s);
        layout.AddView(row2);
        layout.AddView(space);

        return layout;
    }

    private Button CreateButton(string label, ConsoleKey key)
    {
        var button = new Button(this)
        {
            Text = label
        };

        button.Touch += (sender, e) =>
        {
            if (e.Event?.Action == MotionEventActions.Down)
            {
                _inputProvider?.PressKey(key);
            }
            else if (e.Event?.Action == MotionEventActions.Up || e.Event?.Action == MotionEventActions.Cancel)
            {
                _inputProvider?.ReleaseKey(key);
            }
        };

        return button;
    }

    private void CopyAssetIfNeeded(string assetName, string outputName)
    {
        string outputPath = Path.Combine(FilesDir!.AbsolutePath, outputName);

        if (File.Exists(outputPath))
        {
            return;
        }

        using var asset = Assets!.Open(assetName);
        using var output = System.IO.File.Create(outputPath);
        asset.CopyTo(output);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _cts?.Cancel();
        _gameThread?.Join(TimeSpan.FromSeconds(1));
        Input.Dispose();
    }
}
