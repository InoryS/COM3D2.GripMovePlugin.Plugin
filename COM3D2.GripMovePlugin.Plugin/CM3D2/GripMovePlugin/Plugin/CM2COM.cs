using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200000A RID: 10
	internal static class CM2COM
	{
		// Token: 0x06000033 RID: 51 RVA: 0x00004C40 File Offset: 0x00002E40
		public static Component FindOvrCamera()
		{
			Component component = global::UnityEngine.Object.FindObjectOfType<SteamVR_GameView>();
			if (component == null)
			{
				component = GameMain.Instance.OvrMgr.OvrCamera.gameObject.GetComponentInChildren<SteamVR_Camera>();
			}
			return component;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00004C78 File Offset: 0x00002E78
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
