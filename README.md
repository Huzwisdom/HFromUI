# HFromUI

基于 **.NET Framework 4.8 + WinForms + 纯 GDI+ 自绘**的工业组态 UI 控件库：仪表、电机、阀门、罐体、信号灯、图表、坐标绘图…… 不依赖任何第三方 UI 框架，支持透明背景、双缓冲无闪烁、统一 13 色调色板与多种内置样式，可直接在 VS 工具箱中拖放使用。

- 框架：.NET Framework 4.8（AnyCPU）
- 技术：System.Drawing / GDI+，零第三方 UI 依赖
- 协议：GPL-2.0（见 [LICENSE](LICENSE)）

## 特性

- **纯 GDI+ 自绘**：所有工具控件从基类继承，重写 `OnPaint` 完成渲染，无图片资源依赖。
- **多样式 + 统一主题**：每个控件提供 22~23 种 `XxxStyle` 外观，配合 13 色 `HToolTheme` 调色板自由换色。
- **动画与缓动**：仪表指针平滑过渡；电机、风机、传送带等控件支持 `Running` 节拍动画。
- **透明背景 / 双缓冲**：统一开启 `SupportsTransparentBackColor` + `OptimizedDoubleBuffer`，组态画面叠加无闪烁。
- **旧版兼容**：仪表类控件保留 `Legacy` 样式，与旧版 HFrom 控件像素级一致。
- **工具箱友好**：标准 `System.Windows.Forms.Control` 派生类，属性支持设计器序列化与中文分类。

## 解决方案结构

| 项目 | 说明 |
|---|---|
| `HFromUI` | 控件库主项目，输出 `HFromUI.dll` |
| `HCamera` | 工业相机与 Halcon 视觉封装（海康/大华/DVP，需要对应厂商 SDK） |
| `HFromUITestA` / `HFromUITestB` | WinForms 示例工程，可作为用法参考 |

## 控件总览

命名空间统一在 `HFromUI.HControl.*` 下：

| 分类 | 命名空间 | 代表控件 |
|---|---|---|
| HMI 仪表 | `HFromUI.HControl.Tools.Hmi` | HGauge 指针仪表、HDialPlate 圆盘表、HThermometer 温度计、HClock 时钟、HLanternAlarm/HLanternSimple 警示灯、HRotarySwitch 旋转开关、HMarquee 跑马灯、HPlayButton 播放按钮 |
| 按钮开关 | `HFromUI.HControl.Tools.Button` | HPushButton、HToggleButton、HCheckBox、HRadioButton、HSlideSwitch |
| 指示灯 | `HFromUI.HControl.Tools.Indicator` | HAlarmLamp、HSignalLamp、HWarningSign、HWireless |
| 动力设备 | `HFromUI.HControl.Tools.Power` | HMotor、HFan、HBlower、HBelt、HRoller、HElevator、HGrinder、HVibrator、HStirRod |
| 管路阀门 | `HFromUI.HControl.Tools.Valve` | HValves、HDispenseValve、HPipe、HWire、HValveStem |
| 容器 | `HFromUI.HControl.Tools.Vessel` | HBattery、HBeaker、HBottle、HHopper、HChip、HMixSilo |
| 工艺图元 | `HFromUI.HControl.Tools.Process` | HEStop、HFactory、HFire、HRoom、HCleaner、HSwitchButton 等 |
| 自然装饰 | `HFromUI.HControl.Tools.Nature` | HSun、HMoon、HStarSky、HHeart、HFlower、HArrow |
| 图表 | `HFromUI.HControl.Chart` | HChart 多序列图表（坐标轴/图例/菜单/提示） |
| 坐标绘图 | `HFromUI.HControl.Coordinate` | HCoordinatePad 组态画布（点/线/弧/多边形/GCode 图元） |
| 可视窗口 | `HFromUI.HControl.VisualWindow` | HVisualWindow 机器视觉 ROI 工具集 |
| 基础控件 | `HFromUI.HControl` / `HFromUI.HFrom` | HChart、HNumericUpDown、HSearchBox、HDatePicker，以及旧版窗体/进度条/滚动条/下拉框等 |

## 快速开始

### 1. 编译

用 Visual Studio 2022 打开 `HFromUI.sln` 直接生成，或命令行：

```powershell
& "D:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" `
    HFromUI.sln /p:Configuration=Debug
```

产物：`HFromUI\bin\HFromUI.dll`

### 2. 引用并使用

- **工具箱方式**：在 WinForms 设计器工具箱「选择项」浏览添加 `HFromUI.dll`，把控件拖到窗体上，属性窗口中设置 `GaugeStyle`、`Theme`、`Value` 等。
- **代码方式**：

```csharp
using HFromUI.HControl.Tools.Hmi;

var gauge = new HGauge
{
    Size = new Size(300, 260),
    Location = new Point(20, 20),
    GaugeStyle = HGaugeStyle.Sport,
    Theme = HToolTheme.SeaBlue,
    MinValue = 0,
    MaxValue = 200,
    Value = 128,
    UnitText = "km/h",
    SegmentCount = 10,
    ShowAlarmZone = true,
    AlarmRatio = 0.8,
    SmoothAnimation = true,
    Text = "SPEED"
};
Controls.Add(gauge);
```

温度计双刻度示例（属性名注意：温度计为 `Right*`，圆盘表为 `Second*`）：

```csharp
var thermo = new HThermometer
{
    MinValue = -20, MaxValue = 60, Value = 25,
    UnitText = "°C",
    RightUnitText = "°F", RightGain = 1.8, RightOffset = 32,   // 第二刻度
    ThermometerStyle = HThermometerStyle.Industrial
};
```

## HMI 仪表控件说明

| 控件 | 样式数 | 样式枚举 | 说明 |
|---|---|---|---|
| HGauge | 23 | `HGaugeStyle` | 扇形指针仪表，彩弧/柱条进度、读数窗、报警区 |
| HDialPlate | 23 | `HDialPlateStyle` | 全圆表盘，支持双刻度双单位（如 MPa / psi） |
| HThermometer | 23 | `HThermometerStyle` | 温度计，支持横置、双刻度双列标签 |
| HClock | 22 | `HClockStyle` | 指针/数字/翻页等时钟样式 |
| HLanternAlarm | 22 | `HLanternAlarmStyle` | 多层报警灯塔 |
| HLanternSimple | 22 | `HLanternSimpleStyle` | 单体警示灯 |
| HRotarySwitch | 22 | `HRotarySwitchStyle` | 旋钮/扳把/断路器开关 |
| HMarquee | 22 | `HMarqueeStyle` | 跑马灯文字 |
| HPlayButton | 22 | `HPlayButtonStyle` | 播放/暂停按钮 |

> 三个仪表控件的最后一个样式为 `Legacy = 22`，严格复刻旧版 HFrom 的外观（白盘黑框、DimGray 刻度、DodgerBlue 数字、Tomato 指针），需要与旧画面保持一致时使用。

### 仪表基类常用属性（HInstrumentBase）

| 属性 | 类型 | 说明 |
|---|---|---|
| `Theme` | `HToolTheme` | 13 色主题（Classic 为工业经典配色） |
| `MinValue` / `MaxValue` | double | 量程 |
| `Value` | double | 当前值（显示值经缓动平滑过渡） |
| `SmoothAnimation` | bool | 指针/液柱是否缓动动画 |
| `UnitText` | string | 单位文本 |
| `ValueFormat` | string | 数值格式（如 `0.0`、`0.##`） |
| `SegmentCount` | int | 主刻度段数 |

### 主题色（HToolTheme）

`Classic` + 12 种现代色调：SkyBlue、SeaBlue、Emerald、Teal、Amber、Orange、Rose、Red、Purple、Magenta、Coffee、Graphite。HMI、电机、电池、阀门等工具控件共用同一色序，整屏配色可以保持统一。

## 自绘约定（二次开发参考）

- **渲染分层**：控件基类 → 样式描述结构（如 `HGauge.G`）→ 绘制扩展方法（`HHmiDraw` / `HToolDraw` 的 Graphics 扩展：`BezelRing`、`FaceDisk`、`Ticks`、`TickLabels`、`Needle`、`Arc` 等），新增样式一般只需加枚举值 + 样式参数。
- **字体**：统一使用 `HHmiDraw.Pf(...)` 创建 `GraphicsUnit.Pixel` 字体，保证 150% 等高 DPI 下物理尺寸稳定，不要直接 `new Font(name, em, GraphicsUnit.Point)`。
- **文字避让**：刻度数字在指针之后绘制并垫表盘底色圆角底板（chip），保证指针扫过时文字清晰不压线。
- **GDI+ 注意点**：`AddArc` 零尺寸会挂起；`Color.FromArgb(int)` 单参数是 alpha；循环 `RotateTransform` 每轮需对称复位。
- **属性反射名**：样式属性名为「枚举去掉首字母 H + Style」，如 `HGaugeStyle` → `GaugeStyle`；特例：`HRotarySwitch` → `SwitchStyle`、警示灯 → `LanternStyle` / `AlarmStyle`。

## 目录结构（节选）

```
HFromUI/
├─ HControl/
│  ├─ Tools/            工业图元控件
│  │  ├─ Hmi/           仪表/时钟/灯塔/开关
│  │  ├─ Power/         电机/风机/传送带…
│  │  ├─ Valve/         阀门/管道
│  │  ├─ Vessel/        罐体/电池/烧杯
│  │  ├─ Indicator/     信号灯/警示牌
│  │  ├─ Button/        按钮/开关
│  │  └─ …
│  ├─ Chart/            图表
│  ├─ Coordinate/       组态坐标画布
│  ├─ VisualWindow/     视觉 ROI 窗口
│  └─ Base/             控件基类与调色板
├─ HFrom/               旧版窗体/基础控件（保留兼容）
├─ HFile/ HData/ HConvert/   工具类（INI/JSON/XML/日志等）
└─ HAttribute/          设计器多语言特性
HCamera/                相机与 Halcon 封装（需厂商 SDK）
HFromUITestA/B/         示例工程
```

## 许可证

GPL-2.0，详见 [LICENSE](LICENSE)。
