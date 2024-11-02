using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200001F RID: 31
	internal abstract class MenuToolBase : MonoBehaviour
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x00002C83 File Offset: 0x00000E83
		// (set) Token: 0x060000D7 RID: 215 RVA: 0x00002C8B File Offset: 0x00000E8B
		public bool IsMovingSelf { get; set; }

		// Token: 0x060000DA RID: 218 RVA: 0x00006BAC File Offset: 0x00004DAC
		public void ActivateGUI()
		{
			if (this.modeText == null)
			{
				this.modeText = this.GetTextForMode();
			}
			if (this.Gui == null && !GUIQuad.Initialized)
			{
				this.Gui = GUIQuad.Create();
			}
			if (this.Gui == null)
			{
				this.Gui = GUIQuad.Instance;
			}
			if (IMGUIQuad.IsVisble)
			{
				IMGUIQuad.IsVisble = true;
			}
		}

		// Token: 0x060000DB RID: 219 RVA: 0x000024BB File Offset: 0x000006BB
		private void Awake()
		{
		}

		// Token: 0x060000DC RID: 220
		protected abstract void ChangeToNextModel();

		// Token: 0x060000DD RID: 221
		protected abstract bool CheckActivateDirectMode();

		// Token: 0x060000DE RID: 222
		protected abstract bool CheckSwitchCommandTool();

		// Token: 0x060000DF RID: 223
		protected abstract bool CheckSwitchDirectMode();

		// Token: 0x060000E0 RID: 224
		protected abstract bool CheckSwitchIKTool();

		// Token: 0x060000E1 RID: 225 RVA: 0x00002C94 File Offset: 0x00000E94
		private void CommandOrIKToolEnable(bool enable)
		{
			if (!enable)
			{
				this.CommandToolEnable(false);
				this.IKToolEnable(false);
				return;
			}
			if (this.IsLevelForIKMode())
			{
				this.IKToolEnable(true);
				return;
			}
			this.CommandToolEnable(true);
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x00002CBF File Offset: 0x00000EBF
		private void CommandToolEnable(bool newMode)
		{
			if (this.commandTool != null)
			{
				this.commandTool.gameObject.SetActive(newMode);
				this.UpdateModeTextLabel();
			}
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x00006C1C File Offset: 0x00004E1C
		protected void CreatePointer()
		{
			if (this.pointer == null)
			{
				this.pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				this.pointer.name = "pointer";
				this.pointer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				this.pointer.transform.parent = base.transform;
				this.pointer.transform.localPosition = new Vector3(0f, -0.03f, 0.03f);
				this.pointer.GetComponent<SphereCollider>().radius = this.directTouchSensorRatio / this.pointer.transform.TransformVector(Vector3.one).x;
				SphereCollider component = base.GetComponent<SphereCollider>();
				if (component != null)
				{
					component.enabled = false;
				}
				this.pointerNormalMaterial = this.pointer.GetComponent<Renderer>().material;
				this.pointerIKMaterial = this.CreatePointerIKMaterial();
				this.UpdatePointerAndTextVisibility();
			}
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00006D24 File Offset: 0x00004F24
		private Material CreatePointerIKMaterial()
		{
			Shader colorZOrderShader = MaterialHelper.GetColorZOrderShader();
			Color white = Color.white;
			Material material = new Material(colorZOrderShader);
			material.SetColor("_Color", white);
			return material;
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x00002CE6 File Offset: 0x00000EE6
		private IEnumerator DelayedEnableDirectModeCo()
		{
			yield return new WaitForSeconds(0.2f);
			this.DirectModeEnable(true);
			yield break;
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x00006D50 File Offset: 0x00004F50
		protected void DirectModeEnable(bool newMode)
		{
			if (this.modeText == null)
			{
				this.modeText = this.GetTextForMode();
			}
			if (this.grabbingObject != null)
			{
				this.m_grabbingStartTime = 0f;
				this.grabbingObject = null;
			}
			this.OnDirectModeEnabledChanged(newMode);
			this.SaveLastDirectModeStatus(newMode);
			this.CreatePointer();
			if (!newMode)
			{
				this.Gui.Hide();
				this.lastCommandToolActive = this.IsCommandToolActive() || this.IsIKToolActive();
				this.CommandToolEnable(false);
				this.IKToolEnable(false);
				this.TurnOffDirectMode();
				if (this.pointer != null)
				{
					this.UpdatePointerAndTextVisibility();
					return;
				}
			}
			else
			{
				if (this.Gui == null)
				{
					this.ActivateGUI();
				}
				if (GameMain.Instance.CMSystem.OvrUseNewControllerType)
				{
					this.Gui.Hide();
				}
				else
				{
					this.Gui.Show();
				}
				this.CommandOrIKToolEnable(this.lastCommandToolActive);
				this.UpdateModeTextLabel();
				if (this.pointer != null)
				{
					this.UpdatePointerAndTextVisibility();
					return;
				}
			}
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x00002CF5 File Offset: 0x00000EF5
		private void FixedUpdate()
		{
			this.OnFixedUpdate();
		}

		// Token: 0x060000E8 RID: 232
		public abstract DeviceWrapper GetController();

		// Token: 0x060000E9 RID: 233 RVA: 0x00002CFD File Offset: 0x00000EFD
		public static bool GetDisableGrabWhenDirectMode()
		{
			return MenuToolBase.disableGrabWhenDirectMode;
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00002D04 File Offset: 0x00000F04
		public static bool GetDisableGrabWhenIKMode()
		{
			return MenuToolBase.disableGrabWhenIKMode;
		}

		// Token: 0x060000EB RID: 235
		protected abstract OvrGripCollider GetOvrGripCollider();

		// Token: 0x060000EC RID: 236 RVA: 0x00002D0B File Offset: 0x00000F0B
		public static bool GetPointerVisible()
		{
			return MenuToolBase.pointerVisible;
		}

		// Token: 0x060000ED RID: 237
		protected abstract Text GetTextForMode();

		// Token: 0x060000EE RID: 238 RVA: 0x00006E60 File Offset: 0x00005060
		private void IKToolEnable(bool newMode)
		{
			if (this.ikTool != null)
			{
				this.ikTool.EnableIKMode(newMode);
				this.inIKMode = newMode;
				this.UpdateModeTextLabel();
				this.SyncDefaultColliderStatus();
				if (this.pointer != null)
				{
					SphereCollider component = this.pointer.GetComponent<SphereCollider>();
					float x = this.pointer.transform.lossyScale.x;
					float num = this.IKToolSensorRatio / x;
					num = ((!newMode) ? (this.directTouchSensorRatio / x) : (this.IKToolSensorRatio / x));
					component.radius = num;
					if (newMode)
					{
						this.pointer.GetComponent<Renderer>().sharedMaterial = this.pointerIKMaterial;
						return;
					}
					this.pointer.GetComponent<Renderer>().sharedMaterial = this.pointerNormalMaterial;
				}
			}
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00002D12 File Offset: 0x00000F12
		protected bool IsCommandToolActive()
		{
			return !(this.commandTool == null) && this.commandTool.gameObject.activeSelf;
		}

		// Token: 0x060000F0 RID: 240
		public abstract bool IsDirectModeActive();

		// Token: 0x060000F1 RID: 241 RVA: 0x00002D34 File Offset: 0x00000F34
		public virtual bool IsGrabbing()
		{
			return this.grabbingObject != null;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00002821 File Offset: 0x00000A21
		public virtual bool IsDrawMode()
		{
			return false;
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00002D42 File Offset: 0x00000F42
		protected bool IsIKToolActive()
		{
			return !(this.ikTool == null) && this.inIKMode;
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00002D5A File Offset: 0x00000F5A
		public bool IsInNewMode()
		{
			return !(this.vrController == null) && MenuToolBase.f_m_CtrlBehNew.GetValue(this.vrController) == MenuToolBase.f_m_CtrlBehNow.GetValue(this.vrController);
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00002D8E File Offset: 0x00000F8E
		private bool IsLevelForIKMode()
		{
			return FirstPersonCameraHelper.IsIKModeEnabled(Application.loadedLevel);
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00002D9A File Offset: 0x00000F9A
		private void LateUpdate()
		{
			this.SyncDefaultColliderStatus();
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x000024BB File Offset: 0x000006BB
		private void OnDestroy()
		{
		}

		// Token: 0x060000F8 RID: 248
		protected abstract void OnDirectModeEnabledChanged(bool newMode);

		// Token: 0x060000F9 RID: 249 RVA: 0x000024BB File Offset: 0x000006BB
		private void OnDisable()
		{
		}

		// Token: 0x060000FA RID: 250 RVA: 0x000024BB File Offset: 0x000006BB
		private void OnEnable()
		{
		}

		// Token: 0x060000FB RID: 251 RVA: 0x000024BB File Offset: 0x000006BB
		protected virtual void OnFixedUpdate()
		{
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00006F24 File Offset: 0x00005124
		private void OnLevelWasLoaded(int level)
		{
			if (this.pointer != null)
			{
				global::UnityEngine.Object.Destroy(this.pointer);
				this.grabColliderConflictAvoided = false;
				this.pointer = null;
				this.CreatePointer();
			}
			if (this.Gui != null && this.controller != null)
			{
				this.DirectModeEnable(this.IsDirectModeActive());
			}
		}

		// Token: 0x060000FD RID: 253 RVA: 0x000024BB File Offset: 0x000006BB
		private void OnTriggerEnter(Collider collider)
		{
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00002DA2 File Offset: 0x00000FA2
		private void OnTriggerExit(Collider collider)
		{
			if (this.screenGrabbed && collider.GetComponent<MoveableGUIObject>() != null && collider == this.lastGrabbedCollider)
			{
				this.m_pointerColor = Color.white;
				this.screenGrabbed = false;
				this.lastGrabbedCollider = null;
			}
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00006F88 File Offset: 0x00005188
		private void OnTriggerStay(Collider collider)
		{
			if (!this.IsDirectModeActive())
			{
				this.screenGrabbed = false;
				this.lastGrabbedCollider = null;
				return;
			}
			if (collider.GetComponent<GUIQuad>() != null)
			{
				if (GameMain.Instance.OvrMgr.OvrCamera.m_goOvrUiScreen.active)
				{
					Vector3 vector = this.Gui.get_renderer().bounds.ClosestPoint((this.pointer ? this.pointer.transform : base.transform).position);
					Vector3 vector2 = this.Gui.transform.InverseTransformPoint(vector);
					if (this.Gui.transform.InverseTransformPoint(VRContextUtil.FindOVRHeadTransform().position).z > 0f)
					{
						vector2.x *= -1f;
					}
					float num = (vector2.x + 0.5f) * 1280f;
					float num2 = (vector2.y + 0.5f) * 720f;
					UICameraUtil.SetOvrVirtualMousePos(new Vector3(num, num2, 1f));
					this.screenGrabbed = true;
					this.lastGrabbedCollider = collider;
				}
			}
			else if (collider.GetComponent<MoveableGUIObject>() != null && !this.screenGrabbed)
			{
				this.screenGrabbed = true;
				this.lastGrabbedCollider = collider;
			}
			IMGUIQuad component = collider.GetComponent<IMGUIQuad>();
			if (component != null)
			{
				component.MoveCursorTo(this.pointer ? this.pointer.transform : base.transform);
			}
			if (this.screenGrabbed && this.lastGrabbedCollider != null && this.pointer != null)
			{
				this.m_pointerColor = Color.red;
			}
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00007134 File Offset: 0x00005334
		private void OnUpdate()
		{
			this.PreUpdate();
			if (this.controller == null)
			{
				Debug.Log("MenuTool: controller is null");
				this.controller = this.GetController();
			}
			if (this.Gui == null && GUIQuad.Initialized)
			{
				this.Gui = GUIQuad.Instance;
			}
			if (this.ovrGripCollider == null)
			{
				this.ovrGripCollider = this.GetOvrGripCollider();
			}
			if (this.pointer != null && !this.grabColliderConflictAvoided && this.ovrGripCollider != null && this.ovrGripCollider.enabled)
			{
				Physics.IgnoreCollision(this.pointer.GetComponent<SphereCollider>(), this.ovrGripCollider.gameObject.GetComponent<Collider>(), true);
				this.grabColliderConflictAvoided = true;
			}
			if (this.modeText == null)
			{
				this.modeText = this.GetTextForMode();
			}
			if (this.controller != null && this.needActivateDirectMode)
			{
				this.needActivateDirectMode = false;
				base.StartCoroutine(this.DelayedEnableDirectModeCo());
				return;
			}
			if (this.Gui == null && this.controller != null && this.CheckActivateDirectMode())
			{
				this.DirectModeEnable(true);
			}
			if (this.pointer != null && MenuToolBase.needUpdatePointerVisible)
			{
				this.UpdatePointerAndTextVisibility();
				MenuToolBase.needUpdatePointerVisible = false;
			}
			if (this.pointer != null && this.m_pointerColor != Color.black)
			{
				this.pointer.GetComponent<MeshRenderer>().material.color = this.m_pointerColor;
				this.m_pointerColor = Color.black;
			}
			if (this.Gui != null && this.controller != null)
			{
				if (this.CheckActivateDirectMode())
				{
					this.resetGUIPosition();
				}
				if (!this.IsDirectModeActive())
				{
					if (this.CheckSwitchDirectMode())
					{
						this.DirectModeEnable(true);
					}
				}
				else if (this.IsCommandToolActive() && this.CheckSwitchCommandTool())
				{
					this.CommandToolEnable(false);
				}
				else if (!this.IsLevelForIKMode() && this.CheckSwitchCommandTool())
				{
					this.IKToolEnable(false);
					this.CommandToolEnable(true);
				}
				else if (this.IsIKToolActive() && this.CheckSwitchIKTool())
				{
					this.IKToolEnable(false);
				}
				else if (this.IsLevelForIKMode() && this.CheckSwitchIKTool())
				{
					this.CommandToolEnable(false);
					this.IKToolEnable(true);
				}
				else if (this.CheckSwitchDirectMode())
				{
					this.DirectModeEnable(false);
				}
				bool pressDown = this.controller.GetPressDown(this.grabScreenButton);
				bool press = this.controller.GetPress(this.grabScreenButton);
				bool pressUp = this.controller.GetPressUp(this.grabScreenButton);
				if (this.screenGrabbed && this.lastGrabbedCollider != null)
				{
					GameObject gameObject = this.lastGrabbedCollider.gameObject;
					if (!gameObject.activeInHierarchy || !this.lastGrabbedCollider.enabled)
					{
						this.m_pointerColor = Color.white;
						this.screenGrabbed = false;
						this.lastGrabbedCollider = null;
					}
					else if (pressDown && this.grabHandle != null)
					{
						this.grabbingObject = gameObject;
						this.grabHandle.transform.position = gameObject.transform.position;
						this.grabHandle.transform.rotation = gameObject.transform.rotation;
						this.m_grabbingStartTime = Time.time;
					}
				}
				if (press && this.grabbingObject != null)
				{
					this.grabbingObject.transform.position = this.grabHandle.transform.position;
					this.grabbingObject.transform.rotation = this.grabHandle.transform.rotation;
					if (this.grabbingObject.GetComponent<MoveableGUIObject>() != null)
					{
						this.grabbingObject.GetComponent<MoveableGUIObject>().OnMoved();
					}
					if (this.grabScreenButton == EVRButtonId.k_EButton_Axis1 && this.ovrGripCollider != null && this.ovrGripCollider.enabled && this.ovrGripCollider.grip && Time.time - this.m_grabbingStartTime > 0.1f)
					{
						this.ovrGripCollider.ResetGrip();
					}
				}
				if (this.screenGrabbed && (this.grabbingObject != null && pressUp))
				{
					this.m_grabbingStartTime = 0f;
					this.grabbingObject = null;
				}
				if (!this.screenGrabbed && this.IsIKToolActive() && this.controller.GetPressUp(EVRButtonId.k_EButton_Axis0))
				{
					if (!this.moveToHeadEnabled || this.moveToHeadButton != EVRButtonId.k_EButton_Axis0 || !this.controller.GetPress(this.moveSelfButton))
					{
						this.ikTool.ToggleMarkerVisibility();
					}
					this.pressDownTime = 0f;
				}
				if (press && this.IsDirectModeActive() && this.controller.GetPressUp(EVRButtonId.k_EButton_Axis0))
				{
					this.ChangeToNextModel();
				}
				this.tabletMouseMove();
			}
			if (!this.IsDirectModeActive())
			{
				return;
			}
			if (this.controller == null)
			{
				return;
			}
			this.PostUpdate();
		}

		// Token: 0x06000101 RID: 257 RVA: 0x000024BB File Offset: 0x000006BB
		protected virtual void PostStart()
		{
		}

		// Token: 0x06000102 RID: 258 RVA: 0x000024BB File Offset: 0x000006BB
		protected virtual void PostUpdate()
		{
		}

		// Token: 0x06000103 RID: 259
		protected abstract void PreUpdate();

		// Token: 0x06000104 RID: 260 RVA: 0x00007620 File Offset: 0x00005820
		private void resetGUIPosition()
		{
			this.Gui.ResetGUIPosition();
			if (this.IsInNewMode())
			{
				GameMain.Instance.OvrMgr.OvrCamera.ShowUI(true);
				GameMain.Instance.OvrMgr.OvrCamera.UIPosReset(0f);
			}
		}

		// Token: 0x06000105 RID: 261
		protected abstract void RestoreLastDirectModeStatus();

		// Token: 0x06000106 RID: 262
		protected abstract void SaveLastDirectModeStatus(bool mode);

		// Token: 0x06000107 RID: 263 RVA: 0x00002DE1 File Offset: 0x00000FE1
		public static void SetDisableGrabWhenDirectMode(bool value)
		{
			MenuToolBase.disableGrabWhenDirectMode = value;
			Settings.Instance.SetBoolValue("DirectModeDisableGrab", value);
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00002DF9 File Offset: 0x00000FF9
		public static void SetDisableGrabWhenIKMode(bool value)
		{
			MenuToolBase.disableGrabWhenIKMode = value;
			Settings.Instance.SetBoolValue("IKToolDisableGrab", value);
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00002E11 File Offset: 0x00001011
		public void SetPointerAndTextVisibilityByModel(bool visible)
		{
			this.pointerAndTextVisibilityByModel = visible;
			this.UpdatePointerAndTextVisibility();
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00002E20 File Offset: 0x00001020
		public static void SetPointerVisible(bool visible)
		{
			if (MenuToolBase.pointerVisible != visible)
			{
				MenuToolBase.pointerVisible = visible;
				Settings.Instance.SetBoolValue("PointerVisible", visible);
				MenuToolBase.needUpdatePointerVisible = true;
			}
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00007670 File Offset: 0x00005870
		private void Start()
		{
			this.grabScreenButton = Settings.Instance.GetButton("MoveScreenButton", EVRButtonId.k_EButton_Axis1);
			this.activateOrResetSec = Settings.Instance.GetFloatValue("ActivateOrResetSeconds", 1.5f);
			this.mouseCursorSpeed = Settings.Instance.GetFloatValue("MouseCursorSpeed", 0.1f);
			this.yotogiToolButton = Settings.Instance.GetButton("YotogiCommandToolButton", EVRButtonId.k_EButton_Grip);
			this.directTouchSensorRatio = Settings.Instance.GetFloatValue("DirectTouchSensorRatio", 0.1f);
			this.IKToolSensorRatio = Settings.Instance.GetFloatValue("IKTouchSensorRatio", 0.01f);
			MenuToolBase.pointerVisible = Settings.Instance.GetBoolValue("PointerVisible", true);
			if (Settings.Instance.GetBoolValue("YotogiCommandToolEnabled", true))
			{
				if (this.commandTool == null)
				{
					this.commandTool = YotogiCommandTool.Create(base.transform);
				}
				this.commandTool.gameObject.SetActive(false);
			}
			this.moveSelfButton = Settings.Instance.GetButton("MoveSelfButton", EVRButtonId.k_EButton_Grip);
			this.moveToHeadEnabled = Settings.Instance.GetBoolValue("MoveToHeadEnabled", true);
			this.moveToHeadButton = Settings.Instance.GetButton("MoveToHeadButton", EVRButtonId.k_EButton_Axis0);
			this.grabHandle = new GameObject("_grab_handle_");
			this.grabHandle.transform.parent = base.transform;
			this.IKToolButton = Settings.Instance.GetButton("IKToolButton", EVRButtonId.k_EButton_Grip);
			if (Settings.Instance.GetBoolValue("IKToolEnabled", true))
			{
				if (this.ikTool == null)
				{
					this.ikTool = IKTool.Create();
				}
				this.ikTool.EnableIKMode(false);
			}
			if (base.GetComponent<Rigidbody>() == null)
			{
				base.gameObject.AddComponent<Rigidbody>().isKinematic = true;
			}
			this.RestoreLastDirectModeStatus();
			this.controller = this.GetController();
			this.vrController = base.GetComponent<AVRController>();
			if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.HTC)
			{
				this.m_ctrlBeh = base.GetComponent<ViveControllerBehavior2>();
			}
			else if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.Oculus)
			{
				this.m_ctrlBeh = base.GetComponent<OvrControllerBehavior2>();
			}
			this.PostStart();
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00007898 File Offset: 0x00005A98
		private void SyncDefaultColliderStatus()
		{
			SphereCollider component = base.GetComponent<SphereCollider>();
			if (component != null)
			{
				component.enabled = !this.IsIKToolActive();
			}
			if (this.IsDirectModeActive() && this.ovrGripCollider != null)
			{
				this.ovrGripCollider.enabled = true;
				if (MenuToolBase.disableGrabWhenDirectMode)
				{
					this.ovrGripCollider.enabled = false;
				}
				if (MenuToolBase.disableGrabWhenIKMode && this.IsIKToolActive())
				{
					this.ovrGripCollider.enabled = false;
				}
			}
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00002E46 File Offset: 0x00001046
		private void ToggleCommandTool()
		{
			this.CommandToolEnable(!this.IsCommandToolActive());
		}

		// Token: 0x0600010E RID: 270
		protected abstract void TurnOffDirectMode();

		// Token: 0x0600010F RID: 271
		protected abstract void TurnOnDirectMode();

		// Token: 0x06000110 RID: 272 RVA: 0x00007914 File Offset: 0x00005B14
		private void Update()
		{
			try
			{
				this.OnUpdate();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00007948 File Offset: 0x00005B48
		private void UpdateModeTextLabel()
		{
			if (this.modeText != null && this.IsDirectModeActive())
			{
				if (this.IsCommandToolActive())
				{
					this.modeText.text = "DIRECT(com)";
					return;
				}
				if (this.IsIKToolActive())
				{
					this.modeText.text = "DIRECT(IK)";
					return;
				}
				this.modeText.text = "DIRECT";
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x000079B0 File Offset: 0x00005BB0
		private void UpdatePointerAndTextVisibility()
		{
			if (this.pointer != null)
			{
				bool flag = this.pointerAndTextVisibilityByModel && this.IsDirectModeActive() && MenuToolBase.pointerVisible;
				this.pointer.GetComponent<Renderer>().enabled = flag;
			}
			if (this.modeText != null)
			{
				this.modeText.gameObject.SetActive(this.pointerAndTextVisibilityByModel);
			}
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00007A1C File Offset: 0x00005C1C
		public void ToggleNewOldUI()
		{
			GameMain.Instance.CMSystem.OvrUseNewControllerType = !GameMain.Instance.CMSystem.OvrUseNewControllerType;
			GameMain.Instance.OvrMgr.OvrCamera.ChangeControllerNew(GameMain.Instance.CMSystem.OvrUseNewControllerType);
			this.m_bTabletArea = false;
			this.DirectModeEnable(this.IsDirectModeActive());
			IMGUIQuad.IsAttach = IMGUIQuad.IsAttach;
			this.resetGUIPosition();
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00007A90 File Offset: 0x00005C90
		public bool IsOvrTabletActive()
		{
			return GameMain.Instance.OvrMgr.OvrCamera.OvrTablet && GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.isActiveAndEnabled && GameMain.Instance.OvrMgr.OvrCamera.IsUIShow;
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00007AEC File Offset: 0x00005CEC
		private void tabletMouseMove()
		{
			if (!this.IsDirectModeActive())
			{
				return;
			}
			if (!this.IsOvrTabletActive())
			{
				return;
			}
			bool bTabletArea = this.m_bTabletArea;
			float num = 0f;
			this.m_bTabletArea = this._iTabletsHitPointerArea(base.transform.position, ref num);
			if (this.m_bTabletArea && !bTabletArea)
			{
				Transform transform = GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.transform.Find("Screen");
				if (transform)
				{
					this.m_tabletScreenBounds = transform.GetComponent<MeshFilter>().mesh.bounds;
				}
			}
			if (this.m_bTabletArea)
			{
				this.m_pointerColor = Color.magenta;
				Vector3 vector = (this.pointer ? this.pointer.transform.position : base.transform.position);
				Vector3 vector2 = base.transform.rotation * Quaternion.Euler(45f, 0f, 0f) * Vector3.forward;
				Vector3 vector3 = vector - vector2 / 10f;
				Transform transform2 = GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.transform;
				Vector3 vector4 = this.crossPlaneAndLine(transform2.up, transform2.position, vector3, vector2);
				this.m_tabletDist = Math.Abs(transform2.InverseTransformPoint(vector).y);
				if (this.canTabletMove())
				{
					vector4 = transform2.InverseTransformPoint(vector4);
					if (transform2.InverseTransformPoint(VRContextUtil.FindOVRHeadTransform().position).y < 0f)
					{
						vector4.x *= -1f;
					}
					if (GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.IsRotDown)
					{
						vector4 *= -1f;
					}
					Vector2 vector5 = new Vector2(vector4.x, vector4.z);
					vector5.x /= this.m_tabletScreenBounds.size.x;
					vector5.y /= this.m_tabletScreenBounds.size.y;
					if (-3f < vector5.x && vector5.x < 3f && -3f < vector5.y && vector5.y < 3f)
					{
						if (vector5.x > 0.5f)
						{
							vector5.x = 0.5f;
						}
						else if (vector5.x < -0.5f)
						{
							vector5.x = -0.5f;
						}
						if (vector5.y > 0.5f)
						{
							vector5.y = 0.5f;
						}
						else if (vector5.y < -0.5f)
						{
							vector5.y = -0.5f;
						}
						vector5.x = (vector5.x + 0.5f) * 1280f;
						vector5.y = (vector5.y + 0.5f) * 720f;
						Vector2 vector6 = GameMain.Instance.OvrMgr.SystemUICamera.GetOvrVirtualMousePos(this.vrController.m_bHandL);
						Vector2 vector7 = vector5 - vector6;
						float num2 = 5f + num * 20f;
						if (vector7.sqrMagnitude > num2 * num2)
						{
							vector5 = vector6 + vector7 * GameMain.Instance.CMSystem.OvrViveCursorLaserEasing;
							UICameraUtil.SetOvrVirtualMousePos(vector5);
							this.getTabletCtrl();
						}
					}
				}
			}
			if (!this.m_bTabletArea && bTabletArea)
			{
				this.m_tabletDist = float.MaxValue;
				this.m_pointerColor = Color.white;
			}
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00007E90 File Offset: 0x00006090
		public bool _iTabletsHitPointerArea(Vector3 f_vWorldPoint, ref float rate)
		{
			OvrTablet ovrTablet = GameMain.Instance.OvrMgr.OvrCamera.OvrTablet;
			Vector3 vector = ovrTablet.transform.InverseTransformPoint(f_vWorldPoint);
			float num = ovrTablet.m_vHitArea.x * 0.5f * 3f;
			float num2 = ovrTablet.m_vHitArea.z * 0.5f * 6f;
			float num3 = ovrTablet.m_vHitArea.y * 0.5f * 3f;
			bool flag = -num <= vector.x && vector.x <= num && -num2 <= vector.z && vector.z <= num2 && -num3 <= vector.y && vector.y <= num3;
			if (flag)
			{
				float num4 = Math.Abs(vector.x) / num;
				float num5 = Math.Abs(vector.y) / num3;
				float num6 = Math.Abs(vector.z) / num2;
				rate = Math.Max(Math.Max(num4, num5), num6);
			}
			return flag;
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00007F88 File Offset: 0x00006188
		private Vector3 crossPlaneAndLine(Vector3 normal, Vector3 ptOnPlane, Vector3 origin, Vector3 direction)
		{
			float num = Vector3.Dot(normal, ptOnPlane);
			float num2 = Vector3.Dot(normal, origin);
			float num3 = Vector3.Dot(normal, direction);
			return origin + (num - num2) / num3 * direction;
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00007FC0 File Offset: 0x000061C0
		private bool canTabletMove()
		{
			MenuToolBase otherHandMTB = this.getOtherHandMTB();
			if (this.IsMovingSelf)
			{
				return false;
			}
			if (otherHandMTB == null || otherHandMTB.m_ctrlBeh == null)
			{
				return true;
			}
			if (!otherHandMTB.isActiveAndEnabled)
			{
				return true;
			}
			if (otherHandMTB.m_ctrlBeh.isActiveAndEnabled && otherHandMTB.m_ctrlBeh.IsHandPenMode)
			{
				return false;
			}
			if (!otherHandMTB.IsDirectModeActive())
			{
				return true;
			}
			if (!this.m_bCtrlEnable)
			{
				if (!otherHandMTB.m_bCtrlEnable)
				{
					return true;
				}
				if (this.m_tabletDist < 0.15f && otherHandMTB.m_tabletDist > 0.27f)
				{
					return true;
				}
			}
			return this.m_bCtrlEnable;
		}

		// Token: 0x06000119 RID: 281 RVA: 0x0000805C File Offset: 0x0000625C
		private bool getTabletCtrl()
		{
			MenuToolBase otherHandMTB = this.getOtherHandMTB();
			this.m_bCtrlEnable = true;
			if (otherHandMTB)
			{
				otherHandMTB.m_bCtrlEnable = false;
			}
			return this.m_bCtrlEnable;
		}

		// Token: 0x0600011A RID: 282 RVA: 0x0000808C File Offset: 0x0000628C
		public bool IsOvrTabletCtrlOutOfRange()
		{
			if (this.IsOvrTabletActive())
			{
				MenuToolBase otherHandMTB = this.getOtherHandMTB();
				if (this.m_bCtrlEnable)
				{
					return !this.m_bTabletArea;
				}
				if (otherHandMTB && otherHandMTB.m_bCtrlEnable)
				{
					return !otherHandMTB.m_bTabletArea;
				}
			}
			return true;
		}

		// Token: 0x0600011B RID: 283 RVA: 0x000080D8 File Offset: 0x000062D8
		private MenuToolBase getOtherHandMTB()
		{
			if (this.m_otherMenuTool == null && this.vrController != null)
			{
				GameObject gameObject = (this.vrController.m_bHandL ? GameMain.Instance.OvrMgr.ovr_obj.right_controller.controller.gameObject : GameMain.Instance.OvrMgr.ovr_obj.left_controller.controller.gameObject);
				if (gameObject)
				{
					this.m_otherMenuTool = gameObject.GetComponent<MenuToolBase>();
				}
			}
			return this.m_otherMenuTool;
		}

		// Token: 0x04000092 RID: 146
		protected DeviceWrapper controller;

		// Token: 0x04000093 RID: 147
		protected OvrGripCollider ovrGripCollider;

		// Token: 0x04000094 RID: 148
		protected Text modeText;

		// Token: 0x04000095 RID: 149
		protected GUIQuad Gui;

		// Token: 0x04000096 RID: 150
		protected float activateOrResetSec = 1.5f;

		// Token: 0x04000097 RID: 151
		protected EVRButtonId grabScreenButton = EVRButtonId.k_EButton_Axis1;

		// Token: 0x04000098 RID: 152
		protected GameObject pointer;

		// Token: 0x04000099 RID: 153
		protected bool screenGrabbed;

		// Token: 0x0400009A RID: 154
		protected float directTouchSensorRatio = 3f;

		// Token: 0x0400009B RID: 155
		protected bool pressLeft;

		// Token: 0x0400009C RID: 156
		protected float pressDownTime;

		// Token: 0x0400009D RID: 157
		protected Vector3 cursorPosVR;

		// Token: 0x0400009E RID: 158
		protected float mouseCursorSpeed = 0.1f;

		// Token: 0x0400009F RID: 159
		protected EVRButtonId yotogiToolButton = EVRButtonId.k_EButton_Grip;

		// Token: 0x040000A0 RID: 160
		protected bool lastCommandToolActive;

		// Token: 0x040000A1 RID: 161
		protected YotogiCommandTool commandTool;

		// Token: 0x040000A2 RID: 162
		protected EVRButtonId IKToolButton = EVRButtonId.k_EButton_Grip;

		// Token: 0x040000A3 RID: 163
		protected IKTool ikTool;

		// Token: 0x040000A4 RID: 164
		protected float IKToolSensorRatio = 0.01f;

		// Token: 0x040000A5 RID: 165
		protected EVRButtonId moveSelfButton;

		// Token: 0x040000A6 RID: 166
		protected bool moveToHeadEnabled = true;

		// Token: 0x040000A7 RID: 167
		protected EVRButtonId moveToHeadButton;

		// Token: 0x040000A8 RID: 168
		private Collider lastGrabbedCollider;

		// Token: 0x040000A9 RID: 169
		private GameObject grabHandle;

		// Token: 0x040000AA RID: 170
		private GameObject grabbingObject;

		// Token: 0x040000AB RID: 171
		private Material pointerNormalMaterial;

		// Token: 0x040000AC RID: 172
		private Material pointerIKMaterial;

		// Token: 0x040000AD RID: 173
		private bool inIKMode;

		// Token: 0x040000AE RID: 174
		protected bool needActivateDirectMode;

		// Token: 0x040000AF RID: 175
		private static bool disableGrabWhenIKMode = Settings.Instance.GetBoolValue("IKToolDisableGrab", false);

		// Token: 0x040000B0 RID: 176
		private static bool disableGrabWhenDirectMode = Settings.Instance.GetBoolValue("DirectModeDisableGrab", false);

		// Token: 0x040000B1 RID: 177
		private bool pointerAndTextVisibilityByModel = true;

		// Token: 0x040000B2 RID: 178
		private static bool pointerVisible = true;

		// Token: 0x040000B3 RID: 179
		private static bool needUpdatePointerVisible = false;

		// Token: 0x040000B4 RID: 180
		private bool grabColliderConflictAvoided;

		// Token: 0x040000B5 RID: 181
		protected AVRController vrController;

		// Token: 0x040000B6 RID: 182
		private static FieldInfo f_m_CtrlBehNow = typeof(AVRController).GetField("m_CtrlBehNow", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x040000B7 RID: 183
		private static FieldInfo f_m_CtrlBehNew = typeof(AVRController).GetField("m_CtrlBehNew", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x040000B8 RID: 184
		private static FieldInfo f_m_CtrlBehOld = typeof(AVRController).GetField("m_CtrlBehOld", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x040000B9 RID: 185
		private Color m_pointerColor = Color.black;

		// Token: 0x040000BA RID: 186
		private bool m_bTabletArea;

		// Token: 0x040000BB RID: 187
		private Bounds m_tabletScreenBounds;

		// Token: 0x040000BC RID: 188
		private MenuToolBase m_otherMenuTool;

		// Token: 0x040000BD RID: 189
		private AVRControllerBehavior m_ctrlBeh;

		// Token: 0x040000BE RID: 190
		private const float CHANGE_DIST = 0.15f;

		// Token: 0x040000BF RID: 191
		private float m_tabletDist = float.MaxValue;

		// Token: 0x040000C0 RID: 192
		private bool m_bCtrlEnable;

		// Token: 0x040000C1 RID: 193
		private float m_grabbingStartTime;

		// Token: 0x040000C2 RID: 194
		private const float OVR_GRIP_FORCE_RESET_TIME = 0.1f;
	}
}
