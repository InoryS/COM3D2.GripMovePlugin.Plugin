using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000024 RID: 36
	internal class FirstPersonCameraHelper
	{
		// Token: 0x0600012F RID: 303 RVA: 0x00008328 File Offset: 0x00006528
		public static GameObject FindManHead()
		{
			GameObject gameObject = GameObject.Find("__GameMain__/Character/Active/AllOffset/Man[0]");
			Transform[] array = (gameObject ? gameObject.GetComponentsInChildren<Transform>() : new Transform[0])
				.Where((Transform trans) => trans.name.IndexOf("_SM_") > -1)
				.ToArray();

			for (int i = 0; i < array.Length; i++)
			{
				foreach (Transform transform in array[i].gameObject.transform)
				{
					if (transform.name.IndexOf("ManHead") > -1)
					{
						return transform.gameObject;
					}
				}
			}
			return null;
		}


		// Token: 0x06000130 RID: 304 RVA: 0x00008404 File Offset: 0x00006604
		public static bool isChubLip()
		{
			if (FirstPersonCameraHelper.isChublip == null)
			{
				if (!Application.dataPath.Contains("CM3D2OH") && !Application.dataPath.Contains("COM3D2OH"))
				{
					FirstPersonCameraHelper.isChublip = "N";
				}
				else
				{
					FirstPersonCameraHelper.isChublip = "Y";
				}
			}
			return "Y" == FirstPersonCameraHelper.isChublip;
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00002EF2 File Offset: 0x000010F2
		public static bool IsFPSModeEnabled(int level)
		{
			return FirstPersonCameraHelper.EnableFpsScenesName.Contains(GameMain.Instance.GetNowSceneName());
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00002F08 File Offset: 0x00001108
		public static bool IsIKModeEnabled(int level)
		{
			return FirstPersonCameraHelper.EnableIKScenesName.Contains(GameMain.Instance.GetNowSceneName());
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00002F1E File Offset: 0x0000111E
		public static bool IsPhotoMode(int level)
		{
			return FirstPersonCameraHelper.EnablePhotoScenesName.Contains(GameMain.Instance.GetNowSceneName());
		}

		// Token: 0x040000CC RID: 204
		private static string isChublip = null;

		// Token: 0x040000CD RID: 205
		private static string[] EnableFpsScenesName = new string[] { "SceneYotogi", "SceneADV", "SceneFreeModeSelect", "SceneYotogiWithChubLip", "SceneYotogiOld" };

		// Token: 0x040000CE RID: 206
		private static string[] EnableIKScenesName = new string[] { "ScenePhotoMode" };

		// Token: 0x040000CF RID: 207
		private static string[] EnablePhotoScenesName = new string[] { "ScenePhotoMode" };

		// Token: 0x02000025 RID: 37
		public enum Scene
		{
			// Token: 0x040000D1 RID: 209
			SceneCharacterSelect = 1,
			// Token: 0x040000D2 RID: 210
			SceneCompetitiveShow,
			// Token: 0x040000D3 RID: 211
			SceneDaily,
			// Token: 0x040000D4 RID: 212
			SceneDance_DDFL,
			// Token: 0x040000D5 RID: 213
			SceneEdit_ChuB = 4,
			// Token: 0x040000D6 RID: 214
			SceneEdit,
			// Token: 0x040000D7 RID: 215
			SceneLogo,
			// Token: 0x040000D8 RID: 216
			SceneMaidManagement,
			// Token: 0x040000D9 RID: 217
			SceneShop,
			// Token: 0x040000DA RID: 218
			SceneTitle,
			// Token: 0x040000DB RID: 219
			SceneTrophy,
			// Token: 0x040000DC RID: 220
			SceneYotogi_ChuB = 10,
			// Token: 0x040000DD RID: 221
			SceneTryInfo,
			// Token: 0x040000DE RID: 222
			SceneUserEdit,
			// Token: 0x040000DF RID: 223
			SceneWarning,
			// Token: 0x040000E0 RID: 224
			SceneYotogi,
			// Token: 0x040000E1 RID: 225
			SceneADV,
			// Token: 0x040000E2 RID: 226
			SceneStartDaily,
			// Token: 0x040000E3 RID: 227
			SceneToTitle,
			// Token: 0x040000E4 RID: 228
			SceneSingleEffect,
			// Token: 0x040000E5 RID: 229
			SceneStaffRoll,
			// Token: 0x040000E6 RID: 230
			SceneDance_ETY,
			// Token: 0x040000E7 RID: 231
			SceneDance_SL = 22,
			// Token: 0x040000E8 RID: 232
			ScenePhotoMode_ChuB = 24,
			// Token: 0x040000E9 RID: 233
			SceneRecollection = 24,
			// Token: 0x040000EA RID: 234
			ScenePhotoMode = 27
		}
	}
}
