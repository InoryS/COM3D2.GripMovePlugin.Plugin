using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

internal class SlowGUI : MonoBehaviour
{
	private void OnGUI()
	{
		GUI.depth = -1000;
		if (Event.current.type == (EventType)7)
		{
			SendMessage("OnAfterGUI");
		}
	}
}
