using System.Collections.Concurrent;
using _3dEngine.Inputs.Interfaces;

namespace Neo3dEngine.Android;

public class AndroidInputProvider : IInputProvider
{
    private readonly ConcurrentDictionary<ConsoleKey, DateTime> _pressedKeys = new();
    private readonly ConcurrentDictionary<ConsoleKey, ConsoleModifiers> _keyModifiers = new();
    private readonly TimeSpan _keyReleaseTimeout = TimeSpan.FromMilliseconds(90);

    public bool IsAvailable => true;
    public string? InitializationError => null;

    public void PressKey(ConsoleKey key, ConsoleModifiers modifiers = 0)
    {
        _pressedKeys[key] = DateTime.UtcNow;
        _keyModifiers[key] = modifiers;
    }

    public void ReleaseKey(ConsoleKey key)
    {
        _pressedKeys.TryRemove(key, out _);
        _keyModifiers.TryRemove(key, out _);
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
    }
}
