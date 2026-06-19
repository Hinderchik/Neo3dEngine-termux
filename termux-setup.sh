#!/data/data/com.termux/files/usr/bin/bash
set -e

PROJECT_DIR="$HOME/Neo3dEngine-termux"
REPO_URL="https://github.com/Hinderchik/Neo3dEngine-termux"
BRANCH="main"

if ! command -v dotnet >/dev/null 2>&1; then
    echo "Installing .NET SDK..."
    pkg update -y
    pkg install -y dotnet-sdk-8.0
fi

echo "Checking required packages..."
pkg install -y git unzip curl

if [ -d "$PROJECT_DIR" ]; then
    echo "Project already exists at $PROJECT_DIR"
    read -rp "Pull latest changes? [Y/n]: " answer
    answer=${answer:-Y}
    if [[ "$answer" =~ ^[Yy]$ ]]; then
        cd "$PROJECT_DIR"
        git pull origin "$BRANCH"
    fi
else
    echo "Cloning repository..."
    git clone --depth 1 "$REPO_URL" "$PROJECT_DIR"
    cd "$PROJECT_DIR"
fi

echo ""
echo "Available scenes:"
echo "1) PreviewScene (singleplayer, 3D object demo)"
echo "2) PriviewNetworkScene (client/server lobby)"
echo ""
read -rp "Select scene [1-2, default 1]: " choice
choice=${choice:-1}

case "$choice" in
    1) SCENE_NAME="PreviewScene" ;;
    2) SCENE_NAME="PriviewNetworkScene" ;;
    *)
        echo "Invalid choice. Using PreviewScene."
        SCENE_NAME="PreviewScene"
        ;;
esac

echo ""
echo "Building project in Release mode..."
cd "$PROJECT_DIR/SampleGame"
dotnet build -c Release
echo ""
echo "Build complete."

echo ""
echo "Setup finished. To run your scene use:"
echo "  cd $PROJECT_DIR && ./termux-run.sh $SCENE_NAME"
