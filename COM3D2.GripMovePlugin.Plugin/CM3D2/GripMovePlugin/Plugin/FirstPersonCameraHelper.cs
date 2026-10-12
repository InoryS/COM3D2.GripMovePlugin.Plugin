using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class FirstPersonCameraHelper
	{
		public static GameObject FindManHead()
		{
			GameObject gameObject = GameObject.Find("__GameMain__/Character/Active/AllOffset/Man[0]");
			Transform[] array = (gameObject ? gameObject.GetComponentsInChildren<Transform>() : new Transform[0]).Where<Transform>((Transform trans) => trans.name.IndexOf("_SM_") > -1).ToArray<Transform>();
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

		public static bool IsFPSModeEnabled(int level)
		{
			return FirstPersonCameraHelper.EnableFpsScenesName.Contains(GameMain.Instance.GetNowSceneName());
		}

		public static bool IsIKModeEnabled(int level)
		{
			return FirstPersonCameraHelper.EnableIKScenesName.Contains(GameMain.Instance.GetNowSceneName());
		}

		public static bool IsPhotoMode(int level)
		{
			return FirstPersonCameraHelper.EnablePhotoScenesName.Contains(GameMain.Instance.GetNowSceneName());
		}

		private static string isChublip = null;

		private static string[] EnableFpsScenesName = new string[] { "SceneYotogi", "SceneADV", "SceneFreeModeSelect", "SceneYotogiWithChubLip", "SceneYotogiOld" };

		private static string[] EnableIKScenesName = new string[] { "ScenePhotoMode" };

		private static string[] EnablePhotoScenesName = new string[] { "ScenePhotoMode" };

		public enum Scene
		{
			SceneCharacterSelect = 1,
			SceneCompetitiveShow,
			SceneDaily,
			SceneDance_DDFL,
			SceneEdit_ChuB = 4,
			SceneEdit,
			SceneLogo,
			SceneMaidManagement,
			SceneShop,
			SceneTitle,
			SceneTrophy,
			SceneYotogi_ChuB = 10,
			SceneTryInfo,
			SceneUserEdit,
			SceneWarning,
			SceneYotogi,
			SceneADV,
			SceneStartDaily,
			SceneToTitle,
			SceneSingleEffect,
			SceneStaffRoll,
			SceneDance_ETY,
			SceneDance_SL = 22,
			ScenePhotoMode_ChuB = 24,
			SceneRecollection = 24,
			ScenePhotoMode = 27
		}
	}
}
