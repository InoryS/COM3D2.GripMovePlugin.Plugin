using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000031 RID: 49
	internal class UIUtils
	{
		// Token: 0x06000199 RID: 409 RVA: 0x0000ABA0 File Offset: 0x00008DA0
		public static GameObject GetUIRoot()
		{
			GameObject gameObject = GameObject.Find("UI Root");
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
					component.scalingStyle = UIRoot.Scaling.Constrained;
					component.shrinkPortraitUI = false;
					Camera componentInChildren = gameObject.GetComponentInChildren<Camera>();
					componentInChildren.cullingMask = LayerMask.GetMask(new string[] { "NGUI" });
					componentInChildren.renderingPath = RenderingPath.Forward;
					componentInChildren.depth = 21f;
					Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(true);
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("NGUI");
					}
					gameObject.transform.position = new Vector3(0f, 19f, 0f);
					gameObject.transform.localScale = new Vector3(0.001850139f, 0.001850139f, 0.001850139f);
				}
			}
			return gameObject;
		}

		// Token: 0x0600019A RID: 410 RVA: 0x0000ACD0 File Offset: 0x00008ED0
		public static UIAtlas FindAtlas(string s)
		{
			return new List<UIAtlas>(Resources.FindObjectsOfTypeAll<UIAtlas>()).FirstOrDefault((UIAtlas a) => a.name == s);
		}

		// Token: 0x0600019B RID: 411 RVA: 0x0000324B File Offset: 0x0000144B
		public static Transform FindChild(Transform tr, string s)
		{
			return UIUtils.FindChild(tr.gameObject, s).transform;
		}

		// Token: 0x0600019C RID: 412 RVA: 0x0000AD08 File Offset: 0x00008F08
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

				GameObject foundChild = FindChild(transform.gameObject, s);
				if (foundChild != null)
				{
					return foundChild;
				}
			}

			return null;
		}


		// Token: 0x0600019D RID: 413 RVA: 0x0000ADA0 File Offset: 0x00008FA0
		public static GameObject GetButtonTemplateGo()
		{
			if (UIUtils._goButtonTemplate == null)
			{
				UIUtils._goButtonTemplate = global::UnityEngine.Object.Instantiate<GameObject>(GameMain.Instance.gameObject.transform.Find(UIUtils.BTN_TEMPLATE).gameObject, Vector3.zero, Quaternion.identity);
				UIUtils._goButtonTemplate.GetComponent<UIButton>().onClick.Clear();
				UIUtils._goButtonTemplate.SetActive(false);
			}
			return UIUtils._goButtonTemplate;
		}

		// Token: 0x0600019E RID: 414 RVA: 0x0000325E File Offset: 0x0000145E
		public static Font GetDefaultFont()
		{
			if (UIUtils._font == null)
			{
				UIUtils._font = GameObject.Find(UIUtils.SYSTEM_UI_ROOT).GetComponentsInChildren<UILabel>()[0].trueTypeFont;
			}
			return UIUtils._font;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x0000328D File Offset: 0x0000148D
		public static UIAtlas GetSystemDialogAtlas()
		{
			return UIUtils.FindAtlas("SystemDialog");
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x0000AE10 File Offset: 0x00009010
		public static void SetChild(GameObject parent, GameObject child)
		{
			child.layer = parent.layer;
			child.transform.parent = parent.transform;
			child.transform.localPosition = Vector3.zero;
			child.transform.localScale = Vector3.one;
			child.transform.rotation = Quaternion.identity;
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x0000AE6C File Offset: 0x0000906C
		public static GameObject SetCloneChild(GameObject parent, GameObject orignal, string name)
		{
			GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(orignal, Vector3.zero, Quaternion.identity);
			if (!gameObject)
			{
				return null;
			}
			gameObject.name = name;
			UIUtils.SetChild(parent, gameObject);
			return gameObject;
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x0000AEA4 File Offset: 0x000090A4
		public static void SetToggleButtonColor(UIButton button, bool b)
		{
			Color defaultColor = button.defaultColor;
			button.defaultColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, b ? 1f : 0.5f);
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x0000AEE4 File Offset: 0x000090E4
		public static UIButton AddButton(GameObject parent, int w, int h, int fontSize, string goName, string label, EventDelegate.Callback onClick, bool bEnable)
		{
			GameObject gameObject = UIUtils.SetCloneChild(parent, UIUtils.GetButtonTemplateGo(), goName);
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
			UIUtils.SetToggleButtonColor(component, bEnable);
			return component;
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x0000AF58 File Offset: 0x00009158
		public static GameObject GetPopupListTemplateGo()
		{
			if (UIUtils._goPopupListTemplate == null)
			{
				UIUtils._goPopupListTemplate = global::UnityEngine.Object.Instantiate<GameObject>(GameMain.Instance.gameObject.transform.Find(UIUtils.POPUP_TEMPLATE).gameObject, Vector3.zero, Quaternion.identity);
				UIPopupList component = UIUtils._goPopupListTemplate.GetComponent<UIPopupList>();
				component.Clear();
				component.value = null;
				if (component.onChange.Count > 1)
				{
					component.onChange.RemoveRange(1, component.onChange.Count - 1);
				}
				UIUtils._goPopupListTemplate.SetActive(false);
			}
			return UIUtils._goPopupListTemplate;
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x0000AFF4 File Offset: 0x000091F4
		public static UIPopupList AddPopupList(GameObject parent, int w, int h, int fontSize, string goName, List<string> items, List<object> datas, EventDelegate.Callback onChange, int curIdx)
		{
			GameObject gameObject = UIUtils.SetCloneChild(parent, UIUtils.GetPopupListTemplateGo(), goName);
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

		// Token: 0x060001A6 RID: 422 RVA: 0x00003299 File Offset: 0x00001499
		public static UITable AddTable(GameObject parent, int columns, float padX, float padY, UIWidget.Pivot pivot)
		{
			UITable uitable = NGUITools.AddChild<UITable>(parent);
			uitable.pivot = pivot;
			uitable.columns = columns;
			uitable.padding = new Vector2(padX, padY);
			return uitable;
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x000032BD File Offset: 0x000014BD
		public static UITable AddTable(GameObject parent, int columns, float padX, float padY)
		{
			return UIUtils.AddTable(parent, columns, padX, padY, UIWidget.Pivot.TopLeft);
		}

		// Token: 0x0400012E RID: 302
		public const int UIRootWidth = 1920;

		// Token: 0x0400012F RID: 303
		public const int UIRootHeight = 1080;

		// Token: 0x04000130 RID: 304
		private static GameObject _goButtonTemplate;

		// Token: 0x04000131 RID: 305
		private static GameObject _goPopupListTemplate;

		// Token: 0x04000132 RID: 306
		private static Font _font;

		// Token: 0x04000133 RID: 307
		private static readonly string SYSTEM_UI_ROOT = "SystemUI Root";

		// Token: 0x04000134 RID: 308
		private static readonly string BTN_TEMPLATE = "SystemUI Root/ConfigPanel/SystemTab/System/Frame/SystemBlockLeft/SysButtonShowAlways/On";

		// Token: 0x04000135 RID: 309
		private static readonly string POPUP_TEMPLATE = "SystemUI Root/ConfigPanel/SystemTab/System/Frame/SystemBlockLeft/Resolution/PopupBlock/PopupList";
	}
}
