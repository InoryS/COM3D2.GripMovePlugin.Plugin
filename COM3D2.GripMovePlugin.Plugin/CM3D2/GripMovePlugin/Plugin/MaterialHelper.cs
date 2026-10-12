using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class MaterialHelper
	{
		public static Shader GetColorZOrderShader()
		{
			if (MaterialHelper._ColorZOrderShader != null)
			{
				return MaterialHelper._ColorZOrderShader;
			}
			Shader shader;
			try
			{
				if (Application.unityVersion.StartsWith("4"))
				{
					MaterialHelper._ColorZOrderShader = Shader.Find("Unlit");
					shader = MaterialHelper._ColorZOrderShader;
				}
				else
				{
					MaterialHelper._GripMovePluginResources = MaterialHelper._GripMovePluginResources ?? AssetBundle.LoadFromMemory(Resource.gripmovepluginshaders);
					MaterialHelper._ColorZOrderShader = MaterialHelper._GripMovePluginResources.LoadAsset<Shader>("ColorZOrder");
					shader = MaterialHelper._ColorZOrderShader;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				shader = null;
			}
			return shader;
		}

		private static AssetBundle _GripMovePluginResources;

		private static Shader _ColorZOrderShader;
	}
}
