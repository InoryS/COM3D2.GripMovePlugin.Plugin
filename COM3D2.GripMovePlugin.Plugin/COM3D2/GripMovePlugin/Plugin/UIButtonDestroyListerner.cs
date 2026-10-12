using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

internal class UIButtonDestroyListerner : MonoBehaviour
{
	public YotogiCommandTool tool;
	private void OnDestroy() => tool.OnUIButtonDestroy(gameObject);
}
