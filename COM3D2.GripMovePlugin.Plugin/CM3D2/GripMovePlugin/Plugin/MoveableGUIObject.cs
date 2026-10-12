using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	public class MoveableGUIObject : MonoBehaviour
	{
		public void OnMoved()
		{
			foreach (Action<MonoBehaviour> action in this.onMoveLister)
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

		public List<Action<MonoBehaviour>> onMoveLister = new List<Action<MonoBehaviour>>();
	}
}
