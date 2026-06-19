#!/data/data/com.termux/files/usr/bin/bash
set -e

PROJECT_DIR="$HOME/Neo3dEngine-termux"
SCENE_NAME="${1:-PreviewScene}"

if [ ! -d "$PROJECT_DIR" ]; then
    echo "Project not found at $PROJECT_DIR"
    echo "Run ./termux-setup.sh first."
    exit 1
fi

cd "$PROJECT_DIR/SampleGame"

if [ "$SCENE_NAME" = "PriviewNetworkScene" ]; then
    read -rp "Start as [S]erver or [C]lient? [S/c]: " mode
    mode=${mode:-S}

    if [[ "$mode" =~ ^[Ss]$ ]]; then
        read -rp "Port [default 7777]: " port
        port=${port:-7777}
        dotnet run -c Release -- server "$port"
    else
        read -rp "Server IP [default 127.0.0.1]: " ip
        ip=${ip:-127.0.0.1}
        read -rp "Server Port [default 7777]: " port
        port=${port:-7777}
        dotnet run -c Release -- client "$ip" "$port"
    fi
else
    dotnet run -c Release -- "$SCENE_NAME"
fi
