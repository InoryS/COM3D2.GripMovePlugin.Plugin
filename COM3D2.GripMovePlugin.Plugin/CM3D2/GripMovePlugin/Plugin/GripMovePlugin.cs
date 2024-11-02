using System;
using System.Collections;
using GearMenu;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000038 RID: 56
	[PluginName("GripMovePlugin")]
	[PluginVersion("0.8.9.4")]
	public class GripMovePlugin : PluginBase
	{
		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060001E3 RID: 483 RVA: 0x000035AA File Offset: 0x000017AA
		// (set) Token: 0x060001E4 RID: 484 RVA: 0x000035B1 File Offset: 0x000017B1
		public static string PluginDataPath { get; private set; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060001E5 RID: 485 RVA: 0x000035B9 File Offset: 0x000017B9
		private bool isUIVisible
		{
			get
			{
				return !(this.uiPanel == null) && this.uiPanel.gameObject.activeSelf;
			}
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x000035F7 File Offset: 0x000017F7
		public GripMovePlugin()
		{
			GripMovePlugin.PluginDataPath = base.DataPath;
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x000024BB File Offset: 0x000006BB
		private void Awake()
		{
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000D228 File Offset: 0x0000B428
		private void CreateUI()
		{
			if (this.uiPanel != null)
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
			Vector3 vector = new Vector3((float)this.PanelWidth / 2f, 100f, 0f);
			int num = (int)Settings.Instance.GetFloatValue("UIPosX", -10000f);
			int num2 = (int)Settings.Instance.GetFloatValue("UIPosY", -10000f);
			UIPanel component = uiroot.GetComponent<UIPanel>();
			if (component && (float)Math.Abs(num) < component.width - (float)this.PanelWidth / 2f && (float)Math.Abs(num2) < component.height - (float)this.PanelHeight / 2f)
			{
				vector.x = (float)num;
				vector.y = (float)num2;
			}
			this.uiPanel = NGUITools.AddChild<UIPanel>(uiroot);
			this.uiPanel.name = "GripMovePlugin UI";
			this.uiPanel.transform.localPosition = vector;
			GameObject gameObject = this.uiPanel.gameObject;
			gameObject.AddComponent<OnDestroyAction>().Callback = new Action(this.SaveUIPos);
			UISprite bgSpr = NGUITools.AddChild<UISprite>(gameObject);
			bgSpr.name = "BG";
			bgSpr.atlas = systemDialogAtlas;
			bgSpr.spriteName = "cm3d2_dialog_frame";
			bgSpr.type = UIBasicSprite.Type.Sliced;
			bgSpr.SetDimensions(this.PanelWidth, this.PanelHeight);
			UISprite uisprite = NGUITools.AddChild<UISprite>(gameObject);
			uisprite.name = "TitleTab";
			uisprite.depth = bgSpr.depth - 1;
			uisprite.atlas = systemDialogAtlas;
			uisprite.spriteName = "cm3d2_dialog_frame";
			uisprite.type = UIBasicSprite.Type.Sliced;
			uisprite.SetDimensions(300, 80);
			uisprite.autoResizeBoxCollider = true;
			uisprite.gameObject.AddComponent<UIDragObject>().target = gameObject.transform;
			uisprite.gameObject.AddComponent<BoxCollider>().isTrigger = true;
			NGUITools.UpdateWidgetCollider(uisprite.gameObject);
			uisprite.transform.localPosition = new Vector3((float)bgSpr.width / 2f + 4f, (float)(bgSpr.height - uisprite.width) / 2f, 0f);
			uisprite.transform.localRotation *= Quaternion.Euler(0f, 0f, -90f);
			UILabel uilabel = uisprite.gameObject.AddComponent<UILabel>();
			uilabel.depth = uisprite.depth + 1;
			uilabel.width = uisprite.width;
			uilabel.color = Color.white;
			uilabel.trueTypeFont = defaultFont;
			uilabel.fontSize = 18;
			uilabel.text = "GripMovePlugin 0.8.9.4";
			UITable uiTable = NGUITools.AddChild<UITable>(gameObject);
			uiTable.pivot = UIWidget.Pivot.Top;
			uiTable.columns = 1;
			uiTable.padding = new Vector2(10f, 5f);
			uiTable.hideInactive = true;
			uiTable.keepWithinPanel = true;
			uiTable.transform.localPosition = new Vector3(0f, (float)this.PanelHeight / 2f - 50f, 0f);
			Action<string> action = delegate(string text)
			{
				UILabel uilabel2 = NGUITools.AddChild<UILabel>(uiTable.gameObject);
				uilabel2.name = text;
				uilabel2.depth = bgSpr.depth + 1;
				uilabel2.width = this.PanelWidth - 100;
				uilabel2.height = 30;
				uilabel2.alignment = NGUIText.Alignment.Left;
				uilabel2.trueTypeFont = defaultFont;
				uilabel2.fontSize = 22;
				uilabel2.spacingX = 0;
				uilabel2.supportEncoding = true;
				uilabel2.text = text;
				uilabel2.color = Color.white;
			};
			action("移動");
			UITable uitable = UIUtils.AddTable(uiTable.gameObject, 2, 5f, 5f);
			UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "MoveEnabled", "全有効", new EventDelegate.Callback(this.ToggleMoveEnabled), GripMoveControllerBase.GetMoveEnabled());
			UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "LockRotHeadYZ", "前転/後転のみ有効", new EventDelegate.Callback(this.ToggleLockRotHeadYZ), GripMoveControllerBase.GetLockRotHeadYZ());
			UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "LockRotXZ", "縦回転無効", new EventDelegate.Callback(this.ToggleLockRotXZ), GripMoveControllerBase.GetLockRotXZ());
			UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "ResetRotation", "回転リセット", new EventDelegate.Callback(this.ResetRotation), true);
			UIUtils.AddButton(uitable.gameObject, 200, 40, 22, "LockMoveY", "上下移動無効", new EventDelegate.Callback(this.ToggleLockMoveY), GripMoveControllerBase.GetLockMoveY());
			uitable.Reposition();
			action("掴み無効");
			UITable uitable2 = UIUtils.AddTable(uiTable.gameObject, 2, 5f, 5f);
			UIUtils.AddButton(uitable2.gameObject, 200, 40, 22, "DisableGrab", "IKモード", new EventDelegate.Callback(this.ToggleIKModeDisableGrab), MenuToolBase.GetDisableGrabWhenIKMode());
			UIUtils.AddButton(uitable2.gameObject, 200, 40, 22, "DisableGrabDirect", "DIRECTモード", new EventDelegate.Callback(this.ToggleDirectModeDisableGrab), MenuToolBase.GetDisableGrabWhenDirectMode());
			uitable2.Reposition();
			action("表示");
			UITable uitable3 = UIUtils.AddTable(uiTable.gameObject, 2, 5f, 5f);
			UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "SetIMGUIVisble", "旧GUI表示", new EventDelegate.Callback(this.ToggleIMGUIVisible), IMGUIQuad.IsVisble);
			UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "ResetIMGUIPos", "位置リセット", new EventDelegate.Callback(this.ResetIMGUIPos), true);
			UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "IMGUITransparent", "旧GUI透過", new EventDelegate.Callback(this.ToggleIMGUITransparent), IMGUIQuad.IsTransparent);
			UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "IMGUINoVRController", "マウス操作", new EventDelegate.Callback(this.ToggleIMGUINoVRController), IMGUIQuad.IsNoVRController);
			UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "PointerVisible", "ポインター表示", new EventDelegate.Callback(this.TogglePointerVisible), MenuToolBase.GetPointerVisible());
			UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "NewOldUI", "公式タブレットUI", new EventDelegate.Callback(this.ToggleNewOldUI), GameMain.Instance.CMSystem.OvrUseNewControllerType);
			UIUtils.AddButton(uitable3.gameObject, 200, 40, 22, "IMGUIAttach", "GUI合体", new EventDelegate.Callback(this.ToggleIMGUIAttach), IMGUIQuad.IsAttach);
			uitable3.Reposition();
			uiTable.Reposition();
			gameObject.SetActive(false);
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000D908 File Offset: 0x0000BB08
		public void SaveUIPos()
		{
			if (this.uiPanel == null)
			{
				return;
			}
			int num = (int)Settings.Instance.GetFloatValue("UIPosX", -10000f);
			int num2 = (int)Settings.Instance.GetFloatValue("UIPosY", -10000f);
			if (num != (int)this.uiPanel.transform.localPosition.x || num2 != (int)this.uiPanel.transform.localPosition.y)
			{
				Settings.Instance.SetFloatValue("UIPosX", (float)((int)this.uiPanel.transform.localPosition.x));
				Settings.Instance.SetFloatValue("UIPosY", (float)((int)this.uiPanel.transform.localPosition.y));
			}
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000362B File Offset: 0x0000182B
		private IEnumerator InstallGripMoveControllerCo()
		{
			this.vrCameraOrigin = null;
			this.leftInstalled = false;
			this.rightInstalled = false;
			for (;;)
			{
				if (this.vrCameraOrigin == null)
				{
					this.tryGrabCamera();
				}
				if (this.vrCameraOrigin != null && (!this.leftInstalled || !this.rightInstalled))
				{
					this.tryInstallGripMoveController();
				}
				if (this.vrCameraOrigin != null && this.leftInstalled && this.rightInstalled)
				{
					break;
				}
				yield return new WaitForSeconds(0.5f);
			}
			yield break;
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0000D9CC File Offset: 0x0000BBCC
		private void OnGearMenuClick(GameObject gameObject)
		{
			this.CreateUI();
			if (this.uiPanel != null)
			{
				this.uiPanel.gameObject.SetActive(!this.uiPanel.gameObject.activeSelf);
				if (this.uiPanel.gameObject.activeSelf)
				{
					UIPanel component = this.uiPanel.parent.GetComponent<UIPanel>();
					if ((component && Math.Abs(this.uiPanel.transform.localPosition.x) >= component.width - (float)this.PanelWidth / 2f) || Math.Abs(this.uiPanel.transform.localPosition.y) >= component.height - (float)this.PanelHeight / 2f)
					{
						this.uiPanel.transform.localPosition = new Vector3((float)this.PanelWidth / 2f, 100f, 0f);
					}
					Buttons.SetFrameColor(gameObject, Color.black);
					return;
				}
				Buttons.SetFrameColor(gameObject, new Color(0.827f, 0.827f, 0.827f));
			}
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000363A File Offset: 0x0000183A
		private void OnLevelWasLoaded(int level)
		{
			base.StopAllCoroutines();
			this.RegisterGearButton();
			base.StartCoroutine("InstallGripMoveControllerCo");
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000DAF4 File Offset: 0x0000BCF4
		private void RegisterGearButton()
		{
			if (Buttons.Contains(this.gearMenuButtonName))
			{
				Buttons.Remove(this.gearMenuButtonName);
			}
			Buttons.SetText(Buttons.Add(this.gearMenuButtonName, "GripMovePlugin", GripMovePlugin.MenuIconPng, new Action<GameObject>(this.OnGearMenuClick)), "GripMovePlugin");
		}

		// Token: 0x060001EF RID: 495 RVA: 0x00003654 File Offset: 0x00001854
		private void ResetIMGUIPos()
		{
			IMGUIQuad.ResetIMGUIPosition();
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x0000365B File Offset: 0x0000185B
		private void ResetRotation()
		{
			if (this.vrCameraOrigin != null)
			{
				GripMoveControllerBase.ResetRotation(this.vrCameraOrigin);
			}
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x0000DB44 File Offset: 0x0000BD44
		private void Start()
		{
			if (!Environment.CommandLine.ToLower().Contains("/vr"))
			{
				Debug.Log("Is NOT VR Mode. Shutdown GripMovePlugin.");
				global::UnityEngine.Object.DestroyImmediate(this);
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

		// Token: 0x060001F2 RID: 498 RVA: 0x00003676 File Offset: 0x00001876
		private void ToggleDirectModeDisableGrab()
		{
			MenuToolBase.SetDisableGrabWhenDirectMode(!MenuToolBase.GetDisableGrabWhenDirectMode());
			UIUtils.SetToggleButtonColor(UIButton.current, MenuToolBase.GetDisableGrabWhenDirectMode());
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00003694 File Offset: 0x00001894
		private void ToggleIKModeDisableGrab()
		{
			MenuToolBase.SetDisableGrabWhenIKMode(!MenuToolBase.GetDisableGrabWhenIKMode());
			UIUtils.SetToggleButtonColor(UIButton.current, MenuToolBase.GetDisableGrabWhenIKMode());
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x000036B2 File Offset: 0x000018B2
		private void ToggleIMGUINoVRController()
		{
			IMGUIQuad.IsNoVRController = !IMGUIQuad.IsNoVRController;
			UIUtils.SetToggleButtonColor(UIButton.current, IMGUIQuad.IsNoVRController);
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x000036D0 File Offset: 0x000018D0
		private void ToggleIMGUITransparent()
		{
			IMGUIQuad.IsTransparent = !IMGUIQuad.IsTransparent;
			UIUtils.SetToggleButtonColor(UIButton.current, IMGUIQuad.IsTransparent);
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x000036EE File Offset: 0x000018EE
		private void ToggleIMGUIVisible()
		{
			IMGUIQuad.IsVisble = !IMGUIQuad.IsVisble;
			UIUtils.SetToggleButtonColor(UIButton.current, IMGUIQuad.IsVisble);
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x0000370C File Offset: 0x0000190C
		private void ToggleIMGUIAttach()
		{
			IMGUIQuad.IsAttach = !IMGUIQuad.IsAttach;
			UIUtils.SetToggleButtonColor(UIButton.current, IMGUIQuad.IsAttach);
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x0000372A File Offset: 0x0000192A
		private void ToggleLockMoveY()
		{
			GripMoveControllerBase.SetLockMoveY(!GripMoveControllerBase.GetLockMoveY());
			UIUtils.SetToggleButtonColor(UIButton.current, GripMoveControllerBase.GetLockMoveY());
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x00003748 File Offset: 0x00001948
		private void ToggleLockRotHeadYZ()
		{
			GripMoveControllerBase.SetLockRotHeadYZ(!GripMoveControllerBase.GetLockRotHeadYZ());
			UIUtils.SetToggleButtonColor(UIButton.current, GripMoveControllerBase.GetLockRotHeadYZ());
		}

		// Token: 0x060001FA RID: 506 RVA: 0x00003766 File Offset: 0x00001966
		private void ToggleLockRotXZ()
		{
			GripMoveControllerBase.SetLockRotXZ(!GripMoveControllerBase.GetLockRotXZ());
			UIUtils.SetToggleButtonColor(UIButton.current, GripMoveControllerBase.GetLockRotXZ());
		}

		// Token: 0x060001FB RID: 507 RVA: 0x00003784 File Offset: 0x00001984
		private void ToggleMoveEnabled()
		{
			GripMoveControllerBase.SetMoveEnabled(!GripMoveControllerBase.GetMoveEnabled());
			UIUtils.SetToggleButtonColor(UIButton.current, GripMoveControllerBase.GetMoveEnabled());
		}

		// Token: 0x060001FC RID: 508 RVA: 0x000037A2 File Offset: 0x000019A2
		private void TogglePointerVisible()
		{
			MenuToolBase.SetPointerVisible(!MenuToolBase.GetPointerVisible());
			UIUtils.SetToggleButtonColor(UIButton.current, MenuToolBase.GetPointerVisible());
		}

		// Token: 0x060001FD RID: 509 RVA: 0x0000DBAC File Offset: 0x0000BDAC
		private void ToggleNewOldUI()
		{
			bool flag = false;
			if (this.leftController && this.leftController.isActiveAndEnabled)
			{
				flag = this.leftController.ToggleNewOldUI();
			}
			if (!flag && this.rightController && this.rightController.isActiveAndEnabled)
			{
				this.rightController.ToggleNewOldUI();
			}
			UIUtils.SetToggleButtonColor(UIButton.current, GameMain.Instance.CMSystem.OvrUseNewControllerType);
		}

		// Token: 0x060001FE RID: 510 RVA: 0x0000DC24 File Offset: 0x0000BE24
		private void tryGrabCamera()
		{
			GameObject gameObject = GameObject.Find("BaseRoomBase");
			if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.HTC)
			{
				ViveCamera viveCamera = global::UnityEngine.Object.FindObjectOfType<ViveCamera>();
				if (viveCamera != null)
				{
					GameObject gameObject2 = gameObject;
					Console.WriteLine("ViveCamera Found: " + viveCamera.name);
					this.vrCameraOrigin = gameObject2;
					return;
				}
			}
			else if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.Oculus)
			{
				OvrCamera ovrCamera = GameMain.Instance.OvrMgr.OvrCamera;
				if (ovrCamera != null)
				{
					GameObject gameObject3 = gameObject;
					Debug.Log("OvrCameraRig Found: " + ovrCamera.name);
					this.vrCameraOrigin = gameObject3;
					return;
				}
				Console.WriteLine("OvrCamera Not Found");
			}
		}

		// Token: 0x060001FF RID: 511 RVA: 0x000037C0 File Offset: 0x000019C0
		private void tryInstallGripMoveController()
		{
			if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.HTC)
			{
				this.tryInstallGripMoveControllerVIVE();
				return;
			}
			if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.Oculus)
			{
				this.tryInstallGripMoveControllerTouch();
			}
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000DCC8 File Offset: 0x0000BEC8
		private void tryInstallGripMoveControllerTouch()
		{
			OVRCameraRig componentInParent = GameMain.Instance.OvrMgr.OvrCamera.gameObject.GetComponentInParent<OVRCameraRig>();
			Transform leftHandAnchor = componentInParent.leftHandAnchor;
			Transform rightHandAnchor = componentInParent.rightHandAnchor;
			OVRInput.Controller connectedControllers = OVRInput.GetConnectedControllers();
			if (leftHandAnchor != null)
			{
				if (leftHandAnchor.gameObject.GetComponent<GripMoveControllerTouch>() != null)
				{
					leftHandAnchor.gameObject.GetComponent<GripMoveControllerTouch>().target = this.vrCameraOrigin;
					this.leftInstalled = true;
				}
				else if (leftHandAnchor.gameObject.GetComponent<OvrController>() != null && ((connectedControllers & OVRInput.Controller.LTouch) != OVRInput.Controller.None || (connectedControllers & OVRInput.Controller.Touch) != OVRInput.Controller.None))
				{
					GripMoveControllerTouch gripMoveControllerTouch = leftHandAnchor.gameObject.AddComponent<GripMoveControllerTouch>();
					gripMoveControllerTouch.ovrController = OVRInput.Controller.LTouch;
					gripMoveControllerTouch.target = this.vrCameraOrigin;
					this.leftController = gripMoveControllerTouch;
					this.leftInstalled = true;
				}
			}
			if (rightHandAnchor != null)
			{
				if (rightHandAnchor.gameObject.GetComponent<GripMoveControllerTouch>() != null)
				{
					rightHandAnchor.gameObject.GetComponent<GripMoveControllerTouch>().target = this.vrCameraOrigin;
					this.rightInstalled = true;
					return;
				}
				if (rightHandAnchor.gameObject.GetComponent<OvrController>() != null && ((connectedControllers & OVRInput.Controller.RTouch) != OVRInput.Controller.None || (connectedControllers & OVRInput.Controller.Touch) != OVRInput.Controller.None))
				{
					GripMoveControllerTouch gripMoveControllerTouch2 = rightHandAnchor.gameObject.AddComponent<GripMoveControllerTouch>();
					gripMoveControllerTouch2.ovrController = OVRInput.Controller.RTouch;
					gripMoveControllerTouch2.target = this.vrCameraOrigin;
					this.rightController = gripMoveControllerTouch2;
					this.rightInstalled = true;
					return;
				}
			}
		}

		// Token: 0x06000201 RID: 513 RVA: 0x0000DE14 File Offset: 0x0000C014
		private void tryInstallGripMoveControllerVIVE()
		{
			OvrMgr.OvrObject ovr_obj = GameMain.Instance.OvrMgr.ovr_obj;
			if (ovr_obj != null)
			{
				if (ovr_obj.left_controller.controller != null && !this.leftInstalled)
				{
					this.leftController = ovr_obj.left_controller.controller.gameObject.GetComponent<GripMoveController>() ?? ovr_obj.left_controller.controller.gameObject.AddComponent<GripMoveController>();
					this.leftController.target = this.vrCameraOrigin;
					this.leftInstalled = true;
				}
				if (ovr_obj.right_controller.controller != null && !this.rightInstalled)
				{
					this.rightController = ovr_obj.right_controller.controller.gameObject.GetComponent<GripMoveController>() ?? ovr_obj.right_controller.controller.gameObject.AddComponent<GripMoveController>();
					this.rightController.target = this.vrCameraOrigin;
					this.rightInstalled = true;
				}
			}
		}

		// Token: 0x04000160 RID: 352
		public const string NAME = "GripMovePlugin";

		// Token: 0x04000161 RID: 353
		public const string VERSION = "0.8.9.4";

		// Token: 0x04000162 RID: 354
		private GameObject vrCameraOrigin;

		// Token: 0x04000163 RID: 355
		private bool leftInstalled;

		// Token: 0x04000164 RID: 356
		private bool rightInstalled;

		// Token: 0x04000165 RID: 357
		private readonly int PanelWidth = 500;

		// Token: 0x04000166 RID: 358
		private readonly int PanelHeight = 640;

		// Token: 0x04000167 RID: 359
		private UIPanel uiPanel;

		// Token: 0x04000168 RID: 360
		private GripMoveControllerBase leftController;

		// Token: 0x04000169 RID: 361
		private GripMoveControllerBase rightController;

		// Token: 0x0400016A RID: 362
		private string gearMenuButtonName = "GripMovePluginMenu";

		// Token: 0x0400016B RID: 363
		private static byte[] MenuIconPng = new byte[]
		{
			137, 80, 78, 71, 13, 10, 26, 10, 0, 0,
			0, 13, 73, 72, 68, 82, 0, 0, 0, 32,
			0, 0, 0, 32, 8, 6, 0, 0, 0, 115,
			122, 122, 244, 0, 0, 0, 6, 98, 75, 71,
			68, 0, byte.MaxValue, 0, byte.MaxValue, 0, byte.MaxValue, 160, 189, 167,
			147, 0, 0, 2, 144, 73, 68, 65, 84, 88,
			195, 237, 151, 77, 72, 85, 81, 16, 199, 127,
			227, 203, 135, 138, 74, 223, 81, 208, 43, 170,
			133, 24, 4, 34, 65, 155, 106, 211, 199, 38,
			140, 80, 164, 77, 45, 90, 180, 107, 17, 8,
			81, 96, 60, 90, 244, 177, 105, 39, 209, 34,
			144, 32, 112, 85, 208, 38, 168, 69, 80, 139,
			50, 42, 178, 8, 37, 209, 106, 27, 22, 90,
			164, 164, 245, 166, 69, byte.MaxValue, 39, 135, 219, 189,
			215, 247, 244, 161, 27, 7, 14, 115, 230, 220,
			115, 230, 206, 199, 127, 230, 220, 107, 238, 206,
			82, 82, 21, 75, 76, 203, 6, 44, 27, 176,
			162, 82, 138, 204, 172, 30, 168, 5, 126, 186,
			251, 228, 130, 34, 96, byte.MaxValue, 104, 187, 153, 181,
			152, 89, 147, 153, 213, 150, 160, 235, 44, 48,
			10, 156, 89, 80, 10, 204, 44, 3, 92, 4,
			158, 3, 175, 129, 87, 64, 175, 153, 53, 207,
			161, 171, 26, 168, 47, 55, 173, 113, 41, 56,
			5, 228, 3, 185, 14, 232, 4, 38, 128, 211,
			41, 186, 28, 248, 81, 118, 238, 220, 125, 118,
			200, 131, 73, 41, 155, 2, 134, 20, 214, 223,
			64, 111, 184, 55, 58, 116, 118, 45, 80, 151,
			182, 47, 58, 162, 17, 56, 40, 32, 253, 1,
			186, 220, 189, 199, 204, 106, 128, 115, 192, 14,
			165, 104, 29, 208, 6, 100, 128, 91, 64, 135,
			210, 181, 30, 104, 6, 222, 0, 3, 102, 214,
			14, 52, 0, 35, 114, 228, 16, 240, 194, 221,
			135, 211, 34, 208, 35, 239, 159, 196, 120, 184,
			75, 124, 111, 16, 238, 235, 154, 31, 7, 46,
			107, 158, 215, 190, 81, 201, 125, 192, 125, 205,
			63, 71, 35, 20, 5, 76, 171, 248, 237, 152,
			84, 189, 213, 116, 70, 60, 43, 228, 3, 76,
			43, 77, 0, 191, 196, 139, 242, 110, 224, 128,
			230, 57, 25, 155, 88, 5, 45, 226, 131, 65,
			57, 206, 82, 100, 111, 86, 120, 121, 42, 239,
			146, 104, 27, 240, 14, 248, 34, 57, 159, 86,
			5, 89, 160, 0, 140, 75, 190, 38, 76, 20,
			61, 235, 138, 236, 63, 225, 238, 119, 205, 108,
			83, 96, 124, 148, 190, 9, 91, 29, 194, 204,
			102, 51, 171, 118, 247, 153, 56, 12, 184, 66,
			215, 36, 249, 171, 214, 28, 248, 174, 181, 61,
			146, 103, 34, 103, 47, 105, 253, 188, 228, 15,
			146, 31, 73, 174, 149, 115, 94, 212, 31, 135,
			1, 0, 11, 82, 147, 214, 84, 166, 75, 172,
			244, 49, 57, 58, 37, 3, 0, 214, 36, 97,
			160, 160, 181, 70, 201, 23, 128, 43, 9, 138,
			11, 37, 26, 80, 72, 232, 154, 177, 6, 12,
			136, 111, 149, 213, 55, 84, 66, 149, 190, 113,
			167, 147, 30, 190, 20, 111, 175, 224, 141, 187,
			65, 21, 213, 16, 188, 111, 44, 201, 128, 199,
			226, 71, 204, 236, 112, 133, 12, 104, 213, 203,
			79, 10, 95, 0, 195, 73, 101, 248, 64, 188,
			6, 232, 51, 179, 143, 33, 96, 230, 73, 141,
			64, 63, 176, 81, 242, 136, 7, 159, 226, 85,
			145, 110, 55, 174, 171, 24, 96, 165, 106, 59,
			39, 121, 98, 158, 6, 12, 1, 91, 164, 15,
			160, 123, 46, 128, 92, 5, 110, 6, 64, 41,
			0, 15, 3, 92, 20, 17, 156, 137, 105, 98,
			197, 232, 133, 244, 44, 0, 242, 32, 112, 47,
			241, 50, 10, 154, 74, 22, 216, 7, 28, 5,
			246, 3, 171, 130, 103, 171, 213, 213, 58, 35,
			103, 118, 2, 199, 130, 38, 86, 108, 68, 119,
			228, 125, 27, 144, 251, 239, 93, 229, 220, 221,
			229, 140, 208, 128, 180, 125, 203, 159, 229, 85,
			139, 160, 59, 179, 40, byte.MaxValue, 5, 49, 212, 15,
			124, 2, 222, 167, 254, 79, 44, 245, 239, 249,
			95, 223, 51, 170, 193, 122, 190, 54, 45, 0,
			0, 0, 0, 73, 69, 78, 68, 174, 66, 96,
			130
		};
	}
}
