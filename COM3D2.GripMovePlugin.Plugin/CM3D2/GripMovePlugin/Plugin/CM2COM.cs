using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CM3D2.GripMovePlugin.Plugin;

internal static class CM2COM
{
	public static Component FindOvrCamera()
	{
		Component component = Object.FindObjectOfType<SteamVR_GameView>();
		if (component == null)
		{
			component = GameMain.Instance.OvrMgr.OvrCamera.gameObject
				.GetComponentInChildren<SteamVR_Camera>();
		}

		return component;
	}

	public static GameObject FindOvrScreen()
	{
		var gameObject = GameObject.Find("ovr_screen");
		if (gameObject == null)
		{
			var gameObject2 = GameObject.Find("/__GameMain__");
			if (gameObject2)
			{
				Transform transform =
					Array.Find(gameObject2.transform.GetComponentsInChildren<Transform>(true),
						item => item.name == "ovr_screen");
				if (transform)
				{
					gameObject = transform.gameObject;
				}
			}
		}

		return gameObject;
	}
}
