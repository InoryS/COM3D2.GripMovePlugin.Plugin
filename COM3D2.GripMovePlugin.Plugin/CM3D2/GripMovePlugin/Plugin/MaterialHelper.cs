using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000021 RID: 33
	internal class MaterialHelper
	{
		// Token: 0x06000124 RID: 292 RVA: 0x000081BC File Offset: 0x000063BC
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

		// Token: 0x040000C7 RID: 199
		private static AssetBundle _GripMovePluginResources;

		// Token: 0x040000C8 RID: 200
		private static Shader _ColorZOrderShader;
	}
}
