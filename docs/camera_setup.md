# DOCUMENT 2: CAMERA INTEGRATION & SPECTRUM ANALYSIS SETUP
**System:** Genetec Edwards FireWatch Bridge Service  
**Classification:** Supplemental Hospital Life-Safety Systems Middleware  
**Target Audience:** Security System Integrators, Genetec Administrators

---

## 1. Extracting Camera GUID Variables from Genetec
To process video streams, the middleware must map specific physical camera assets using their unique 36-character Global Unique Identifiers (GUIDs).

### 1.1 Extraction using Genetec Config Tool
1. Launch the **Genetec Config Tool** and log in using deployment admin credentials.
2. Navigate to the **Video Task Bar** and click on the **Logical View** or **Hardware View**.
3. Select the target physical camera watching the hospital wing or room.
4. Click on the **Identity** tab or open the advanced property panel.
5. Locate the **GUID** parameter (e.g., `8f3b2a1c-4d5e-6f7a-8b9c-0d1e2f3a4b5c`) and copy it to a secure staging scratchpad.

---

## 2. Mapping Cameras inside `appsettings.json`
Open `C:\HospitalApps\FireWatch\appsettings.json` in a secure text editor. Map the exact camera data parameters under the array block titled `"HospitalMap"`:

```json
{
  "GenetecConfig": {
    "DirectoryServer": "10.100.20.5",
    "ServiceUser": "EdwardsBridgeUser",
    "ServicePassword": "SecurePassword123",
    "KiwiFireEventGuid": "00000000-0000-0000-0000-000000000000"
  },
  "HospitalMap": [
    {
      "CameraGuid": "8f3b2a1c-4d5e-6f7a-8b9c-0d1e2f3a4b5c",
      "GenetecName": "CAM-ICU-4F-WEST-01",
      "EdwardsNode": "NODE04",
      "EdwardsZone": "ZONE-ICU-4W",
      "PhysicalRoom": "ICU Room 412"
    },
    {
      "CameraGuid": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
      "GenetecName": "CAM-ER-LOBBY-02",
      "EdwardsNode": "NODE01",
      "EdwardsZone": "ZONE-ER-MAIN",
      "PhysicalRoom": "ER Waiting Area"
    }
  ]
}
```

---

## 3. Tuning the Spectrum Detection Coefficients
The engine analyzes frames in real-time, checking pixel bytes against the physics of fire color profiles. If false alarms occur due to certain interior lighting setups, tune the limits inside your engine files:

### 3.1 Luminance Filters (`FireDetectionEngine.cs`)
* Locate `private const int RedThreshold = 190;`
* **Calibration Rule:** If an environment has bright orange or red background elements, raise this value (up to `220`) to desensitize the analyzer. If the environment is dark, lower this limit (down to `160`) to catch weak, developing fires earlier.

### 3.2 Target Mass Triggers (`fire_cuda_engine.py`)
* Locate `private const double TriggerPercentage = 1.5;`
* **Calibration Rule:** This dictates the minimum total pixel area matching the fire spectrum required to trigger an alarm state. If a camera has a very wide field of view, lower this to `0.5%` so small, distant flames are caught. For narrow, tight room angles, increase this to `3.0%` to ignore minor visual artifacts like computer screen wallpapers.
