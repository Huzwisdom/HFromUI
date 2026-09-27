using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace HFromUI.HBase
{
    using HFromUI.HLangage;
    /// <summary>
    /// 基于 CodeDOM 的动态 C# 代码编译器。
    /// 可将字符串形式的源码编译为内存中的程序集，并方便地创建实例或调用方法。
    /// 继承自 HReflectionBase，可直接使用其反射工具。
    /// </summary>
    public class HCodeDomCompiler : HReflectionBase
    {
        /// <summary>
        /// 获取编译时默认引用的程序集列表（自动收集当前 AppDomain 中所有已加载的非动态程序集）。
        /// 避免手动添加大量基础库，适合大多数场景。
        /// </summary>
        protected virtual IEnumerable<string> GetDefaultAssemblyReferences()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => a.Location)
                .Distinct();
        }

        /// <summary>
        /// 将 C# 源代码字符串编译为内存中的程序集。
        /// </summary>
        /// <param name="sourceCode">完整的 C# 源代码（可包含多个类，需包含必要的 using）</param>
        /// <param name="additionalReferences">额外需引用的程序集路径，默认已包含当前域所有已加载程序集</param>
        /// <returns>编译成功的程序集</returns>
        public Assembly CompileSourceToAssembly(string sourceCode, IEnumerable<string> additionalReferences = null)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
                throw new ArgumentException(HTranslation.GetContent("源代码不能为空"), nameof(sourceCode));

            using (var provider = new CSharpCodeProvider())
            {
                var options = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false,
                    CompilerOptions = "/optimize",   // 可选优化
                };

                // 合并默认引用和额外引用
                var allReferences = new HashSet<string>(GetDefaultAssemblyReferences(), StringComparer.OrdinalIgnoreCase);
                if (additionalReferences != null)
                {
                    foreach (string refPath in additionalReferences)
                    {
                        if (!string.IsNullOrWhiteSpace(refPath))
                            allReferences.Add(refPath);
                    }
                }
                options.ReferencedAssemblies.AddRange(allReferences.ToArray());

                // 编译
                CompilerResults results = provider.CompileAssemblyFromSource(options, sourceCode);

                // 处理编译警告（非错误，可记录日志）
                if (results.Errors.HasWarnings)
                {
                    var warnings = results.Errors.Cast<CompilerError>().Where(e => e.IsWarning);
                    // 此处可扩展为日志输出，暂不抛出异常
                }

                // 检查编译错误
                if (results.Errors.HasErrors)
                {
                    var errorMsgs = results.Errors.Cast<CompilerError>()
                        .Where(e => !e.IsWarning)
                        .Select(e => HTranslation.GetContent($"行 {e.Line}: {e.ErrorText}"));
                    throw new CompilationException(HTranslation.GetContent("动态编译失败:\n") + string.Join("\n", errorMsgs));
                }

                return results.CompiledAssembly;
            }
        }

        /// <summary>
        /// 编译源码并返回指定类型的实例（无参构造函数）。
        /// </summary>
        /// <typeparam name="T">基类或接口类型，实例将被转换为此类型</typeparam>
        /// <param name="sourceCode">源代码</param>
        /// <param name="typeFullName">要创建类型的完整名称（命名空间.类名）</param>
        /// <param name="additionalReferences">额外程序集引用</param>
        /// <returns>创建的对象实例</returns>
        public T CompileAndCreateInstance<T>(string sourceCode, string typeFullName, IEnumerable<string> additionalReferences = null) where T : class
        {
            Assembly asm = CompileSourceToAssembly(sourceCode, additionalReferences);
            Type targetType = asm.GetType(typeFullName);
            if (targetType == null)
                throw new Exception(HTranslation.GetContent($"编译后的程序集中未找到类型 {typeFullName}"));

            // 使用基类的 CreateInstance<T> 方法
            return CreateInstance<T>(targetType);
        }

        /// <summary>
        /// 编译源码并调用指定类型的静态方法。
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="typeFullName">类型全名</param>
        /// <param name="methodName">静态方法名</param>
        /// <param name="parameters">方法参数</param>
        /// <param name="additionalReferences">额外引用</param>
        /// <returns>方法返回值</returns>
        public object CompileAndCallStaticMethod(string sourceCode, string typeFullName, string methodName, object[] parameters = null, IEnumerable<string> additionalReferences = null)
        {
            Assembly asm = CompileSourceToAssembly(sourceCode, additionalReferences);
            Type targetType = asm.GetType(typeFullName);
            if (targetType == null)
                throw new Exception(HTranslation.GetContent($"编译后的程序集中未找到类型 {typeFullName}"));
            return InvokeStaticMethod(targetType, methodName, parameters ?? new object[0]);
        }

        /// <summary>
        /// 编译源码并调用指定类型的实例方法（先创建实例再调用）。
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="typeFullName">类型全名</param>
        /// <param name="methodName">实例方法名</param>
        /// <param name="parameters">方法参数</param>
        /// <param name="additionalReferences">额外引用</param>
        /// <returns>方法返回值</returns>
        public object CompileAndCallInstanceMethod(string sourceCode, string typeFullName, string methodName, object[] parameters = null, IEnumerable<string> additionalReferences = null)
        {
            Assembly asm = CompileSourceToAssembly(sourceCode, additionalReferences);
            Type targetType = asm.GetType(typeFullName);
            if (targetType == null)
                throw new Exception(HTranslation.GetContent($"编译后的程序集中未找到类型 {typeFullName}"));

            object instance = CreateInstance(targetType);
            return InvokeMethod(instance, methodName, parameters ?? new object[0]);
        }

        /// <summary>
        /// 从 .cs 文件读取源代码并编译为程序集。
        /// </summary>
        /// <param name="filePath">.cs 文件完整路径</param>
        /// <param name="additionalReferences">额外引用</param>
        /// <returns>程序集</returns>
        public Assembly CompileFromFile(string filePath, IEnumerable<string> additionalReferences = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(HTranslation.GetContent($"找不到源文件: {filePath}"));
            string sourceCode = File.ReadAllText(filePath, Encoding.UTF8);
            return CompileSourceToAssembly(sourceCode, additionalReferences);
        }
    }

    /// <summary>
    /// 动态编译异常类，用于封装编译错误信息。
    /// </summary>
    public class CompilationException : Exception
    {
        public CompilationException(string message) : base(message) { }
        public CompilationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
