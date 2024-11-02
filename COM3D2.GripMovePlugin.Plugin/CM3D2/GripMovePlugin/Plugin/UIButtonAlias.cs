using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200000E RID: 14
	internal class UIButtonAlias : MonoBehaviour
	{
		// Token: 0x06000043 RID: 67 RVA: 0x00004FE0 File Offset: 0x000031E0
		private void Awake()
		{
			this.textMesh = base.gameObject.AddComponent<TextMesh>();
			this.textMesh.fontSize = 128;
			this.textRenderer = base.gameObject.GetComponent<MeshRenderer>();
			float num = 0.001f;
			base.transform.localScale = new Vector3(num, num, num);
		}

		// Token: 0x06000044 RID: 68 RVA: 0x000025A5 File Offset: 0x000007A5
		public void SetUIButton(UIButton uiButton)
		{
			this.uiButton = uiButton;
			this.uiLabel = uiButton.gameObject.GetComponentInChildren<UILabel>();
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000024BB File Offset: 0x000006BB
		private void Start()
		{
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000025BF File Offset: 0x000007BF
		private void Update()
		{
			this.UpdateLabel();
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00005038 File Offset: 0x00003238
		public void UpdateLabel()
		{
			if (this.textMesh != null && this.uiButton != null && this.uiLabel != null)
			{
				this.textMesh.text = this.uiLabel.text;
				if (!this.uiButton.gameObject.activeInHierarchy)
				{
					this.textMesh.color = Color.grey;
					return;
				}
				if (this.commandTool.IsSelected(this))
				{
					this.textMesh.color = Color.white;
					return;
				}
				this.textMesh.color = this.uiLabel.color;
			}
		}

		// Token: 0x0400002A RID: 42
		public UIButton uiButton;

		// Token: 0x0400002B RID: 43
		public UILabel uiLabel;

		// Token: 0x0400002C RID: 44
		public MeshRenderer textRenderer;

		// Token: 0x0400002D RID: 45
		public TextMesh textMesh;

		// Token: 0x0400002E RID: 46
		public YotogiCommandTool commandTool;
	}
}
