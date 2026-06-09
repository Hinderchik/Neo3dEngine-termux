namespace _3dEngine.Interfaces;

internal interface IInputProvider
{
    void Update();
    bool IsGetKey(ConsoleKey key);
    bool IsGetKey(int virtualKey);
    bool IsShift { get; }
    bool IsCtrl  { get; }
    bool IsAlt { get; }
}