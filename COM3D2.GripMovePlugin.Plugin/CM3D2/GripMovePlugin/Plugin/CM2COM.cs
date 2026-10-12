using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal static class CM2COM
	{
		public static Component FindOvrCamera()
		{
			Component component = UnityEngine.Object.FindObjectOfType<SteamVR_GameView>();
			if (component == null)
			{
				component = GameMain.Instance.OvrMgr.OvrCamera.gameObject.GetComponentInChildren<SteamVR_Camera>();
			}
			return component;
		}

		public static GameObject FindOvrScreen()
		{
			GameObject gameObject = GameObject.Find("ovr_screen");
			if (gameObject == null)
			{
				GameObject gameObject2 = GameObject.Find("/__GameMain__");
				if (gameObject2)
				{
					Transform transform = Array.Find<Transform>(gameObject2.transform.GetComponentsInChildren<Transform>(true), (Transform item) => item.name == "ovr_screen");
					if (transform)
					{
						gameObject = transform.gameObject;
					}
				}
			}
			return gameObject;
		}
	}
}
