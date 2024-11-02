using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000016 RID: 22
	internal abstract class GripMoveControllerBase : MonoBehaviour
	{
		// Token: 0x0600007F RID: 127 RVA: 0x000024BB File Offset: 0x000006BB
		private void Awake()
		{
		}

		// Token: 0x06000080 RID: 128 RVA: 0x0000525C File Offset: 0x0000345C
		protected virtual bool CheckGripMoveIgnore()
		{
			foreach (EVRButtonId evrbuttonId in new EVRButtonId[]
			{
				EVRButtonId.k_EButton_Grip,
				EVRButtonId.k_EButton_Axis1,
				EVRButtonId.k_EButton_Axis0
			})
			{
				if (this.moveSelfButton != evrbuttonId && this.controller.GetPress(evrbuttonId))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000081 RID: 129
		public abstract DeviceWrapper GetController();

		// Token: 0x06000082 RID: 130
		protected abstract Transform GetHeadTransform();

		// Token: 0x06000083 RID: 131 RVA: 0x0000288D File Offset: 0x00000A8D
		public static bool GetLockMoveY()
		{
			return GripMoveControllerBase.lockMoveY;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00002894 File Offset: 0x00000A94
		public static bool GetLockRotHeadYZ()
		{
			return GripMoveControllerBase.lockRotHeadYZ;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x0000289B File Offset: 0x00000A9B
		public static bool GetLockRotXZ()
		{
			return GripMoveControllerBase.lockRotXZ;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000028A2 File Offset: 0x00000AA2
		public static bool GetMoveEnabled()
		{
			return GripMoveControllerBase.moveEnabled;
		}

		// Token: 0x06000087 RID: 135
		protected abstract OvrGripCollider GetOvrGripCollider();

		// Token: 0x06000088 RID: 136
		protected abstract Text GetTextForMode();

		// Token: 0x06000089 RID: 137 RVA: 0x000028A9 File Offset: 0x00000AA9
		private bool IsInGrabbing()
		{
			return !(this.menuTool == null) && this.menuTool.IsGrabbing();
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000028C6 File Offset: 0x00000AC6
		private bool IsInDrawMoveMode()
		{
			return !(this.menuTool == null) && this.menuTool.IsDrawMode();
		}

		// Token: 0x0600008B RID: 139 RVA: 0x000028E3 File Offset: 0x00000AE3
		private bool IsNewModeAndDirectInactive()
		{
			return !(this.menuTool == null) && this.menuTool.IsInNewMode() && !this.menuTool.IsDirectModeActive();
		}

		// Token: 0x0600008C RID: 140 RVA: 0x000052AC File Offset: 0x000034AC
		private void OVRGripForceResetWhenMoving()
		{
			if (this.moveSelfButton != EVRButtonId.k_EButton_Axis1)
			{
				return;
			}
			if (this.m_movingSelf && this.ovrGripCollider != null && this.ovrGripCollider.enabled && this.ovrGripCollider.grip && Time.time - this.m_movingStartTime > 0.1f)
			{
				this.ovrGripCollider.ResetGrip();
			}
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00002912 File Offset: 0x00000B12
		private bool IsOVRGripActiveAndConflicted()
		{
			return this.moveSelfButton == EVRButtonId.k_EButton_Axis1 && (this.ovrGripCollider != null && this.ovrGripCollider.enabled && this.ovrGripCollider.grip);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x0000294B File Offset: 0x00000B4B
		private void OnLevelWasLoaded(int level)
		{
			this.head = null;
		}

		// Token: 0x0600008F RID: 143
		protected abstract void PostStart();

		// Token: 0x06000090 RID: 144
		protected abstract void PreUpdate();

		// Token: 0x06000091 RID: 145 RVA: 0x00002954 File Offset: 0x00000B54
		private Vector3 RemoveLockedAxis(Vector3 v)
		{
			bool flag = GripMoveControllerBase.lockRotHeadYZ;
			if (GripMoveControllerBase.lockRotXZ)
			{
				v.y = 0f;
			}
			return v;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00005314 File Offset: 0x00003514
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

		// Token: 0x06000093 RID: 147 RVA: 0x00002970 File Offset: 0x00000B70
		public void ResetRotation()
		{
			GripMoveControllerBase.ResetRotation(this.target);
		}

		// Token: 0x06000094 RID: 148 RVA: 0x000053B0 File Offset: 0x000035B0
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

		// Token: 0x06000095 RID: 149 RVA: 0x0000297D File Offset: 0x00000B7D
		public static void SetLockMoveY(bool value)
		{
			GripMoveControllerBase.lockMoveY = value;
			Settings.Instance.SetBoolValue("LockMoveY", value);
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00002995 File Offset: 0x00000B95
		public static void SetLockRotHeadYZ(bool value)
		{
			GripMoveControllerBase.lockRotHeadYZ = value;
			Settings.Instance.SetBoolValue("LockRotateHeadYZ", value);
		}

		// Token: 0x06000097 RID: 151 RVA: 0x000029AD File Offset: 0x00000BAD
		public static void SetLockRotXZ(bool value)
		{
			GripMoveControllerBase.lockRotXZ = value;
			Settings.Instance.SetBoolValue("LockRotateXZ", value);
		}

		// Token: 0x06000098 RID: 152 RVA: 0x000029C5 File Offset: 0x00000BC5
		public static void SetMoveEnabled(bool value)
		{
			GripMoveControllerBase.moveEnabled = value;
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00005404 File Offset: 0x00003604
		private void Start()
		{
			this.m_txMode = this.GetTextForMode();
			this.ovrGripCollider = base.GetComponent<OvrGripCollider>();
			this.moveSelfButton = Settings.Instance.GetButton("MoveSelfButton", EVRButtonId.k_EButton_Grip);
			GripMoveControllerBase.moveToHeadEnabled = Settings.Instance.GetBoolValue("MoveToHeadEnabled", true);
			GripMoveControllerBase.hideManHeadWhenJump = Settings.Instance.GetBoolValue("HideManHeadWhenJump", true);
			this.moveToHeadButton = Settings.Instance.GetButton("MoveToHeadButton", EVRButtonId.k_EButton_Axis0);
			this.eyePosDeltaUp = Settings.Instance.GetFloatValue("EyePosDeltaUp", 0.1f);
			this.eyePosDeltaForward = Settings.Instance.GetFloatValue("EyePosDeltaForward", 0.05f);
			this.moveScreenButton = Settings.Instance.GetButton("MoveScreenButton", EVRButtonId.k_EButton_Axis1);
			this.controller = this.GetController();
			this.PostStart();
		}

		// Token: 0x0600009A RID: 154 RVA: 0x000054E0 File Offset: 0x000036E0
		private void Update()
		{
			this.PreUpdate();
			if (this.head == null && FirstPersonCameraHelper.IsFPSModeEnabled(Application.loadedLevel))
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

		// Token: 0x0600009B RID: 155 RVA: 0x000029CD File Offset: 0x00000BCD
		private IEnumerator UpdateMarkerPos()
		{
			yield return new WaitForEndOfFrame();
			this.marker.transform.position = this.transform.position;
			this.marker.transform.rotation = this.transform.rotation;
			yield break;
		}

		// Token: 0x0600009C RID: 156 RVA: 0x000029DC File Offset: 0x00000BDC
		public bool ToggleNewOldUI()
		{
			if (this.menuTool)
			{
				this.menuTool.ToggleNewOldUI();
				return true;
			}
			return false;
		}

		// Token: 0x04000049 RID: 73
		protected DeviceWrapper controller;

		// Token: 0x0400004A RID: 74
		protected GameObject marker;

		// Token: 0x0400004B RID: 75
		protected GameObject handle;

		// Token: 0x0400004C RID: 76
		public GameObject target;

		// Token: 0x0400004D RID: 77
		private Text m_txMode;

		// Token: 0x0400004E RID: 78
		protected EVRButtonId moveSelfButton;

		// Token: 0x0400004F RID: 79
		protected EVRButtonId moveScreenButton;

		// Token: 0x04000050 RID: 80
		private static bool lockRotHeadYZ = Settings.Instance.GetBoolValue("LockRotateHeadYZ", false);

		// Token: 0x04000051 RID: 81
		private static bool lockRotXZ = Settings.Instance.GetBoolValue("LockRotateXZ", true);

		// Token: 0x04000052 RID: 82
		private static bool moveToHeadEnabled = true;

		// Token: 0x04000053 RID: 83
		private static bool hideManHeadWhenJump = true;

		// Token: 0x04000054 RID: 84
		protected EVRButtonId moveToHeadButton;

		// Token: 0x04000055 RID: 85
		private GameObject head;

		// Token: 0x04000056 RID: 86
		private float eyePosDeltaUp = 0.1f;

		// Token: 0x04000057 RID: 87
		private float eyePosDeltaForward = 0.05f;

		// Token: 0x04000058 RID: 88
		private float fpModePressTime;

		// Token: 0x04000059 RID: 89
		private Transform headTransform;

		// Token: 0x0400005A RID: 90
		private OvrGripCollider ovrGripCollider;

		// Token: 0x0400005B RID: 91
		private static bool lockMoveY = Settings.Instance.GetBoolValue("LockMoveY", false);

		// Token: 0x0400005C RID: 92
		private static bool moveEnabled = true;

		// Token: 0x0400005D RID: 93
		protected MenuToolBase menuTool;

		// Token: 0x0400005E RID: 94
		private bool m_movingSelf;

		// Token: 0x0400005F RID: 95
		private float m_movingStartTime;

		// Token: 0x04000060 RID: 96
		private const float OVR_GRIP_FORCE_RESET_TIME = 0.1f;
	}
}
