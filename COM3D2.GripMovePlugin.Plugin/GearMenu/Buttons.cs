using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using I2.Loc;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace GearMenu
{
	// Token: 0x02000003 RID: 3
	public static class Buttons
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000006 RID: 6 RVA: 0x00002358 File Offset: 0x00000558
		public static string Name
		{
			get
			{
				return Buttons.Name_;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000007 RID: 7 RVA: 0x0000235F File Offset: 0x0000055F
		public static string Version
		{
			get
			{
				return Buttons.Version_;
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002366 File Offset: 0x00000566
		public static GameObject Add(PluginBase plugin, byte[] pngData, Action<GameObject> action)
		{
			return Buttons.Add(null, plugin, pngData, action);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00004354 File Offset: 0x00002554
		public static GameObject Add(string name, PluginBase plugin, byte[] pngData, Action<GameObject> action)
		{
			PluginNameAttribute pluginNameAttribute = Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginNameAttribute)) as PluginNameAttribute;
			PluginVersionAttribute pluginVersionAttribute = Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginVersionAttribute)) as PluginVersionAttribute;
			string text = ((pluginNameAttribute == null) ? plugin.Name : pluginNameAttribute.Name);
			string text2 = ((pluginVersionAttribute == null) ? string.Empty : pluginVersionAttribute.Version);
			string text3 = string.Format("{0} {1}", text, text2);
			return Buttons.Add(name, text3, pngData, action);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002371 File Offset: 0x00000571
		public static GameObject Add(string label, byte[] pngData, Action<GameObject> action)
		{
			return Buttons.Add(null, label, pngData, action);
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000043D4 File Offset: 0x000025D4
		public static GameObject Add(string name, string label, byte[] pngData, Action<GameObject> action)
		{
			GameObject goButton = null;
			if (Buttons.Contains(name))
			{
				Buttons.Remove(name);
			}
			if (action == null)
			{
				return goButton;
			}
			try
			{
				goButton = NGUITools.AddChild(Buttons.Grid, UTY.GetChildObject(Buttons.Grid, "Config", true));
				if (name != null)
				{
					goButton.name = name;
				}
				EventDelegate.Set(goButton.GetComponent<UIButton>().onClick, delegate
				{
					action(goButton);
				});
				UIEventTrigger component = goButton.GetComponent<UIEventTrigger>();
				EventDelegate.Add(component.onHoverOut, delegate
				{
					Buttons.VisibleExplanation(null);
				});
				EventDelegate.Add(component.onDragStart, delegate
				{
					Buttons.VisibleExplanation(null);
				});
				Buttons.SetText(goButton, label);
				if (pngData == null)
				{
					pngData = DefaultIcon.Png;
				}
				UISprite component2 = goButton.GetComponent<UISprite>();
				component2.type = UIBasicSprite.Type.Filled;
				component2.fillAmount = 0f;
				Texture2D texture2D = new Texture2D(1, 1);
				texture2D.LoadImage(pngData);
				UITexture uitexture = NGUITools.AddWidget<UITexture>(goButton);
				uitexture.material = new Material(uitexture.shader);
				uitexture.material.mainTexture = texture2D;
				uitexture.MakePixelPerfect();
				Buttons.Reposition();
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

		// Token: 0x0600000C RID: 12 RVA: 0x0000237C File Offset: 0x0000057C
		public static void Remove(string name)
		{
			Buttons.Remove(Buttons.Find(name));
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002389 File Offset: 0x00000589
		public static void Remove(GameObject go)
		{
			NGUITools.Destroy(go);
			Buttons.Reposition();
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002396 File Offset: 0x00000596
		public static bool Contains(string name)
		{
			return Buttons.Find(name) != null;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000023A4 File Offset: 0x000005A4
		public static bool Contains(GameObject go)
		{
			return Buttons.Contains(go.name);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000023B1 File Offset: 0x000005B1
		public static void SetFrameColor(string name, Color color)
		{
			Buttons.SetFrameColor(Buttons.Find(name), color);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00004580 File Offset: 0x00002780
		public static void SetFrameColor(GameObject go, Color color)
		{
			UITexture componentInChildren = go.GetComponentInChildren<UITexture>();
			if (componentInChildren == null)
			{
				return;
			}
			Texture2D texture2D = componentInChildren.mainTexture as Texture2D;
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

		// Token: 0x06000012 RID: 18 RVA: 0x000023BF File Offset: 0x000005BF
		public static void ResetFrameColor(string name)
		{
			Buttons.ResetFrameColor(Buttons.Find(name));
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000023CC File Offset: 0x000005CC
		public static void ResetFrameColor(GameObject go)
		{
			Buttons.SetFrameColor(go, Buttons.DefaultFrameColor);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000023D9 File Offset: 0x000005D9
		public static void SetText(string name, string label)
		{
			Buttons.SetText(Buttons.Find(name), label);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00004614 File Offset: 0x00002814
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
			EventDelegate.Add(component.onHoverOver, delegate
			{
				Buttons.VisibleExplanation(label);
			});
			if (go.GetComponent<UIButton>().state == UIButtonColor.State.Hover)
			{
				Buttons.VisibleExplanation(label);
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000046C0 File Offset: 0x000028C0
		private static GameObject Find(string name)
		{
			Transform transform = Buttons.GridUI.GetChildList().FirstOrDefault((Transform c) => c.gameObject.name == name);
			if (!(transform == null))
			{
				return transform.gameObject;
			}
			return null;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000023E7 File Offset: 0x000005E7
		private static void Reposition()
		{
			Buttons.SetAndCallOnReposition(Buttons.GridUI);
			Buttons.GridUI.repositionNow = true;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00004708 File Offset: 0x00002908
		private static void SetAndCallOnReposition(UIGrid uiGrid)
		{
			string onRepositionVersion = Buttons.GetOnRepositionVersion(uiGrid);
			if (onRepositionVersion == null)
			{
				return;
			}
			if (onRepositionVersion == string.Empty || string.Compare(onRepositionVersion, Buttons.Version, false) < 0)
			{
				uiGrid.onReposition = new UIGrid.OnReposition(new Buttons.OnRepositionHandler(Buttons.Version).OnReposition);
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

		// Token: 0x06000019 RID: 25 RVA: 0x00004790 File Offset: 0x00002990
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
			if (text == null || !text.StartsWith(Buttons.Name))
			{
				return null;
			}
			return text;
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600001A RID: 26 RVA: 0x000023FE File Offset: 0x000005FE
		public static SystemShortcut SysShortcut
		{
			get
			{
				return GameMain.Instance.SysShortcut;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600001B RID: 27 RVA: 0x0000240A File Offset: 0x0000060A
		public static UIPanel SysShortcutPanel
		{
			get
			{
				return Buttons.SysShortcut.GetComponent<UIPanel>();
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600001C RID: 28 RVA: 0x000047FC File Offset: 0x000029FC
		public static UISprite SysShortcutExplanation
		{
			get
			{
				FieldInfo field = typeof(SystemShortcut).GetField("m_spriteExplanation", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field == null)
				{
					return null;
				}
				return field.GetValue(Buttons.SysShortcut) as UISprite;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002416 File Offset: 0x00000616
		public static GameObject Base
		{
			get
			{
				return Buttons.SysShortcut.gameObject.transform.Find("Base").gameObject;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600001E RID: 30 RVA: 0x00002436 File Offset: 0x00000636
		public static UISprite BaseSprite
		{
			get
			{
				return Buttons.Base.GetComponent<UISprite>();
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002442 File Offset: 0x00000642
		public static GameObject Grid
		{
			get
			{
				return Buttons.Base.gameObject.transform.Find("Grid").gameObject;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00002462 File Offset: 0x00000662
		public static UIGrid GridUI
		{
			get
			{
				return Buttons.Grid.GetComponent<UIGrid>();
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00004838 File Offset: 0x00002A38
		private static void VisibleExplanation(string lable)
		{
			try
			{
				Type type = Buttons.SysShortcut.GetType();
				MethodInfo methodInfo = type.GetMethod("VisibleExplanation", new Type[]
				{
					typeof(string),
					typeof(bool)
				});
				if (methodInfo != null)
				{
					methodInfo.Invoke(Buttons.SysShortcut, new object[]
					{
						lable,
						lable != null
					});
				}
				else
				{
					methodInfo = type.GetMethod("VisibleExplanation", new Type[]
					{
						typeof(int),
						typeof(bool)
					});
					methodInfo.Invoke(Buttons.SysShortcut, new object[] { int.MinValue, false });
				}
			}
			catch (Exception)
			{
				Debug.LogError("ChangeDressing:ギアメニューの登録に失敗");
			}
		}

		// Token: 0x04000016 RID: 22
		private static string Name_ = "CM3D2.GearMenu.Buttons";

		// Token: 0x04000017 RID: 23
		private static string Version_ = Buttons.Name_ + " 0.0.2.0";

		// Token: 0x04000018 RID: 24
		public static readonly Color DefaultFrameColor = new Color(1f, 1f, 1f, 0f);

		// Token: 0x02000004 RID: 4
		private class OnRepositionHandler
		{
			// Token: 0x06000023 RID: 35 RVA: 0x000024AC File Offset: 0x000006AC
			public OnRepositionHandler(string version)
			{
				this.Version = version;
			}

			// Token: 0x06000024 RID: 36 RVA: 0x000024BB File Offset: 0x000006BB
			public void OnReposition()
			{
			}

			// Token: 0x06000025 RID: 37 RVA: 0x00004918 File Offset: 0x00002B18
			public void PreOnReposition()
			{
				UIGrid gridUI = Buttons.GridUI;
				UISprite baseSprite = Buttons.BaseSprite;
				float num = 0.75f;
				float pixelSizeAdjustment = UIRoot.GetPixelSizeAdjustment(Buttons.Base);
				gridUI.cellHeight = gridUI.cellWidth;
				gridUI.arrangement = UIGrid.Arrangement.CellSnap;
				gridUI.sorting = UIGrid.Sorting.None;
				gridUI.pivot = UIWidget.Pivot.TopRight;
				gridUI.maxPerLine = (int)((float)Screen.width / (gridUI.cellWidth / pixelSizeAdjustment) * num);
				List<Transform> childList = gridUI.GetChildList();
				int count = childList.Count;
				int num2 = Math.Min(gridUI.maxPerLine, count);
				int num3 = Math.Max(1, (count - 1) / gridUI.maxPerLine + 1);
				int num4 = (int)(gridUI.cellWidth * 3f / 2f + 8f);
				int num5 = (int)(gridUI.cellHeight / 2f);
				float num6 = (float)num5 * 1.5f + 1f;
				baseSprite.pivot = UIWidget.Pivot.TopRight;
				baseSprite.width = (int)((float)num4 + gridUI.cellWidth * (float)num2);
				baseSprite.height = (int)((float)num5 + gridUI.cellHeight * (float)num3 + 2f);
				Buttons.Base.transform.localPosition = new Vector3(946f, 502f + num6, 0f);
				Buttons.Grid.transform.localPosition = new Vector3(-2f + (-(float)num2 - 1f + (float)num3 - 1f) * gridUI.cellWidth, -1f - num6, 0f);
				int num7 = 0;
				string[] array = (GameMain.Instance.CMSystem.NetUse ? Buttons.OnRepositionHandler.OnlineButtonNames : Buttons.OnRepositionHandler.OfflineButtonNames);
				foreach (Transform transform in childList)
				{
					int num8 = num7++;
					int num9 = Array.IndexOf<string>(array, transform.gameObject.name);
					if (num9 >= 0)
					{
						num8 = num9;
					}
					float num10 = (-(float)num8 % (float)gridUI.maxPerLine + (float)num2 - 1f) * gridUI.cellWidth;
					float num11 = (float)(num8 / gridUI.maxPerLine) * gridUI.cellHeight;
					transform.localPosition = new Vector3(num10, -num11, 0f);
				}
				UISprite sysShortcutExplanation = Buttons.SysShortcutExplanation;
				Vector3 localPosition = sysShortcutExplanation.gameObject.transform.localPosition;
				localPosition.y = Buttons.Base.transform.localPosition.y - (float)baseSprite.height - (float)sysShortcutExplanation.height;
				sysShortcutExplanation.gameObject.transform.localPosition = localPosition;
			}

			// Token: 0x04000019 RID: 25
			public string Version;

			// Token: 0x0400001A RID: 26
			private static string[] OnlineButtonNames = new string[] { "Config", "Ss", "SsUi", "Shop", "ToTitle", "Info", "Exit" };

			// Token: 0x0400001B RID: 27
			private static string[] OfflineButtonNames = new string[] { "Config", "Ss", "SsUi", "ToTitle", "Info", "Exit" };
		}
	}
}
