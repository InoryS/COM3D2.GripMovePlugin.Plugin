using System;
using System.Collections;
using GearMenu;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.GripMovePlugin.Plugin;

[PluginName("GripMovePlugin")]
[PluginVersion("0.8.9.4")]
public class GripMovePlugin : PluginBase
{
	public const string NAME = "GripMovePlugin";

	public const string VERSION = "0.8.9.4";

	private static readonly byte[] MenuIconPng = new byte[]
	{
		137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 32, 0, 0, 0, 32,
		8, 6, 0, 0, 0, 115, 122, 122, 244, 0, 0, 0, 6, 98, 75, 71, 68, 0, byte.MaxValue, 0,
		byte.MaxValue, 0, byte.MaxValue, 160, 189, 167, 147, 0, 0, 2, 144, 73, 68, 65, 84, 88,
		195, 237, 151, 77, 72, 85, 81, 16, 199, 127, 227, 203, 135, 138, 74, 223, 81, 208, 43,
		170, 133, 24, 4, 34, 65, 155, 106, 211, 199, 38, 140, 80, 164, 77, 45, 90, 180, 107, 17,
		8, 81, 96, 60, 90, 244, 177, 105, 39, 209, 34, 144, 32, 112, 85, 208, 38, 168, 69, 80,
		139, 50, 42, 178, 8, 37, 209, 106, 27, 22, 90, 164, 164, 245, 166, 69, byte.MaxValue,
		39, 135, 219, 189, 215, 247, 244, 161, 27, 7, 14, 115, 230, 220, 115, 230, 206, 199,
		127, 230, 220, 107, 238, 206, 82, 82, 21, 75, 76, 203, 6, 44, 27, 176, 162, 82, 138,
		204, 172, 30, 168, 5, 126, 186, 251, 228, 130, 34, 96, byte.MaxValue, 104, 187, 153,
		181, 152, 89, 147, 153, 213, 150, 160, 235, 44, 48, 10, 156, 89, 80, 10, 204, 44, 3, 92,
		4, 158, 3, 175, 129, 87, 64, 175, 153, 53, 207, 161, 171, 26, 168, 47, 55, 173, 113, 41,
		56, 5, 228, 3, 185, 14, 232, 4, 38, 128, 211, 41, 186, 28, 248, 81, 118, 238, 220, 125,
		118, 200, 131, 73, 41, 155, 2, 134, 20, 214, 223, 64, 111, 184, 55, 58, 116, 118, 45,
		80, 151, 182, 47, 58, 162, 17, 56, 40, 32, 253, 1, 186, 220, 189, 199, 204, 106, 128,
		115, 192, 14, 165, 104, 29, 208, 6, 100, 128, 91, 64, 135, 210, 181, 30, 104, 6, 222, 0,
		3, 102, 214, 14, 52, 0, 35, 114, 228, 16, 240, 194, 221, 135, 211, 34, 208, 35, 239,
		159, 196, 120, 184, 75, 124, 111, 16, 238, 235, 154, 31, 7, 46, 107, 158, 215, 190, 81,
		201, 125, 192, 125, 205, 63, 71, 35, 20, 5, 76, 171, 248, 237, 152, 84, 189, 213, 116,
		70, 60, 43, 228, 3, 76, 43, 77, 0, 191, 196, 139, 242, 110, 224, 128, 230, 57, 25, 155,
		88, 5, 45, 226, 131, 65, 57, 206, 82, 100, 111, 86, 120, 121, 42, 239, 146, 104, 27,
		240, 14, 248, 34, 57, 159, 86, 5, 89, 160, 0, 140, 75, 190, 38, 76, 20, 61, 235, 138,
		236, 63, 225, 238, 119, 205, 108, 83, 96, 124, 148, 190, 9, 91, 29, 194, 204, 102, 51,
		171, 118, 247, 153, 56, 12, 184, 66, 215, 36, 249, 171, 214, 28, 248, 174, 181, 61, 146,
		103, 34, 103, 47, 105, 253, 188, 228, 15, 146, 31, 73, 174, 149, 115, 94, 212, 31, 135,
		1, 0, 11, 82, 147, 214, 84, 166, 75, 172, 244, 49, 57, 58, 37, 3, 0, 214, 36, 97, 160,
		160, 181, 70, 201, 23, 128, 43, 9, 138, 11, 37, 26, 80, 72, 232, 154, 177, 6, 12, 136,
		111, 149, 213, 55, 84, 66, 149, 190, 113, 167, 147, 30, 190, 20, 111, 175, 224, 141,
		187, 65, 21, 213, 16, 188, 111, 44, 201, 128, 199, 226, 71, 204, 236, 112, 133, 12, 104,
		213, 203, 79, 10, 95, 0, 195, 73, 101, 248, 64, 188, 6, 232, 51, 179, 143, 33, 96, 230,
		73, 141, 64, 63, 176, 81, 242, 136, 7, 159, 226, 85, 145, 110, 55, 174, 171, 24, 96,
		165, 106, 59, 39, 121, 98, 158, 6, 12, 1, 91, 164, 15, 160, 123, 46, 128, 92, 5, 110, 6,
		64, 41, 0, 15, 3, 92, 20, 17, 156, 137, 105, 98, 197, 232, 133, 244, 44, 0, 242, 32,
		112, 47, 241, 50, 10, 154, 74, 22, 216, 7, 28, 5, 246, 3, 171, 130, 103, 171, 213, 213,
		58, 35, 103, 118, 2, 199, 130, 38, 86, 108, 68, 119, 228, 125, 27, 144, 251, 239, 93,
		229, 220, 221, 229, 140, 208, 128, 180, 125, 203, 159, 229, 85, 139, 160, 59, 179, 40,
		byte.MaxValue, 5, 49, 212, 15, 124, 2, 222, 167, 254, 79, 44, 245, 239, 249, 95, 223,
		51, 170, 193, 122, 190, 54, 45, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130
	};

	private readonly int PanelHeight = 640;

	private readonly int PanelWidth = 500;

	private readonly string gearMenuButtonName = "GripMovePluginMenu";

	private GripMoveControllerBase leftController;

	private bool leftInstalled;

	private GripMoveControllerBase rightController;

	private bool rightInstalled;

	private UIPanel uiPanel;

	private GameObject vrCameraOrigin;

	public GripMovePlugin()
	{
		PluginDataPath = DataPath;
	}

	public static string PluginDataPath { get; private set; }

	private bool isUIVisible => uiPanel != null && uiPanel.gameObject.activeSelf;

	private void Awake()
	{
	}

	private void Start()
	{
		if (!Environment.CommandLine.ToLower().Contains("/vr"))
		{
			Debug.Log("Is NOT VR Mode. Shutdown GripMovePlugin.");
			DestroyImmediate(this);
			return;
		}

		Debug.Log("Start GripMovePlugin");
		try
		{
			IKTool.Create();
			DirectTouchTool.Instance.enabled = true;
		}
		catch (Exception ex)
		{
			Debug.Log(ex);
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		StopAllCoroutines();
		RegisterGearButton();
		StartCoroutine("InstallGripMoveControllerCo");
	}

	private void CreateUI()
	{
		if (uiPanel != null)
		{
			return;
		}

		Font defaultFont = UIUtils.GetDefaultFont();
		UIAtlas systemDialogAtlas = UIUtils.GetSystemDialogAtlas();
		GameObject uiroot = UIUtils.GetUIRoot();
		if (defaultFont == null || systemDialogAtlas == null || uiroot == null)
		{
			return;
		}

		Vector3 vector;
		vector = new Vector3(PanelWidth / 2f, 100f, 0f);
		int num = (int)Settings.Instance.GetFloatValue("UIPosX", -10000f);
		int num2 = (int)Settings.Instance.GetFloatValue("UIPosY", -10000f);
		UIPanel component = uiroot.GetComponent<UIPanel>();
		if (component && Math.Abs(num) < component.width - PanelWidth / 2f &&
		    Math.Abs(num2) < component.height - PanelHeight / 2f)
		{
			vector.x = num;
			vector.y = num2;
		}

		uiPanel = NGUITools.AddChild<UIPanel>(uiroot);
		uiPanel.name = "GripMovePlugin UI";
		uiPanel.transform.localPosition = vector;
		GameObject gameObject = uiPanel.gameObject;
		gameObject.AddComponent<OnDestroyAction>().Callback = SaveUIPos;
		UISprite bgSpr = NGUITools.AddChild<UISprite>(gameObject);
		bgSpr.name = "BG";
		bgSpr.atlas = systemDialogAtlas;
		bgSpr.spriteName = "cm3d2_dialog_frame";
		bgSpr.type = (UIBasicSprite.Type)1;
		bgSpr.SetDimensions(PanelWidth, PanelHeight);
		UISprite uisprite = NGUITools.AddChild<UISprite>(gameObject);
		uisprite.name = "TitleTab";
		uisprite.depth = bgSpr.depth - 1;
		uisprite.atlas = systemDialogAtlas;
		uisprite.spriteName = "cm3d2_dialog_frame";
		uisprite.type = (UIBasicSprite.Type)1;
		uisprite.SetDimensions(300, 80);
		uisprite.autoResizeBoxCollider = true;
		uisprite.gameObject.AddComponent<UIDragObject>().target = gameObject.transform;
		uisprite.gameObject.AddComponent<BoxCollider>().isTrigger = true;
		NGUITools.UpdateWidgetCollider(uisprite.gameObject);
		uisprite.transform.localPosition = new Vector3(bgSpr.width / 2f + 4f,
			(bgSpr.height - uisprite.width) / 2f, 0f);
		uisprite.transform.localRotation *= Quaternion.Euler(0f, 0f, -90f);
		UILabel uilabel = uisprite.gameObject.AddComponent<UILabel>();
		uilabel.depth = uisprite.depth + 1;
		uilabel.width = uisprite.width;
		uilabel.color = Color.white;
		uilabel.trueTypeFont = defaultFont;
		uilabel.fontSize = 18;
		uilabel.text = "GripMovePlugin 0.8.9.4";
		UITable uiTable = NGUITools.AddChild<UITable>(gameObject);
		uiTable.pivot = (UIWidget.Pivot)1;
		uiTable.columns = 1;
		uiTable.padding = new Vector2(10f, 5f);
		uiTable.hideInactive = true;
		uiTable.keepWithinPanel = true;
		uiTable.transform.localPosition = new Vector3(0f, PanelHeight / 2f - 50f, 0f);
		Action<string> action = delegate(string text)
		{
			UILabel uilabel2 = NGUITools.AddChild<UILabel>(uiTable.gameObject);
			uilabel2.name = text;
			uilabel2.depth = bgSpr.depth + 1;
			uilabel2.width = PanelWidth - 100;
			uilabel2.height = 30;
			uilabel2.alignment = (NGUIText.Alignment)1;
			uilabel2.trueTypeFont = defaultFont;
			uilabel2.fontSize = 22;
			uilabel2.spacingX = 0;
			uilabel2.supportEncoding = true;
			uilabel2.text = text;
			uilabel2.color = Color.white;
		};
		action("移動");
		UITable uitable = UIUtils.AddTable(uiTable.gameObject, 2, 5f, 5f);
		UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "MoveEnabled", "全有効", ToggleMoveEnabled,
			GripMoveControllerBase.GetMoveEnabled());
		UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "LockRotHeadYZ", "前転/後転のみ有効",
			ToggleLockRotHeadYZ, GripMoveControllerBase.GetLockRotHeadYZ());
		UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "LockRotXZ", "縦回転無効", ToggleLockRotXZ,
			GripMoveControllerBase.GetLockRotXZ());
		UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "ResetRotation", "回転リセット", ResetRotation,
			true);
		UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "LockMoveY", "上下移動無効", ToggleLockMoveY,
			GripMoveControllerBase.GetLockMoveY());
		uitable.Reposition();
		action("掴み無効");
		UITable uitable2 = UIUtils.AddTable(uiTable.gameObject, 2, 5f, 5f);
		UIUtils.AddButton(uitable2.gameObject, 200, 40, 22, "DisableGrab", "IKモード",
			ToggleIKModeDisableGrab, MenuToolBase.GetDisableGrabWhenIKMode());
		UIUtils.AddButton(uitable2.gameObject, 200, 40, 22, "DisableGrabDirect", "DIRECTモード",
			ToggleDirectModeDisableGrab, MenuToolBase.GetDisableGrabWhenDirectMode());
		uitable2.Reposition();
		action("表示");
		UITable uitable3 = UIUtils.AddTable(uiTable.gameObject, 2, 5f, 5f);
		UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "SetIMGUIVisble", "旧GUI表示",
			ToggleIMGUIVisible, IMGUIQuad.IsVisble);
		UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "ResetIMGUIPos", "位置リセット",
			ResetIMGUIPos, true);
		UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "IMGUITransparent", "旧GUI透過",
			ToggleIMGUITransparent, IMGUIQuad.IsTransparent);
		UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "IMGUINoVRController", "マウス操作",
			ToggleIMGUINoVRController, IMGUIQuad.IsNoVRController);
		UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "PointerVisible", "ポインター表示",
			TogglePointerVisible, MenuToolBase.GetPointerVisible());
		UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "NewOldUI", "公式タブレットUI", ToggleNewOldUI,
			GameMain.Instance.CMSystem.OvrUseNewControllerType);
		UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "IMGUIAttach", "GUI合体",
			ToggleIMGUIAttach, IMGUIQuad.IsAttach);
		uitable3.Reposition();
		uiTable.Reposition();
		gameObject.SetActive(false);
	}

	public void SaveUIPos()
	{
		if (uiPanel == null)
		{
			return;
		}

		int num = (int)Settings.Instance.GetFloatValue("UIPosX", -10000f);
		int num2 = (int)Settings.Instance.GetFloatValue("UIPosY", -10000f);
		if (num != (int)uiPanel.transform.localPosition.x ||
		    num2 != (int)uiPanel.transform.localPosition.y)
		{
			Settings.Instance.SetFloatValue("UIPosX", (int)uiPanel.transform.localPosition.x);
			Settings.Instance.SetFloatValue("UIPosY", (int)uiPanel.transform.localPosition.y);
		}
	}

	private IEnumerator InstallGripMoveControllerCo()
	{
		vrCameraOrigin = null;
		leftInstalled = false;
		rightInstalled = false;
		for (;;)
		{
			if (vrCameraOrigin == null)
			{
				tryGrabCamera();
			}

			if (vrCameraOrigin != null && (!leftInstalled || !rightInstalled))
			{
				tryInstallGripMoveController();
			}

			if (vrCameraOrigin != null && leftInstalled && rightInstalled)
			{
				break;
			}

			yield return new WaitForSeconds(0.5f);
		}
	}

	private void OnGearMenuClick(GameObject gameObject)
	{
		CreateUI();
		if (uiPanel != null)
		{
			uiPanel.gameObject.SetActive(!uiPanel.gameObject.activeSelf);
			if (uiPanel.gameObject.activeSelf)
			{
				UIPanel component = uiPanel.parent.GetComponent<UIPanel>();
				if ((component && Math.Abs(uiPanel.transform.localPosition.x) >=
					    component.width - PanelWidth / 2f) ||
				    Math.Abs(uiPanel.transform.localPosition.y) >=
				    component.height - PanelHeight / 2f)
				{
					uiPanel.transform.localPosition = new Vector3(PanelWidth / 2f, 100f, 0f);
				}

				Buttons.SetFrameColor(gameObject, Color.black);
				return;
			}

			Buttons.SetFrameColor(gameObject, new Color(0.827f, 0.827f, 0.827f));
		}
	}

	private void RegisterGearButton()
	{
		if (Buttons.Contains(gearMenuButtonName))
		{
			Buttons.Remove(gearMenuButtonName);
		}

		Buttons.SetText(
			Buttons.Add(gearMenuButtonName, "GripMovePlugin", MenuIconPng, OnGearMenuClick),
			"GripMovePlugin");
	}

	private void ResetIMGUIPos() => IMGUIQuad.ResetIMGUIPosition();

	private void ResetRotation()
	{
		if (vrCameraOrigin != null)
		{
			GripMoveControllerBase.ResetRotation(vrCameraOrigin);
		}
	}

	private void ToggleDirectModeDisableGrab()
	{
		MenuToolBase.SetDisableGrabWhenDirectMode(!MenuToolBase.GetDisableGrabWhenDirectMode());
		UIUtils.SetToggleButtonColor(UIButton.current, MenuToolBase.GetDisableGrabWhenDirectMode());
	}

	private void ToggleIKModeDisableGrab()
	{
		MenuToolBase.SetDisableGrabWhenIKMode(!MenuToolBase.GetDisableGrabWhenIKMode());
		UIUtils.SetToggleButtonColor(UIButton.current, MenuToolBase.GetDisableGrabWhenIKMode());
	}

	private void ToggleIMGUINoVRController()
	{
		IMGUIQuad.IsNoVRController = !IMGUIQuad.IsNoVRController;
		UIUtils.SetToggleButtonColor(UIButton.current, IMGUIQuad.IsNoVRController);
	}

	private void ToggleIMGUITransparent()
	{
		IMGUIQuad.IsTransparent = !IMGUIQuad.IsTransparent;
		UIUtils.SetToggleButtonColor(UIButton.current, IMGUIQuad.IsTransparent);
	}

	private void ToggleIMGUIVisible()
	{
		IMGUIQuad.IsVisble = !IMGUIQuad.IsVisble;
		UIUtils.SetToggleButtonColor(UIButton.current, IMGUIQuad.IsVisble);
	}

	private void ToggleIMGUIAttach()
	{
		IMGUIQuad.IsAttach = !IMGUIQuad.IsAttach;
		UIUtils.SetToggleButtonColor(UIButton.current, IMGUIQuad.IsAttach);
	}

	private void ToggleLockMoveY()
	{
		GripMoveControllerBase.SetLockMoveY(!GripMoveControllerBase.GetLockMoveY());
		UIUtils.SetToggleButtonColor(UIButton.current, GripMoveControllerBase.GetLockMoveY());
	}

	private void ToggleLockRotHeadYZ()
	{
		GripMoveControllerBase.SetLockRotHeadYZ(!GripMoveControllerBase.GetLockRotHeadYZ());
		UIUtils.SetToggleButtonColor(UIButton.current, GripMoveControllerBase.GetLockRotHeadYZ());
	}

	private void ToggleLockRotXZ()
	{
		GripMoveControllerBase.SetLockRotXZ(!GripMoveControllerBase.GetLockRotXZ());
		UIUtils.SetToggleButtonColor(UIButton.current, GripMoveControllerBase.GetLockRotXZ());
	}

	private void ToggleMoveEnabled()
	{
		GripMoveControllerBase.SetMoveEnabled(!GripMoveControllerBase.GetMoveEnabled());
		UIUtils.SetToggleButtonColor(UIButton.current, GripMoveControllerBase.GetMoveEnabled());
	}

	private void TogglePointerVisible()
	{
		MenuToolBase.SetPointerVisible(!MenuToolBase.GetPointerVisible());
		UIUtils.SetToggleButtonColor(UIButton.current, MenuToolBase.GetPointerVisible());
	}

	private void ToggleNewOldUI()
	{
		bool flag = false;
		if (leftController && leftController.isActiveAndEnabled)
		{
			flag = leftController.ToggleNewOldUI();
		}

		if (!flag && rightController && rightController.isActiveAndEnabled)
		{
			rightController.ToggleNewOldUI();
		}

		UIUtils.SetToggleButtonColor(UIButton.current,
			GameMain.Instance.CMSystem.OvrUseNewControllerType);
	}

	private void tryGrabCamera()
	{
		var gameObject = GameObject.Find("BaseRoomBase");
		if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)2)
		{
			ViveCamera viveCamera = FindObjectOfType<ViveCamera>();
			if (viveCamera != null)
			{
				GameObject gameObject2 = gameObject;
				Console.WriteLine("ViveCamera Found: " + viveCamera.name);
				vrCameraOrigin = gameObject2;
			}
		}
		else if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)1)
		{
			OvrCamera ovrCamera = GameMain.Instance.OvrMgr.OvrCamera;
			if (ovrCamera != null)
			{
				GameObject gameObject3 = gameObject;
				Debug.Log("OvrCameraRig Found: " + ovrCamera.name);
				vrCameraOrigin = gameObject3;
				return;
			}

			Console.WriteLine("OvrCamera Not Found");
		}
	}

	private void tryInstallGripMoveController()
	{
		if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)2)
		{
			tryInstallGripMoveControllerVIVE();
			return;
		}

		if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)1)
		{
			tryInstallGripMoveControllerTouch();
		}
	}

	private void tryInstallGripMoveControllerTouch()
	{
		OVRCameraRig componentInParent = GameMain.Instance.OvrMgr.OvrCamera.gameObject
			.GetComponentInParent<OVRCameraRig>();
		Transform leftHandAnchor = componentInParent.leftHandAnchor;
		Transform rightHandAnchor = componentInParent.rightHandAnchor;
		OVRInput.Controller connectedControllers = OVRInput.GetConnectedControllers();
		if (leftHandAnchor != null)
		{
			if (leftHandAnchor.gameObject.GetComponent<GripMoveControllerTouch>() != null)
			{
				leftHandAnchor.gameObject.GetComponent<GripMoveControllerTouch>().target =
					vrCameraOrigin;
				leftInstalled = true;
			}
			else if (leftHandAnchor.gameObject.GetComponent<OvrController>() != null &&
			         ((connectedControllers & (OVRInput.Controller)1) != 0 ||
			          (connectedControllers & (OVRInput.Controller)3) != 0))
			{
				GripMoveControllerTouch gripMoveControllerTouch =
					leftHandAnchor.gameObject.AddComponent<GripMoveControllerTouch>();
				gripMoveControllerTouch.ovrController = (OVRInput.Controller)1;
				gripMoveControllerTouch.target = vrCameraOrigin;
				leftController = gripMoveControllerTouch;
				leftInstalled = true;
			}
		}

		if (rightHandAnchor != null)
		{
			if (rightHandAnchor.gameObject.GetComponent<GripMoveControllerTouch>() != null)
			{
				rightHandAnchor.gameObject.GetComponent<GripMoveControllerTouch>().target =
					vrCameraOrigin;
				rightInstalled = true;
				return;
			}

			if (rightHandAnchor.gameObject.GetComponent<OvrController>() != null &&
			    ((connectedControllers & (OVRInput.Controller)2) != 0 ||
			     (connectedControllers & (OVRInput.Controller)3) != 0))
			{
				GripMoveControllerTouch gripMoveControllerTouch2 =
					rightHandAnchor.gameObject.AddComponent<GripMoveControllerTouch>();
				gripMoveControllerTouch2.ovrController = (OVRInput.Controller)2;
				gripMoveControllerTouch2.target = vrCameraOrigin;
				rightController = gripMoveControllerTouch2;
				rightInstalled = true;
			}
		}
	}

	private void tryInstallGripMoveControllerVIVE()
	{
		OvrMgr.OvrObject ovr_obj = GameMain.Instance.OvrMgr.ovr_obj;
		if (ovr_obj != null)
		{
			if (ovr_obj.left_controller.controller != null && !leftInstalled)
			{
				leftController =
					ovr_obj.left_controller.controller.gameObject
						.GetComponent<GripMoveController>() ?? ovr_obj.left_controller.controller
						.gameObject.AddComponent<GripMoveController>();
				leftController.target = vrCameraOrigin;
				leftInstalled = true;
			}

			if (ovr_obj.right_controller.controller != null && !rightInstalled)
			{
				rightController =
					ovr_obj.right_controller.controller.gameObject
						.GetComponent<GripMoveController>() ?? ovr_obj.right_controller.controller
						.gameObject.AddComponent<GripMoveController>();
				rightController.target = vrCameraOrigin;
				rightInstalled = true;
			}
		}
	}
}
