using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

internal class OnDestroyAction : MonoBehaviour
{
	public Action Callback { get; set; }

	private void OnDestroy()
	{
		if (Callback != null)
		{
			Callback();
		}
	}
}
