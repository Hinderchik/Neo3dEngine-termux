using System.Collections.Concurrent;
using _3dEngine.Inputs.Interfaces;

namespace _3dEngine.Inputs.Implementations;

internal sealed class TermuxInputProvider : IInputProvider
{
    private readonly ConcurrentDictionary<ConsoleKey, DateTime> _pressedKeys = new();
    private readonly ConcurrentDictionary<ConsoleKey, ConsoleModifiers> _keyModifiers = new();
    private readonly Thread _inputThread;
    private readonly CancellationTokenSource _cts = new();
    private readonly TimeSpan _keyReleaseTimeout = TimeSpan.FromMilliseconds(90);

    public bool IsAvailable => true;
    public string? InitializationError => null;

    public TermuxInputProvider()
    {
        _inputThread = new Thread(ReadKeysLoop)
        {
            IsBackground = true,
            Name = "TermuxInputReader"
        };
        _inputThread.Start();
    }

    private void ReadKeysLoop()
    {
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                _pressedKeys[keyInfo.Key] = DateTime.UtcNow;
                _keyModifiers[keyInfo.Key] = keyInfo.Modifiers;
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    public void Update()
    {
        var now = DateTime.UtcNow;

        foreach (var kvp in _pressedKeys.ToArray())
        {
            if ((now - kvp.Value) > _keyReleaseTimeout)
            {
                _pressedKeys.TryRemove(kvp.Key, out _);
                _keyModifiers.TryRemove(kvp.Key, out _);
            }
        }
    }

    public bool IsGetKey(ConsoleKey key) => _pressedKeys.ContainsKey(key);

    public bool IsGetKey(int virtualKey) => IsGetKey((ConsoleKey)virtualKey);

    public bool IsShift => HasModifier(ConsoleModifiers.Shift);
    public bool IsCtrl => HasModifier(ConsoleModifiers.Control);
    public bool IsAlt => HasModifier(ConsoleModifiers.Alt);

    private bool HasModifier(ConsoleModifiers modifier)
    {
        foreach (var kvp in _keyModifiers)
        {
            if ((kvp.Value & modifier) != 0)
            {
                return true;
            }
        }

        return false;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _inputThread.Join(TimeSpan.FromMilliseconds(250));
        _cts.Dispose();
    }
}
