using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR;

namespace COM3D2.GripMovePlugin.Plugin;

internal abstract class GripMoveControllerBase : MonoBehaviour
{
	private const float OVR_GRIP_FORCE_RESET_TIME = 0.1f;

	private static bool lockRotHeadYZ = Settings.Instance.GetBoolValue("LockRotateHeadYZ", false);

	private static bool lockRotXZ = Settings.Instance.GetBoolValue("LockRotateXZ", true);

	private static bool moveToHeadEnabled = true;

	private static bool hideManHeadWhenJump = true;

	private static bool lockMoveY = Settings.Instance.GetBoolValue("LockMoveY", false);

	private static bool moveEnabled = true;

	public GameObject target;

	protected DeviceWrapper controller;

	private float eyePosDeltaForward = 0.05f;

	private float eyePosDeltaUp = 0.1f;

	private float fpModePressTime;

	protected GameObject handle;

	private GameObject head;

	private Transform headTransform;

	private bool m_movingSelf;

	private float m_movingStartTime;

	private Text m_txMode;

	protected GameObject marker;

	protected MenuToolBase menuTool;

#if COM3D25_UNITY_2022
	protected AVRControllerButtons.BTN moveScreenButton;

	protected AVRControllerButtons.BTN moveSelfButton;

	protected AVRControllerButtons.BTN moveToHeadButton;
#else
	protected EVRButtonId moveScreenButton;

	protected EVRButtonId moveSelfButton;

	protected EVRButtonId moveToHeadButton;
#endif

	private OvrGripCollider ovrGripCollider;

	private void Awake()
	{
	}

	private void Start()
	{
		m_txMode = GetTextForMode();
		ovrGripCollider = GetComponent<OvrGripCollider>();
#if COM3D25_UNITY_2022
		moveSelfButton = Settings.Instance.GetButton("MoveSelfButton", AVRControllerButtons.BTN.GRIP);
#else
		moveSelfButton = Settings.Instance.GetButton("MoveSelfButton", (EVRButtonId)2);
#endif
		moveToHeadEnabled = Settings.Instance.GetBoolValue("MoveToHeadEnabled", true);
		hideManHeadWhenJump = Settings.Instance.GetBoolValue("HideManHeadWhenJump", true);
#if COM3D25_UNITY_2022
		moveToHeadButton =
			Settings.Instance.GetButton("MoveToHeadButton", AVRControllerButtons.BTN.STICK_PAD);
#else
		moveToHeadButton = Settings.Instance.GetButton("MoveToHeadButton", (EVRButtonId)32);
#endif
		eyePosDeltaUp = Settings.Instance.GetFloatValue("EyePosDeltaUp", 0.1f);
		eyePosDeltaForward = Settings.Instance.GetFloatValue("EyePosDeltaForward", 0.05f);
#if COM3D25_UNITY_2022
		moveScreenButton = Settings.Instance.GetButton("MoveScreenButton", AVRControllerButtons.BTN.TRIGGER);
#else
		moveScreenButton = Settings.Instance.GetButton("MoveScreenButton", (EVRButtonId)33);
#endif
		controller = GetController();
		PostStart();
	}

	private void Update()
	{
		PreUpdate();
		if (head == null &&
		    FirstPersonCameraHelper.IsFPSModeEnabled(SceneManager.GetActiveScene().buildIndex))
		{
			head = FirstPersonCameraHelper.FindManHead();
		}

		if (ovrGripCollider == null)
		{
			ovrGripCollider = GetOvrGripCollider();
		}

		if (marker == null)
		{
			marker = new GameObject("_marker_" + name);
			marker.transform.parent = transform.parent;
			marker.transform.position = transform.position;
			marker.transform.rotation = transform.rotation;
		}

		if (handle == null)
		{
			handle = new GameObject("_handle_" + name);
			handle.transform.parent = transform.parent;
			handle.transform.position = transform.position;
			handle.transform.rotation = transform.rotation;
		}

		OVRGripForceResetWhenMoving();
		bool movingSelf = m_movingSelf;
		m_movingSelf = false;
		if (controller != null && !IsOVRGripActiveAndConflicted() && !IsInGrabbing() &&
		    moveEnabled && IsNewModeAndDirectInactive() && IsInDrawMoveMode() && target != null)
		{
			Vector3 vector = marker.transform.position - transform.position;
			if (lockMoveY)
			{
				vector.y = 0f;
			}

			Quaternion quaternion =
				marker.transform.rotation * Quaternion.Inverse(transform.rotation);
			Quaternion quaternion2 = RemoveLockedAxisRot(quaternion);
			Transform parent = target.transform.parent;
			handle.transform.position = transform.position;
			handle.transform.rotation = transform.rotation;
			target.transform.parent = handle.transform;
			handle.transform.rotation = quaternion2 * handle.transform.rotation;
			target.transform.parent = parent;
			m_movingSelf = true;
		}

		if (controller != null && !IsOVRGripActiveAndConflicted() && !IsInGrabbing() &&
		    moveEnabled && !IsNewModeAndDirectInactive())
		{
			controller.GetPressDown(moveSelfButton);
			controller.GetPressUp(moveSelfButton);
			if (controller.GetPress(moveSelfButton) && !controller.GetPressDown(moveToHeadButton) &&
			    !CheckGripMoveIgnore() && target != null)
			{
				Vector3 vector2 = marker.transform.position - transform.position;
				if (lockMoveY)
				{
					vector2.y = 0f;
				}

				Quaternion quaternion3 =
					marker.transform.rotation * Quaternion.Inverse(transform.rotation);
				quaternion3 = RemoveLockedAxisRot(quaternion3);
				Transform parent2 = target.transform.parent;
				handle.transform.position = transform.position;
				handle.transform.rotation = transform.rotation;
				target.transform.parent = handle.transform;
				handle.transform.rotation = quaternion3 * handle.transform.rotation;
				handle.transform.position = handle.transform.position + vector2;
				target.transform.parent = parent2;
				m_movingSelf = true;
			}

			if (controller.GetPress(moveSelfButton) && controller.GetPressDown(moveToHeadButton))
			{
				fpModePressTime = Time.time;
			}

			if (controller.GetPress(moveSelfButton) && controller.GetPressDown(moveToHeadButton) &&
			    Time.time - fpModePressTime > 3f)
			{
				ResetRotation();
			}

			if (controller.GetPress(moveSelfButton) && controller.GetPressUp(moveToHeadButton))
			{
				if (Time.time - fpModePressTime > 3f)
				{
					ResetRotation();
				}
				else if (target != null && head != null)
				{
					Vector3 vector3 = head.transform.TransformDirection(new Vector3(0f, -1f, 0f));
					Vector3 vector4 = head.transform.TransformDirection(new Vector3(0f, 0f, 1f));
					if (headTransform == null)
					{
						headTransform = GetHeadTransform();
					}

					Vector3 vector5 = headTransform.TransformDirection(new Vector3(0f, 0f, 1f));
					Vector3 vector6 = RemoveLockedAxis(vector3);
					var quaternion4 = Quaternion.FromToRotation(RemoveLockedAxis(vector5), vector6);
					Vector3 vector7 = head.transform.position - headTransform.position +
					                  vector3.normalized * eyePosDeltaForward +
					                  vector4.normalized * eyePosDeltaUp;
					Transform parent3 = target.transform.parent;
					handle.transform.position = headTransform.position;
					handle.transform.rotation = headTransform.rotation;
					target.transform.parent = handle.transform;
					handle.transform.rotation =
						RemoveLockedAxisRot(quaternion4) * handle.transform.rotation;
					handle.transform.position = handle.transform.position + vector7;
					target.transform.parent = parent3;
					if (hideManHeadWhenJump)
					{
						SkinnedMeshRenderer component = head.GetComponent<SkinnedMeshRenderer>();
						if (component != null)
						{
							component.enabled = false;
						}
					}

					StartCoroutine("UpdateMarkerPos");
				}
			}
		}

		marker.transform.position = transform.position;
		marker.transform.rotation = transform.rotation;
		if (movingSelf != m_movingSelf)
		{
			m_movingStartTime = m_movingSelf ? Time.time : 0f;
		}

		if (menuTool)
		{
			menuTool.IsMovingSelf = m_movingSelf;
		}
	}

	private void OnLevelWasLoaded(int level) => head = null;

	protected virtual bool CheckGripMoveIgnore()
	{
#if COM3D25_UNITY_2022
		if (moveSelfButton != AVRControllerButtons.BTN.GRIP &&
		    controller.GetPress(AVRControllerButtons.BTN.GRIP))
		{
			return true;
		}

		if (moveSelfButton != AVRControllerButtons.BTN.TRIGGER &&
		    controller.GetPress(AVRControllerButtons.BTN.TRIGGER))
		{
			return true;
		}

		if (moveSelfButton != AVRControllerButtons.BTN.STICK_PAD &&
		    controller.GetPress(AVRControllerButtons.BTN.STICK_PAD))
		{
			return true;
		}

		return false;
#else
		foreach (EVRButtonId evrbuttonId in new[]
		         {
			         (EVRButtonId)2, (EVRButtonId)33, (EVRButtonId)32
		         })
		{
			if (moveSelfButton != evrbuttonId && controller.GetPress(evrbuttonId))
			{
				return true;
			}
		}

		return false;
#endif
	}

	public abstract DeviceWrapper GetController();

	protected abstract Transform GetHeadTransform();

	public static bool GetLockMoveY() => lockMoveY;

	public static bool GetLockRotHeadYZ() => lockRotHeadYZ;

	public static bool GetLockRotXZ() => lockRotXZ;

	public static bool GetMoveEnabled() => moveEnabled;

	protected abstract OvrGripCollider GetOvrGripCollider();

	protected abstract Text GetTextForMode();

	private bool IsInGrabbing() => menuTool != null && menuTool.IsGrabbing();

	private bool IsInDrawMoveMode() => menuTool != null && menuTool.IsDrawMode();

	private bool IsNewModeAndDirectInactive() => menuTool != null && menuTool.IsInNewMode() &&
	                                             !menuTool.IsDirectModeActive();

	private void OVRGripForceResetWhenMoving()
	{
#if COM3D25_UNITY_2022
		if (moveSelfButton != AVRControllerButtons.BTN.TRIGGER)
#else
		if (moveSelfButton != (EVRButtonId)33)
#endif
		{
			return;
		}

		if (m_movingSelf && ovrGripCollider != null && ovrGripCollider.enabled &&
		    ovrGripCollider.grip && Time.time - m_movingStartTime > 0.1f)
		{
			ovrGripCollider.ResetGrip();
		}
	}

#if COM3D25_UNITY_2022
	private bool IsOVRGripActiveAndConflicted() => moveSelfButton == AVRControllerButtons.BTN.TRIGGER &&
	                                               ovrGripCollider != null &&
	                                               ovrGripCollider.enabled && ovrGripCollider.grip;
#else
	private bool IsOVRGripActiveAndConflicted() => moveSelfButton == (EVRButtonId)33 &&
	                                               ovrGripCollider != null &&
	                                               ovrGripCollider.enabled && ovrGripCollider.grip;
#endif

	protected abstract void PostStart();

	protected abstract void PreUpdate();

	private Vector3 RemoveLockedAxis(Vector3 v)
	{
		bool flag = lockRotHeadYZ;
		if (lockRotXZ)
		{
			v.y = 0f;
		}

		return v;
	}

	private Quaternion RemoveLockedAxisRot(Quaternion q)
	{
		if (lockRotHeadYZ)
		{
			if (headTransform == null)
			{
				headTransform = GetHeadTransform();
			}

			Transform transform = headTransform;
			Vector3 vector = q * transform.up;
			vector = transform.InverseTransformDirection(vector);
			vector.x = 0f;
			vector = transform.TransformDirection(vector);
			return Quaternion.FromToRotation(transform.up, vector);
		}

		if (lockRotXZ)
		{
			Vector3 eulerAngles = q.eulerAngles;
			eulerAngles.x = 0f;
			eulerAngles.z = 0f;
			return Quaternion.Euler(eulerAngles);
		}

		return q;
	}

	public void ResetRotation() => ResetRotation(target);

	public static void ResetRotation(GameObject target)
	{
		if (target != null)
		{
			Vector3 eulerAngles = target.transform.rotation.eulerAngles;
			eulerAngles.x = 0f;
			eulerAngles.z = 0f;
			target.transform.rotation = Quaternion.Euler(eulerAngles);
		}
	}

	public static void SetLockMoveY(bool value)
	{
		lockMoveY = value;
		Settings.Instance.SetBoolValue("LockMoveY", value);
	}

	public static void SetLockRotHeadYZ(bool value)
	{
		lockRotHeadYZ = value;
		Settings.Instance.SetBoolValue("LockRotateHeadYZ", value);
	}

	public static void SetLockRotXZ(bool value)
	{
		lockRotXZ = value;
		Settings.Instance.SetBoolValue("LockRotateXZ", value);
	}

	public static void SetMoveEnabled(bool value) => moveEnabled = value;

	private IEnumerator UpdateMarkerPos()
	{
		yield return new WaitForEndOfFrame();
		marker.transform.position = transform.position;
		marker.transform.rotation = transform.rotation;
		yield break;
	}

	public bool ToggleNewOldUI()
	{
		if (menuTool)
		{
			menuTool.ToggleNewOldUI();
			return true;
		}

		return false;
	}
}
