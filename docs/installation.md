# DOCUMENT 1: CORE SOFTWARE INSTALLATION & SERVER PROVISIONING
**System:** Genetec Edwards FireWatch Bridge Service  
**Classification:** Supplemental Hospital Life-Safety Systems Middleware  
**Target Audience:** Network Administrators, Field Commissioning Engineers

---

## 1. Prerequisites Checklist

### 1.1 Host Operating System Requirements
* Windows Server 2019 / 2022 Datacenter or Windows 10/11 Professional (64-bit).
* **Administrative Privileges** are mandatory for installation and runtime execution.

### 1.2 Hardware & Acceleration Assets
* **NVIDIA Enterprise GPU:** Dedicated Quadro, Tesla, or RTX Series card featuring Compute Capability 6.0+.
* **CPU Fallback Infrastructure:** Multi-core Intel Xeon or AMD EPYC processor (Minimum 4 physical cores allocated) to handle fallback parallel calculations.

---

## 2. Fundamental Software Dependency Setup
Before creating the service directory, install these packages sequentially on the host machine:

1. **Python Runtime Engine:**
   * Download and install **Python 3.10** or higher (64-bit).
   * **CRITICAL:** Ensure the checkbox labeled **"Add Python to PATH"** is ticked during the installation phase.
2. **NVIDIA Enterprise Drivers & CUDA Toolkit:**
   * Install the latest stable NVIDIA Enterprise Graphics Driver.
   * Download and install the **NVIDIA CUDA Toolkit** matching your exact graphics hardware version profile.
3. **Microsoft .NET Runtime Environment:**
   * Install the **.NET Runtime (64-bit)** matching your compilation targets (`.NET Core 6/8` or `.NET Framework 4.8`).

---

## 3. Directory Provisioning and Code Compilation

### 3.1 Server File Architecture Setup
1. Create the persistent root installation directory on the host hospital server:  
   `C:\HospitalApps\FireWatch\`
2. Verify that your live directory space populates with these specific runtime file components:
   * `GenetecEdwardsBridge.exe` (Main Compiled Windows Service)
   * `appsettings.json` (Live Network Mapping Configurations)
   * `fire_cuda_engine.py` (Hardware-Accelerated Math Engine)
   * `[Genetec SDK DLLs]` (Associated Genetec Security Center library dependencies)

### 3.2 Compilation Step (Generating the Executable)
Open an elevated PowerShell prompt on your development environment and execute the following commands to compile your repository code into the production-ready `.exe`:
```powershell
cd C:\YourSourceCodeDirectory\Fire-Watch
dotnet build --configuration Release
Copy-Item ".\bin\Release\net8.0\GenetecEdwardsBridge.exe" "C:\HospitalApps\FireWatch\"
```

### 3.3 Install Backend Parallel Processing Libraries
Open an elevated Windows Command Prompt and install the kernel vectorization modules using the Python package manager:
```cmd
pip install numpy numba llvmlite
```

---

## 4. Hardware Verification
Verify that the underlying calculation nodes are successfully communicating with the server's graphics processors by issuing this string check command:
```cmd
python -c "from numba import cuda; print('CUDA Engine Binding Status:', cuda.is_available())"
```
* **Expected Result:** `CUDA Engine Binding Status: True`
* *Operational Note:* If the trace reads `False`, check the server's NVIDIA driver layout. The application will down-shift to multi-core parallel CPU calculation automatically to preserve continuity, but processing capacity will be throttled.
