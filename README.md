# fb2cng_FullConfig

A modern Windows graphical template configurator for the [**fb2cng (fbc)**](https://github.com/rupor-github/fb2cng) CLI converter. 

This application completely eliminates the need to manually edit complex YAML files or study the Go template language. It provides an intuitive, high-performance visual interface to customize structure rules, parameters, and filename templates for converting FB2 electronic books.

`fb2cng_FullConfig` is a major, next-generation evolution of the previously developed [**fb2cng_Configurator**](https://github.com/Jurchos/fb2cng_Configurator.git). It is designed as a core part of the comprehensive **fb2cng toolkit** and natively integrates with [**fb2cng GUI**](https://github.com/Jurchos/fb2cng_GUI.git).

---

### ⚠️ Project Evolution & Disclaimer
This project is a deep architectural rewrite and expansion of the older .NET Framework 4.8 tool. It was created by a beginner/non-programmer for learning and code-understanding purposes, built in active collaboration with **Gemini AI** as a development assistant.

> **Note:** Because of its educational and experimental nature, the source code contains an abundance of descriptive comments written in **Ukrainian** (apologies in advance for any inconvenience!).

---

## 🚀 Key Features & What's New

Compared to the legacy version, **fb2cng_FullConfig** delivers a massive upgrade in both functionality and technology:

* 🔄 **Full Two-Way Editing** – You can now not only generate new configuration profiles from scratch but also **open, parse, and edit previously created YAML templates**.
* ⚙️ **Expanded Parameter Suite** – Full visual control over an extended set of options, including Reader Size & Screen settings (Width, Height, DPI), Soft Hyphens, Image Transparency, JPEG Quality levels, Cover Page generator rules, Annotations, and Dropcaps.
* ⚡ **Ultra-Fast Next-Gen Engine** – Ported directly to **.NET 10**, leveraging advanced memory optimization (`OrdinalIgnoreCase` parsing), ultra-efficient multi-thread synchronization (`System.Threading.Lock`), and modern Win32 interop pinning.
* 🖥️ **Pixel-Perfect Adaptive UI** – Features a clean modern layout with native, dynamic **DPI Scaling** and a polished **Dark/Light theme engine** with hardware-anti-aliased rounded controls.

---

## 🔧 How to Use

1. **Load Configuration**: Upon startup, you can load the default software config, browse and open an existing `user.yaml` template, or start building configurations from scratch.
2. **Visual Customization**: Navigate through the modern tabbed layout (`document:`, `metainformation:`, `logging:`) to modify structural variables, file paths, and output formats.
3. **Save & Apply**: Click **Save**. The program will automatically compute all parameters, validate layout structures, and write a clean, standardized user.yaml config file ready for production use.

---

## 📦 Installation & Quick Start

### Option 1: Download Ready-to-Run (Recommended)
1. Go to the [Releases](../../releases) page of this repository.
2. Download the standalone executable.
3. Place it into your toolset folder and run it.

### Option 2: Build from Source
1. **Clone the repository:**
   ```bash
   git clone https://github.com/Jurchos/fb2cng_FullConfig.git
   ```
2. **Open & Build:**
   Open the solution file in **Visual Studio 2026 / VS Code** (with .NET 10 SDK installed) and build the project in `Release` mode.

---

## 🛠️ Built With

* **C# 14** 
* **.NET 10 (Modern .NET Runtime)**
* **Windows Forms (WinForms)**

---

## 📜 License

This project is licensed under the [MIT License](LICENSE) — feel free to use, modify, and distribute it in your own workflows.

