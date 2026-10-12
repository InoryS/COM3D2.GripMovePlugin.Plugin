using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

internal class YotogiCommandTool : MonoBehaviour
{
	public Transform headTransform;

	private readonly float commandLineSize = 0.02f;

	private bool alwaysLookAtCamera;

	private Dictionary<GameObject, UIButtonAlias> cache = new();

	private GameObject commandListPlane;

	private float commandToolSize = 1f;

	private GameObject commandUnitGo;

	private int currentCommandIndex = -1;

	private float currentCommandLoc;

	private bool dirty;

	private float maxY;

	private float minY;

	private float scrollSpeed = 1f;

	private UIButtonAlias selectedCommandItem;

	private void Awake()
	{
	}

	private void Reset()
	{
		cache = new Dictionary<GameObject, UIButtonAlias>();
		commandUnitGo = null;
		selectedCommandItem = null;
		maxY = 0f;
		minY = 0f;
		currentCommandIndex = 0;
		currentCommandLoc = 0f;
		UpdateCommandListLocation(0f);
		ClearUIButtonAliasObjects();
		dirty = false;
	}

	private void Start()
	{
		commandToolSize = Settings.Instance.GetFloatValue("YotogiCommandToolSize", 1f);
		scrollSpeed = Settings.Instance.GetFloatValue("YotogiCommandToolSpeed", 1f);
		alwaysLookAtCamera = Settings.Instance.GetBoolValue("YotogiCommandToolLookAtCamera", false);
		commandListPlane = new GameObject("YotogiCommandListOnHand");
		commandListPlane.transform.parent = transform;
		commandListPlane.transform.localPosition = Vector3.zero;
		commandListPlane.transform.localRotation = Quaternion.identity;
		commandListPlane.transform.localScale = Vector3.one * commandToolSize;
		commandListPlane.layer = LayerMask.NameToLayer("Ignore Raycast");
		Reset();
	}

	private void Update()
	{
		try
		{
			Update2();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.StackTrace);
		}
	}

	private void OnEnable()
	{
		if (commandListPlane != null)
		{
			commandListPlane.SetActive(true);
		}
	}

	private void OnDisable()
	{
		Reset();
		if (commandListPlane != null)
		{
			commandListPlane.SetActive(false);
		}
	}

	private void OnLevelWasLoaded(int level) => Reset();

	private void ClearUIButtonAliasObjects()
	{
		if (gameObject != null)
		{
			UIButtonAlias[] componentsInChildren =
				transform.gameObject.GetComponentsInChildren<UIButtonAlias>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Destroy(componentsInChildren[i].gameObject);
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
		if (commandUnitGo == null || !commandUnitGo.activeInHierarchy ||
		    !GameMain.Instance.MainCamera.IsFadeStateNon())
		{
			return;
		}

		foreach (object obj in commandUnitGo.transform)
		{
			var transform = (Transform)obj;
			if (transform.name != null && transform.name.StartsWith(text) &&
			    transform.gameObject != null)
			{
				int count = cache.Count;
				GameObject gameObject = transform.gameObject;
				if (!cache.ContainsKey(gameObject))
				{
					UIButton component = gameObject.GetComponent<UIButton>();
					if (component != null)
					{
						var gameObject2 = new GameObject("UIButtonAlias_" + transform.name);
						UIButtonAlias uibuttonAlias = gameObject2.AddComponent<UIButtonAlias>();
						uibuttonAlias.SetUIButton(component);
						uibuttonAlias.commandTool = this;
						cache.Add(gameObject, uibuttonAlias);
						gameObject2.transform.parent = commandListPlane.transform;
						gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
						gameObject2.transform.localRotation = Quaternion.identity;
						gameObject2.transform.localScale =
							gameObject2.transform.localScale * commandToolSize;
						if (count % 2 != 0)
						{
							minY = -1 * (count + 1) / 2;
							gameObject2.transform.position += commandListPlane.transform.up *
							                                  commandLineSize * commandToolSize *
							                                  minY;
						}
						else
						{
							maxY = count / 2;
							gameObject2.transform.position += commandListPlane.transform.up *
							                                  commandLineSize * commandToolSize *
							                                  maxY;
						}

						component.gameObject.AddComponent<UIButtonDestroyListerner>().tool = this;
						UpdateCommandListLocation(0f);
					}
				}
			}
		}
	}

	public void InvokeCurrentCommand()
	{
		if (cache.Count == 0 || selectedCommandItem == null)
		{
			return;
		}

		try
		{
			GameObject gameObject = selectedCommandItem.uiButton.gameObject;
			if (gameObject.activeInHierarchy)
			{
				gameObject.SendMessage("OnClick");
			}
		}
		catch (Exception)
		{
		}
	}

	public bool IsSelected(UIButtonAlias alias) => selectedCommandItem == alias;

	private void LookAtVRHeadCamera()
	{
		if (alwaysLookAtCamera)
		{
			if (headTransform == null)
			{
				Component component = CM2COM.FindOvrCamera();
				if (component != null)
				{
					headTransform = component.gameObject.transform;
				}
			}

			if (headTransform != null)
			{
				Vector3 vector = headTransform.position - transform.position;
				transform.rotation = Quaternion.LookRotation(-1f * vector);
				return;
			}

			transform.rotation = Quaternion.LookRotation(-1f * transform.parent.up);
		}
	}

	public void OnUIButtonDestroy(GameObject uiButton)
	{
		if (cache.ContainsKey(uiButton))
		{
			dirty = true;
		}
	}

	private void Update2()
	{
		if (dirty)
		{
			Reset();
		}

		if (commandUnitGo == null)
		{
			commandUnitGo =
				GameObject.Find(
					"/UI Root/YotogiPlayPanel/CommandViewer/SkillViewer/MaskGroup/SkillGroup/CommandParent/CommandUnit");
		}

		if (commandUnitGo != null)
		{
			InstantiateCommandAliases();
		}

		LookAtVRHeadCamera();
	}

	public void UpdateCommandListLocation(float diff)
	{
		currentCommandLoc += diff * scrollSpeed;
		if (currentCommandLoc > maxY + 0.45f)
		{
			currentCommandLoc = maxY + 0.45f;
		}

		if (currentCommandLoc < minY - 0.45f)
		{
			currentCommandLoc = minY - 0.45f;
		}

		int num = (int)Math.Round(currentCommandLoc);
		if (num < 0)
		{
			currentCommandIndex = -1 * num * 2 - 1;
		}
		else
		{
			currentCommandIndex = num * 2;
		}

		if (commandListPlane != null)
		{
			commandListPlane.transform.position = transform.position + transform.right * 0.06f;
			commandListPlane.transform.position += -1f * commandListPlane.transform.up *
			                                       commandLineSize * commandToolSize *
			                                       currentCommandLoc;
		}

		if (cache.Count == 0)
		{
			selectedCommandItem = null;
		}

		try
		{
			selectedCommandItem = cache.ElementAt(currentCommandIndex).Value;
		}
		catch (Exception)
		{
			selectedCommandItem = null;
		}
	}
}
