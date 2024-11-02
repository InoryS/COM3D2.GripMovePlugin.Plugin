using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200002C RID: 44
	public class GUIQuad : MoveableGUIObject
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000166 RID: 358 RVA: 0x00003123 File Offset: 0x00001323
		public static bool Initialized
		{
			get
			{
				return GUIQuad.instance != null;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000167 RID: 359 RVA: 0x00003130 File Offset: 0x00001330
		public static GUIQuad Instance
		{
			get
			{
				return GUIQuad.instance;
			}
		}

		// Token: 0x06000169 RID: 361 RVA: 0x000092E0 File Offset: 0x000074E0
		private void Awake()
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			base.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			this.screenSize = Settings.Instance.GetFloatValue("ScreenSize", 0.25f);
		}

		// Token: 0x0600016A RID: 362 RVA: 0x0000933C File Offset: 0x0000753C
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
				global::UnityEngine.Object.DestroyImmediate(component);
			}
			if (guiquad.GetComponent<BoxCollider>() == null)
			{
				guiquad.gameObject.AddComponent<BoxCollider>().isTrigger = true;
			}
			guiquad.gameObject.AddComponent<Rigidbody>().isKinematic = true;
			guiquad.ResetGUIPosition();
			return guiquad;
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00009400 File Offset: 0x00007600
		public void ResetGUIPosition()
		{
			Transform transform = VRContextUtil.FindOVRHeadTransform() ?? GameMain.Instance.OvrMgr.OvrCamera.gameObject.transform;
			base.transform.localScale = new Vector3(this.screenSize, this.screenSize, 1f);
			base.transform.position = transform.TransformPoint(new Vector3(0f, 0f, 0.3f));
			base.transform.rotation = Quaternion.LookRotation(transform.TransformVector(new Vector3(0f, 0f, 1f)));
			base.transform.parent = transform.parent;
			this.UpdateAspect();
		}

		// Token: 0x0600016C RID: 364 RVA: 0x000094B8 File Offset: 0x000076B8
		public MeshRenderer get_renderer()
		{
			MeshRenderer meshRenderer;
			if ((meshRenderer = this.renderer) == null)
			{
				meshRenderer = (this.renderer = base.GetComponent<MeshRenderer>());
			}
			return meshRenderer;
		}

		// Token: 0x0600016D RID: 365 RVA: 0x000094E0 File Offset: 0x000076E0
		public void setMeterial(Texture tex)
		{
			Material material = new Material(Shader.Find("Unlit/Transparent OVR UI") ?? Shader.Find("Unlit/Transparent"));
			material.mainTexture = tex;
			this.get_renderer().material = material;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x0000313F File Offset: 0x0000133F
		public void Hide()
		{
			this.switchVisibility(false);
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00003148 File Offset: 0x00001348
		public void Show()
		{
			this.switchVisibility(true);
		}

		// Token: 0x06000170 RID: 368 RVA: 0x00009520 File Offset: 0x00007720
		private void switchVisibility(bool newMode)
		{
			if (this.ovr_screen != null)
			{
				base.gameObject.SetActive(newMode);
				this.setMeterial(this.ovr_screen.GetComponentInChildren<MeshRenderer>().material.mainTexture);
				this.ovr_screen.SetActive(!GameMain.Instance.CMSystem.OvrUseNewControllerType && !newMode);
			}
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00003151 File Offset: 0x00001351
		private void Start()
		{
			this.renderer = base.GetComponent<MeshRenderer>();
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00009588 File Offset: 0x00007788
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

		// Token: 0x06000173 RID: 371 RVA: 0x00009640 File Offset: 0x00007840
		public void UpdateAspect()
		{
			float y = base.transform.localScale.y;
			float num = y / 720f * 1280f;
			base.transform.localScale = new Vector3(num, y, 1f);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00009684 File Offset: 0x00007884
		public void UpdateAspect(int width, int height)
		{
			float y = base.transform.localScale.y;
			float num = y / (float)height * (float)width;
			base.transform.localScale = new Vector3(num, y, 1f);
		}

		// Token: 0x04000108 RID: 264
		private MeshRenderer renderer;

		// Token: 0x04000109 RID: 265
		private GameObject ovr_screen;

		// Token: 0x0400010A RID: 266
		private GameObject ovrUIGo;

		// Token: 0x0400010B RID: 267
		private float screenSize;

		// Token: 0x0400010C RID: 268
		private static GUIQuad instance;
	}
}
