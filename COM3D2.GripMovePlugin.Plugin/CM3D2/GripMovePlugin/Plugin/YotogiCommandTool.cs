using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class YotogiCommandTool : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void ClearUIButtonAliasObjects()
		{
			if (base.gameObject != null)
			{
				UIButtonAlias[] componentsInChildren = base.transform.gameObject.GetComponentsInChildren<UIButtonAlias>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
				}
			}
		}

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
				if (transform.name != null && transform.name.StartsWith(text) && transform.gameObject != null)
				{
					int count = this.cache.Count;
					GameObject gameObject = transform.gameObject;
					if (!this.cache.ContainsKey(gameObject))
					{
						UIButton component = gameObject.GetComponent<UIButton>();
						if (component != null)
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

		public bool IsSelected(UIButtonAlias alias)
		{
			return this.selectedCommandItem == alias;
		}

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

		private void OnDisable()
		{
			this.Reset();
			if (this.commandListPlane != null)
			{
				this.commandListPlane.SetActive(false);
			}
		}

		private void OnEnable()
		{
			if (this.commandListPlane != null)
			{
				this.commandListPlane.SetActive(true);
			}
		}

		private void OnLevelWasLoaded(int level)
		{
			this.Reset();
		}

		public void OnUIButtonDestroy(GameObject uiButton)
		{
			if (this.cache.ContainsKey(uiButton))
			{
				this.dirty = true;
			}
		}

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
				this.selectedCommandItem = this.cache.ElementAt<KeyValuePair<GameObject, UIButtonAlias>>(this.currentCommandIndex).Value;
			}
			catch (Exception)
			{
				this.selectedCommandItem = null;
			}
		}

		private GameObject commandUnitGo;

		private GameObject commandListPlane;

		private Dictionary<GameObject, UIButtonAlias> cache = new Dictionary<GameObject, UIButtonAlias>();

		private float maxY;

		private float minY;

		private int currentCommandIndex = -1;

		private float scrollSpeed = 1f;

		private float currentCommandLoc;

		private float commandLineSize = 0.02f;

		private float commandToolSize = 1f;

		private UIButtonAlias selectedCommandItem;

		private bool dirty;

		private bool alwaysLookAtCamera;

		public Transform headTransform;
	}
}
