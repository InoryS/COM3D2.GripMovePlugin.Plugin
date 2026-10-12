using System;
using System.Collections;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class IMGUIQuad : MoveableGUIObject
	{
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

		public bool NowAttached
		{
			get
			{
				return this.attachParent > IMGUIQuad.AttachParent.None;
			}
		}

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
				UnityEngine.Object.DestroyImmediate(component);
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

		private void attachInGUIQuad(bool bAttachInGui)
		{
			Transform transform = null;
			Vector3 vector;
			vector = new(1f, 1f, 0.0001f);
			if (bAttachInGui)
			{
				if (GameMain.Instance.CMSystem.OvrUseNewControllerType && GameMain.Instance.OvrMgr.OvrCamera.OvrTablet && GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.isActiveAndEnabled)
				{
					transform = GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.transform.Find("Screen");
					if (transform)
					{
						vector = new(0.5092f, 0.2894f, 0.001500021f);
						vector.z += (IMGUIQuad.IsTransparent ? 0.0001f : (-0.0001f));
						this.attachParent = IMGUIQuad.AttachParent.OvrTablet;
					}
				}
				if (transform == null && GUIQuad.Instance && GUIQuad.Instance.isActiveAndEnabled)
				{
					transform = GUIQuad.Instance.transform;
					vector = new(1f, 1f, 0.0001f);
					this.attachParent = IMGUIQuad.AttachParent.GUIQuad;
				}
			}
			if (transform == null)
			{
				Transform transform2 = VRContextUtil.FindOVRHeadTransform();
				transform = ((transform2 == null) ? null : transform2.parent);
				float num = this.uiSize;
				float num2 = num / (float)Screen.height * (float)Screen.width;
				vector = new(num2, num, 0.0001f);
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

		private void makeGuiTexture()
		{
			if (this.guiTexture)
			{
				this.guiTexture.Release();
			}
			this.guiTexture = new RenderTexture(Screen.width, Screen.height, 16, (RenderTextureFormat)7);
			this.guiTexture.Create();
			this.UpdateMaterial(IMGUIQuad.IsTransparent);
		}

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
				if (Cursor.lockState == (CursorLockMode)1)
				{
					Cursor.lockState = 0;
					Cursor.visible = true;
				}
				if (Input.GetMouseButtonUp(0))
				{
					this.lastMousePos = Input.mousePosition;
					base.StartCoroutine(this.moveCursorAtNextFrame());
				}
			}
		}

		private IEnumerator moveCursorAtNextFrame()
		{
			yield return null;
			this.MoveCursorTo((int)this.lastMousePos.x, (int)this.lastMousePos.y);
			yield break;
		}

		public void MoveCursorTo(int x, int y)
		{
			Cursor.lockState = 0;
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

		internal void OnBeforeGUI()
		{
			if (Event.current.type == (EventType)7)
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

		private void OnGUI()
		{
			GUI.depth = -999;
			if (!this.NowAttached && this.isMouseWithinWindow() && this.mouseCursorTexture != null && Event.current.type == (EventType)7)
			{
				GUI.DrawTexture(this.getRawMousePosition(), this.mouseCursorTexture);
			}
		}

		internal void OnAfterGUI()
		{
			if (Event.current.type == (EventType)7)
			{
				RenderTexture.active = this.prevRT;
				this.bMoveCursor = false;
			}
		}

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

		private bool isMouseWithinWindow()
		{
			Vector3 mousePosition = Input.mousePosition;
			return 0f <= mousePosition.x && mousePosition.x <= (float)Screen.width && 0f <= mousePosition.y && mousePosition.y <= (float)Screen.height;
		}

		private Rect getRawMousePosition()
		{
			Vector3 mousePosition = Input.mousePosition;
			float num = mousePosition.x * (float)this.guiTexture.width / (float)Screen.width;
			float num2 = mousePosition.y * (float)this.guiTexture.height / (float)Screen.height;
			return new Rect(num, (float)this.guiTexture.height - num2, 32f, 32f);
		}

		public static void ResetIMGUIPosition()
		{
			IMGUIQuad.Instance.resetIMGUIPosition();
		}

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

		public void UpdateAspect()
		{
			if (!this.NowAttached)
			{
				float y = base.transform.localScale.y;
				float num = y / (float)Screen.height * (float)Screen.width;
				base.transform.localScale = new Vector3(num, y, 0.0001f);
			}
		}

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

		private const float SCALE_Z = 0.0001f;

		private static IMGUIQuad _instance;

		private static bool bVisble = Settings.Instance.GetBoolValue("IMGUIVisble", false);

		private static bool bAttach = Settings.Instance.GetBoolValue("IMGUIAttach", false);

		private static bool bTransparent = Settings.Instance.GetBoolValue("IMGUITransparent", false);

		private static bool bNoVRController = Settings.Instance.GetBoolValue("IMGUINoVRController", false);

		private RenderTexture guiTexture;

		private Texture2D mouseCursorTexture;

		private float uiSize = 0.5f;

		private RenderTexture prevRT;

		private BoxCollider collider;

		private GameObject screenQuad;

		private Renderer renderer;

		private Vector3 lastMousePos;

		private Vector3 lastOvrVirtualMousePos;

		private Vector2 newCursorPos = Vector2.zero;

		private bool bMoveCursor;

		private IMGUIQuad.AttachParent attachParent;

		private enum AttachParent
		{
			None,
			GUIQuad,
			OvrTablet
		}
	}
}
