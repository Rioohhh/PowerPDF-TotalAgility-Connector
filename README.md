# Tungsten Power PDF – Connector for Tungsten TotalAgility

This repository provides a .NET Framework-based connector for integrating Tungsten Power PDF with Tungsten TotalAgility. It allows users to send the currently open PDF document directly to Tungsten TotalAgility, creating a job on a configured process with a single click from the Power PDF ribbon.

> **This is a fork** of [TungstenAutomationLabs/PowerPDF-TotalAgility-Connector](https://github.com/TungstenAutomationLabs/PowerPDF-TotalAgility-Connector). See [Changes in this fork](#-changes-in-this-fork).

---

## 📁 Folder Structure

```
PowerPDF-TotalAgility-Connector/
├── PPDF.TotalAgility.Connector/        # Source project (.csproj)
├── PPDFTATester/                        # Standalone test harness (WinForms)
├── References/                          # External DLLs (DMSConnector.dll)
├── install.ps1                          # Installer / uninstaller for Power PDF
├── PPDF.TotalAgility.Connector.sln      # Visual Studio solution file
├── README.md
```

---

## ✨ Changes in this fork

- **Multiple connections (environments).** Keep several TotalAgility environments, for example one per customer, and pick the one to use from a dropdown under *File → Options → Connectors → TotalAgility*. Every connection has its own SDK URL, login and document types. The **Send** dialog shows a coloured banner with the name and host of the environment the PDF goes to, so you do not send something to the wrong customer.
- **Document types (profiles).** Instead of one fixed process, you define several profiles, for example *Invoice* → invoice process and *Office supplies request* → office supplies process. Each profile has its own process and its own initialization variable values. When you click **Send to TotalAgility**, you pick the document type and the PDF goes to that process.
- **TLS 1.2 / 1.3.** Power PDF starts the connector with SSL3/TLS 1.0 only, which Tungsten Cloud rejects ("The underlying connection was closed: An unexpected error occurred on a send"). The connector now enables TLS 1.2, and 1.3 where available.
- **`install.ps1`** does the whole installation (copy, regasm, ribbon XML, cache) in one go.
- Settings of earlier versions (one connection, one process or one list of document types) are converted automatically the first time the new version runs.

---

## 🚀 Installation (recommended)

You need **administrator rights** on the PC and the Power PDF version this was built for (Power PDF 51, 32-bit, `C:\Program Files (x86)\Kofax\Power PDF 51`).

1. **Close Power PDF.**
2. Download `PPDF-TotalAgility-Connector-deploy.zip` from the [Releases](../../releases) page and **extract it** to a normal folder, for example `C:\Temp\ppdf-ta`. The folder contains `PPDF.TotalAgility.Connector.dll`, `Newtonsoft.Json.dll`, language folders and `install.ps1`.
3. Open **PowerShell as administrator** (right-click Start → *Terminal (Admin)* or *Windows PowerShell (Admin)*).
4. Run:

   ```powershell
   powershell -ExecutionPolicy Bypass -File "C:\Temp\ppdf-ta\install.ps1"
   ```

5. Start Power PDF. You should see **Send to TotalAgility** and **Configure** in the *Connectors* ribbon tab.

`install.ps1` performs these steps:

| Step | What it does |
|---|---|
| 1 | Copies the connector files to `<Power PDF>\bin\Connectors` |
| 2 | Registers the DLL with `regasm /codebase` (the 32-bit regasm for `Program Files (x86)`, the 64-bit one otherwise) |
| 3 | Adds the TotalAgility buttons to `Resource\PowerPDF\UILayout\Publish Mode.xml` and the display name to every `Resource\PowerPDF\<language>\NameAndTitle.xml`. A `.bak` copy is made once next to each edited file |
| 4 | Deletes the cached ribbon layout `%appdata%\Kofax\PDF\PowerPDF\UILayout\Publish.xml` |

The script can be run again safely (for example to update to a new version): it does not add the XML entries twice.

**Different Power PDF folder or version** (for example Power PDF 2025, 64-bit):

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Temp\ppdf-ta\install.ps1" -PowerPdfRoot "C:\Program Files\Tungsten\Power PDF 2025"
```

**Uninstall:**

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Temp\ppdf-ta\install.ps1" -Uninstall
```

This unregisters the DLL, removes the connector files and removes the ribbon entries (the `Newtonsoft.Json.dll` is left in place because other connectors may use it).

### Installing from a build instead of the zip

After building the solution in Visual Studio (Release), run the script with the build output as source:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1 -SourceDir ".\PPDF.TotalAgility.Connector\bin\Release"
```

> The Release build has a post-build step that closes Power PDF and copies the DLL to `C:\Program Files\Tungsten\Power PDF 2025\bin\Connectors\`. Adjust or remove it in the project properties if that is not your setup.

---

## 🛠️ Manual installation

Use this only if you cannot run `install.ps1`.

### 1. Copy the connector DLL and dependencies

Copy these files to the Power PDF Connectors folder:

| File | Source |
|---|---|
| `PPDF.TotalAgility.Connector.dll` | `bin\Release\` |
| `Newtonsoft.Json.dll` | `bin\Release\` |
| `Microsoft.Web.WebView2.Core.dll`, `Microsoft.Web.WebView2.WinForms.dll`, `runtimes\` | `bin\Release\` (only for *Federated Security* login) |

**Power PDF 51:** `C:\Program Files (x86)\Kofax\Power PDF 51\bin\Connectors\`
**Power PDF 2025:** `C:\Program Files\Tungsten\Power PDF 2025\bin\Connectors\`

### 2. Register the connector DLL

Run from an **elevated (Administrator) command prompt**:

**Power PDF 51 (32-bit):**
```
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe" "[Connectors folder]\PPDF.TotalAgility.Connector.dll" /codebase
```

**Power PDF 2025 (64-bit):**
```
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" "[Connectors folder]\PPDF.TotalAgility.Connector.dll" /codebase
```

To unregister:
```
regasm.exe /unregister "[Connectors folder]\PPDF.TotalAgility.Connector.dll"
```

### 3. Update Publish Mode configuration

Open (the file is UTF-16, so use an editor that preserves that, such as Notepad):
```
[Power PDF install folder]\Resource\PowerPDF\UILayout\Publish Mode.xml
```

Locate the line `<toolbar name="connectors" shortKey ="N">` (the spacing may differ) and add the following immediately after it:
```xml
<PFFGroup name="connector::TotalAgility" GroupType="PFFTitleBlock" autoZip="0">
    <PFFButton name="connector::tool::TotalAgility:menuItem0"/>
    <PFFButton name="connector::tool::TotalAgility:menuItem1"/>
</PFFGroup>
```

`menuItem0` is **Send to TotalAgility**, `menuItem1` is **Configure**.

### 4. Add the connector display name

Edit every language file, for example:
```
[Power PDF install folder]\Resource\PowerPDF\ENU\NameAndTitle.xml
[Power PDF install folder]\Resource\PowerPDF\NLD\NameAndTitle.xml
```

Add under the `<!--connectors toolbar-->` section:
```xml
<PFFGroup name="connector::TotalAgility" title="Send to TotalAgility" />
```

### 5. Clear the cache and restart

Delete the cached layout file if it exists:
```
%appdata%\Kofax\PDF\PowerPDF\UILayout\Publish.xml
```

Restart Power PDF.

---

## ⚙️ Configuration (per user)

Every user does this once after installation:

1. Go to **File → Options → Connectors → TotalAgility**
2. Under **Environment** you see the connection that is in use. Click **New** for another one, give it a **Name** (for example the customer), and enter its **TA SDK URL** (e.g. `https://yourtenant.tungstencloud.com/Services/Sdk` or `http://server/TotalAgility/Services/Sdk`)
3. Choose the authentication type and enter your **User ID** and **Password** (*Standard*), then click **Connect**
4. Click **Save**. The connection that is shown becomes the active one, the one the **Send** button uses
5. Click **Configure** in the TotalAgility ribbon to manage the document types **of the active connection**:
   - **New** adds a profile; give it a name (for example *Invoice*)
   - Select the **Process** for that profile
   - Fill in the **Initialization Variables** shown for it (every profile keeps its own values)
   - Add more profiles for your other document types, then click **Save**

Document types belong to one connection, because process IDs differ per environment. For each customer you configure its own list.

**Switching environment:** choose another entry in the **Environment** dropdown. A connection that has been saved before becomes active immediately. A connection you just created becomes active when you click **Save**. **Delete** removes the shown connection with its document types and saved credentials (applied on **Save**; the last connection cannot be deleted).

### Where the settings are stored

Settings are stored per Windows user:

| Location | Content |
|---|---|
| `HKCU\Software\ScanSoft\Connectors\TotalAgility` → `TA_CONNECTIONS` | All connections as JSON: name, SDK URL, authentication type and the document types. No credentials |
| same key → `TA_ACTIVE_CONNECTION`, `TA_SHOW_CONFIRMATION` | The active connection and the confirmation switch |
| `HKCU\Software\ScanSoft\Connectors\TotalAgilitySecrets` (a *separate* key) | Per connection: user ID and password, **encrypted with the Windows account of the user (DPAPI) and therefore not usable on another user or PC**, and the temporary session |

Values of earlier versions (`TA_SDK_URL`, `TA_USER_ID`, `TA_PASSWORD`, `TA_PROFILES`, ...) stay in the first key after the upgrade but are no longer used. You may delete them.

### Pre-configuring several PCs

Export the connections of a configured PC. Only the first key is exported, so no credentials are included:

```powershell
reg export "HKCU\Software\ScanSoft\Connectors\TotalAgility" C:\Temp\ta-settings.reg /y
```

Open the `.reg` file in a text editor and **delete the lines `TA_USER_ID`, `TA_PASSWORD` and `TA_SESSION_ID`** if they are present (leftovers of an earlier version). Users then double-click the file (or you push the values with a GPO/Intune script), open Power PDF and only enter their own User ID and Password per environment. With *Windows AD* authentication no password is needed.

---

## 🎯 Usage

1. Open a PDF document in Power PDF
2. Click **Send to TotalAgility** in the Connectors ribbon tab
3. Check the coloured banner at the top: it names the **environment** the PDF will be sent to. If it is not the right one, click **Cancel** and change the environment under *File → Options → Connectors → TotalAgility*
4. Choose the document type (profile) and click **Send**. **Manage...** opens the same screen as **Configure** for this environment
5. The connector authenticates, submits the PDF and creates a job on the process of that profile
6. A result screen shows the **Job ID**, the **Process Name** with the environment in brackets, the **Document Name** and the **Created At** timestamp

The dialog is only skipped when there is exactly one connection with exactly one document type and the confirmation is switched off (*Do not show this message again*, or *Show confirmation before sending* in the options). With several connections you always see the banner.

---

## 🧹 Troubleshooting

| Issue | Solution |
|---|---|
| Connector not appearing in ribbon | Check the `Publish Mode.xml` entry and delete cached `Publish.xml`, or re-run `install.ps1` |
| Connector not appearing in Options | Verify the DLL is registered via regasm and the key exists under `HKLM\Software\WOW6432Node\ScanSoft\Connectors\TotalAgility` |
| 64-bit registration error | Use `Framework64` regasm for Power PDF 2025 (64-bit) |
| *Network error … De onderliggende verbinding is gesloten / The underlying connection was closed* | Install this fork's build (TLS 1.2 fix). Older builds only offer TLS 1.0 |
| Job creation fails | Verify SDK URL, credentials and the process configuration in **Configure** |
| *No document type has been configured for ...* | Click **Configure**, add a profile and select a process (every connection has its own list) |
| *No TotalAgility connection has been configured* | Enter the SDK URL and credentials under *File → Options → Connectors → TotalAgility* and click **Save** |
| Wrong environment in the Send dialog | Choose the right one in the **Environment** dropdown in the options and click **Save** |
| Newtonsoft.Json not found | Copy `Newtonsoft.Json.dll` to the Connectors folder |
| Install script says Power PDF is running | Close Power PDF (check Task Manager for `PowerPDF.exe`) and run it again |
| Debug log | `C:\Temp\TAConnectorDebug.txt` (created automatically). It contains no passwords, user names or session ids |

---

## 🔨 Building

Open `PPDF.TotalAgility.Connector.sln` in Visual Studio (.NET Framework 4.8). NuGet restores `Newtonsoft.Json` and `Microsoft.Web.WebView2`; `References\DMSConnector.dll` comes with the repository. See `Documentation\PPDF Connector - Developer's Guide.docx` for the connector interface.

---

## 📬 Support

For questions or contributions, please raise an issue or submit a pull request to this repository.
