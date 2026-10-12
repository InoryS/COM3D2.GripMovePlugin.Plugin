using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

internal class UIButtonAlias : MonoBehaviour
{
	public UIButton uiButton;

	public UILabel uiLabel;

	public MeshRenderer textRenderer;

	public TextMesh textMesh;

	public YotogiCommandTool commandTool;

	private void Awake()
	{
		textMesh = gameObject.AddComponent<TextMesh>();
		textMesh.fontSize = 128;
		textRenderer = gameObject.GetComponent<MeshRenderer>();
		float num = 0.001f;
		transform.localScale = new Vector3(num, num, num);
	}

	private void Start()
	{
	}

	private void Update() => UpdateLabel();

	public void SetUIButton(UIButton uiButton)
	{
		this.uiButton = uiButton;
		uiLabel = uiButton.gameObject.GetComponentInChildren<UILabel>();
	}

	public void UpdateLabel()
	{
		if (textMesh != null && uiButton != null && uiLabel != null)
		{
			textMesh.text = uiLabel.text;
			if (!uiButton.gameObject.activeInHierarchy)
			{
				textMesh.color = Color.grey;
				return;
			}

			if (commandTool.IsSelected(this))
			{
				textMesh.color = Color.white;
				return;
			}

			textMesh.color = uiLabel.color;
		}
	}
}
