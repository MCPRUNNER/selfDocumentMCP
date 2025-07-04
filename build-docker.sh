#!/bin/bash

# GitVisionMCP Docker Build Script

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Default values
BUILD_TYPE="production"
PLATFORM="linux/amd64"
TAG="gitvisionmcp:latest"
PUSH=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -d|--dev)
            BUILD_TYPE="development"
            TAG="gitvisionmcp:dev"
            shift
            ;;
        -p|--platform)
            PLATFORM="$2"
            shift 2
            ;;
        -t|--tag)
            TAG="$2"
            shift 2
            ;;
        --push)
            PUSH=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  -d, --dev           Build development image"
            echo "  -p, --platform      Target platform (default: linux/amd64)"
            echo "  -t, --tag           Docker tag (default: gitvisionmcp:latest)"
            echo "  --push              Push image to registry"
            echo "  -h, --help          Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

print_status "Starting Docker build for GitVisionMCP"
print_status "Build type: $BUILD_TYPE"
print_status "Platform: $PLATFORM"
print_status "Tag: $TAG"

# Check if Docker is available
if ! command -v docker &> /dev/null; then
    print_error "Docker is not installed or not in PATH"
    exit 1
fi

# Check if we're in the correct directory
if [[ ! -f "GitVisionMCP.csproj" ]]; then
    print_error "GitVisionMCP.csproj not found. Are you in the project root?"
    exit 1
fi

# Build the appropriate image
if [[ "$BUILD_TYPE" == "development" ]]; then
    print_status "Building development image with hot reload..."
    docker build -f Dockerfile.dev -t "$TAG" --platform "$PLATFORM" .
else
    print_status "Building production image..."
    docker build -f Dockerfile -t "$TAG" --platform "$PLATFORM" .
fi

if [[ $? -eq 0 ]]; then
    print_status "Build completed successfully!"
    
    # Show image info
    docker images | grep gitvisionmcp
    
    # Push if requested
    if [[ "$PUSH" == true ]]; then
        print_status "Pushing image to registry..."
        docker push "$TAG"
    fi
    
    print_status "Ready to run with:"
    if [[ "$BUILD_TYPE" == "development" ]]; then
        echo "  docker-compose --profile dev up gitvisionmcp-dev"
    else
        echo "  docker-compose up gitvisionmcp"
    fi
    echo "  OR"
    echo "  docker run -d --name gitvisionmcp-container \\"
    echo "    -v \"\$(pwd)/workspace:/workspace:ro\" \\"
    echo "    -v \"\$(pwd)/logs:/app/logs\" \\"
    echo "    $TAG"
else
    print_error "Build failed!"
    exit 1
fi
