using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

public class MoveableGUIObject : MonoBehaviour
{
	public List<Action<MonoBehaviour>> onMoveLister = new();

	public void OnMoved()
	{
		foreach (Action<MonoBehaviour> action in onMoveLister)
		{
			try
			{
				action(this);
			}
			catch
			{
			}
		}
	}
}
