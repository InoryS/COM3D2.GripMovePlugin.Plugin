using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using I2.Loc;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace GearMenu;

public static class Buttons
{
	public static readonly Color DefaultFrameColor = new(1f, 1f, 1f, 0f);
	public static string Name { get; } = "CM3D2.GearMenu.Buttons";

	public static string Version { get; } = Name + " 0.0.2.0";

	public static SystemShortcut SysShortcut => GameMain.Instance.SysShortcut;

	public static UIPanel SysShortcutPanel => SysShortcut.GetComponent<UIPanel>();

	public static UISprite SysShortcutExplanation
	{
		get
		{
			FieldInfo fieldInfo = typeof(SystemShortcut).GetField("m_spriteExplanation",
				BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
			{
				return null;
			}

			return fieldInfo.GetValue(SysShortcut) as UISprite;
		}
	}

	public static GameObject Base => SysShortcut.gameObject.transform.Find("Base").gameObject;

	public static UISprite BaseSprite => Base.GetComponent<UISprite>();

	public static GameObject Grid => Base.gameObject.transform.Find("Grid").gameObject;

	public static UIGrid GridUI => Grid.GetComponent<UIGrid>();

	public static GameObject Add(PluginBase plugin, byte[] pngData, Action<GameObject> action) =>
		Add(null, plugin, pngData, action);

	public static GameObject Add(string name, PluginBase plugin, byte[] pngData,
		Action<GameObject> action)
	{
		var pluginNameAttribute =
			Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginNameAttribute)) as
				PluginNameAttribute;
		var pluginVersionAttribute =
			Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginVersionAttribute)) as
				PluginVersionAttribute;
		string text = pluginNameAttribute == null ? plugin.Name : pluginNameAttribute.Name;
		string text2 = pluginVersionAttribute == null
			? string.Empty
			: pluginVersionAttribute.Version;
		string text3 = string.Format("{0} {1}", text, text2);
		return Add(name, text3, pngData, action);
	}

	public static GameObject Add(string label, byte[] pngData, Action<GameObject> action) =>
		Add(null, label, pngData, action);

	public static GameObject Add(string name, string label, byte[] pngData,
		Action<GameObject> action)
	{
		GameObject goButton = null;
		if (Contains(name))
		{
			Remove(name);
		}

		if (action == null)
		{
			return goButton;
		}

		try
		{
			goButton = NGUITools.AddChild(Grid, UTY.GetChildObject(Grid, "Config", true));
			if (name != null)
			{
				goButton.name = name;
			}

			EventDelegate.Set(goButton.GetComponent<UIButton>().onClick,
				delegate { action(goButton); });
			UIEventTrigger component = goButton.GetComponent<UIEventTrigger>();
			EventDelegate.Add(component.onHoverOut, delegate { VisibleExplanation(null); });
			EventDelegate.Add(component.onDragStart, delegate { VisibleExplanation(null); });
			SetText(goButton, label);
			if (pngData == null)
			{
				pngData = DefaultIcon.Png;
			}

			UISprite component2 = goButton.GetComponent<UISprite>();
			component2.type = (UIBasicSprite.Type)3;
			component2.fillAmount = 0f;
			var texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(pngData);
			UITexture uitexture = NGUITools.AddWidget<UITexture>(goButton);
			uitexture.material = new Material(uitexture.shader);
			uitexture.material.mainTexture = texture2D;
			uitexture.MakePixelPerfect();
			Reposition();
		}
		catch
		{
			if (goButton != null)
			{
				NGUITools.Destroy(goButton);
				goButton = null;
			}

			throw;
		}

		return goButton;
	}

	public static void Remove(string name) => Remove(Find(name));

	public static void Remove(GameObject go)
	{
		NGUITools.Destroy(go);
		Reposition();
	}

	public static bool Contains(string name) => Find(name) != null;

	public static bool Contains(GameObject go) => Contains(go.name);

	public static void SetFrameColor(string name, Color color) => SetFrameColor(Find(name), color);

	public static void SetFrameColor(GameObject go, Color color)
	{
		UITexture componentInChildren = go.GetComponentInChildren<UITexture>();
		if (componentInChildren == null)
		{
			return;
		}

		var texture2D = componentInChildren.mainTexture as Texture2D;
		if (texture2D == null)
		{
			return;
		}

		for (int i = 1; i < texture2D.width - 1; i++)
		{
			texture2D.SetPixel(i, 0, color);
			texture2D.SetPixel(i, texture2D.height - 1, color);
		}

		for (int j = 1; j < texture2D.height - 1; j++)
		{
			texture2D.SetPixel(0, j, color);
			texture2D.SetPixel(texture2D.width - 1, j, color);
		}

		texture2D.Apply();
	}

	public static void ResetFrameColor(string name) => ResetFrameColor(Find(name));

	public static void ResetFrameColor(GameObject go) => SetFrameColor(go, DefaultFrameColor);

	public static void SetText(string name, string label) => SetText(Find(name), label);

	public static void SetText(GameObject go, string label)
	{
		string text = "System/ショートカット/" + label;
		TermData termData = LocalizationManager.Sources[0].AddTerm(text);
		if (termData != null)
		{
			for (int i = 0; i < termData.Languages.Length; i++)
			{
				termData.Languages[i] = label;
			}

			label = text;
		}

		UIEventTrigger component = go.GetComponent<UIEventTrigger>();
		component.onHoverOver.Clear();
		EventDelegate.Add(component.onHoverOver, delegate { VisibleExplanation(label); });
		if (go.GetComponent<UIButton>().state == (UIButtonColor.State)1)
		{
			VisibleExplanation(label);
		}
	}

	private static GameObject Find(string name)
	{
		Transform transform = GridUI.GetChildList().FirstOrDefault(c => c.gameObject.name == name);
		if (transform != null)
		{
			return transform.gameObject;
		}

		return null;
	}

	private static void Reposition()
	{
		SetAndCallOnReposition(GridUI);
		GridUI.repositionNow = true;
	}

	private static void SetAndCallOnReposition(UIGrid uiGrid)
	{
		string onRepositionVersion = GetOnRepositionVersion(uiGrid);
		if (onRepositionVersion == null)
		{
			return;
		}

		if (onRepositionVersion == string.Empty ||
		    string.Compare(onRepositionVersion, Version, false) < 0)
		{
			uiGrid.onReposition = new OnRepositionHandler(Version).OnReposition;
		}

		if (uiGrid.onReposition != null)
		{
			object target = uiGrid.onReposition.Target;
			if (target != null)
			{
				MethodInfo method = target.GetType().GetMethod("PreOnReposition");
				if (method != null)
				{
					method.Invoke(target, new object[0]);
				}
			}
		}
	}

	private static string GetOnRepositionVersion(UIGrid uiGrid)
	{
		if (uiGrid.onReposition == null)
		{
			return string.Empty;
		}

		object target = uiGrid.onReposition.Target;
		if (target == null)
		{
			return null;
		}

		Type type = target.GetType();
		if (type == null)
		{
			return null;
		}

		FieldInfo field = type.GetField("Version", BindingFlags.Instance | BindingFlags.Public);
		if (field == null)
		{
			return null;
		}

		string text = field.GetValue(target) as string;
		if (text == null || !text.StartsWith(Name))
		{
			return null;
		}

		return text;
	}

	private static void VisibleExplanation(string lable)
	{
		try
		{
			Type type = SysShortcut.GetType();
			MethodInfo methodInfo = type.GetMethod("VisibleExplanation",
				new[] { typeof(string), typeof(bool) });
			if (methodInfo != null)
			{
				methodInfo.Invoke(SysShortcut, new object[] { lable, lable != null });
			}
			else
			{
				methodInfo = type.GetMethod("VisibleExplanation",
					new[] { typeof(int), typeof(bool) });
				methodInfo.Invoke(SysShortcut, new object[] { int.MinValue, false });
			}
		}
		catch (Exception)
		{
			Debug.LogError("ChangeDressing:ギアメニューの登録に失敗");
		}
	}

	private class OnRepositionHandler
	{
		private static readonly string[] OnlineButtonNames =
			new[] { "Config", "Ss", "SsUi", "Shop", "ToTitle", "Info", "Exit" };

		private static readonly string[] OfflineButtonNames =
			new[] { "Config", "Ss", "SsUi", "ToTitle", "Info", "Exit" };

		public string Version;

		public OnRepositionHandler(string version)
		{
			Version = version;
		}

		public void OnReposition()
		{
		}

		public void PreOnReposition()
		{
			UIGrid gridUI = GridUI;
			UISprite baseSprite = BaseSprite;
			float num = 0.75f;
			float pixelSizeAdjustment = UIRoot.GetPixelSizeAdjustment(Base);
			gridUI.cellHeight = gridUI.cellWidth;
			gridUI.arrangement = (UIGrid.Arrangement)2;
			gridUI.sorting = 0;
			gridUI.pivot = (UIWidget.Pivot)2;
			gridUI.maxPerLine =
				(int)(Screen.width / (gridUI.cellWidth / pixelSizeAdjustment) * num);
			List<Transform> childList = gridUI.GetChildList();
			int count = childList.Count;
			int num2 = Math.Min(gridUI.maxPerLine, count);
			int num3 = Math.Max(1, (count - 1) / gridUI.maxPerLine + 1);
			int num4 = (int)(gridUI.cellWidth * 3f / 2f + 8f);
			int num5 = (int)(gridUI.cellHeight / 2f);
			float num6 = num5 * 1.5f + 1f;
			baseSprite.pivot = (UIWidget.Pivot)2;
			baseSprite.width = (int)(num4 + gridUI.cellWidth * num2);
			baseSprite.height = (int)(num5 + gridUI.cellHeight * num3 + 2f);
			Base.transform.localPosition = new Vector3(946f, 502f + num6, 0f);
			Grid.transform.localPosition =
				new Vector3(-2f + (-(float)num2 - 1f + num3 - 1f) * gridUI.cellWidth, -1f - num6,
					0f);
			int num7 = 0;
			string[] array = GameMain.Instance.CMSystem.NetUse
				? OnlineButtonNames
				: OfflineButtonNames;
			foreach (Transform transform in childList)
			{
				int num8 = num7++;
				int num9 = Array.IndexOf(array, transform.gameObject.name);
				if (num9 >= 0)
				{
					num8 = num9;
				}

				float num10 = (-(float)num8 % gridUI.maxPerLine + num2 - 1f) * gridUI.cellWidth;
				float num11 = num8 / gridUI.maxPerLine * gridUI.cellHeight;
				transform.localPosition = new Vector3(num10, -num11, 0f);
			}

			UISprite sysShortcutExplanation = SysShortcutExplanation;
			Vector3 localPosition = sysShortcutExplanation.gameObject.transform.localPosition;
			localPosition.y = Base.transform.localPosition.y - baseSprite.height -
			                  sysShortcutExplanation.height;
			sysShortcutExplanation.gameObject.transform.localPosition = localPosition;
		}
	}
}
