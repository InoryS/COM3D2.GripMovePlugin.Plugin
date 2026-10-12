using System;
using System.Collections;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

internal class IMGUIQuad : MoveableGUIObject
{
	private const float SCALE_Z = 0.0001f;

	private static IMGUIQuad _instance;

	private static bool bVisble = Settings.Instance.GetBoolValue("IMGUIVisble", false);

	private static bool bAttach = Settings.Instance.GetBoolValue("IMGUIAttach", false);

	private static bool bTransparent = Settings.Instance.GetBoolValue("IMGUITransparent", false);

	private static bool bNoVRController =
		Settings.Instance.GetBoolValue("IMGUINoVRController", false);

	private AttachParent attachParent;

	private bool bMoveCursor;

	private BoxCollider collider;

	private RenderTexture guiTexture;

	private Vector3 lastMousePos;

	private Vector3 lastOvrVirtualMousePos;

	private Texture2D mouseCursorTexture;

	private Vector2 newCursorPos = Vector2.zero;

	private RenderTexture prevRT;

	private Renderer renderer;

	private GameObject screenQuad;

	private float uiSize = 0.5f;

	public static IMGUIQuad Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GameObject("IMGUIQuad").AddComponent<IMGUIQuad>();
				_instance.crate();
				_instance.gameObject.SetActive(IsVisble);
			}

			return _instance;
		}
	}

	public static bool IsVisble
	{
		get => bVisble;
		set
		{
			bVisble = value;
			Settings.Instance.SetBoolValue("IMGUIVisble", value);
			Instance.gameObject.SetActive(value);
		}
	}

	public static bool IsAttach
	{
		get => bAttach;
		set
		{
			bAttach = value;
			Settings.Instance.SetBoolValue("IMGUIAttach", value);
			Instance.attachInGUIQuad(value);
		}
	}

	public static bool IsTransparent
	{
		get => bTransparent;
		set
		{
			if (bTransparent == value)
			{
				return;
			}

			bTransparent = value;
			Settings.Instance.SetBoolValue("IMGUITransparent", value);
			Instance.UpdateMaterial(value);
		}
	}

	public static bool IsNoVRController
	{
		get => bNoVRController;
		set
		{
			if (bNoVRController == value)
			{
				return;
			}

			bNoVRController = value;
			Settings.Instance.SetBoolValue("IMGUINoVRController", value);
		}
	}

	public bool NowAttached => attachParent > AttachParent.None;

	private void Update()
	{
		if (guiTexture && (guiTexture.width != Screen.width || guiTexture.height != Screen.height))
		{
			makeGuiTexture();
		}

		if (NowAttached && collider && collider.enabled)
		{
			collider.enabled = !NowAttached;
		}

		if (IsNoVRController)
		{
			if (Cursor.lockState == (CursorLockMode)1)
			{
				Cursor.lockState = 0;
				Cursor.visible = true;
			}

			if (Input.GetMouseButtonUp(0))
			{
				lastMousePos = Input.mousePosition;
				StartCoroutine(moveCursorAtNextFrame());
			}
		}
	}

	private void OnGUI()
	{
		GUI.depth = -999;
		if (!NowAttached && isMouseWithinWindow() && mouseCursorTexture != null &&
		    Event.current.type == (EventType)7)
		{
			GUI.DrawTexture(getRawMousePosition(), mouseCursorTexture);
		}
	}

	private void crate()
	{
		screenQuad = DoubleSidePlane.Create(null, null, 1f);
		screenQuad.name = "IMGUIScreenQuad";
		screenQuad.transform.parent = transform;
		screenQuad.transform.localPosition = Vector3.zero;
		screenQuad.transform.localRotation = Quaternion.identity;
		screenQuad.transform.localScale = Vector3.one;
		MeshCollider component = screenQuad.GetComponent<MeshCollider>();
		if (component)
		{
			component.enabled = false;
			DestroyImmediate(component);
		}

		mouseCursorTexture = new Texture2D(32, 32);
		mouseCursorTexture.LoadImage(Resource.mouse_cursor, false);
		mouseCursorTexture.Apply();
		uiSize = Settings.Instance.GetFloatValue("IMGUISize", 0.5f);
		Settings.Instance.SetFloatValue("IMGUISize", uiSize);
		collider = gameObject.AddComponent<BoxCollider>();
		collider.size = new Vector3(1f, 1f, 1f);
		collider.isTrigger = true;
		gameObject.AddComponent<Rigidbody>().isKinematic = true;
		attachInGUIQuad(IsAttach);
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		gameObject.AddComponent<FastGUI>();
		gameObject.AddComponent<SlowGUI>();
		makeGuiTexture();
	}

	private void attachInGUIQuad(bool bAttachInGui)
	{
		Transform transform = null;
		Vector3 vector;
		vector = new Vector3(1f, 1f, 0.0001f);
		if (bAttachInGui)
		{
			if (GameMain.Instance.CMSystem.OvrUseNewControllerType &&
			    GameMain.Instance.OvrMgr.OvrCamera.OvrTablet && GameMain.Instance.OvrMgr.OvrCamera
				    .OvrTablet.isActiveAndEnabled)
			{
				transform = GameMain.Instance.OvrMgr.OvrCamera.OvrTablet.transform.Find("Screen");
				if (transform)
				{
					vector = new Vector3(0.5092f, 0.2894f, 0.001500021f);
					vector.z += IsTransparent ? 0.0001f : -0.0001f;
					attachParent = AttachParent.OvrTablet;
				}
			}

			if (transform == null && GUIQuad.Instance && GUIQuad.Instance.isActiveAndEnabled)
			{
				transform = GUIQuad.Instance.transform;
				vector = new Vector3(1f, 1f, 0.0001f);
				attachParent = AttachParent.GUIQuad;
			}
		}

		if (transform == null)
		{
			Transform transform2 = VRContextUtil.FindOVRHeadTransform();
			transform = transform2 == null ? null : transform2.parent;
			float num = uiSize;
			float num2 = num / Screen.height * Screen.width;
			vector = new Vector3(num2, num, 0.0001f);
			attachParent = AttachParent.None;
		}

		this.transform.parent = transform;
		this.transform.localScale = vector;
		if (collider)
		{
			collider.enabled = !NowAttached;
			collider.size = collider.enabled ? Vector3.one : Vector3.zero;
		}

		UpdateMaterial(IsTransparent);
		if (renderer)
		{
			renderer.enabled = true;
		}

		resetIMGUIPosition();
	}

	private void makeGuiTexture()
	{
		if (guiTexture)
		{
			guiTexture.Release();
		}

		guiTexture = new RenderTexture(Screen.width, Screen.height, 16, (RenderTextureFormat)7);
		guiTexture.Create();
		UpdateMaterial(IsTransparent);
	}

	private IEnumerator moveCursorAtNextFrame()
	{
		yield return null;
		MoveCursorTo((int)lastMousePos.x, (int)lastMousePos.y);
		yield break;
	}

	public void MoveCursorTo(int x, int y)
	{
		Cursor.lockState = 0;
		Cursor.visible = true;
		bMoveCursor = true;
		if (Screen.fullScreen)
		{
			RECT windowRect = Win32API.GetWindowRect();
			x = x * windowRect.Width / Screen.width;
			y = y * windowRect.Height / Screen.height;
		}

		newCursorPos = new Vector2(x, y);
	}

	public void MoveCursorTo(Transform trans)
	{
		Vector3 vector = renderer.bounds.ClosestPoint(trans.position);
		Vector3 vector2 = transform.InverseTransformPoint(vector);
		if (transform.InverseTransformPoint(VRContextUtil.FindOVRHeadTransform().position).z > 0f)
		{
			vector2.x *= -1f;
		}

		float num = (vector2.x + 0.5f) * Screen.width;
		float num2 = (vector2.y + 0.5f) * Screen.height;
		MoveCursorTo((int)num, (int)num2);
	}

	internal void OnBeforeGUI()
	{
		if (Event.current.type == (EventType)7)
		{
			prevRT = RenderTexture.active;
			RenderTexture.active = guiTexture;
			GL.Clear(true, true, Color.clear);
			if (NowAttached)
			{
				Vector3 ovrVirtualMouseCurrentSidePos =
					UICameraUtil.GetOvrVirtualMouseCurrentSidePos();
				if ((int)ovrVirtualMouseCurrentSidePos.x != (int)lastOvrVirtualMousePos.x ||
				    (int)ovrVirtualMouseCurrentSidePos.y != (int)lastOvrVirtualMousePos.y)
				{
					MoveCursorTo((int)(ovrVirtualMouseCurrentSidePos.x * Screen.width / 1280f),
						(int)(ovrVirtualMouseCurrentSidePos.y * Screen.height / 720f));
					lastOvrVirtualMousePos = ovrVirtualMouseCurrentSidePos;
				}
			}

			if (bMoveCursor)
			{
				setCursorPos(newCursorPos.x, newCursorPos.y);
			}
		}
	}

	internal void OnAfterGUI()
	{
		if (Event.current.type == (EventType)7)
		{
			RenderTexture.active = prevRT;
			bMoveCursor = false;
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

		var point = new POINT((int)posX, (int)(Screen.height - posY));
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
		return 0f <= mousePosition.x && mousePosition.x <= Screen.width && 0f <= mousePosition.y &&
		       mousePosition.y <= Screen.height;
	}

	private Rect getRawMousePosition()
	{
		Vector3 mousePosition = Input.mousePosition;
		float num = mousePosition.x * guiTexture.width / Screen.width;
		float num2 = mousePosition.y * guiTexture.height / Screen.height;
		return new Rect(num, guiTexture.height - num2, 32f, 32f);
	}

	public static void ResetIMGUIPosition() => Instance.resetIMGUIPosition();

	private void resetIMGUIPosition()
	{
		if (NowAttached)
		{
			this.transform.localPosition = Vector3.zero;
			this.transform.localRotation = Quaternion.identity;
			return;
		}

		Transform transform = VRContextUtil.FindOVRHeadTransform() ??
		                      GameMain.Instance.OvrMgr.OvrCamera.transform;
		this.transform.position = transform.TransformPoint(new Vector3(0f, 0f, 0.4f));
		this.transform.rotation =
			Quaternion.LookRotation(transform.TransformVector(new Vector3(0f, 0f, 1f)));
	}

	public void UpdateAspect()
	{
		if (!NowAttached)
		{
			float y = transform.localScale.y;
			float num = y / Screen.height * Screen.width;
			transform.localScale = new Vector3(num, y, 0.0001f);
		}
	}

	public void UpdateMaterial(bool transparent)
	{
		try
		{
			renderer = renderer ?? screenQuad.GetComponent<Renderer>();
			if (transparent)
			{
				string text = attachParent == AttachParent.OvrTablet
					? "Unlit/Transparent"
					: "Unlit/Transparent OVR UI";
				renderer.material = new Material(Shader.Find(text));
			}
			else
			{
				renderer.material = new Material(Shader.Find("Unlit/Texture"));
			}

			renderer.sortingOrder = NowAttached && transparent ? 200 : 0;
			renderer.material.mainTexture = guiTexture;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
	}

	private enum AttachParent
	{
		None,
		GUIQuad,
		OvrTablet
	}
}
