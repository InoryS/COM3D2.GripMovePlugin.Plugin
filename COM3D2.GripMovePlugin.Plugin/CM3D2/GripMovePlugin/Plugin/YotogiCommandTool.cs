using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000037 RID: 55
	internal class YotogiCommandTool : MonoBehaviour
	{
		// Token: 0x060001D3 RID: 467 RVA: 0x000024BB File Offset: 0x000006BB
		private void Awake()
		{
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0000CA6C File Offset: 0x0000AC6C
		private void ClearUIButtonAliasObjects()
		{
			if (base.gameObject != null)
			{
				UIButtonAlias[] componentsInChildren = base.transform.gameObject.GetComponentsInChildren<UIButtonAlias>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					global::UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
				}
			}
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x0000CAB4 File Offset: 0x0000ACB4
		public static YotogiCommandTool Create(Transform baseTransform)
		{
			return new GameObject("YotogiCommandTool")
			{
				transform = 
				{
					parent = baseTransform,
					position = baseTransform.position + baseTransform.right * 0.01f,
					rotation = Quaternion.LookRotation(-1f * baseTransform.up)
				},
				layer = LayerMask.NameToLayer("Ignore Raycast")
			}.AddComponent<YotogiCommandTool>();
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x0000CB34 File Offset: 0x0000AD34
		private void InstantiateCommandAliases()
		{
			string text = "cm:";
			if (this.commandUnitGo == null || !this.commandUnitGo.activeInHierarchy || !GameMain.Instance.MainCamera.IsFadeStateNon())
			{
				return;
			}
			foreach (object obj in this.commandUnitGo.transform)
			{
				Transform transform = (Transform)obj;
				if (transform.name != null && transform.name.StartsWith(text) && !(transform.gameObject == null))
				{
					int count = this.cache.Count;
					GameObject gameObject = transform.gameObject;
					if (!this.cache.ContainsKey(gameObject))
					{
						UIButton component = gameObject.GetComponent<UIButton>();
						if (!(component == null))
						{
							GameObject gameObject2 = new GameObject("UIButtonAlias_" + transform.name);
							UIButtonAlias uibuttonAlias = gameObject2.AddComponent<UIButtonAlias>();
							uibuttonAlias.SetUIButton(component);
							uibuttonAlias.commandTool = this;
							this.cache.Add(gameObject, uibuttonAlias);
							gameObject2.transform.parent = this.commandListPlane.transform;
							gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
							gameObject2.transform.localRotation = Quaternion.identity;
							gameObject2.transform.localScale = gameObject2.transform.localScale * this.commandToolSize;
							if (count % 2 != 0)
							{
								this.minY = (float)(-1 * (count + 1) / 2);
								gameObject2.transform.position += this.commandListPlane.transform.up * this.commandLineSize * this.commandToolSize * this.minY;
							}
							else
							{
								this.maxY = (float)(count / 2);
								gameObject2.transform.position += this.commandListPlane.transform.up * this.commandLineSize * this.commandToolSize * this.maxY;
							}
							component.gameObject.AddComponent<UIButtonDestroyListerner>().tool = this;
							this.UpdateCommandListLocation(0f);
						}
					}
				}
			}
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x0000CDB4 File Offset: 0x0000AFB4
		public void InvokeCurrentCommand()
		{
			if (this.cache.Count == 0 || this.selectedCommandItem == null)
			{
				return;
			}
			try
			{
				GameObject gameObject = this.selectedCommandItem.uiButton.gameObject;
				if (gameObject.activeInHierarchy)
				{
					gameObject.SendMessage("OnClick");
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000353F File Offset: 0x0000173F
		public bool IsSelected(UIButtonAlias alias)
		{
			return this.selectedCommandItem == alias;
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x0000CE18 File Offset: 0x0000B018
		private void LookAtVRHeadCamera()
		{
			if (this.alwaysLookAtCamera)
			{
				if (this.headTransform == null)
				{
					Component component = CM2COM.FindOvrCamera();
					if (component != null)
					{
						this.headTransform = component.gameObject.transform;
					}
				}
				if (this.headTransform != null)
				{
					Vector3 vector = this.headTransform.position - base.transform.position;
					base.transform.rotation = Quaternion.LookRotation(-1f * vector);
					return;
				}
				base.transform.rotation = Quaternion.LookRotation(-1f * base.transform.parent.up);
			}
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000354D File Offset: 0x0000174D
		private void OnDisable()
		{
			this.Reset();
			if (this.commandListPlane != null)
			{
				this.commandListPlane.SetActive(false);
			}
		}

		// Token: 0x060001DB RID: 475 RVA: 0x0000356F File Offset: 0x0000176F
		private void OnEnable()
		{
			if (this.commandListPlane != null)
			{
				this.commandListPlane.SetActive(true);
			}
		}

		// Token: 0x060001DC RID: 476 RVA: 0x0000358B File Offset: 0x0000178B
		private void OnLevelWasLoaded(int level)
		{
			this.Reset();
		}

		// Token: 0x060001DD RID: 477 RVA: 0x00003593 File Offset: 0x00001793
		public void OnUIButtonDestroy(GameObject uiButton)
		{
			if (this.cache.ContainsKey(uiButton))
			{
				this.dirty = true;
			}
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000CED0 File Offset: 0x0000B0D0
		private void Reset()
		{
			this.cache = new Dictionary<GameObject, UIButtonAlias>();
			this.commandUnitGo = null;
			this.selectedCommandItem = null;
			this.maxY = 0f;
			this.minY = 0f;
			this.currentCommandIndex = 0;
			this.currentCommandLoc = 0f;
			this.UpdateCommandListLocation(0f);
			this.ClearUIButtonAliasObjects();
			this.dirty = false;
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0000CF38 File Offset: 0x0000B138
		private void Start()
		{
			this.commandToolSize = Settings.Instance.GetFloatValue("YotogiCommandToolSize", 1f);
			this.scrollSpeed = Settings.Instance.GetFloatValue("YotogiCommandToolSpeed", 1f);
			this.alwaysLookAtCamera = Settings.Instance.GetBoolValue("YotogiCommandToolLookAtCamera", false);
			this.commandListPlane = new GameObject("YotogiCommandListOnHand");
			this.commandListPlane.transform.parent = base.transform;
			this.commandListPlane.transform.localPosition = Vector3.zero;
			this.commandListPlane.transform.localRotation = Quaternion.identity;
			this.commandListPlane.transform.localScale = Vector3.one * this.commandToolSize;
			this.commandListPlane.layer = LayerMask.NameToLayer("Ignore Raycast");
			this.Reset();
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000D01C File Offset: 0x0000B21C
		private void Update()
		{
			try
			{
				this.Update2();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x0000D050 File Offset: 0x0000B250
		private void Update2()
		{
			if (this.dirty)
			{
				this.Reset();
			}
			if (this.commandUnitGo == null)
			{
				this.commandUnitGo = GameObject.Find("/UI Root/YotogiPlayPanel/CommandViewer/SkillViewer/MaskGroup/SkillGroup/CommandParent/CommandUnit");
			}
			if (this.commandUnitGo != null)
			{
				this.InstantiateCommandAliases();
			}
			this.LookAtVRHeadCamera();
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000D0A4 File Offset: 0x0000B2A4
		public void UpdateCommandListLocation(float diff)
		{
			this.currentCommandLoc += diff * this.scrollSpeed;
			if (this.currentCommandLoc > this.maxY + 0.45f)
			{
				this.currentCommandLoc = this.maxY + 0.45f;
			}
			if (this.currentCommandLoc < this.minY - 0.45f)
			{
				this.currentCommandLoc = this.minY - 0.45f;
			}
			int num = (int)Math.Round((double)this.currentCommandLoc);
			if (num < 0)
			{
				this.currentCommandIndex = -1 * num * 2 - 1;
			}
			else
			{
				this.currentCommandIndex = num * 2;
			}
			if (this.commandListPlane != null)
			{
				this.commandListPlane.transform.position = base.transform.position + base.transform.right * 0.06f;
				this.commandListPlane.transform.position += -1f * this.commandListPlane.transform.up * this.commandLineSize * this.commandToolSize * this.currentCommandLoc;
			}
			if (this.cache.Count == 0)
			{
				this.selectedCommandItem = null;
			}
			try
			{
				this.selectedCommandItem = this.cache.ElementAt(this.currentCommandIndex).Value;
			}
			catch (Exception)
			{
				this.selectedCommandItem = null;
			}
		}

		// Token: 0x04000152 RID: 338
		private GameObject commandUnitGo;

		// Token: 0x04000153 RID: 339
		private GameObject commandListPlane;

		// Token: 0x04000154 RID: 340
		private Dictionary<GameObject, UIButtonAlias> cache = new Dictionary<GameObject, UIButtonAlias>();

		// Token: 0x04000155 RID: 341
		private float maxY;

		// Token: 0x04000156 RID: 342
		private float minY;

		// Token: 0x04000157 RID: 343
		private int currentCommandIndex = -1;

		// Token: 0x04000158 RID: 344
		private float scrollSpeed = 1f;

		// Token: 0x04000159 RID: 345
		private float currentCommandLoc;

		// Token: 0x0400015A RID: 346
		private float commandLineSize = 0.02f;

		// Token: 0x0400015B RID: 347
		private float commandToolSize = 1f;

		// Token: 0x0400015C RID: 348
		private UIButtonAlias selectedCommandItem;

		// Token: 0x0400015D RID: 349
		private bool dirty;

		// Token: 0x0400015E RID: 350
		private bool alwaysLookAtCamera;

		// Token: 0x0400015F RID: 351
		public Transform headTransform;
	}
}
