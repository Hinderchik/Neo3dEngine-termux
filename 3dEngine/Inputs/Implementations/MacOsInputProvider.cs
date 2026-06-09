using System.Runtime.InteropServices;
using _3dEngine.Interfaces;

namespace _3dEngine.Inputs.Implementations;

internal class MacOsInputProvider : IInputProvider
{
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern bool CGEventSourceKeyState(int stateID, ushort key);
    
    private const int HidSystemState = 1;

    public bool IsAvailable { get; private set; }
    public string? InitializationError { get; private set; }

    public MacOsInputProvider()
    {
        try
        {
            _ = CGEventSourceKeyState(HidSystemState, 0);
            IsAvailable = true;
        }
        catch (DllNotFoundException)
        {
            InitializationError = "WARNING: CoreGraphics framework not found. Are you running on macOS?";
            IsAvailable = false;
        }
        catch (Exception ex)
        {
            InitializationError = "WARNING: macOS CoreGraphics input failed to initialize.\n" +
                                  $"Details: {ex.Message}\n" +
                                  "-> If keys are not detected, grant 'Input Monitoring' or 'Accessibility' permissions to Terminal in System Settings.";
            IsAvailable = false;
        }
    }
    
    
    public void Update(){}

    public bool IsGetKey(ConsoleKey key)
    {
        if (!IsAvailable) return false;

        if (ConsoleKeyToMacKeyCodeMap.TryGetValue(key, out ushort keyCode))
        {
            return CGEventSourceKeyState(HidSystemState, keyCode);
        }

        return false;
    }

    public bool IsGetKey(int virtualKey)
    {
        return IsGetKey((ConsoleKey)virtualKey);
    }

    public bool IsShift => CGEventSourceKeyState(HidSystemState, 56) || CGEventSourceKeyState(HidSystemState, 60);
    public bool IsCtrl  => CGEventSourceKeyState(HidSystemState, 59) || CGEventSourceKeyState(HidSystemState, 62);
    public bool IsAlt   => CGEventSourceKeyState(HidSystemState, 58) || CGEventSourceKeyState(HidSystemState, 61);
    
    
    #region КАРТА СОПОСТАВЛЕНИЯ CONSOLEKEY -> MAC VIRTUAL KEYCODE

        // Маппинг согласно системному файлу Events.h из Carbon Framework macOS
        private static readonly Dictionary<ConsoleKey, ushort> ConsoleKeyToMacKeyCodeMap = new()
        {
            // Системные и управляющие клавиши
            { ConsoleKey.Backspace, 51 },
            { ConsoleKey.Tab, 48 },
            { ConsoleKey.Enter, 36 },
            { ConsoleKey.Escape, 53 },
            { ConsoleKey.Spacebar, 49 },
            { ConsoleKey.LeftArrow, 123 },
            { ConsoleKey.RightArrow, 124 },
            { ConsoleKey.DownArrow, 125 },
            { ConsoleKey.UpArrow, 126 },

            // Буквы (ANSI раскладка Mac)
            { ConsoleKey.A, 0 },
            { ConsoleKey.S, 1 },
            { ConsoleKey.D, 2 },
            { ConsoleKey.F, 3 },
            { ConsoleKey.H, 4 },
            { ConsoleKey.G, 5 },
            { ConsoleKey.Z, 6 },
            { ConsoleKey.X, 7 },
            { ConsoleKey.C, 8 },
            { ConsoleKey.V, 9 },
            { ConsoleKey.B, 11 },
            { ConsoleKey.Q, 12 },
            { ConsoleKey.W, 13 },
            { ConsoleKey.E, 14 },
            { ConsoleKey.R, 15 },
            { ConsoleKey.Y, 16 },
            { ConsoleKey.T, 17 },
            { ConsoleKey.O, 31 },
            { ConsoleKey.U, 32 },
            { ConsoleKey.I, 34 },
            { ConsoleKey.P, 35 },
            { ConsoleKey.L, 37 },
            { ConsoleKey.J, 38 },
            { ConsoleKey.K, 40 },
            { ConsoleKey.M, 46 },
            { ConsoleKey.N, 45 },

            // Цифры (основная клавиатура)
            { ConsoleKey.D1, 18 },
            { ConsoleKey.D2, 19 },
            { ConsoleKey.D3, 20 },
            { ConsoleKey.D4, 21 },
            { ConsoleKey.D6, 22 },
            { ConsoleKey.D5, 23 },
            { ConsoleKey.D9, 25 },
            { ConsoleKey.D7, 26 },
            { ConsoleKey.D8, 28 },
            { ConsoleKey.D0, 29 },

            { ConsoleKey.LeftWindows, 55 }, // Command Left
            { ConsoleKey.RightWindows, 54 }, // Command Right

            // Нампад
            { ConsoleKey.NumPad0, 82 },
            { ConsoleKey.NumPad1, 83 },
            { ConsoleKey.NumPad2, 84 },
            { ConsoleKey.NumPad3, 85 },
            { ConsoleKey.NumPad4, 86 },
            { ConsoleKey.NumPad5, 87 },
            { ConsoleKey.NumPad6, 88 },
            { ConsoleKey.NumPad7, 89 },
            { ConsoleKey.NumPad8, 91 },
            { ConsoleKey.NumPad9, 92 },
            { ConsoleKey.Multiply, 67 },
            { ConsoleKey.Add, 69 },
            { ConsoleKey.Subtract, 78 },
            { ConsoleKey.Decimal, 65 },
            { ConsoleKey.Divide, 75 },

            // Функциональные клавиши (основные)
            { ConsoleKey.F1, 122 },
            { ConsoleKey.F2, 120 },
            { ConsoleKey.F3, 99 },
            { ConsoleKey.F4, 118 },
            { ConsoleKey.F5, 96 },
            { ConsoleKey.F6, 97 },
            { ConsoleKey.F7, 98 },
            { ConsoleKey.F8, 100 },
            { ConsoleKey.F9, 101 },
            { ConsoleKey.F10, 109 },
            { ConsoleKey.F11, 103 },
            { ConsoleKey.F12, 111 },

            // OEM знаки препинания
            { ConsoleKey.OemPlus, 24 },    // Equal (=)
            { ConsoleKey.OemMinus, 27 },   // Minus (-)
            { ConsoleKey.Oem4, 33 },       // Left Bracket ([)
            { ConsoleKey.Oem6, 30 },       // Right Bracket (])
            { ConsoleKey.Oem1, 41 },       // Semicolon (;)
            { ConsoleKey.Oem7, 39 },       // Quote (')
            { ConsoleKey.OemComma, 43 },   // Comma (,)
            { ConsoleKey.OemPeriod, 47 },  // Period (.)
            { ConsoleKey.Oem2, 42 },       // Slash (/)
            { ConsoleKey.Oem3, 50 }        // Tilde (`)
        };

        #endregion
}