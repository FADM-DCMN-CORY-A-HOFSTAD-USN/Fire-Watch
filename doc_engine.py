#!/usr/bin/env python3
"""
FireWatch CLI Documentation Engine.
Provides interactive, scannable, and color-coded enterprise installation manuals
directly via the terminal command line environment using Typer and Rich.
"""

import typer
from rich.console import Console
from rich.panel import Panel
from rich.table import Table
from rich.syntax import Syntax

# Initialize Typer app and Rich Console rendering modules
app = typer.Typer(help="🔥 FireWatch Bridge Interactive Enterprise Documentation Engine CLI")
console = Console()

@app.command(name="server")
def display_server_install():
    """📘 View Document 1: Core Software Installation & Server Provisioning Configuration Rules."""
    console.print(Panel("[bold cyan]DOCUMENT 1: CORE SOFTWARE INSTALLATION & SERVER PROVISIONING[/bold cyan]", expand=False))
    
    # Render Prerequisites Table
    table = Table(title="Hardware & OS Baseline Prerequisites", show_header=True, header_style="bold magenta")
    table.add_column("Asset Component", style="dim", width=22)
    table.add_column("Deployment Constraint Boundary Specification")
    table.add_row("Host OS", "Windows Server 2019 / 2022 Datacenter (64-bit with Administrator Access)")
    table.add_row("GPU (Preferred)", "Dedicated NVIDIA Enterprise (Quadro, Tesla, RTX) with Compute Capability 6.0+")
    table.add_row("CPU (Fallback)", "Multi-core Intel Xeon / AMD EPYC (Minimum 4 physical cores allocated)")
    table.add_row("Runtimes", "Python 3.10+ (Added to Path) & Microsoft .NET Runtime (64-bit Core 6/8)")
    console.print(table)

    # Display build instructions
    console.print("\n[bold yellow][*] Compilation & Library Ingestion Instructions:[/bold yellow]")
    build_cmd = (
        "cd C:YourSourceCodeDirectoryFire-Watch\n"
        "dotnet build --configuration Release\n"
        "Copy-Item \".binReleasenet8.0GenetecEdwardsBridge.exe\" \"C:HospitalAppsFireWatch\"\n"
        "pip install numpy numba llvmlite"
    )
    console.print(Syntax(build_cmd, "powershell", theme="monokai", line_numbers=True))

@app.command(name="camera")
def display_camera_setup():
    """🎥 View Document 2: Camera GUID Extraction & Spectrum Analysis Tuning Calibration Data."""
    console.print(Panel("[bold green]DOCUMENT 2: CAMERA INTEGRATION & SPECTRUM ANALYSIS SETUP[/bold green]", expand=False))
    console.print("[bold yellow][*] Step 1: Extract Camera GUID variables via Genetec Config Tool[/bold yellow]")
    console.print("   Logical View -> Select Target Camera -> Identity Tab -> Copy 36-char GUID string.\n")
    
    console.print("[bold yellow][*] Step 2: Target JSON Object Layout Schema Structure (`appsettings.json`):[/bold yellow]")
    json_template = """{
  "HospitalMap": [
    {
      "CameraGuid": "8f3b2a1c-4d5e-6f7a-8b9c-0d1e2f3a4b5c",
      "GenetecName": "CAM-ICU-4F-WEST-01",
      "EdwardsNode": "NODE04",
      "EdwardsZone": "ZONE-ICU-4W",
      "PhysicalRoom": "ICU Room 412"
    }
  ]
}"""
    console.print(Syntax(json_template, "json", theme="monokai"))

@app.command(name="edwards")
def display_edwards_setup():
    """📟 View Document 3: Edwards FireWorks Receiver System Integration & Delimiter Mapping Specifications."""
    console.print(Panel("[bold magenta]DOCUMENT 3: EDWARDS FIREWORKS WORKSTATION INTEGRATION GUIDE[/bold magenta]", expand=False))
    
    table = Table(title="FireWorks Text/ASCII Receiver Configuration", show_header=True, header_style="bold cyan")
    table.add_column("Driver Setup Rule", style="bold", width=25)
    table.add_column("Assigned Value Parameter Mapping")
    table.add_row("Driver Target Type", "Generic Text/ASCII System Receiver Driver")
    table.add_row("Default Port Listener", "TCP Socket Port 2323")
    table.add_row("Message Start Byte", "ASCII Character 2 ([STX])")
    table.add_row("Message End Byte", "ASCII Character 3 ([ETX])")
    table.add_row("Field Delimiter Token", "Standard , (Comma Separated Values)")
    table.add_row("Watchdog Timeout Window", "120 Seconds (Fires Yellow Communications Fault if missed)")
    console.print(table)

@app.command(name="commands")
def display_cli_commands():
    """🛠️ View Document 4: Administrative Windows Service Control Command Reference Cheat Sheet."""
    console.print(Panel("[bold white]DOCUMENT 4: SERVICE ADMINISTRATIVE COMMAND REFERENCE SHEET[/bold white]", expand=False))
    
    cmd_txt = (
        "# Engage Application System Monitoring Loop\n"
        "sc start GenetecEdwardsFireWatch\n\n"
        "# Disengage Application Monitoring Safely\n"
        "sc stop GenetecEdwardsFireWatch\n\n"
        "# Query Real-Time Hardware Diagnostic Operational State\n"
        "sc query GenetecEdwardsFireWatch\n\n"
        "# Review Live Action Logs via PowerShell Terminal\n"
        "Get-EventLog -LogName Application -Source GenetecEdwardsFireWatch -Newest 10 | Format-List"
    )
    console.print(Syntax(cmd_txt, "bat", theme="monokai", line_numbers=True))

if __name__ == "__main__":
    app()
