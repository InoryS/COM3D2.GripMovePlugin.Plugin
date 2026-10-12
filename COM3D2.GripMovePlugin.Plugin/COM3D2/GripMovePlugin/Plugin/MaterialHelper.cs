using System;
using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

internal class MaterialHelper
{
	private static AssetBundle _GripMovePluginResources;

	private static Shader _ColorZOrderShader;

	public static Shader GetColorZOrderShader()
	{
		if (_ColorZOrderShader != null)
		{
			return _ColorZOrderShader;
		}

		Shader shader;
		try
		{
			if (Application.unityVersion.StartsWith("4"))
			{
				_ColorZOrderShader = Shader.Find("Unlit");
				shader = _ColorZOrderShader;
			}
			else
			{
				_GripMovePluginResources = _GripMovePluginResources ??
				                           AssetBundle.LoadFromMemory(
					                           Resource.gripmovepluginshaders);
				_ColorZOrderShader = _GripMovePluginResources.LoadAsset<Shader>("ColorZOrder");
				shader = _ColorZOrderShader;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			shader = null;
		}

		return shader;
	}
}
