using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal abstract class GripMoveControllerBase : MonoBehaviour
	{
		private void Awake()
		{
		}

		protected virtual bool CheckGripMoveIgnore()
		{
			foreach (EVRButtonId evrbuttonId in new EVRButtonId[] { (EVRButtonId)2, (EVRButtonId)33, (EVRButtonId)32 })
			{
				if (this.moveSelfButton != evrbuttonId && this.controller.GetPress(evrbuttonId))
				{
					return true;
				}
			}
			return false;
		}

		public abstract DeviceWrapper GetController();

		protected abstract Transform GetHeadTransform();

		public static bool GetLockMoveY()
		{
			return GripMoveControllerBase.lockMoveY;
		}

		public static bool GetLockRotHeadYZ()
		{
			return GripMoveControllerBase.lockRotHeadYZ;
		}

		public static bool GetLockRotXZ()
		{
			return GripMoveControllerBase.lockRotXZ;
		}

		public static bool GetMoveEnabled()
		{
			return GripMoveControllerBase.moveEnabled;
		}

		protected abstract OvrGripCollider GetOvrGripCollider();

		protected abstract Text GetTextForMode();

		private bool IsInGrabbing()
		{
			return this.menuTool != null && this.menuTool.IsGrabbing();
		}

		private bool IsInDrawMoveMode()
		{
			return this.menuTool != null && this.menuTool.IsDrawMode();
		}

		private bool IsNewModeAndDirectInactive()
		{
			return this.menuTool != null && this.menuTool.IsInNewMode() && !this.menuTool.IsDirectModeActive();
		}

		private void OVRGripForceResetWhenMoving()
		{
			if (this.moveSelfButton != (EVRButtonId)33)
			{
				return;
			}
			if (this.m_movingSelf && this.ovrGripCollider != null && this.ovrGripCollider.enabled && this.ovrGripCollider.grip && Time.time - this.m_movingStartTime > 0.1f)
			{
				this.ovrGripCollider.ResetGrip();
			}
		}

		private bool IsOVRGripActiveAndConflicted()
		{
			return this.moveSelfButton == (EVRButtonId)33 && (this.ovrGripCollider != null && this.ovrGripCollider.enabled && this.ovrGripCollider.grip);
		}

		private void OnLevelWasLoaded(int level)
		{
			this.head = null;
		}

		protected abstract void PostStart();

		protected abstract void PreUpdate();

		private Vector3 RemoveLockedAxis(Vector3 v)
		{
			bool flag = GripMoveControllerBase.lockRotHeadYZ;
			if (GripMoveControllerBase.lockRotXZ)
			{
				v.y = 0f;
			}
			return v;
		}

		private Quaternion RemoveLockedAxisRot(Quaternion q)
		{
			if (GripMoveControllerBase.lockRotHeadYZ)
			{
				if (this.headTransform == null)
				{
					this.headTransform = this.GetHeadTransform();
				}
				Transform transform = this.headTransform;
				Vector3 vector = q * transform.up;
				vector = transform.InverseTransformDirection(vector);
				vector.x = 0f;
				vector = transform.TransformDirection(vector);
				return Quaternion.FromToRotation(transform.up, vector);
			}
			if (GripMoveControllerBase.lockRotXZ)
			{
				Vector3 eulerAngles = q.eulerAngles;
				eulerAngles.x = 0f;
				eulerAngles.z = 0f;
				return Quaternion.Euler(eulerAngles);
			}
			return q;
		}

		public void ResetRotation()
		{
			GripMoveControllerBase.ResetRotation(this.target);
		}

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
			GripMoveControllerBase.lockMoveY = value;
			Settings.Instance.SetBoolValue("LockMoveY", value);
		}

		public static void SetLockRotHeadYZ(bool value)
		{
			GripMoveControllerBase.lockRotHeadYZ = value;
			Settings.Instance.SetBoolValue("LockRotateHeadYZ", value);
		}

		public static void SetLockRotXZ(bool value)
		{
			GripMoveControllerBase.lockRotXZ = value;
			Settings.Instance.SetBoolValue("LockRotateXZ", value);
		}

		public static void SetMoveEnabled(bool value)
		{
			GripMoveControllerBase.moveEnabled = value;
		}

		private void Start()
		{
			this.m_txMode = this.GetTextForMode();
			this.ovrGripCollider = base.GetComponent<OvrGripCollider>();
			this.moveSelfButton = Settings.Instance.GetButton("MoveSelfButton", (EVRButtonId)2);
			GripMoveControllerBase.moveToHeadEnabled = Settings.Instance.GetBoolValue("MoveToHeadEnabled", true);
			GripMoveControllerBase.hideManHeadWhenJump = Settings.Instance.GetBoolValue("HideManHeadWhenJump", true);
			this.moveToHeadButton = Settings.Instance.GetButton("MoveToHeadButton", (EVRButtonId)32);
			this.eyePosDeltaUp = Settings.Instance.GetFloatValue("EyePosDeltaUp", 0.1f);
			this.eyePosDeltaForward = Settings.Instance.GetFloatValue("EyePosDeltaForward", 0.05f);
			this.moveScreenButton = Settings.Instance.GetButton("MoveScreenButton", (EVRButtonId)33);
			this.controller = this.GetController();
			this.PostStart();
		}

		private void Update()
		{
			this.PreUpdate();
			if (this.head == null && FirstPersonCameraHelper.IsFPSModeEnabled(SceneManager.GetActiveScene().buildIndex))
			{
				this.head = FirstPersonCameraHelper.FindManHead();
			}
			if (this.ovrGripCollider == null)
			{
				this.ovrGripCollider = this.GetOvrGripCollider();
			}
			if (this.marker == null)
			{
				this.marker = new GameObject("_marker_" + base.name);
				this.marker.transform.parent = base.transform.parent;
				this.marker.transform.position = base.transform.position;
				this.marker.transform.rotation = base.transform.rotation;
			}
			if (this.handle == null)
			{
				this.handle = new GameObject("_handle_" + base.name);
				this.handle.transform.parent = base.transform.parent;
				this.handle.transform.position = base.transform.position;
				this.handle.transform.rotation = base.transform.rotation;
			}
			this.OVRGripForceResetWhenMoving();
			bool movingSelf = this.m_movingSelf;
			this.m_movingSelf = false;
			if (this.controller != null && !this.IsOVRGripActiveAndConflicted() && !this.IsInGrabbing() && GripMoveControllerBase.moveEnabled && this.IsNewModeAndDirectInactive() && this.IsInDrawMoveMode() && this.target != null)
			{
				Vector3 vector = this.marker.transform.position - base.transform.position;
				if (GripMoveControllerBase.lockMoveY)
				{
					vector.y = 0f;
				}
				Quaternion quaternion = this.marker.transform.rotation * Quaternion.Inverse(base.transform.rotation);
				Quaternion quaternion2 = this.RemoveLockedAxisRot(quaternion);
				Transform parent = this.target.transform.parent;
				this.handle.transform.position = base.transform.position;
				this.handle.transform.rotation = base.transform.rotation;
				this.target.transform.parent = this.handle.transform;
				this.handle.transform.rotation = quaternion2 * this.handle.transform.rotation;
				this.target.transform.parent = parent;
				this.m_movingSelf = true;
			}
			if (this.controller != null && !this.IsOVRGripActiveAndConflicted() && !this.IsInGrabbing() && GripMoveControllerBase.moveEnabled && !this.IsNewModeAndDirectInactive())
			{
				this.controller.GetPressDown(this.moveSelfButton);
				this.controller.GetPressUp(this.moveSelfButton);
				if (this.controller.GetPress(this.moveSelfButton) && !this.controller.GetPressDown(this.moveToHeadButton) && !this.CheckGripMoveIgnore() && this.target != null)
				{
					Vector3 vector2 = this.marker.transform.position - base.transform.position;
					if (GripMoveControllerBase.lockMoveY)
					{
						vector2.y = 0f;
					}
					Quaternion quaternion3 = this.marker.transform.rotation * Quaternion.Inverse(base.transform.rotation);
					quaternion3 = this.RemoveLockedAxisRot(quaternion3);
					Transform parent2 = this.target.transform.parent;
					this.handle.transform.position = base.transform.position;
					this.handle.transform.rotation = base.transform.rotation;
					this.target.transform.parent = this.handle.transform;
					this.handle.transform.rotation = quaternion3 * this.handle.transform.rotation;
					this.handle.transform.position = this.handle.transform.position + vector2;
					this.target.transform.parent = parent2;
					this.m_movingSelf = true;
				}
				if (this.controller.GetPress(this.moveSelfButton) && this.controller.GetPressDown(this.moveToHeadButton))
				{
					this.fpModePressTime = Time.time;
				}
				if (this.controller.GetPress(this.moveSelfButton) && this.controller.GetPressDown(this.moveToHeadButton) && Time.time - this.fpModePressTime > 3f)
				{
					this.ResetRotation();
				}
				if (this.controller.GetPress(this.moveSelfButton) && this.controller.GetPressUp(this.moveToHeadButton))
				{
					if (Time.time - this.fpModePressTime > 3f)
					{
						this.ResetRotation();
					}
					else if (this.target != null && this.head != null)
					{
						Vector3 vector3 = this.head.transform.TransformDirection(new Vector3(0f, -1f, 0f));
						Vector3 vector4 = this.head.transform.TransformDirection(new Vector3(0f, 0f, 1f));
						if (this.headTransform == null)
						{
							this.headTransform = this.GetHeadTransform();
						}
						Vector3 vector5 = this.headTransform.TransformDirection(new Vector3(0f, 0f, 1f));
						Vector3 vector6 = this.RemoveLockedAxis(vector3);
						Quaternion quaternion4 = Quaternion.FromToRotation(this.RemoveLockedAxis(vector5), vector6);
						Vector3 vector7 = this.head.transform.position - this.headTransform.position + vector3.normalized * this.eyePosDeltaForward + vector4.normalized * this.eyePosDeltaUp;
						Transform parent3 = this.target.transform.parent;
						this.handle.transform.position = this.headTransform.position;
						this.handle.transform.rotation = this.headTransform.rotation;
						this.target.transform.parent = this.handle.transform;
						this.handle.transform.rotation = this.RemoveLockedAxisRot(quaternion4) * this.handle.transform.rotation;
						this.handle.transform.position = this.handle.transform.position + vector7;
						this.target.transform.parent = parent3;
						if (GripMoveControllerBase.hideManHeadWhenJump)
						{
							SkinnedMeshRenderer component = this.head.GetComponent<SkinnedMeshRenderer>();
							if (component != null)
							{
								component.enabled = false;
							}
						}
						base.StartCoroutine("UpdateMarkerPos");
					}
				}
			}
			this.marker.transform.position = base.transform.position;
			this.marker.transform.rotation = base.transform.rotation;
			if (movingSelf != this.m_movingSelf)
			{
				this.m_movingStartTime = (this.m_movingSelf ? Time.time : 0f);
			}
			if (this.menuTool)
			{
				this.menuTool.IsMovingSelf = this.m_movingSelf;
			}
		}

		private IEnumerator UpdateMarkerPos()
		{
			yield return new WaitForEndOfFrame();
			this.marker.transform.position = this.transform.position;
			this.marker.transform.rotation = this.transform.rotation;
			yield break;
		}

		public bool ToggleNewOldUI()
		{
			if (this.menuTool)
			{
				this.menuTool.ToggleNewOldUI();
				return true;
			}
			return false;
		}

		protected DeviceWrapper controller;

		protected GameObject marker;

		protected GameObject handle;

		public GameObject target;

		private Text m_txMode;

		protected EVRButtonId moveSelfButton;

		protected EVRButtonId moveScreenButton;

		private static bool lockRotHeadYZ = Settings.Instance.GetBoolValue("LockRotateHeadYZ", false);

		private static bool lockRotXZ = Settings.Instance.GetBoolValue("LockRotateXZ", true);

		private static bool moveToHeadEnabled = true;

		private static bool hideManHeadWhenJump = true;

		protected EVRButtonId moveToHeadButton;

		private GameObject head;

		private float eyePosDeltaUp = 0.1f;

		private float eyePosDeltaForward = 0.05f;

		private float fpModePressTime;

		private Transform headTransform;

		private OvrGripCollider ovrGripCollider;

		private static bool lockMoveY = Settings.Instance.GetBoolValue("LockMoveY", false);

		private static bool moveEnabled = true;

		protected MenuToolBase menuTool;

		private bool m_movingSelf;

		private float m_movingStartTime;

		private const float OVR_GRIP_FORCE_RESET_TIME = 0.1f;
	}
}
