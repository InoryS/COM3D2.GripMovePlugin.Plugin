using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal abstract class MenuToolBase : MonoBehaviour
	{
		public bool IsMovingSelf { get; set; }

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

		private void Awake()
		{
		}

		protected abstract void ChangeToNextModel();

		protected abstract bool CheckActivateDirectMode();

		protected abstract bool CheckSwitchCommandTool();

		protected abstract bool CheckSwitchDirectMode();

		protected abstract bool CheckSwitchIKTool();

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

		private void CommandToolEnable(bool newMode)
		{
			if (this.commandTool != null)
			{
				this.commandTool.gameObject.SetActive(newMode);
				this.UpdateModeTextLabel();
			}
		}

		protected void CreatePointer()
		{
			if (this.pointer == null)
			{
				this.pointer = GameObject.CreatePrimitive(0);
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

		private Material CreatePointerIKMaterial()
		{
			Shader colorZOrderShader = MaterialHelper.GetColorZOrderShader();
			Color white = Color.white;
			Material material = new Material(colorZOrderShader);
			material.SetColor("_Color", white);
			return material;
		}

		private IEnumerator DelayedEnableDirectModeCo()
		{
			yield return new WaitForSeconds(0.2f);
			this.DirectModeEnable(true);
			yield break;
		}

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

		private void FixedUpdate()
		{
			this.OnFixedUpdate();
		}

		public abstract DeviceWrapper GetController();

		public static bool GetDisableGrabWhenDirectMode()
		{
			return MenuToolBase.disableGrabWhenDirectMode;
		}

		public static bool GetDisableGrabWhenIKMode()
		{
			return MenuToolBase.disableGrabWhenIKMode;
		}

		protected abstract OvrGripCollider GetOvrGripCollider();

		public static bool GetPointerVisible()
		{
			return MenuToolBase.pointerVisible;
		}

		protected abstract Text GetTextForMode();

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

		protected bool IsCommandToolActive()
		{
			return this.commandTool != null && this.commandTool.gameObject.activeSelf;
		}

		public abstract bool IsDirectModeActive();

		public virtual bool IsGrabbing()
		{
			return this.grabbingObject != null;
		}

		public virtual bool IsDrawMode()
		{
			return false;
		}

		protected bool IsIKToolActive()
		{
			return this.ikTool != null && this.inIKMode;
		}

		public bool IsInNewMode()
		{
			return this.vrController != null && MenuToolBase.f_m_CtrlBehNew.GetValue(this.vrController) == MenuToolBase.f_m_CtrlBehNow.GetValue(this.vrController);
		}

		private bool IsLevelForIKMode()
		{
			return FirstPersonCameraHelper.IsIKModeEnabled(SceneManager.GetActiveScene().buildIndex);
		}

		private void LateUpdate()
		{
			this.SyncDefaultColliderStatus();
		}

		private void OnDestroy()
		{
		}

		protected abstract void OnDirectModeEnabledChanged(bool newMode);

		private void OnDisable()
		{
		}

		private void OnEnable()
		{
		}

		protected virtual void OnFixedUpdate()
		{
		}

		private void OnLevelWasLoaded(int level)
		{
			if (this.pointer != null)
			{
				UnityEngine.Object.Destroy(this.pointer);
				this.grabColliderConflictAvoided = false;
				this.pointer = null;
				this.CreatePointer();
			}
			if (this.Gui != null && this.controller != null)
			{
				this.DirectModeEnable(this.IsDirectModeActive());
			}
		}

		private void OnTriggerEnter(Collider collider)
		{
		}

		private void OnTriggerExit(Collider collider)
		{
			if (this.screenGrabbed && collider.GetComponent<MoveableGUIObject>() != null && collider == this.lastGrabbedCollider)
			{
				this.m_pointerColor = Color.white;
				this.screenGrabbed = false;
				this.lastGrabbedCollider = null;
			}
		}

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
				if (GameMain.Instance.OvrMgr.OvrCamera.m_goOvrUiScreen.activeSelf)
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
					if (this.grabScreenButton == (EVRButtonId)33 && this.ovrGripCollider != null && this.ovrGripCollider.enabled && this.ovrGripCollider.grip && Time.time - this.m_grabbingStartTime > 0.1f)
					{
						this.ovrGripCollider.ResetGrip();
					}
				}
				if (this.screenGrabbed && (this.grabbingObject != null && pressUp))
				{
					this.m_grabbingStartTime = 0f;
					this.grabbingObject = null;
				}
				if (!this.screenGrabbed && this.IsIKToolActive() && this.controller.GetPressUp(32))
				{
					if (!this.moveToHeadEnabled || this.moveToHeadButton != (EVRButtonId)32 || !this.controller.GetPress(this.moveSelfButton))
					{
						this.ikTool.ToggleMarkerVisibility();
					}
					this.pressDownTime = 0f;
				}
				if (press && this.IsDirectModeActive() && this.controller.GetPressUp(32))
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

		protected virtual void PostStart()
		{
		}

		protected virtual void PostUpdate()
		{
		}

		protected abstract void PreUpdate();

		private void resetGUIPosition()
		{
			this.Gui.ResetGUIPosition();
			if (this.IsInNewMode())
			{
				GameMain.Instance.OvrMgr.OvrCamera.ShowUI(true);
				GameMain.Instance.OvrMgr.OvrCamera.UIPosReset(0f);
			}
		}

		protected abstract void RestoreLastDirectModeStatus();

		protected abstract void SaveLastDirectModeStatus(bool mode);

		public static void SetDisableGrabWhenDirectMode(bool value)
		{
			MenuToolBase.disableGrabWhenDirectMode = value;
			Settings.Instance.SetBoolValue("DirectModeDisableGrab", value);
		}

		public static void SetDisableGrabWhenIKMode(bool value)
		{
			MenuToolBase.disableGrabWhenIKMode = value;
			Settings.Instance.SetBoolValue("IKToolDisableGrab", value);
		}

		public void SetPointerAndTextVisibilityByModel(bool visible)
		{
			this.pointerAndTextVisibilityByModel = visible;
			this.UpdatePointerAndTextVisibility();
		}

		public static void SetPointerVisible(bool visible)
		{
			if (MenuToolBase.pointerVisible != visible)
			{
				MenuToolBase.pointerVisible = visible;
				Settings.Instance.SetBoolValue("PointerVisible", visible);
				MenuToolBase.needUpdatePointerVisible = true;
			}
		}

		private void Start()
		{
			this.grabScreenButton = Settings.Instance.GetButton("MoveScreenButton", (EVRButtonId)33);
			this.activateOrResetSec = Settings.Instance.GetFloatValue("ActivateOrResetSeconds", 1.5f);
			this.mouseCursorSpeed = Settings.Instance.GetFloatValue("MouseCursorSpeed", 0.1f);
			this.yotogiToolButton = Settings.Instance.GetButton("YotogiCommandToolButton", (EVRButtonId)2);
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
			this.moveSelfButton = Settings.Instance.GetButton("MoveSelfButton", (EVRButtonId)2);
			this.moveToHeadEnabled = Settings.Instance.GetBoolValue("MoveToHeadEnabled", true);
			this.moveToHeadButton = Settings.Instance.GetButton("MoveToHeadButton", (EVRButtonId)32);
			this.grabHandle = new GameObject("_grab_handle_");
			this.grabHandle.transform.parent = base.transform;
			this.IKToolButton = Settings.Instance.GetButton("IKToolButton", (EVRButtonId)2);
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
			if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)2)
			{
				this.m_ctrlBeh = base.GetComponent<ViveControllerBehavior2>();
			}
			else if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)1)
			{
				this.m_ctrlBeh = base.GetComponent<OvrControllerBehavior2>();
			}
			this.PostStart();
		}

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

		private void ToggleCommandTool()
		{
			this.CommandToolEnable(!this.IsCommandToolActive());
		}

		protected abstract void TurnOffDirectMode();

		protected abstract void TurnOnDirectMode();

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

		public void ToggleNewOldUI()
		{
			GameMain.Instance.CMSystem.OvrUseNewControllerType = !GameMain.Instance.CMSystem.OvrUseNewControllerType;
			GameMain.Instance.OvrMgr.OvrCamera.ChangeControllerNew(GameMain.Instance.CMSystem.OvrUseNewControllerType);
			this.m_bTabletArea = false;
			this.DirectModeEnable(this.IsDirectModeActive());
			IMGUIQuad.IsAttach = IMGUIQuad.IsAttach;
			this.resetGUIPosition();
		}

		public bool IsOvrTabletActive()
		{
			return GameMain.Instance.OvrMgr.OvrCamera.OvrTablet && GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.isActiveAndEnabled && GameMain.Instance.OvrMgr.OvrCamera.IsUIShow;
		}

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
					Vector2 vector5;
					vector5 = new(vector4.x, vector4.z);
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

		private Vector3 crossPlaneAndLine(Vector3 normal, Vector3 ptOnPlane, Vector3 origin, Vector3 direction)
		{
			float num = Vector3.Dot(normal, ptOnPlane);
			float num2 = Vector3.Dot(normal, origin);
			float num3 = Vector3.Dot(normal, direction);
			return origin + (num - num2) / num3 * direction;
		}

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

		protected DeviceWrapper controller;

		protected OvrGripCollider ovrGripCollider;

		protected Text modeText;

		protected GUIQuad Gui;

		protected float activateOrResetSec = 1.5f;

		protected EVRButtonId grabScreenButton = (EVRButtonId)33;

		protected GameObject pointer;

		protected bool screenGrabbed;

		protected float directTouchSensorRatio = 3f;

		protected bool pressLeft;

		protected float pressDownTime;

		protected Vector3 cursorPosVR;

		protected float mouseCursorSpeed = 0.1f;

		protected EVRButtonId yotogiToolButton = (EVRButtonId)2;

		protected bool lastCommandToolActive;

		protected YotogiCommandTool commandTool;

		protected EVRButtonId IKToolButton = (EVRButtonId)2;

		protected IKTool ikTool;

		protected float IKToolSensorRatio = 0.01f;

		protected EVRButtonId moveSelfButton;

		protected bool moveToHeadEnabled = true;

		protected EVRButtonId moveToHeadButton;

		private Collider lastGrabbedCollider;

		private GameObject grabHandle;

		private GameObject grabbingObject;

		private Material pointerNormalMaterial;

		private Material pointerIKMaterial;

		private bool inIKMode;

		protected bool needActivateDirectMode;

		private static bool disableGrabWhenIKMode = Settings.Instance.GetBoolValue("IKToolDisableGrab", false);

		private static bool disableGrabWhenDirectMode = Settings.Instance.GetBoolValue("DirectModeDisableGrab", false);

		private bool pointerAndTextVisibilityByModel = true;

		private static bool pointerVisible = true;

		private static bool needUpdatePointerVisible = false;

		private bool grabColliderConflictAvoided;

		protected AVRController vrController;

		private static FieldInfo f_m_CtrlBehNow = typeof(AVRController).GetField("m_CtrlBehNow", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		private static FieldInfo f_m_CtrlBehNew = typeof(AVRController).GetField("m_CtrlBehNew", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		private static FieldInfo f_m_CtrlBehOld = typeof(AVRController).GetField("m_CtrlBehOld", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

		private Color m_pointerColor = Color.black;

		private bool m_bTabletArea;

		private Bounds m_tabletScreenBounds;

		private MenuToolBase m_otherMenuTool;

		private AVRControllerBehavior m_ctrlBeh;

		private const float CHANGE_DIST = 0.15f;

		private float m_tabletDist = float.MaxValue;

		private bool m_bCtrlEnable;

		private float m_grabbingStartTime;

		private const float OVR_GRIP_FORCE_RESET_TIME = 0.1f;
	}
}
