
## Core Architecture & Technical Features
*   **Dual-Engine Analytics Layer:** 
    *   `fire_cuda_engine.py`: Leverages native NVIDIA GPU kernels via CUDA Python to execute parallel pixel matrix operations for rapid visual flame and structural smoke parsing.
    *   `FireDetectionEngine.cs` / `doc_engine.py`: Employs multi-core CPU scheduling and Numba Just-In-Time (`NJIT`) compilation to handle fallback algorithmic processing.
*   **Low-Overhead Memory Mapping:** Streams unmanaged video buffers directly into hardware-accelerated detection pipes, bypassing traditional garbage collection bottlenecks in high-throughput environments.
*   **Enterprise Middleware Bridging:** Implements `UnivacAegisBridge.cs`, `EdwardsAegisNode.cs`, and `EdwardsProtocolEncoder.cs` to handle thread-safe byte serialization, translating visual alerts into serial-ready or IP-ready Edwards proprietary incident flags.
*   **Production Deployment:** Configured as a native Windows Service (`FireWatchService.Main.cs`) with integrated multi-threaded background networking (`FireWatchService.Network.cs`).

## Repository Directory & Core Modules
*   `FireWatchService.cs` / `.Main.cs` — Core Windows Service wrapper managing the daemon lifecycle, thread-pools, and event hooks.
*   `FireDetectionEngine.cs` — The central C# manager coordinating image buffer allocation, thread-safe memory handoffs, and target processing queues.
*   `fire_cuda_engine.py` — The primary GPU-bound analytics module mapping raw video matrices directly to CUDA threads for lightning-fast spatial-temporal filtering.
*   `EdwardsProtocolEncoder.cs` — Telemetry and protocol assembly layer converting computer vision event triggers into structural Edwards FireWorks data telegrams.
*   `Deploy-FireWatch.ps1` — Automated deployment, service installation, and system environment configuration script for Windows production servers.
*   `TestHarness.py` — High-fidelity simulation tool to mock incoming Genetec RTSP/SDK streams and validate downstream hardware execution without an active security desk environment.

## Installation & Setup

### Prerequisites
*   **Operating System:** Windows Server 2019/2022 or Windows 10/11 (64-bit)
*   **SDKs:** .NET SDK 6.0 / 8.0+ & Python 3.10+
*   **Hardware Acceleration:** NVIDIA GPU (Compute Capability 6.1+) with **CUDA Toolkit 11.8+** installed.
*   **External Integration:** Authorized access to a Genetec Security Center SDK license and an active network link to an Edwards FireWorks server node.

### Quick Deployment Steps
1.  **Clone the Repository:**
    ```bash
    git clone https://github.com
    cd Fire-Watch
    ```
2.  **Configure Environment Parameters:**
    Open `appsettings.json` and adjust the binding configurations, camera stream tokens, and Edwards node destinations:
    ```json
    {
      "GenetecConfig": { "ServerIp": "10.0.0.5", "PollingIntervalMs": 33 },
      "EdwardsConfig": { "NodeId": 101, "ComPort": "COM3", "BaudRate": 9600 }
    }
    ```
3.  **Install Python Dependencies:**
    ```bash
    pip install -r docs/requirements.txt
    # Ensure numba and cuda-python are properly mapped to your local CUDA path
    ```
4.  **Build and Deploy the Windows Service:**
    Run the automated deployment script via an elevated PowerShell prompt:
    ```powershell
    Set-ExecutionPolicy Bypass -Scope Process
    .\Deploy-FireWatch.ps1 -Install
    ```

## Contributions & Peer Review
This system interfaces directly with structural life-safety mechanisms. Hardware engineers, enterprise C# architects, and CUDA developers are invited to optimize processing bottlenecks.
*   To update parallel pixel scanning algorithms or introduce custom CUDA primitives, please open a **Pull Request**.
*   To document performance metrics, hardware timing constraints, or report SDK integration issues, utilize the **Issues** tab.

## License
This project is licensed under the terms of the [Boost Software License 1.0 (BSL-1.0)](LICENSE) — granting free permissions to use, modify, and distribute the software for both commercial and private execution.
