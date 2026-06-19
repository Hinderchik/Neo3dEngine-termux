namespace SampleGame;

public static class TermuxEntry
{
    public static void LaunchFromArgs(string[] args)
    {
        if (args.Length == 0)
        {
            Program.Main();
            return;
        }

        string scene = args[0].ToLowerInvariant();

        switch (scene)
        {
            case "previewscene":
                RunPreviewScene();
                break;
            case "priviewnetworkscene":
            case "networkscene":
                RunNetworkScene(args);
                break;
            default:
                Console.WriteLine($"Unknown scene: {args[0]}. Falling back to default menu.");
                Program.Main();
                break;
        }
    }

    private static void RunPreviewScene()
    {
        var scene = new Scenes.PreviewScene(new _3dEngine.Implementation.DisplayManagerAsync());
        new _3dEngine.Frame(scene, new _3dEngine.Implementation.ConsoleScreenAsync()).MainLoop();
    }

    private static void RunNetworkScene(string[] args)
    {
        bool isServer = args.Length > 1 && args[1].Equals("server", StringComparison.OrdinalIgnoreCase);
        string ip = "127.0.0.1";
        int port = 7777;

        if (isServer)
        {
            if (args.Length > 2 && int.TryParse(args[2], out int parsedPort))
            {
                port = parsedPort;
            }
        }
        else
        {
            if (args.Length > 1)
            {
                ip = args[1];
            }
            if (args.Length > 2 && int.TryParse(args[2], out int parsedPort))
            {
                port = parsedPort;
            }
        }

        var scene = new Scenes.PriviewNetworkScene(
            new _3dEngine.Implementation.DisplayManagerAsync(),
            isServer,
            ip,
            port);
        new _3dEngine.Frame(scene, new _3dEngine.Implementation.ConsoleScreenAsync()).MainLoop();
    }
}
