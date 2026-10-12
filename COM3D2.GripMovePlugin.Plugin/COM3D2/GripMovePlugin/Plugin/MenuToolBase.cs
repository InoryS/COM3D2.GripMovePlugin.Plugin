using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if !COM3D25_UNITY_2022
using Valve.VR;
#endif

namespace COM3D2.GripMovePlugin.Plugin;

internal abstract class MenuToolBase : MonoBehaviour
{
	private const float CHANGE_DIST = 0.15f;

	private const float OVR_GRIP_FORCE_RESET_TIME = 0.1f;

	private static bool disableGrabWhenIKMode =
		Settings.Instance.GetBoolValue("IKToolDisableGrab", false);

	private static bool disableGrabWhenDirectMode =
		Settings.Instance.GetBoolValue("DirectModeDisableGrab", false);

	private static bool pointerVisible = true;

	private static bool needUpdatePointerVisible;

	private static readonly FieldInfo f_m_CtrlBehNow =
		typeof(AVRController).GetField("m_CtrlBehNow",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.GetField);

	private static readonly FieldInfo f_m_CtrlBehNew =
		typeof(AVRController).GetField("m_CtrlBehNew",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.GetField);

	private static FieldInfo f_m_CtrlBehOld = typeof(AVRController).GetField("m_CtrlBehOld",
		BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.GetField);

	protected GUIQuad Gui;

#if COM3D25_UNITY_2022
	protected AVRControllerButtons.BTN IKToolButton = AVRControllerButtons.BTN.GRIP;
#else
	protected EVRButtonId IKToolButton = (EVRButtonId)2;
#endif

	protected float IKToolSensorRatio = 0.01f;

	protected float activateOrResetSec = 1.5f;

	protected YotogiCommandTool commandTool;

	protected DeviceWrapper controller;

	protected Vector3 cursorPosVR;

	protected float directTouchSensorRatio = 3f;

	private bool grabColliderConflictAvoided;

	private GameObject grabHandle;

#if COM3D25_UNITY_2022
	protected AVRControllerButtons.BTN grabScreenButton = AVRControllerButtons.BTN.TRIGGER;
#else
	protected EVRButtonId grabScreenButton = (EVRButtonId)33;
#endif

	private GameObject grabbingObject;

	protected IKTool ikTool;

	private bool inIKMode;

	protected bool lastCommandToolActive;

	private Collider lastGrabbedCollider;

	private bool m_bCtrlEnable;

	private bool m_bTabletArea;

	private AVRControllerBehavior m_ctrlBeh;

	private float m_grabbingStartTime;

	private MenuToolBase m_otherMenuTool;

	private Color m_pointerColor = Color.black;

	private float m_tabletDist = float.MaxValue;

	private Bounds m_tabletScreenBounds;

	protected Text modeText;

	protected float mouseCursorSpeed = 0.1f;

#if COM3D25_UNITY_2022
	protected AVRControllerButtons.BTN moveSelfButton;

	protected AVRControllerButtons.BTN moveToHeadButton;
#else
	protected EVRButtonId moveSelfButton;

	protected EVRButtonId moveToHeadButton;
#endif

	protected bool moveToHeadEnabled = true;

	protected bool needActivateDirectMode;

	protected OvrGripCollider ovrGripCollider;

	protected GameObject pointer;

	private bool pointerAndTextVisibilityByModel = true;

	private Material pointerIKMaterial;

	private Material pointerNormalMaterial;

	protected float pressDownTime;

	protected bool pressLeft;

	protected bool screenGrabbed;

	protected AVRController vrController;

#if COM3D25_UNITY_2022
	protected AVRControllerButtons.BTN yotogiToolButton = AVRControllerButtons.BTN.GRIP;
#else
	protected EVRButtonId yotogiToolButton = (EVRButtonId)2;
#endif
	public bool IsMovingSelf { get; set; }

	private void Awake()
	{
	}

	private void Start()
	{
#if COM3D25_UNITY_2022
		grabScreenButton = Settings.Instance.GetButton("MoveScreenButton", AVRControllerButtons.BTN.TRIGGER);
#else
		grabScreenButton = Settings.Instance.GetButton("MoveScreenButton", (EVRButtonId)33);
#endif
		activateOrResetSec = Settings.Instance.GetFloatValue("ActivateOrResetSeconds", 1.5f);
		mouseCursorSpeed = Settings.Instance.GetFloatValue("MouseCursorSpeed", 0.1f);
#if COM3D25_UNITY_2022
		yotogiToolButton = Settings.Instance.GetButton("YotogiCommandToolButton", AVRControllerButtons.BTN.GRIP);
#else
		yotogiToolButton = Settings.Instance.GetButton("YotogiCommandToolButton", (EVRButtonId)2);
#endif
		directTouchSensorRatio = Settings.Instance.GetFloatValue("DirectTouchSensorRatio", 0.1f);
		IKToolSensorRatio = Settings.Instance.GetFloatValue("IKTouchSensorRatio", 0.01f);
		pointerVisible = Settings.Instance.GetBoolValue("PointerVisible", true);
		if (Settings.Instance.GetBoolValue("YotogiCommandToolEnabled", true))
		{
			if (commandTool == null)
			{
				commandTool = YotogiCommandTool.Create(transform);
			}

			commandTool.gameObject.SetActive(false);
		}

#if COM3D25_UNITY_2022
		moveSelfButton = Settings.Instance.GetButton("MoveSelfButton", AVRControllerButtons.BTN.GRIP);
#else
		moveSelfButton = Settings.Instance.GetButton("MoveSelfButton", (EVRButtonId)2);
#endif
		moveToHeadEnabled = Settings.Instance.GetBoolValue("MoveToHeadEnabled", true);
#if COM3D25_UNITY_2022
		moveToHeadButton = Settings.Instance.GetButton("MoveToHeadButton", AVRControllerButtons.BTN.VIRTUAL_L_CLICK);
#else
		moveToHeadButton = Settings.Instance.GetButton("MoveToHeadButton", (EVRButtonId)32);
#endif
		grabHandle = new GameObject("_grab_handle_");
		grabHandle.transform.parent = transform;
#if COM3D25_UNITY_2022
		IKToolButton = Settings.Instance.GetButton("IKToolButton", AVRControllerButtons.BTN.GRIP);
#else
		IKToolButton = Settings.Instance.GetButton("IKToolButton", (EVRButtonId)2);
#endif
		if (Settings.Instance.GetBoolValue("IKToolEnabled", true))
		{
			if (ikTool == null)
			{
				ikTool = IKTool.Create();
			}

			ikTool.EnableIKMode(false);
		}

		if (GetComponent<Rigidbody>() == null)
		{
			gameObject.AddComponent<Rigidbody>().isKinematic = true;
		}

		RestoreLastDirectModeStatus();
		controller = GetController();
		vrController = GetComponent<AVRController>();
		if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)2)
		{
			m_ctrlBeh = GetComponent<ViveControllerBehavior2>();
		}
		else if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)1)
		{
			m_ctrlBeh = GetComponent<OvrControllerBehavior2>();
		}

		PostStart();
	}

	private void Update()
	{
		try
		{
			OnUpdate();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private void FixedUpdate() => OnFixedUpdate();

	private void LateUpdate() => SyncDefaultColliderStatus();

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}

	private void OnLevelWasLoaded(int level)
	{
		if (pointer != null)
		{
			Destroy(pointer);
			grabColliderConflictAvoided = false;
			pointer = null;
			CreatePointer();
		}

		if (Gui != null && controller != null)
		{
			DirectModeEnable(IsDirectModeActive());
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
	}

	private void OnTriggerExit(Collider collider)
	{
		if (screenGrabbed && collider.GetComponent<MoveableGUIObject>() != null &&
		    collider == lastGrabbedCollider)
		{
			m_pointerColor = Color.white;
			screenGrabbed = false;
			lastGrabbedCollider = null;
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		if (!IsDirectModeActive())
		{
			screenGrabbed = false;
			lastGrabbedCollider = null;
			return;
		}

		if (collider.GetComponent<GUIQuad>() != null)
		{
			if (GameMain.Instance.OvrMgr.OvrCamera.m_goOvrUiScreen.activeSelf)
			{
				Vector3 vector = Gui.get_renderer().bounds
					.ClosestPoint((pointer ? pointer.transform : transform).position);
				Vector3 vector2 = Gui.transform.InverseTransformPoint(vector);
				if (Gui.transform
					    .InverseTransformPoint(VRContextUtil.FindOVRHeadTransform().position).z >
				    0f)
				{
					vector2.x *= -1f;
				}

				float num = (vector2.x + 0.5f) * 1280f;
				float num2 = (vector2.y + 0.5f) * 720f;
				UICameraUtil.SetOvrVirtualMousePos(new Vector3(num, num2, 1f));
				screenGrabbed = true;
				lastGrabbedCollider = collider;
			}
		}
		else if (collider.GetComponent<MoveableGUIObject>() != null && !screenGrabbed)
		{
			screenGrabbed = true;
			lastGrabbedCollider = collider;
		}

		IMGUIQuad component = collider.GetComponent<IMGUIQuad>();
		if (component != null)
		{
			component.MoveCursorTo(pointer ? pointer.transform : transform);
		}

		if (screenGrabbed && lastGrabbedCollider != null && pointer != null)
		{
			m_pointerColor = Color.red;
		}
	}

	public void ActivateGUI()
	{
		if (modeText == null)
		{
			modeText = GetTextForMode();
		}

		if (Gui == null && !GUIQuad.Initialized)
		{
			Gui = GUIQuad.Create();
		}

		if (Gui == null)
		{
			Gui = GUIQuad.Instance;
		}

		if (IMGUIQuad.IsVisble)
		{
			IMGUIQuad.IsVisble = true;
		}
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
			CommandToolEnable(false);
			IKToolEnable(false);
			return;
		}

		if (IsLevelForIKMode())
		{
			IKToolEnable(true);
			return;
		}

		CommandToolEnable(true);
	}

	private void CommandToolEnable(bool newMode)
	{
		if (commandTool != null)
		{
			commandTool.gameObject.SetActive(newMode);
			UpdateModeTextLabel();
		}
	}

	protected void CreatePointer()
	{
		if (pointer == null)
		{
			pointer = GameObject.CreatePrimitive(0);
			pointer.name = "pointer";
			pointer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			pointer.transform.parent = transform;
			pointer.transform.localPosition = new Vector3(0f, -0.03f, 0.03f);
			pointer.GetComponent<SphereCollider>().radius = directTouchSensorRatio /
			                                                pointer.transform
				                                                .TransformVector(Vector3.one).x;
			SphereCollider component = GetComponent<SphereCollider>();
			if (component != null)
			{
				component.enabled = false;
			}

			pointerNormalMaterial = pointer.GetComponent<Renderer>().material;
			pointerIKMaterial = CreatePointerIKMaterial();
			UpdatePointerAndTextVisibility();
		}
	}

	private Material CreatePointerIKMaterial()
	{
		Shader colorZOrderShader = MaterialHelper.GetColorZOrderShader();
		Color white = Color.white;
		var material = new Material(colorZOrderShader);
		material.SetColor("_Color", white);
		return material;
	}

	private IEnumerator DelayedEnableDirectModeCo()
	{
		yield return new WaitForSeconds(0.2f);
		DirectModeEnable(true);
		yield break;
	}

	protected void DirectModeEnable(bool newMode)
	{
		if (modeText == null)
		{
			modeText = GetTextForMode();
		}

		if (grabbingObject != null)
		{
			m_grabbingStartTime = 0f;
			grabbingObject = null;
		}

		OnDirectModeEnabledChanged(newMode);
		SaveLastDirectModeStatus(newMode);
		CreatePointer();
		if (!newMode)
		{
			Gui.Hide();
			lastCommandToolActive = IsCommandToolActive() || IsIKToolActive();
			CommandToolEnable(false);
			IKToolEnable(false);
			TurnOffDirectMode();
			if (pointer != null)
			{
				UpdatePointerAndTextVisibility();
			}
		}
		else
		{
			if (Gui == null)
			{
				ActivateGUI();
			}

			if (GameMain.Instance.CMSystem.OvrUseNewControllerType)
			{
				Gui.Hide();
			}
			else
			{
				Gui.Show();
			}

			CommandOrIKToolEnable(lastCommandToolActive);
			UpdateModeTextLabel();
			if (pointer != null)
			{
				UpdatePointerAndTextVisibility();
			}
		}
	}

	public abstract DeviceWrapper GetController();

	public static bool GetDisableGrabWhenDirectMode() => disableGrabWhenDirectMode;

	public static bool GetDisableGrabWhenIKMode() => disableGrabWhenIKMode;

	protected abstract OvrGripCollider GetOvrGripCollider();

	public static bool GetPointerVisible() => pointerVisible;

	protected abstract Text GetTextForMode();

	private void IKToolEnable(bool newMode)
	{
		if (ikTool != null)
		{
			ikTool.EnableIKMode(newMode);
			inIKMode = newMode;
			UpdateModeTextLabel();
			SyncDefaultColliderStatus();
			if (pointer != null)
			{
				SphereCollider component = pointer.GetComponent<SphereCollider>();
				float x = pointer.transform.lossyScale.x;
				float num = IKToolSensorRatio / x;
				num = !newMode ? directTouchSensorRatio / x : IKToolSensorRatio / x;
				component.radius = num;
				if (newMode)
				{
					pointer.GetComponent<Renderer>().sharedMaterial = pointerIKMaterial;
					return;
				}

				pointer.GetComponent<Renderer>().sharedMaterial = pointerNormalMaterial;
			}
		}
	}

	protected bool IsCommandToolActive() =>
		commandTool != null && commandTool.gameObject.activeSelf;

	public abstract bool IsDirectModeActive();

	public virtual bool IsGrabbing() => grabbingObject != null;

	public virtual bool IsDrawMode() => false;

	protected bool IsIKToolActive() => ikTool != null && inIKMode;

	public bool IsInNewMode() => vrController != null &&
	                             f_m_CtrlBehNew.GetValue(vrController) ==
	                             f_m_CtrlBehNow.GetValue(vrController);

	private bool IsLevelForIKMode() =>
		FirstPersonCameraHelper.IsIKModeEnabled(SceneManager.GetActiveScene().buildIndex);

	protected abstract void OnDirectModeEnabledChanged(bool newMode);

	protected virtual void OnFixedUpdate()
	{
	}

	private void OnUpdate()
	{
		PreUpdate();
		if (controller == null)
		{
			Debug.Log("MenuTool: controller is null");
			controller = GetController();
		}

		if (Gui == null && GUIQuad.Initialized)
		{
			Gui = GUIQuad.Instance;
		}

		if (ovrGripCollider == null)
		{
			ovrGripCollider = GetOvrGripCollider();
		}

		if (pointer != null && !grabColliderConflictAvoided && ovrGripCollider != null &&
		    ovrGripCollider.enabled)
		{
			Physics.IgnoreCollision(pointer.GetComponent<SphereCollider>(),
				ovrGripCollider.gameObject.GetComponent<Collider>(), true);
			grabColliderConflictAvoided = true;
		}

		if (modeText == null)
		{
			modeText = GetTextForMode();
		}

		if (controller != null && needActivateDirectMode)
		{
			needActivateDirectMode = false;
			StartCoroutine(DelayedEnableDirectModeCo());
			return;
		}

		if (Gui == null && controller != null && CheckActivateDirectMode())
		{
			DirectModeEnable(true);
		}

		if (pointer != null && needUpdatePointerVisible)
		{
			UpdatePointerAndTextVisibility();
			needUpdatePointerVisible = false;
		}

		if (pointer != null && m_pointerColor != Color.black)
		{
			pointer.GetComponent<MeshRenderer>().material.color = m_pointerColor;
			m_pointerColor = Color.black;
		}

		if (Gui != null && controller != null)
		{
			if (CheckActivateDirectMode())
			{
				resetGUIPosition();
			}

			if (!IsDirectModeActive())
			{
				if (CheckSwitchDirectMode())
				{
					DirectModeEnable(true);
				}
			}
			else if (IsCommandToolActive() && CheckSwitchCommandTool())
			{
				CommandToolEnable(false);
			}
			else if (!IsLevelForIKMode() && CheckSwitchCommandTool())
			{
				IKToolEnable(false);
				CommandToolEnable(true);
			}
			else if (IsIKToolActive() && CheckSwitchIKTool())
			{
				IKToolEnable(false);
			}
			else if (IsLevelForIKMode() && CheckSwitchIKTool())
			{
				CommandToolEnable(false);
				IKToolEnable(true);
			}
			else if (CheckSwitchDirectMode())
			{
				DirectModeEnable(false);
			}

			bool pressDown = controller.GetPressDown(grabScreenButton);
			bool press = controller.GetPress(grabScreenButton);
			bool pressUp = controller.GetPressUp(grabScreenButton);
			if (screenGrabbed && lastGrabbedCollider != null)
			{
				GameObject gameObject = lastGrabbedCollider.gameObject;
				if (!gameObject.activeInHierarchy || !lastGrabbedCollider.enabled)
				{
					m_pointerColor = Color.white;
					screenGrabbed = false;
					lastGrabbedCollider = null;
				}
				else if (pressDown && grabHandle != null)
				{
					grabbingObject = gameObject;
					grabHandle.transform.position = gameObject.transform.position;
					grabHandle.transform.rotation = gameObject.transform.rotation;
					m_grabbingStartTime = Time.time;
				}
			}

			if (press && grabbingObject != null)
			{
				grabbingObject.transform.position = grabHandle.transform.position;
				grabbingObject.transform.rotation = grabHandle.transform.rotation;
				if (grabbingObject.GetComponent<MoveableGUIObject>() != null)
				{
					grabbingObject.GetComponent<MoveableGUIObject>().OnMoved();
				}

#if COM3D25_UNITY_2022
				if (grabScreenButton == AVRControllerButtons.BTN.TRIGGER && ovrGripCollider != null &&
#else
				if (grabScreenButton == (EVRButtonId)33 && ovrGripCollider != null &&
#endif
				    ovrGripCollider.enabled && ovrGripCollider.grip &&
				    Time.time - m_grabbingStartTime > 0.1f)
				{
					ovrGripCollider.ResetGrip();
				}
			}

			if (screenGrabbed && grabbingObject != null && pressUp)
			{
				m_grabbingStartTime = 0f;
				grabbingObject = null;
			}

			if (!screenGrabbed && IsIKToolActive() && controller.GetPressUp(
#if COM3D25_UNITY_2022
				AVRControllerButtons.BTN.VIRTUAL_L_CLICK
#else
				32
#endif
			))
			{
				if (!moveToHeadEnabled || moveToHeadButton !=
#if COM3D25_UNITY_2022
					AVRControllerButtons.BTN.VIRTUAL_L_CLICK
#else
					(EVRButtonId)32
#endif
				||
				    !controller.GetPress(moveSelfButton))
				{
					ikTool.ToggleMarkerVisibility();
				}

				pressDownTime = 0f;
			}

			if (press && IsDirectModeActive() && controller.GetPressUp(
#if COM3D25_UNITY_2022
				AVRControllerButtons.BTN.VIRTUAL_L_CLICK
#else
				32
#endif
			))
			{
				ChangeToNextModel();
			}

			tabletMouseMove();
		}

		if (!IsDirectModeActive())
		{
			return;
		}

		if (controller == null)
		{
			return;
		}

		PostUpdate();
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
		Gui.ResetGUIPosition();
		if (IsInNewMode())
		{
			GameMain.Instance.OvrMgr.OvrCamera.ShowUI(true);
			GameMain.Instance.OvrMgr.OvrCamera.UIPosReset();
		}
	}

	protected abstract void RestoreLastDirectModeStatus();

	protected abstract void SaveLastDirectModeStatus(bool mode);

	public static void SetDisableGrabWhenDirectMode(bool value)
	{
		disableGrabWhenDirectMode = value;
		Settings.Instance.SetBoolValue("DirectModeDisableGrab", value);
	}

	public static void SetDisableGrabWhenIKMode(bool value)
	{
		disableGrabWhenIKMode = value;
		Settings.Instance.SetBoolValue("IKToolDisableGrab", value);
	}

	public void SetPointerAndTextVisibilityByModel(bool visible)
	{
		pointerAndTextVisibilityByModel = visible;
		UpdatePointerAndTextVisibility();
	}

	public static void SetPointerVisible(bool visible)
	{
		if (pointerVisible != visible)
		{
			pointerVisible = visible;
			Settings.Instance.SetBoolValue("PointerVisible", visible);
			needUpdatePointerVisible = true;
		}
	}

	private void SyncDefaultColliderStatus()
	{
		SphereCollider component = GetComponent<SphereCollider>();
		if (component != null)
		{
			component.enabled = !IsIKToolActive();
		}

		if (IsDirectModeActive() && ovrGripCollider != null)
		{
			ovrGripCollider.enabled = true;
			if (disableGrabWhenDirectMode)
			{
				ovrGripCollider.enabled = false;
			}

			if (disableGrabWhenIKMode && IsIKToolActive())
			{
				ovrGripCollider.enabled = false;
			}
		}
	}

	private void ToggleCommandTool() => CommandToolEnable(!IsCommandToolActive());

	protected abstract void TurnOffDirectMode();

	protected abstract void TurnOnDirectMode();

	private void UpdateModeTextLabel()
	{
		if (modeText != null && IsDirectModeActive())
		{
			if (IsCommandToolActive())
			{
				modeText.text = "DIRECT(com)";
				return;
			}

			if (IsIKToolActive())
			{
				modeText.text = "DIRECT(IK)";
				return;
			}

			modeText.text = "DIRECT";
		}
	}

	private void UpdatePointerAndTextVisibility()
	{
		if (pointer != null)
		{
			bool flag = pointerAndTextVisibilityByModel && IsDirectModeActive() && pointerVisible;
			pointer.GetComponent<Renderer>().enabled = flag;
		}

		if (modeText != null)
		{
			modeText.gameObject.SetActive(pointerAndTextVisibilityByModel);
		}
	}

	public void ToggleNewOldUI()
	{
		GameMain.Instance.CMSystem.OvrUseNewControllerType =
			!GameMain.Instance.CMSystem.OvrUseNewControllerType;
		GameMain.Instance.OvrMgr.OvrCamera.ChangeControllerNew(GameMain.Instance.CMSystem
			.OvrUseNewControllerType);
		m_bTabletArea = false;
		DirectModeEnable(IsDirectModeActive());
		IMGUIQuad.IsAttach = IMGUIQuad.IsAttach;
		resetGUIPosition();
	}

	public bool IsOvrTabletActive() => GameMain.Instance.OvrMgr.OvrCamera.OvrTablet &&
	                                   GameMain.Instance.OvrMgr.OvrCamera.OvrTablet
		                                   .isActiveAndEnabled && GameMain.Instance.OvrMgr.OvrCamera
		                                   .IsUIShow;

	private void tabletMouseMove()
	{
		if (!IsDirectModeActive())
		{
			return;
		}

		if (!IsOvrTabletActive())
		{
			return;
		}

		bool bTabletArea = m_bTabletArea;
		float num = 0f;
		m_bTabletArea = _iTabletsHitPointerArea(this.transform.position, ref num);
		if (m_bTabletArea && !bTabletArea)
		{
			Transform transform =
				GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.transform.Find("Screen");
			if (transform)
			{
				m_tabletScreenBounds = transform.GetComponent<MeshFilter>().mesh.bounds;
			}
		}

		if (m_bTabletArea)
		{
			m_pointerColor = Color.magenta;
			Vector3 vector = pointer ? pointer.transform.position : transform.position;
			Vector3 vector2 = transform.rotation * Quaternion.Euler(45f, 0f, 0f) * Vector3.forward;
			Vector3 vector3 = vector - vector2 / 10f;
			Transform transform2 = GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.transform;
			Vector3 vector4 =
				crossPlaneAndLine(transform2.up, transform2.position, vector3, vector2);
			m_tabletDist = Math.Abs(transform2.InverseTransformPoint(vector).y);
			if (canTabletMove())
			{
				vector4 = transform2.InverseTransformPoint(vector4);
				if (transform2.InverseTransformPoint(VRContextUtil.FindOVRHeadTransform().position)
					    .y < 0f)
				{
					vector4.x *= -1f;
				}

				if (GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.IsRotDown)
				{
					vector4 *= -1f;
				}

				Vector2 vector5;
				vector5 = new Vector2(vector4.x, vector4.z);
				vector5.x /= m_tabletScreenBounds.size.x;
				vector5.y /= m_tabletScreenBounds.size.y;
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
					Vector2 vector6 =
						GameMain.Instance.OvrMgr.SystemUICamera.GetOvrVirtualMousePos(vrController
							.m_bHandL);
					Vector2 vector7 = vector5 - vector6;
					float num2 = 5f + num * 20f;
					if (vector7.sqrMagnitude > num2 * num2)
					{
						vector5 = vector6 +
						          vector7 * GameMain.Instance.CMSystem.OvrViveCursorLaserEasing;
						UICameraUtil.SetOvrVirtualMousePos(vector5);
						getTabletCtrl();
					}
				}
			}
		}

		if (!m_bTabletArea && bTabletArea)
		{
			m_tabletDist = float.MaxValue;
			m_pointerColor = Color.white;
		}
	}

	public bool _iTabletsHitPointerArea(Vector3 f_vWorldPoint, ref float rate)
	{
		OvrTablet ovrTablet = GameMain.Instance.OvrMgr.OvrCamera.OvrTablet;
		Vector3 vector = ovrTablet.transform.InverseTransformPoint(f_vWorldPoint);
		float num = ovrTablet.m_vHitArea.x * 0.5f * 3f;
		float num2 = ovrTablet.m_vHitArea.z * 0.5f * 6f;
		float num3 = ovrTablet.m_vHitArea.y * 0.5f * 3f;
		bool flag = -num <= vector.x && vector.x <= num && -num2 <= vector.z && vector.z <= num2 &&
		            -num3 <= vector.y && vector.y <= num3;
		if (flag)
		{
			float num4 = Math.Abs(vector.x) / num;
			float num5 = Math.Abs(vector.y) / num3;
			float num6 = Math.Abs(vector.z) / num2;
			rate = Math.Max(Math.Max(num4, num5), num6);
		}

		return flag;
	}

	private Vector3 crossPlaneAndLine(Vector3 normal, Vector3 ptOnPlane, Vector3 origin,
		Vector3 direction)
	{
		float num = Vector3.Dot(normal, ptOnPlane);
		float num2 = Vector3.Dot(normal, origin);
		float num3 = Vector3.Dot(normal, direction);
		return origin + (num - num2) / num3 * direction;
	}

	private bool canTabletMove()
	{
		MenuToolBase otherHandMTB = getOtherHandMTB();
		if (IsMovingSelf)
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

		if (!m_bCtrlEnable)
		{
			if (!otherHandMTB.m_bCtrlEnable)
			{
				return true;
			}

			if (m_tabletDist < 0.15f && otherHandMTB.m_tabletDist > 0.27f)
			{
				return true;
			}
		}

		return m_bCtrlEnable;
	}

	private bool getTabletCtrl()
	{
		MenuToolBase otherHandMTB = getOtherHandMTB();
		m_bCtrlEnable = true;
		if (otherHandMTB)
		{
			otherHandMTB.m_bCtrlEnable = false;
		}

		return m_bCtrlEnable;
	}

	public bool IsOvrTabletCtrlOutOfRange()
	{
		if (IsOvrTabletActive())
		{
			MenuToolBase otherHandMTB = getOtherHandMTB();
			if (m_bCtrlEnable)
			{
				return !m_bTabletArea;
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
		if (m_otherMenuTool == null && vrController != null)
		{
			GameObject gameObject = vrController.m_bHandL
				? GameMain.Instance.OvrMgr.ovr_obj.right_controller.controller.gameObject
				: GameMain.Instance.OvrMgr.ovr_obj.left_controller.controller.gameObject;
			if (gameObject)
			{
				m_otherMenuTool = gameObject.GetComponent<MenuToolBase>();
			}
		}

		return m_otherMenuTool;
	}
}
