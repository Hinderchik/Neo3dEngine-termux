#!/data/data/com.termux/files/usr/bin/bash
set -e

TERMUX_DIR="$HOME/Neo3dEngine-termux"
REPO_URL="https://github.com/Hinderchik/Neo3dEngine-termux"
BRANCH="main"

print_header() {
    echo "================================"
    echo " Neo3dEngine Termux Setup "
    echo "================================"
}

check_dotnet() {
    if command -v dotnet >/dev/null 2>&1; then
        echo ".NET SDK already installed: $(dotnet --version)"
    else
        echo "Installing .NET SDK..."
        pkg update -y
        pkg install -y dotnet-sdk-8.0
    fi
}

install_packages() {
    echo "Checking required packages..."
    pkg install -y git
}

clone_repo() {
    if [ -d "$TERMUX_DIR" ]; then
        echo "Project already exists at $TERMUX_DIR"
        read -rp "Pull latest changes? [Y/n]: " answer
        answer=${answer:-Y}
        if [[ "$answer" =~ ^[Yy]$ ]]; then
            cd "$TERMUX_DIR"
            git pull origin "$BRANCH"
        fi
    else
        echo "Cloning repository..."
        git clone --depth 1 "$REPO_URL" "$TERMUX_DIR"
        cd "$TERMUX_DIR"
    fi
}

select_scene() {
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

    export TERMUX_SELECTED_SCENE="$SCENE_NAME"
    echo "Selected scene: $SCENE_NAME"
}

build_project() {
    echo ""
    echo "Building project in Release mode..."
    cd "$TERMUX_DIR/SampleGame"
    dotnet build -c Release
    echo ""
    echo "Build complete."
}

main() {
    print_header
    check_dotnet
    install_packages
    clone_repo
    select_scene
    build_project

    echo ""
    echo "Setup finished. To run your scene use:"
    echo "  cd $TERMUX_DIR && ./termux-run.sh $TERMUX_SELECTED_SCENE"
}

main
