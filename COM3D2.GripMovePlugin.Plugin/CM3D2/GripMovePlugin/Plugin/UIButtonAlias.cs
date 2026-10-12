using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class UIButtonAlias : MonoBehaviour
	{
		private void Awake()
		{
			this.textMesh = base.gameObject.AddComponent<TextMesh>();
			this.textMesh.fontSize = 128;
			this.textRenderer = base.gameObject.GetComponent<MeshRenderer>();
			float num = 0.001f;
			base.transform.localScale = new Vector3(num, num, num);
		}

		public void SetUIButton(UIButton uiButton)
		{
			this.uiButton = uiButton;
			this.uiLabel = uiButton.gameObject.GetComponentInChildren<UILabel>();
		}

		private void Start()
		{
		}

		private void Update()
		{
			this.UpdateLabel();
		}

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

		public UIButton uiButton;

		public UILabel uiLabel;

		public MeshRenderer textRenderer;

		public TextMesh textMesh;

		public YotogiCommandTool commandTool;
	}
}
