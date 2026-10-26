# FAN Firebird Tool

**English** | [Türkçe](README.tr.md)

A portable Windows tool to **back up, restore and create Firebird 2.5 – 5.0 databases** and to **run multiple SQL scripts in a chosen order**.
No installation and no Firebird server needed: the official Firebird command-line tools are bundled and run in embedded mode.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-lightgrey)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4)
![Firebird](https://img.shields.io/badge/Firebird-2.5%20%7C%203.0%20%7C%204.0%20%7C%205.0-orange)

| Light theme | Dark theme |
|---|---|
| ![Main window, light theme](docs/images/main-light.png) | ![Main window, dark theme](docs/images/main-dark.png) |

![Running scripts](docs/images/scripts.png)

## Features

- **Four Firebird versions in one tool:** 2.5, 3.0, 4.0 and 5.0, selectable per operation.
- **Backup and restore** with `gbak`, live verbose output and a cancel button. Results are written to a temporary file first and moved into place only when the operation succeeds, so a cancelled or failed run never leaves a half-written file behind.
- **Migrate between versions:** back up with an old version, restore with a newer one (for example 2.5 → 5.0).
- **Create empty databases** with a page size (only the sizes the selected version supports) and a default character set.
- **Run SQL scripts in order:** add a folder or single files (Explorer-style natural sort, `2_x.sql` before `10_x.sql`), drag and drop, reorder, remove. Each script shows its own status (waiting, running, succeeded, failed, not run) and a summary is shown at the end. Optional *continue on error*.
- **Portable:** everything lives in its own folder; settings are saved to `settings.ini` next to the exe.
- **English and Turkish UI**, chosen automatically from the Windows language. New languages are a single text file (see below).
- **Light and dark theme**, following the Windows setting by default.
- Optional timestamps in the log to see where long operations spend their time.

## Download and run

1. Open the [latest release](https://github.com/BarisKaratas1983/FANFirebirdTool/releases/latest).
2. Download `FANFirebirdTool-<version>-x64.zip` (64-bit Windows, recommended) or `-x86.zip` (32-bit Windows).
3. Extract it to any folder you can write to (not *Program Files*) and run `FANFirebirdTool.exe`.

**Requirements:** Windows 10 or 11. .NET Framework 4.8 is already part of these Windows versions. Nothing else needs to be installed; the packages were tested on a clean Windows Sandbox without network access.

`SHA256SUMS.txt` in each release lists the checksums of the zip files.

## Good to know

- In embedded mode the user name and password are not checked; the default `SYSDBA` / `masterkey` is fine.
- A backup can be restored with the same or a **newer** Firebird version, not with an older one (for example a Firebird 5.0 backup cannot be restored with 2.5).
- Scripts run through `isql`. Statements that `isql` commits immediately (such as DDL) stay in the database even if the run is cancelled, so taking a backup before running scripts is recommended.
- Firebird 2.5's `gbak` is noticeably slower than the newer versions on the same database. This is the behaviour of the engine itself.

## How it works

Each Firebird version lives in its own folder next to the exe:

```
FANFirebirdTool.exe
engines\
  fb25\  fb30\  fb40\  fb50\    gbak.exe, isql.exe and the embedded engine libraries
licenses\
```

Operations start `gbak.exe` or `isql.exe` of the selected version as a separate process and stream their output into the log. Because the tools run as separate processes, every engine uses its own libraries and the versions never conflict with each other or with a Firebird server installed on the machine.

The code is split into layers:

| Folder | Purpose |
|---|---|
| `Core` | Process runner, localization, settings, theme, result type |
| `Engines` | Firebird version definitions (folders, page sizes, character sets) |
| `Operations` | Backup, restore, create and script runner; no UI code |
| `Controls` | Reusable UI parts and helpers |
| `Forms` | Windows (the whole UI is built in code, no designer files) |

## Building from source

**Requirements:** Visual Studio 2022 or 2026 with the *.NET desktop development* workload.

1. Clone the repository.
2. Prepare the engine folders. The Firebird binaries are not stored in the repository; take them from the official [Firebird zip kits](https://firebirdsql.org/en/server-packages/) (version 2.5.9, 3.0, 4.0 and 5.0, in 64-bit and 32-bit):

   ```
   engines\x64\fb25  fb30  fb40  fb50
   engines\x86\fb25  fb30  fb40  fb50
   ```

   - **3.0, 4.0, 5.0:** extract the zip kit into the folder.
   - **2.5:** take `gbak.exe`, `isql.exe` and the other tools from the `bin` folder of the full zip kit and the contents of the *embed* zip kit, then copy `fbembed.dll` as `fbclient.dll` so that the tools run in embedded mode.

3. Open `FANFirebirdTool.slnx`, select **x64** or **x86** and build. The engines of the selected platform are copied next to the exe automatically.
4. To create the release zips, run `build.cmd`. The packages are written to the `dist` folder.

## Adding a language

1. Copy `FANFirebirdTool/Languages/en.txt` to a new file named with the two-letter language code, for example `de.txt`.
2. Translate the values (the text after `=`) and set `language.name` (for example `language.name=Deutsch`).
3. Add the file to the project as an *Embedded Resource* and build.

The language appears in the language list automatically. Missing keys fall back to English.

## Known limitations

- On displays scaled above 100 %, Windows stretches the window, so text can look slightly soft.
- Message boxes and file dialogs are drawn by Windows and stay in the light theme.

## About the name

*FAN* stands for **FreeAndNil**, a nod to the well-known Delphi routine and the author's nickname.

## License

FAN Firebird Tool is released under the [MIT License](LICENSE).

The release packages include the official Firebird tools, distributed under the InterBase Public License and the Initial Developer's Public License, and icons from [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons) (MIT). See [licenses/THIRD-PARTY-NOTICES.txt](licenses/THIRD-PARTY-NOTICES.txt).

## Author

**Barış Karataş**
[GitHub](https://github.com/BarisKaratas1983) · [LinkedIn](https://www.linkedin.com/in/baris-karatas) · [baris.karatas@yahoo.com](mailto:baris.karatas@yahoo.com)
