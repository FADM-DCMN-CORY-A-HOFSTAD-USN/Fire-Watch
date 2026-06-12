# DOCUMENT 4: SERVICE ADMINISTRATIVE COMMAND REFERENCE SHEET
**System:** Genetec Edwards FireWatch Bridge Service  
**Classification:** Supplemental Hospital Life-Safety Systems Middleware  
**Target Audience:** System Administrators, Helpdesk Operators, Plant Technicians

---

## 1. Automated Pipeline Installation Script Execution
To deploy the application folder assets onto the Windows Service Control Manager database, execute the deployment automation script from an elevated PowerShell console session:

```powershell
cd C:\HospitalApps\FireWatch\
.\Deploy-FireWatch.ps1
```

---

## 2. Critical Service Control Commands (`sc.exe`)
These management commands must be run from an **Elevated Command Prompt (Run as Administrator)**.

### 2.1 Starting and Stopping Lifecycle Tasks
* **Engage Application System Monitoring:**
  ```cmd
  sc start GenetecEdwardsFireWatch
  ```
* **Disengage Application System Monitoring Safely:**
  ```cmd
  sc stop GenetecEdwardsFireWatch
  ```

### 2.2 Querying Real-Time Operational State Traces
To look up if the application is executing calculations, crashed, or hung, issue this query check status command:
```cmd
sc query GenetecEdwardsFireWatch
```
* *Verification Target:* Verify the terminal console output block reads `STATE : 4 RUNNING`.

### 2.3 Removing / Purging Application System Registration
If you need to change deployment directories or perform clean system teardowns, run this purge tracking command:
```cmd
sc delete GenetecEdwardsFireWatch
```
*⚠️ **Crucial Note:** You must execute `sc stop GenetecEdwardsFireWatch` and close any active system event log windows before running the delete command.*

---

## 3. Live Log Auditing and Verification Traces
The application records all runtime events—including video connection losses, camera hot-swap success messages, and validation error reports—directly into the native operating system log files.

### 3.1 Reviewing Logs using Terminal Commands
Open an elevated PowerShell window to quickly inspect the 10 most recent actions recorded by your service:
```powershell
Get-EventLog -LogName Application -Source GenetecEdwardsFireWatch -Newest 10 | Format-List TimeGenerated, EntryType, Message
```

### 3.2 Troubleshooting Common Structural Log Entries
* **Event Type - Error:** `Configuration hot-swap aborted due to schema/data errors`
  * *Meaning:* A plant engineer edited `appsettings.json`, but entered an invalid IP structure or a malformed Camera GUID length. The service rejected the save and safely kept running using the old data map.
* **Event Type - Warning:** `Fire alert dropped! Received alert for Camera GUID '...', but it has no mapped node/zone`
  * *Meaning:* The fire engine detected a visual fire spectrum signature on a camera, but that camera GUID is not registered in the array block. Check your mapping fields.
