using System;
using System.Collections;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000019 RID: 25
	internal class IMGUIQuad : MoveableGUIObject
	{
		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000AB RID: 171 RVA: 0x00005E94 File Offset: 0x00004094
		public static IMGUIQuad Instance
		{
			get
			{
				if (IMGUIQuad._instance == null)
				{
					IMGUIQuad._instance = new GameObject("IMGUIQuad").AddComponent<IMGUIQuad>();
					IMGUIQuad._instance.crate();
					IMGUIQuad._instance.gameObject.SetActive(IMGUIQuad.IsVisble);
				}
				return IMGUIQuad._instance;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000AC RID: 172 RVA: 0x00002AE4 File Offset: 0x00000CE4
		// (set) Token: 0x060000AD RID: 173 RVA: 0x00002AEB File Offset: 0x00000CEB
		public static bool IsVisble
		{
			get
			{
				return IMGUIQuad.bVisble;
			}
			set
			{
				IMGUIQuad.bVisble = value;
				Settings.Instance.SetBoolValue("IMGUIVisble", value);
				IMGUIQuad.Instance.gameObject.SetActive(value);
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000AE RID: 174 RVA: 0x00002B13 File Offset: 0x00000D13
		// (set) Token: 0x060000AF RID: 175 RVA: 0x00002B1A File Offset: 0x00000D1A
		public static bool IsAttach
		{
			get
			{
				return IMGUIQuad.bAttach;
			}
			set
			{
				IMGUIQuad.bAttach = value;
				Settings.Instance.SetBoolValue("IMGUIAttach", value);
				IMGUIQuad.Instance.attachInGUIQuad(value);
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x00002B3D File Offset: 0x00000D3D
		// (set) Token: 0x060000B1 RID: 177 RVA: 0x00002B44 File Offset: 0x00000D44
		public static bool IsTransparent
		{
			get
			{
				return IMGUIQuad.bTransparent;
			}
			set
			{
				if (IMGUIQuad.bTransparent == value)
				{
					return;
				}
				IMGUIQuad.bTransparent = value;
				Settings.Instance.SetBoolValue("IMGUITransparent", value);
				IMGUIQuad.Instance.UpdateMaterial(value);
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x00002B70 File Offset: 0x00000D70
		// (set) Token: 0x060000B3 RID: 179 RVA: 0x00002B77 File Offset: 0x00000D77
		public static bool IsNoVRController
		{
			get
			{
				return IMGUIQuad.bNoVRController;
			}
			set
			{
				if (IMGUIQuad.bNoVRController == value)
				{
					return;
				}
				IMGUIQuad.bNoVRController = value;
				Settings.Instance.SetBoolValue("IMGUINoVRController", value);
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000B4 RID: 180 RVA: 0x00002B98 File Offset: 0x00000D98
		public bool NowAttached
		{
			get
			{
				return this.attachParent > IMGUIQuad.AttachParent.None;
			}
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00005EE8 File Offset: 0x000040E8
		private void crate()
		{
			this.screenQuad = DoubleSidePlane.Create(null, null, 1f);
			this.screenQuad.name = "IMGUIScreenQuad";
			this.screenQuad.transform.parent = base.transform;
			this.screenQuad.transform.localPosition = Vector3.zero;
			this.screenQuad.transform.localRotation = Quaternion.identity;
			this.screenQuad.transform.localScale = Vector3.one;
			MeshCollider component = this.screenQuad.GetComponent<MeshCollider>();
			if (component)
			{
				component.enabled = false;
				global::UnityEngine.Object.DestroyImmediate(component);
			}
			this.mouseCursorTexture = new Texture2D(32, 32);
			this.mouseCursorTexture.LoadImage(Resource.mouse_cursor, false);
			this.mouseCursorTexture.Apply();
			this.uiSize = Settings.Instance.GetFloatValue("IMGUISize", 0.5f);
			Settings.Instance.SetFloatValue("IMGUISize", this.uiSize);
			this.collider = base.gameObject.AddComponent<BoxCollider>();
			this.collider.size = new Vector3(1f, 1f, 1f);
			this.collider.isTrigger = true;
			base.gameObject.AddComponent<Rigidbody>().isKinematic = true;
			this.attachInGUIQuad(IMGUIQuad.IsAttach);
			base.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			base.gameObject.AddComponent<FastGUI>();
			base.gameObject.AddComponent<SlowGUI>();
			this.makeGuiTexture();
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00006074 File Offset: 0x00004274
		private void attachInGUIQuad(bool bAttachInGui)
		{
			Transform transform = null;
			Vector3 vector = new Vector3(1f, 1f, 0.0001f);
			if (bAttachInGui)
			{
				if (GameMain.Instance.CMSystem.OvrUseNewControllerType && GameMain.Instance.OvrMgr.OvrCamera.OvrTablet && GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.isActiveAndEnabled)
				{
					transform = GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.transform.Find("Screen");
					if (transform)
					{
						vector = new Vector3(0.5092f, 0.2894f, 0.001500021f);
						vector.z += (IMGUIQuad.IsTransparent ? 0.0001f : (-0.0001f));
						this.attachParent = IMGUIQuad.AttachParent.OvrTablet;
					}
				}
				if (transform == null && GUIQuad.Instance && GUIQuad.Instance.isActiveAndEnabled)
				{
					transform = GUIQuad.Instance.transform;
					vector = new Vector3(1f, 1f, 0.0001f);
					this.attachParent = IMGUIQuad.AttachParent.GUIQuad;
				}
			}
			if (transform == null)
			{
				Transform transform2 = VRContextUtil.FindOVRHeadTransform();
				transform = ((transform2 == null) ? null : transform2.parent);
				float num = this.uiSize;
				float num2 = num / (float)Screen.height * (float)Screen.width;
				vector = new Vector3(num2, num, 0.0001f);
				this.attachParent = IMGUIQuad.AttachParent.None;
			}
			base.transform.parent = transform;
			base.transform.localScale = vector;
			if (this.collider)
			{
				this.collider.enabled = !this.NowAttached;
				this.collider.size = (this.collider.enabled ? Vector3.one : Vector3.zero);
			}
			this.UpdateMaterial(IMGUIQuad.IsTransparent);
			if (this.renderer)
			{
				this.renderer.enabled = true;
			}
			this.resetIMGUIPosition();
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00006274 File Offset: 0x00004474
		private void makeGuiTexture()
		{
			if (this.guiTexture)
			{
				this.guiTexture.Release();
			}
			this.guiTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Default);
			this.guiTexture.Create();
			this.UpdateMaterial(IMGUIQuad.IsTransparent);
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x000062C8 File Offset: 0x000044C8
		private void Update()
		{
			if (this.guiTexture && (this.guiTexture.width != Screen.width || this.guiTexture.height != Screen.height))
			{
				this.makeGuiTexture();
			}
			if (this.NowAttached && this.collider && this.collider.enabled)
			{
				this.collider.enabled = !this.NowAttached;
			}
			if (IMGUIQuad.IsNoVRController)
			{
				if (Cursor.lockState == CursorLockMode.Locked)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
				if (Input.GetMouseButtonUp(0))
				{
					this.lastMousePos = Input.mousePosition;
					base.StartCoroutine(this.moveCursorAtNextFrame());
				}
			}
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00002BA3 File Offset: 0x00000DA3
		private IEnumerator moveCursorAtNextFrame()
		{
			yield return null;
			this.MoveCursorTo((int)this.lastMousePos.x, (int)this.lastMousePos.y);
			yield break;
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00006380 File Offset: 0x00004580
		public void MoveCursorTo(int x, int y)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			this.bMoveCursor = true;
			if (Screen.fullScreen)
			{
				RECT windowRect = Win32API.GetWindowRect();
				x = x * windowRect.Width / Screen.width;
				y = y * windowRect.Height / Screen.height;
			}
			this.newCursorPos = new Vector2((float)x, (float)y);
		}

		// Token: 0x060000BB RID: 187 RVA: 0x000063E0 File Offset: 0x000045E0
		public void MoveCursorTo(Transform trans)
		{
			Vector3 vector = this.renderer.bounds.ClosestPoint(trans.position);
			Vector3 vector2 = base.transform.InverseTransformPoint(vector);
			if (base.transform.InverseTransformPoint(VRContextUtil.FindOVRHeadTransform().position).z > 0f)
			{
				vector2.x *= -1f;
			}
			float num = (vector2.x + 0.5f) * (float)Screen.width;
			float num2 = (vector2.y + 0.5f) * (float)Screen.height;
			this.MoveCursorTo((int)num, (int)num2);
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00006478 File Offset: 0x00004678
		internal void OnBeforeGUI()
		{
			if (Event.current.type == EventType.Repaint)
			{
				this.prevRT = RenderTexture.active;
				RenderTexture.active = this.guiTexture;
				GL.Clear(true, true, Color.clear);
				if (this.NowAttached)
				{
					Vector3 ovrVirtualMouseCurrentSidePos = UICameraUtil.GetOvrVirtualMouseCurrentSidePos();
					if ((int)ovrVirtualMouseCurrentSidePos.x != (int)this.lastOvrVirtualMousePos.x || (int)ovrVirtualMouseCurrentSidePos.y != (int)this.lastOvrVirtualMousePos.y)
					{
						this.MoveCursorTo((int)(ovrVirtualMouseCurrentSidePos.x * (float)Screen.width / 1280f), (int)(ovrVirtualMouseCurrentSidePos.y * (float)Screen.height / 720f));
						this.lastOvrVirtualMousePos = ovrVirtualMouseCurrentSidePos;
					}
				}
				if (this.bMoveCursor)
				{
					this.setCursorPos(this.newCursorPos.x, this.newCursorPos.y);
				}
			}
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00006548 File Offset: 0x00004748
		private void OnGUI()
		{
			GUI.depth = -999;
			if (!this.NowAttached && this.isMouseWithinWindow() && this.mouseCursorTexture != null && Event.current.type == EventType.Repaint)
			{
				GUI.DrawTexture(this.getRawMousePosition(), this.mouseCursorTexture);
			}
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00002BB2 File Offset: 0x00000DB2
		internal void OnAfterGUI()
		{
			if (Event.current.type == EventType.Repaint)
			{
				RenderTexture.active = this.prevRT;
				this.bMoveCursor = false;
			}
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000065A0 File Offset: 0x000047A0
		private void setCursorPos(float posX, float posY)
		{
			if (Screen.fullScreen)
			{
				RECT windowRect = Win32API.GetWindowRect();
				Win32API.SetCursorPos((int)posX, windowRect.Height - (int)posY);
				return;
			}
			POINT point = new POINT((int)posX, (int)((float)Screen.height - posY));
			if (point.x < 0 || Screen.width < point.x || point.y < 0 || Screen.height < point.y)
			{
				return;
			}
			Win32API.ClientToScreen(ref point);
			Win32API.SetCursorPos(point.x, point.y);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00006624 File Offset: 0x00004824
		private bool isMouseWithinWindow()
		{
			Vector3 mousePosition = Input.mousePosition;
			return 0f <= mousePosition.x && mousePosition.x <= (float)Screen.width && 0f <= mousePosition.y && mousePosition.y <= (float)Screen.height;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00006674 File Offset: 0x00004874
		private Rect getRawMousePosition()
		{
			Vector3 mousePosition = Input.mousePosition;
			float num = mousePosition.x * (float)this.guiTexture.width / (float)Screen.width;
			float num2 = mousePosition.y * (float)this.guiTexture.height / (float)Screen.height;
			return new Rect(num, (float)this.guiTexture.height - num2, 32f, 32f);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00002BD3 File Offset: 0x00000DD3
		public static void ResetIMGUIPosition()
		{
			IMGUIQuad.Instance.resetIMGUIPosition();
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x000066DC File Offset: 0x000048DC
		private void resetIMGUIPosition()
		{
			if (this.NowAttached)
			{
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
				return;
			}
			Transform transform = VRContextUtil.FindOVRHeadTransform() ?? GameMain.Instance.OvrMgr.OvrCamera.transform;
			base.transform.position = transform.TransformPoint(new Vector3(0f, 0f, 0.4f));
			base.transform.rotation = Quaternion.LookRotation(transform.TransformVector(new Vector3(0f, 0f, 1f)));
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00006780 File Offset: 0x00004980
		public void UpdateAspect()
		{
			if (!this.NowAttached)
			{
				float y = base.transform.localScale.y;
				float num = y / (float)Screen.height * (float)Screen.width;
				base.transform.localScale = new Vector3(num, y, 0.0001f);
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x000067D0 File Offset: 0x000049D0
		public void UpdateMaterial(bool transparent)
		{
			try
			{
				this.renderer = this.renderer ?? this.screenQuad.GetComponent<Renderer>();
				if (transparent)
				{
					string text = ((this.attachParent == IMGUIQuad.AttachParent.OvrTablet) ? "Unlit/Transparent" : "Unlit/Transparent OVR UI");
					this.renderer.material = new Material(Shader.Find(text));
				}
				else
				{
					this.renderer.material = new Material(Shader.Find("Unlit/Texture"));
				}
				this.renderer.sortingOrder = ((this.NowAttached && transparent) ? 200 : 0);
				this.renderer.material.mainTexture = this.guiTexture;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		// Token: 0x04000070 RID: 112
		private const float SCALE_Z = 0.0001f;

		// Token: 0x04000071 RID: 113
		private static IMGUIQuad _instance;

		// Token: 0x04000072 RID: 114
		private static bool bVisble = Settings.Instance.GetBoolValue("IMGUIVisble", false);

		// Token: 0x04000073 RID: 115
		private static bool bAttach = Settings.Instance.GetBoolValue("IMGUIAttach", false);

		// Token: 0x04000074 RID: 116
		private static bool bTransparent = Settings.Instance.GetBoolValue("IMGUITransparent", false);

		// Token: 0x04000075 RID: 117
		private static bool bNoVRController = Settings.Instance.GetBoolValue("IMGUINoVRController", false);

		// Token: 0x04000076 RID: 118
		private RenderTexture guiTexture;

		// Token: 0x04000077 RID: 119
		private Texture2D mouseCursorTexture;

		// Token: 0x04000078 RID: 120
		private float uiSize = 0.5f;

		// Token: 0x04000079 RID: 121
		private RenderTexture prevRT;

		// Token: 0x0400007A RID: 122
		private BoxCollider collider;

		// Token: 0x0400007B RID: 123
		private GameObject screenQuad;

		// Token: 0x0400007C RID: 124
		private Renderer renderer;

		// Token: 0x0400007D RID: 125
		private Vector3 lastMousePos;

		// Token: 0x0400007E RID: 126
		private Vector3 lastOvrVirtualMousePos;

		// Token: 0x0400007F RID: 127
		private Vector2 newCursorPos = Vector2.zero;

		// Token: 0x04000080 RID: 128
		private bool bMoveCursor;

		// Token: 0x04000081 RID: 129
		private IMGUIQuad.AttachParent attachParent;

		// Token: 0x0200001A RID: 26
		private enum AttachParent
		{
			// Token: 0x04000083 RID: 131
			None,
			// Token: 0x04000084 RID: 132
			GUIQuad,
			// Token: 0x04000085 RID: 133
			OvrTablet
		}
	}
}
