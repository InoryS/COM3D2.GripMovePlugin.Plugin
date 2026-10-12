using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

internal class UIButtonDestroyListerner : MonoBehaviour
{
	public YotogiCommandTool tool;
	private void OnDestroy() => tool.OnUIButtonDestroy(gameObject);
}
