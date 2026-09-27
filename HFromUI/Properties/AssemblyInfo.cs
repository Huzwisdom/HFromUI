using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("HFromUI")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("HFromUI")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("fbbbc9bd-a8ce-4505-91de-31f21ca6a519")]

// 程序集的版本信息由下列四个值组成: 
//
//      主版本
//      次版本
//      生成号
//      修订号
//
//可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值
//通过使用 "*"，如下所示:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("6.0.2.4")]
[assembly: AssemblyFileVersion("6.0.2.4")]

// Demo 工程需要访问 HDT* 内部类型（弹出窗/面板/名称工具）用于演示页
[assembly: InternalsVisibleTo("HFromUITextA")]
// 单元测试工程需要访问 HDT* 内部类型（框式几何/配色板）
[assembly: InternalsVisibleTo("HFromUIDateTimeTests")]
