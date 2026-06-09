using System.Runtime.InteropServices;
using System.Text;
using _3dEngine.Interfaces;

namespace _3dEngine.Inputs.Implementations;

internal class User32InputProvider : IInputProvider
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    public bool IsAvailable { get; private set; }
    public string? InitializationError { get; private set; }
    
    private bool _isFocused;
    
    public User32InputProvider()
    {
        try
        {
            _ = GetAsyncKeyState(0);
            IsAvailable = true;
        }
        catch (DllNotFoundException)
        {
            InitializationError = "WARNING: 'user32.dll' not found (running on Windows Server Core/Nano or inside a Container?).";
            IsAvailable = false;
        }
        catch (Exception ex)
        {
            InitializationError = $"WARNING: Windows User32 input failed to initialize: {ex.Message}";
            IsAvailable = false;
        }
    }
    
    public void Update()
    {
        _isFocused = IsAppFocused();
    }

    public bool IsGetKey(ConsoleKey key)
    {
        if(!_isFocused) return false;
        
        return (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }

    public bool IsGetKey(int virtualKey)
    {
        if(!_isFocused) return false;
        
        return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    
    public bool IsShift => IsGetKey(0x10);
    public bool IsCtrl => IsGetKey(0x11);
    public bool IsAlt => IsGetKey(0x12);


    private bool IsAppFocused()
    {
        IntPtr handle = GetForegroundWindow();
        if(handle == IntPtr.Zero) return false;
        
        const int nChars = 256;
        StringBuilder buffer = new StringBuilder(nChars);

        if (GetWindowText(handle, buffer, nChars) > 0)
        {
            string activeWindowTitle = buffer.ToString();
            
            return activeWindowTitle.Contains(Console.Title);
        }
        
        return false;
    }
}