using System.IO;
using System.Reflection;

namespace CM3D2.GripMovePlugin.Plugin;

/// <summary>
///     内嵌资源访问器。
///     资源以原始二进制文件形式随程序集嵌入（见 Resources 目录），
///     通过清单资源名直接读取，不再依赖 .resx / ResGen。
/// </summary>
internal static class Resource
{
	private const string ResourcePrefix = "CM3D2.GripMovePlugin.Plugin.Resources.";

	internal static byte[] gripmovepluginshaders =>
		Load(ResourcePrefix + "gripmovepluginshaders.bin");

	internal static byte[] mouse_cursor => Load(ResourcePrefix + "mouse_cursor.png");

	private static byte[] Load(string name)
	{
		using Stream manifestResourceStream =
			Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		if (manifestResourceStream == null)
		{
			return null;
		}

		byte[] array = new byte[manifestResourceStream.Length];
		manifestResourceStream.Read(array, 0, array.Length);
		return array;
	}
}
