using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

public class GUIQuad : MoveableGUIObject
{
	private GameObject ovrUIGo;

	private GameObject ovr_screen;

	private MeshRenderer renderer;

	private float screenSize;
	public static bool Initialized => Instance != null;

	public static GUIQuad Instance { get; private set; }

	private void Awake()
	{
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		screenSize = Settings.Instance.GetFloatValue("ScreenSize", 0.25f);
	}

	private void Start() => renderer = GetComponent<MeshRenderer>();

	private void Update()
	{
		if (ovr_screen == null)
		{
			ovr_screen = CM2COM.FindOvrScreen();
			if (ovr_screen != null)
			{
				setMeterial(ovr_screen.GetComponentInChildren<MeshRenderer>().material.mainTexture);
			}
		}

		if (ovrUIGo == null && ovr_screen != null)
		{
			ovrUIGo = ovr_screen.transform.parent.gameObject;
		}

		if (ovrUIGo != null && get_renderer() != null)
		{
			get_renderer().enabled = ovrUIGo.activeSelf;
		}
	}

	public static GUIQuad Create()
	{
		if (Instance != null)
		{
			return Instance;
		}

		GameObject gameObject = CM2COM.FindOvrScreen();
		if (gameObject == null)
		{
			return null;
		}

		GUIQuad guiquad = DoubleSidePlane.Create(null, null, 0f).AddComponent<GUIQuad>();
		Instance = guiquad;
		guiquad.name = "GUIQuad";
		guiquad.ovr_screen = gameObject;
		guiquad.setMeterial(gameObject.GetComponentInChildren<MeshRenderer>().material.mainTexture);
		MeshCollider component = guiquad.GetComponent<MeshCollider>();
		if (component)
		{
			component.enabled = false;
			DestroyImmediate(component);
		}

		if (guiquad.GetComponent<BoxCollider>() == null)
		{
			guiquad.gameObject.AddComponent<BoxCollider>().isTrigger = true;
		}

		guiquad.gameObject.AddComponent<Rigidbody>().isKinematic = true;
		guiquad.ResetGUIPosition();
		return guiquad;
	}

	public void ResetGUIPosition()
	{
		Transform transform = VRContextUtil.FindOVRHeadTransform() ??
		                      GameMain.Instance.OvrMgr.OvrCamera.gameObject.transform;
		this.transform.localScale = new Vector3(screenSize, screenSize, 1f);
		this.transform.position = transform.TransformPoint(new Vector3(0f, 0f, 0.3f));
		this.transform.rotation =
			Quaternion.LookRotation(transform.TransformVector(new Vector3(0f, 0f, 1f)));
		this.transform.parent = transform.parent;
		UpdateAspect();
	}

	public MeshRenderer get_renderer()
	{
		MeshRenderer meshRenderer;
		if ((meshRenderer = renderer) == null)
		{
			meshRenderer = renderer = GetComponent<MeshRenderer>();
		}

		return meshRenderer;
	}

	public void setMeterial(Texture tex)
	{
		var material = new Material(Shader.Find("Unlit/Transparent OVR UI") ??
		                            Shader.Find("Unlit/Transparent"));
		material.mainTexture = tex;
		get_renderer().material = material;
	}

	public void Hide() => switchVisibility(false);

	public void Show() => switchVisibility(true);

	private void switchVisibility(bool newMode)
	{
		if (ovr_screen != null)
		{
			gameObject.SetActive(newMode);
			setMeterial(ovr_screen.GetComponentInChildren<MeshRenderer>().material.mainTexture);
			ovr_screen.SetActive(!GameMain.Instance.CMSystem.OvrUseNewControllerType && !newMode);
		}
	}

	public void UpdateAspect()
	{
		float y = transform.localScale.y;
		float num = y / 720f * 1280f;
		transform.localScale = new Vector3(num, y, 1f);
	}

	public void UpdateAspect(int width, int height)
	{
		float y = transform.localScale.y;
		float num = y / height * width;
		transform.localScale = new Vector3(num, y, 1f);
	}
}
