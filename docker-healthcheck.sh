#!/bin/bash
# Health check script for GitVisionMCP Docker container

set -e

# Function to check if the application is running
check_app_health() {
    # Check if the GitVisionMCP process is running
    if pgrep -f "GitVisionMCP.dll" > /dev/null; then
        echo "✓ GitVisionMCP process is running"
        return 0
    else
        echo "✗ GitVisionMCP process is not running"
        return 1
    fi
}

# Function to check if Git is available
check_git_availability() {
    if command -v git &> /dev/null; then
        echo "✓ Git is available"
        git --version
        return 0
    else
        echo "✗ Git is not available"
        return 1
    fi
}

# Function to check workspace mount
check_workspace_mount() {
    if [ -d "/workspace" ]; then
        echo "✓ Workspace directory is mounted"
        ls -la /workspace | head -5
        return 0
    else
        echo "✗ Workspace directory is not mounted"
        return 1
    fi
}

# Function to check logs directory
check_logs_directory() {
    if [ -d "/app/logs" ]; then
        echo "✓ Logs directory is available"
        return 0
    else
        echo "! Logs directory is not available (optional)"
        return 0  # Not critical
    fi
}

# Main health check
echo "=== GitVisionMCP Health Check ==="
echo "Timestamp: $(date)"
echo

# Perform checks
CHECKS_PASSED=0
TOTAL_CHECKS=4

echo "1. Checking Git availability..."
if check_git_availability; then
    ((CHECKS_PASSED++))
fi
echo

echo "2. Checking workspace mount..."
if check_workspace_mount; then
    ((CHECKS_PASSED++))
fi
echo

echo "3. Checking logs directory..."
if check_logs_directory; then
    ((CHECKS_PASSED++))
fi
echo

echo "4. Checking application process..."
if check_app_health; then
    ((CHECKS_PASSED++))
fi
echo

# Summary
echo "=== Health Check Summary ==="
echo "Checks passed: $CHECKS_PASSED/$TOTAL_CHECKS"

if [ $CHECKS_PASSED -eq $TOTAL_CHECKS ]; then
    echo "✓ All health checks passed - Container is healthy"
    exit 0
elif [ $CHECKS_PASSED -ge 2 ]; then
    echo "! Some health checks failed - Container may have issues"
    exit 1
else
    echo "✗ Multiple health checks failed - Container is unhealthy"
    exit 2
fi
