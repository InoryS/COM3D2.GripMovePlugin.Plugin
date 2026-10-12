// ============================================================================
//  COM3D2.GripMovePlugin 静态验证工具（不需要游戏本体）
//
//  为什么需要它：
//    编译通过只保证「类型和签名对得上」。下面这些编译器**完全不检查**，
//    名字错了只有运行时才炸：
//      1) 按字符串反射  typeof(X).GetField("m_xxx") / GetMethod("Xxx")
//      2) 条件编译是否真的把对应平台的代码编进去了（#if 写反了不会报错）
//      3) 内嵌资源是否真的打进去了、字节对不对
//      4) 移植的代码是不是死代码（方法存在但没人调用）
//
//  用法：
//    dotnet run --project tools/CodeVerify -- \
//        --dll COM3D2.GripMovePlugin.Plugin/bin/Release/COM3D25/net48/COM3D2.GripMovePlugin.Plugin.dll \
//        --src COM3D2.GripMovePlugin.Plugin \
//        --ref <游戏程序集目录或 dll> [--ref ...] \
//        [--xr-src <XR 反编译源码目录>] \
//        [--res  <资源目录>]
//
//  参考程序集建议用**真实游戏**的 Assembly-CSharp.dll（不是 NuGet GameLibs），
//  因为反射是运行时对着真游戏做的。例如：
//    COM3D2   : <游戏>/COM3D2x64_Data/Managed/Assembly-CSharp.dll
//    COM3D2.5 : <游戏>/COM3D2_5x64_Data/Managed/Assembly-CSharp.dll
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace CodeVerify
{
	internal static class Program
	{
		private static MetadataReader R;
		private static PEReader PE;
		private static readonly Dictionary<int, OpCode> Ops = BuildOps();
		private static int _fail;

		private static int Main(string[] args)
		{
			try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }

			string dll = Arg(args, "--dll");
			string src = Arg(args, "--src");
			var refs = Args(args, "--ref");
			string xrSrc = Arg(args, "--xr-src");
			string resDir = Arg(args, "--res");

			if (dll == null)
			{
				Console.WriteLine("缺少 --dll 参数，见本文件顶部注释。");
				return 2;
			}

			using var fs = File.OpenRead(dll);
			DllPath = Path.GetFullPath(dll);
			PE = new PEReader(fs);
			R = PE.GetMetadataReader();

			Console.WriteLine("待验证: " + Path.GetFileName(dll));

			Section1_Identity();
			Section2_EntryPoint();
			Section3_PlatformIsolation();
			if (resDir != null) Section4_Resources(resDir);
			if (xrSrc != null) Section5_PortCompleteness(xrSrc);
			Section6_Reachability();
			if (src != null && refs.Count > 0) Section7_ReflectionTargets(src, refs);

			Console.WriteLine();
			Console.WriteLine(_fail == 0 ? "全部通过 ✔" : $"有 {_fail} 项未通过 ✘");
			return _fail == 0 ? 0 : 1;
		}

		// ---------------------------------------------------------------- 1
		private static void Section1_Identity()
		{
			Head("1) 程序集标识");
			var ad = R.GetAssemblyDefinition();
			Console.WriteLine("  名称: " + R.GetString(ad.Name));
			Console.WriteLine("  版本: " + ad.Version);
		}

		// ---------------------------------------------------------------- 2
		private static void Section2_EntryPoint()
		{
			Head("2) 插件入口");
			foreach (var th in R.TypeDefinitions)
			{
				var td = R.GetTypeDefinition(th);
				if (R.GetString(td.Name) != "GripMovePlugin") continue;

				Console.WriteLine("  类型: " + R.GetString(td.Namespace) + "." + R.GetString(td.Name));
				Console.WriteLine("  基类: " + TypeName(td.BaseType));
				foreach (var cah in td.GetCustomAttributes())
				{
					var ca = R.GetCustomAttribute(cah);
					var n = AttrName(ca.Constructor);
					if (!n.StartsWith("Plugin")) continue;
					var blob = R.GetBlobBytes(ca.Value);
					var s = blob.Length > 5
						? System.Text.Encoding.UTF8.GetString(blob, 3, blob.Length - 5)
						: "";
					Console.WriteLine($"  特性: [{n}(\"{s}\")]");
				}
			}
		}

		// ---------------------------------------------------------------- 3
		private static void Section3_PlatformIsolation()
		{
			Head("3) 平台符号隔离");
			var names = new HashSet<string>();
			foreach (var th in R.TypeDefinitions) names.Add(R.GetString(R.GetTypeDefinition(th).Name));

			Check(names.Contains("GripMovePlugin"), "存在 GripMovePlugin");
			Check(names.Contains("DeviceWrapper"), "存在 DeviceWrapper");

			// XR 与 VR 的互斥类型，必须「一边有一边无」
			string[] xr = { "DeviceXRController", "GripMoveControllerXR", "MenuToolXR" };
			string[] vr = { "DeviceViveController", "DeviceOculusTouchController", "GripMoveController",
				"GripMoveControllerTouch", "MenuTool", "MenuToolTouch", "VIVEModelReplaceTool" };

			int xrCount = xr.Count(names.Contains);
			int vrCount = vr.Count(names.Contains);
			Console.WriteLine($"  XR 专属类型 {xrCount}/{xr.Length}，VR 专属类型 {vrCount}/{vr.Length}");
			Check(xrCount == 0 || vrCount == 0, "XR 与 VR 类型没有混编（同一产物只应含一边）");
			foreach (var n in xr) Console.WriteLine($"    {(names.Contains(n) ? "有" : "无")}  {n}");
			foreach (var n in vr) Console.WriteLine($"    {(names.Contains(n) ? "有" : "无")}  {n}");
		}

		// ---------------------------------------------------------------- 4
		private static void Section4_Resources(string resDir)
		{
			Head("4) 内嵌资源");
			var names = new List<string>();
			var dllBytes = File.ReadAllBytes(DllPath);

			foreach (var h in R.ManifestResources)
			{
				var mr = R.GetManifestResource(h);
				var n = R.GetString(mr.Name);
				names.Add(n);
				if (!mr.Implementation.IsNil) continue;
				var dir = PE.PEHeaders.CorHeader.ResourcesDirectory;
				var sec = PE.GetSectionData(dir.RelativeVirtualAddress + (int)mr.Offset);
				Console.WriteLine($"  清单资源 {n} ({sec.GetReader().ReadInt32()} 字节)");
			}

			foreach (var f in Directory.EnumerateFiles(resDir))
			{
				var bytes = File.ReadAllBytes(f);
				var fileName = Path.GetFileName(f);
				Check(names.Any(n => n.EndsWith(fileName)), $"清单名包含 {fileName}");
				Check(Contains(dllBytes, bytes), $"{fileName} 在 DLL 内字节一致");
			}
		}

		// ---------------------------------------------------------------- 5
		private static void Section5_PortCompleteness(string xrSrc)
		{
			Head("5) 移植完整性（XR 反编译源码里声明的类型是否都在本 DLL 里）");
			var names = new HashSet<string>();
			foreach (var th in R.TypeDefinitions) names.Add(R.GetString(R.GetTypeDefinition(th).Name));

			var decl = new Regex(
				@"(?:^|\s)(?:public|internal|protected|private)?\s*(?:sealed\s+|abstract\s+|static\s+|partial\s+)*(?:class|struct|enum|interface)\s+(\w+)",
				RegexOptions.Multiline);
			var declared = new SortedSet<string>();
			foreach (var f in Directory.EnumerateFiles(xrSrc, "*.cs", SearchOption.AllDirectories))
				foreach (Match m in decl.Matches(File.ReadAllText(f)))
					declared.Add(m.Groups[1].Value);

			var missing = declared.Where(d => !names.Contains(d) && !names.Contains(d.TrimEnd('_'))).ToList();
			Console.WriteLine($"  源码声明 {declared.Count} 个类型");
			Check(missing.Count == 0, missing.Count == 0 ? "全部存在" : "缺失: " + string.Join(", ", missing));
		}

		// ---------------------------------------------------------------- 6
		private static void Section6_Reachability()
		{
			Head("6) 代码路径可达性（方法是否真的被调用）");
			var called = new HashSet<string>();
			foreach (var th in R.TypeDefinitions)
			{
				var td = R.GetTypeDefinition(th);
				foreach (var mh in td.GetMethods())
				{
					var md = R.GetMethodDefinition(mh);
					if (md.RelativeVirtualAddress == 0) continue;
					byte[] il;
					try { il = PE.GetMethodBody(md.RelativeVirtualAddress).GetILBytes(); } catch { continue; }
					CollectCalls(il, called);
				}
			}

			foreach (var n in new[] { "tryInstallGripMoveControllerXR", "InstallTouchToXR", "GetOrCreate", "PostStart" })
				Console.WriteLine($"    {(called.Contains(n) ? "被调用  " : "未被调用")}  {n}");
			Console.WriteLine($"    (扫描方法 {_scanned} 个，其中 {_desync} 个 IL 解析中断)");
		}

		private static int _scanned;
		private static int _desync;

		// ---------------------------------------------------------------- 7
		private static void Section7_ReflectionTargets(string srcDir, List<string> refDlls)
		{
			Head("7) 反射目标校验（typeof(X).GetField/GetMethod/GetProperty 的字面量是否真实存在）");

			var index = new Dictionary<string, Dictionary<string, HashSet<string>>>();
			foreach (var dll in refDlls)
			{
				using var fs = File.OpenRead(dll);
				using var pe = new PEReader(fs);
				var r = pe.GetMetadataReader();
				foreach (var th in r.TypeDefinitions)
				{
					var td = r.GetTypeDefinition(th);
					var tname = r.GetString(td.Name);
					if (!index.TryGetValue(tname, out var members))
					{
						members = new Dictionary<string, HashSet<string>>();
						index[tname] = members;
					}
					void Add(string kind, string n)
					{
						if (!members.TryGetValue(kind, out var set)) { set = new HashSet<string>(); members[kind] = set; }
						set.Add(n);
					}
					foreach (var fh in td.GetFields()) Add("F", r.GetString(r.GetFieldDefinition(fh).Name));
					foreach (var mh in td.GetMethods()) Add("M", r.GetString(r.GetMethodDefinition(mh).Name));
					foreach (var ph in td.GetProperties()) Add("P", r.GetString(r.GetPropertyDefinition(ph).Name));
				}
			}

			var pat = new Regex(@"typeof\((?<t>\w+)\)\.Get(?<k>Field|Method|Property)\(\s*""(?<m>[^""]+)""");
			int ok = 0, total = 0;
			var bad = new List<string>();
			foreach (var f in Directory.EnumerateFiles(srcDir, "*.cs", SearchOption.AllDirectories))
			{
				if (f.Contains(@"\obj\") || f.Contains(@"\bin\")) continue;
				var lines = File.ReadAllLines(f);
				for (int i = 0; i < lines.Length; i++)
				{
					foreach (Match m in pat.Matches(lines[i]))
					{
						total++;
						var kind = m.Groups["k"].Value switch { "Field" => "F", "Method" => "M", _ => "P" };
						var t = m.Groups["t"].Value;
						var member = m.Groups["m"].Value;
						string why = null;
						if (!index.TryGetValue(t, out var members)) why = "类型不存在";
						else if (!members.TryGetValue(kind, out var set)) why = "无此类成员";
						else if (!set.Contains(member)) why = "成员不存在";

						if (why == null) { ok++; continue; }
						bad.Add($"    {Path.GetFileName(f)}:{i + 1}  typeof({t}).Get{m.Groups["k"].Value}(\"{member}\") -> {why}");
					}
				}
			}

			Console.WriteLine($"  参考程序集: {string.Join(", ", refDlls.Select(Path.GetFileName))}");
			Check(ok == total, $"通过 {ok}/{total}");
			foreach (var b in bad) Console.WriteLine(b);
		}

		// ---------------------------------------------------------------- 工具
		private static string DllPath;

		private static void Head(string s)
		{
			Console.WriteLine();
			Console.WriteLine("========== " + s + " ==========");
		}

		private static void Check(bool ok, string msg)
		{
			Console.WriteLine("  " + (ok ? "✔ " : "✘ ") + msg);
			if (!ok) _fail++;
		}

		private static string Arg(string[] a, string key)
		{
			int i = Array.IndexOf(a, key);
			return i >= 0 && i + 1 < a.Length ? a[i + 1] : null;
		}

		private static List<string> Args(string[] a, string key)
		{
			var list = new List<string>();
			for (int i = 0; i < a.Length - 1; i++)
			{
				if (a[i] != key) continue;
				var v = a[i + 1];
				// 目录则展开成里面的 Assembly-CSharp*.dll
				if (Directory.Exists(v))
					list.AddRange(Directory.EnumerateFiles(v, "Assembly-CSharp*.dll"));
				else
					list.Add(v);
			}
			return list;
		}

		private static Dictionary<int, OpCode> BuildOps()
		{
			var d = new Dictionary<int, OpCode>();
			foreach (var f in typeof(OpCodes).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
			{
				var oc = (OpCode)f.GetValue(null);
				// 注意：OpCode.Value 是 short，两字节指令 0xFE00|x 作为 short 是负数，
				// 必须转成 ushort 再当键，否则和扫描时算出的正数对不上。
				d[(ushort)oc.Value] = oc;
			}
			return d;
		}

		private static void CollectCalls(byte[] il, HashSet<string> called)
		{
			_scanned++;
			int i = 0;
			while (i < il.Length)
			{
				int v = il[i++];
				if (v == 0xFE) { if (i >= il.Length) { _desync++; return; } v = 0xFE00 | il[i++]; }
				if (!Ops.TryGetValue(v, out var op)) { _desync++; return; }
				switch (op.OperandType)
				{
					case OperandType.InlineNone: break;
					case OperandType.ShortInlineI:
					case OperandType.ShortInlineVar:
					case OperandType.ShortInlineBrTarget: i += 1; break;
					case OperandType.InlineVar: i += 2; break;
					case OperandType.InlineI:
					case OperandType.InlineBrTarget:
					case OperandType.InlineString:
					case OperandType.ShortInlineR:
					case OperandType.InlineSig: i += 4; break;
					case OperandType.InlineI8:
					case OperandType.InlineR: i += 8; break;
					case OperandType.InlineSwitch:
						{ if (i + 4 > il.Length) return; int n = BitConverter.ToInt32(il, i); i += 4 + 4 * n; break; }
					case OperandType.InlineField:
					case OperandType.InlineMethod:
					case OperandType.InlineType:
					case OperandType.InlineTok:
						{
							if (i + 4 > il.Length) return;
							int tok = BitConverter.ToInt32(il, i); i += 4;
							try
							{
								var h = MetadataTokens.EntityHandle(tok);
								if (h.Kind == HandleKind.MemberReference)
									called.Add(R.GetString(R.GetMemberReference((MemberReferenceHandle)h).Name));
								else if (h.Kind == HandleKind.MethodDefinition)
									called.Add(R.GetString(R.GetMethodDefinition((MethodDefinitionHandle)h).Name));
							}
							catch { }
							break;
						}
					default: break;
				}
			}
		}

		private static bool Contains(byte[] hay, byte[] needle)
		{
			if (needle.Length == 0 || hay.Length < needle.Length) return false;
			for (int i = 0; i <= hay.Length - needle.Length; i++)
			{
				bool ok = true;
				for (int j = 0; j < needle.Length; j++) if (hay[i + j] != needle[j]) { ok = false; break; }
				if (ok) return true;
			}
			return false;
		}

		private static string AttrName(EntityHandle h)
		{
			try
			{
				if (h.Kind == HandleKind.MemberReference)
				{
					var mr = R.GetMemberReference((MemberReferenceHandle)h);
					if (mr.Parent.Kind == HandleKind.TypeReference)
						return R.GetString(R.GetTypeReference((TypeReferenceHandle)mr.Parent).Name);
				}
			}
			catch { }
			return "?";
		}

		private static string TypeName(EntityHandle h)
		{
			if (h.IsNil) return "(无)";
			try
			{
				switch (h.Kind)
				{
					case HandleKind.TypeReference: return R.GetString(R.GetTypeReference((TypeReferenceHandle)h).Name);
					case HandleKind.TypeDefinition: return R.GetString(R.GetTypeDefinition((TypeDefinitionHandle)h).Name);
					default: return h.Kind.ToString();
				}
			}
			catch { return "?"; }
		}
	}
}
