using _3dEngine;
using _3dEngine.Implementation;
using SampleGame.Scenes;

namespace Neo3dEngine.Android;

public static class AndroidSceneLauncher
{
    public static void RunPreviewScene(AndroidScreen screen)
    {
        var scene = new PreviewScene(new DisplayManagerAsync());
        new Frame(scene, screen).MainLoop();
    }

    public static void RunNetworkScene(AndroidScreen screen, bool isServer, string ip, int port)
    {
        var scene = new PriviewNetworkScene(new DisplayManagerAsync(), isServer, ip, port);
        new Frame(scene, screen).MainLoop();
    }
}
