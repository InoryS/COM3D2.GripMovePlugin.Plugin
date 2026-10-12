using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	public class GUIQuad : MoveableGUIObject
	{
		public static bool Initialized
		{
			get
			{
				return GUIQuad.instance != null;
			}
		}

		public static GUIQuad Instance
		{
			get
			{
				return GUIQuad.instance;
			}
		}

		private void Awake()
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			base.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			this.screenSize = Settings.Instance.GetFloatValue("ScreenSize", 0.25f);
		}

		public static GUIQuad Create()
		{
			if (GUIQuad.instance != null)
			{
				return GUIQuad.instance;
			}
			GameObject gameObject = CM2COM.FindOvrScreen();
			if (gameObject == null)
			{
				return null;
			}
			GUIQuad guiquad = DoubleSidePlane.Create(null, null, 0f).AddComponent<GUIQuad>();
			GUIQuad.instance = guiquad;
			guiquad.name = "GUIQuad";
			guiquad.ovr_screen = gameObject;
			guiquad.setMeterial(gameObject.GetComponentInChildren<MeshRenderer>().material.mainTexture);
			MeshCollider component = guiquad.GetComponent<MeshCollider>();
			if (component)
			{
				component.enabled = false;
				UnityEngine.Object.DestroyImmediate(component);
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
			Transform transform = VRContextUtil.FindOVRHeadTransform() ?? GameMain.Instance.OvrMgr.OvrCamera.gameObject.transform;
			base.transform.localScale = new Vector3(this.screenSize, this.screenSize, 1f);
			base.transform.position = transform.TransformPoint(new Vector3(0f, 0f, 0.3f));
			base.transform.rotation = Quaternion.LookRotation(transform.TransformVector(new Vector3(0f, 0f, 1f)));
			base.transform.parent = transform.parent;
			this.UpdateAspect();
		}

		public MeshRenderer get_renderer()
		{
			MeshRenderer meshRenderer;
			if ((meshRenderer = this.renderer) == null)
			{
				meshRenderer = (this.renderer = base.GetComponent<MeshRenderer>());
			}
			return meshRenderer;
		}

		public void setMeterial(Texture tex)
		{
			Material material = new Material(Shader.Find("Unlit/Transparent OVR UI") ?? Shader.Find("Unlit/Transparent"));
			material.mainTexture = tex;
			this.get_renderer().material = material;
		}

		public void Hide()
		{
			this.switchVisibility(false);
		}

		public void Show()
		{
			this.switchVisibility(true);
		}

		private void switchVisibility(bool newMode)
		{
			if (this.ovr_screen != null)
			{
				base.gameObject.SetActive(newMode);
				this.setMeterial(this.ovr_screen.GetComponentInChildren<MeshRenderer>().material.mainTexture);
				this.ovr_screen.SetActive(!GameMain.Instance.CMSystem.OvrUseNewControllerType && !newMode);
			}
		}

		private void Start()
		{
			this.renderer = base.GetComponent<MeshRenderer>();
		}

		private void Update()
		{
			if (this.ovr_screen == null)
			{
				this.ovr_screen = CM2COM.FindOvrScreen();
				if (this.ovr_screen != null)
				{
					this.setMeterial(this.ovr_screen.GetComponentInChildren<MeshRenderer>().material.mainTexture);
				}
			}
			if (this.ovrUIGo == null && this.ovr_screen != null)
			{
				this.ovrUIGo = this.ovr_screen.transform.parent.gameObject;
			}
			if (this.ovrUIGo != null && this.get_renderer() != null)
			{
				this.get_renderer().enabled = this.ovrUIGo.activeSelf;
			}
		}

		public void UpdateAspect()
		{
			float y = base.transform.localScale.y;
			float num = y / 720f * 1280f;
			base.transform.localScale = new Vector3(num, y, 1f);
		}

		public void UpdateAspect(int width, int height)
		{
			float y = base.transform.localScale.y;
			float num = y / (float)height * (float)width;
			base.transform.localScale = new Vector3(num, y, 1f);
		}

		private MeshRenderer renderer;

		private GameObject ovr_screen;

		private GameObject ovrUIGo;

		private float screenSize;

		private static GUIQuad instance;
	}
}
