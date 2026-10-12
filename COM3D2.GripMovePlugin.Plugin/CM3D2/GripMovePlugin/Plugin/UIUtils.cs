using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

internal class UIUtils
{
	public const int UIRootWidth = 1920;

	public const int UIRootHeight = 1080;

	private static GameObject _goButtonTemplate;

	private static GameObject _goPopupListTemplate;

	private static Font _font;

	private static readonly string SYSTEM_UI_ROOT = "SystemUI Root";

	private static readonly string BTN_TEMPLATE =
		"SystemUI Root/ConfigPanel/SystemTab/System/Frame/SystemBlockLeft/SysButtonShowAlways/On";

	private static readonly string POPUP_TEMPLATE =
		"SystemUI Root/ConfigPanel/SystemTab/System/Frame/SystemBlockLeft/Resolution/PopupBlock/PopupList";

	public static GameObject GetUIRoot()
	{
		var gameObject = GameObject.Find("UI Root");
		if (gameObject == null)
		{
			gameObject = NGUITools.CreateUI(false).gameObject;
			if (gameObject != null)
			{
				UIRoot component = gameObject.GetComponent<UIRoot>();
				component.adjustByDPI = false;
				component.fitHeight = true;
				component.fitWidth = true;
				component.manualHeight = 1080;
				component.manualWidth = 1920;
				component.maximumHeight = 1080;
				component.minimumHeight = 1920;
				component.scalingStyle = (UIRoot.Scaling)1;
				component.shrinkPortraitUI = false;
				Camera componentInChildren = gameObject.GetComponentInChildren<Camera>();
				componentInChildren.cullingMask = LayerMask.GetMask("NGUI");
				componentInChildren.renderingPath = (RenderingPath)1;
				componentInChildren.depth = 21f;
				Transform[] componentsInChildren =
					gameObject.GetComponentsInChildren<Transform>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("NGUI");
				}

				gameObject.transform.position = new Vector3(0f, 19f, 0f);
				gameObject.transform.localScale =
					new Vector3(0.001850139f, 0.001850139f, 0.001850139f);
			}
		}

		return gameObject;
	}

	public static UIAtlas FindAtlas(string s) =>
		new List<UIAtlas>(Resources.FindObjectsOfTypeAll<UIAtlas>())
			.FirstOrDefault(a => a.name == s);

	public static Transform FindChild(Transform tr, string s) =>
		FindChild(tr.gameObject, s).transform;

	public static GameObject FindChild(GameObject go, string s)
	{
		if (go == null)
		{
			return null;
		}

		foreach (Transform transform in go.transform)
		{
			if (transform.gameObject.name == s)
			{
				return transform.gameObject;
			}

			GameObject gameObject = FindChild(transform.gameObject, s);
			if (gameObject)
			{
				return gameObject;
			}
		}

		return null;
	}

	public static GameObject GetButtonTemplateGo()
	{
		if (_goButtonTemplate == null)
		{
			_goButtonTemplate =
				Object.Instantiate(
					GameMain.Instance.gameObject.transform.Find(BTN_TEMPLATE).gameObject,
					Vector3.zero, Quaternion.identity);
			_goButtonTemplate.GetComponent<UIButton>().onClick.Clear();
			_goButtonTemplate.SetActive(false);
		}

		return _goButtonTemplate;
	}

	public static Font GetDefaultFont()
	{
		if (_font == null)
		{
			_font = GameObject.Find(SYSTEM_UI_ROOT).GetComponentsInChildren<UILabel>()[0]
				.trueTypeFont;
		}

		return _font;
	}

	public static UIAtlas GetSystemDialogAtlas() => FindAtlas("SystemDialog");

	public static void SetChild(GameObject parent, GameObject child)
	{
		child.layer = parent.layer;
		child.transform.parent = parent.transform;
		child.transform.localPosition = Vector3.zero;
		child.transform.localScale = Vector3.one;
		child.transform.rotation = Quaternion.identity;
	}

	public static GameObject SetCloneChild(GameObject parent, GameObject orignal, string name)
	{
		GameObject gameObject = Object.Instantiate(orignal, Vector3.zero, Quaternion.identity);
		if (!gameObject)
		{
			return null;
		}

		gameObject.name = name;
		SetChild(parent, gameObject);
		return gameObject;
	}

	public static void SetToggleButtonColor(UIButton button, bool b)
	{
		Color defaultColor = button.defaultColor;
		button.defaultColor =
			new Color(defaultColor.r, defaultColor.g, defaultColor.b, b ? 1f : 0.5f);
	}

	public static UIButton AddButton(GameObject parent, int w, int h, int fontSize, string goName,
		string label, EventDelegate.Callback onClick, bool bEnable)
	{
		GameObject gameObject = SetCloneChild(parent, GetButtonTemplateGo(), goName);
		gameObject.SetActive(true);
		UIButton component = gameObject.GetComponent<UIButton>();
		gameObject.GetComponent<UISprite>().SetDimensions(w, h);
		UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>();
		componentInChildren.width = w;
		componentInChildren.spacingX = 0;
		componentInChildren.supportEncoding = true;
		componentInChildren.fontSize = fontSize;
		componentInChildren.text = label;
		EventDelegate.Add(component.onClick, onClick);
		SetToggleButtonColor(component, bEnable);
		return component;
	}

	public static GameObject GetPopupListTemplateGo()
	{
		if (_goPopupListTemplate == null)
		{
			_goPopupListTemplate = Object.Instantiate(
				GameMain.Instance.gameObject.transform.Find(POPUP_TEMPLATE).gameObject,
				Vector3.zero, Quaternion.identity);
			UIPopupList component = _goPopupListTemplate.GetComponent<UIPopupList>();
			component.Clear();
			component.value = null;
			if (component.onChange.Count > 1)
			{
				component.onChange.RemoveRange(1, component.onChange.Count - 1);
			}

			_goPopupListTemplate.SetActive(false);
		}

		return _goPopupListTemplate;
	}

	public static UIPopupList AddPopupList(GameObject parent, int w, int h, int fontSize,
		string goName, List<string> items, List<object> datas, EventDelegate.Callback onChange,
		int curIdx)
	{
		GameObject gameObject = SetCloneChild(parent, GetPopupListTemplateGo(), goName);
		gameObject.SetActive(true);
		UIPopupList component = gameObject.GetComponent<UIPopupList>();
		gameObject.GetComponent<UISprite>().SetDimensions(w, h);
		component.fontSize = fontSize;
		if (items != null)
		{
			component.items = items;
		}

		if (datas != null)
		{
			component.itemData = datas;
		}

		if (curIdx < component.items.Count)
		{
			component.value = component.items[curIdx];
		}

		EventDelegate.Add(component.onChange, onChange);
		UILabel componentInChildren = component.GetComponentInChildren<UILabel>();
		componentInChildren.width = w;
		componentInChildren.fontSize = fontSize;
		return component;
	}

	public static UITable AddTable(GameObject parent, int columns, float padX, float padY,
		UIWidget.Pivot pivot)
	{
		UITable uitable = NGUITools.AddChild<UITable>(parent);
		uitable.pivot = pivot;
		uitable.columns = columns;
		uitable.padding = new Vector2(padX, padY);
		return uitable;
	}

	public static UITable AddTable(GameObject parent, int columns, float padX, float padY) =>
		AddTable(parent, columns, padX, padY, 0);
}
