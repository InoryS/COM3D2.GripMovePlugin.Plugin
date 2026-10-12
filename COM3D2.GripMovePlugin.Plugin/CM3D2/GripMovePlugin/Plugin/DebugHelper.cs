using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CM3D2.GripMovePlugin.Plugin
{
	public static class DebugHelper
	{
		public static string DocRoot
		{
			get
			{
				return Environment.CurrentDirectory;
			}
		}

		public static void Analyze(Transform node, int depth, int idx, TextWriter writer)
		{
			object[] array = new object[6];
			array[0] = node.name;
			array[1] = node.tag;
			array[2] = node.gameObject.layer;
			array[3] = LayerMask.LayerToName(node.gameObject.layer);
			object[] array2 = array;
			array2[4] = node.gameObject.activeInHierarchy.ToString().ToLower();
			array2[5] = node.localPosition;
			writer.Write("\"[{0,3}] : {1}", idx, node.name);
			if (node.childCount > 0)
			{
				writer.Write("  子[{0}]", node.childCount);
			}
			writer.Write("\":");
			writer.Write("{{ \"name\": \"{0}\", \"tag\": \"{1}\", \"layerNo\": \"{2}\", \"layer\": \"{3}\", \"active\": {4}, \"position\": \"{5}\"", array2);
			if (node.GetComponent<Camera>() != null)
			{
				string[] layerNames = DebugHelper.GetLayerNames(node.GetComponent<Camera>().cullingMask);
				writer.Write(", \"culling mask\": [");
				foreach (string text in layerNames)
				{
					writer.Write("\"{0}, {1}\", ", LayerMask.NameToLayer(text), text);
				}
				writer.Write("]");
			}
			if (node.GetComponent<Canvas>() != null)
			{
				writer.Write(", \"render mode\": \"{0}\"", node.GetComponent<Canvas>().renderMode);
			}
			if (node.GetComponent<Text>() != null)
			{
				writer.Write(", \"text\":\"{0}\"", node.GetComponent<Text>().text);
			}
			if (node.GetComponent<Image>() != null && node.GetComponent<Image>().sprite != null && node.GetComponent<Image>().sprite.texture != null)
			{
				writer.Write(", \"image\":\"{0}\"", node.GetComponent<Image>().sprite.texture.name);
			}
			writer.Write(", \"components\": [");
			foreach (Component component in node.GetComponents<Component>())
			{
				writer.Write("\"{0}\", ", component.GetType().Name);
			}
			writer.Write("], \"children\": {");
			for (int k = 0; k < node.childCount; k++)
			{
				DebugHelper.Analyze(node.GetChild(k), depth + 1, k, writer);
			}
			writer.Write("}},");
		}

		public static void Dump(TextWriter writer)
		{
			writer.Write("{");
			GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
			int num = 0;
			foreach (GameObject gameObject in array)
			{
				if (gameObject.transform.parent == null)
				{
					DebugHelper.Analyze(gameObject.transform, 0, num, writer);
					num++;
				}
			}
			writer.Write("}");
		}

		public static void ExportToCollada(string path, Transform root)
		{
			using StreamWriter streamWriter = File.CreateText(path);
			streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<COLLADA version=\"1.5.0\" xmlns=\"http://www.collada.org/2008/03/COLLADASchema\">\r\n\t<asset>\r\n\t\t<contributor>\r\n\t\t\t<authoring_tool>mdlanim</authoring_tool>\r\n\t\t</contributor>\r\n\t\t<created>2015-03-05T22:34:03Z</created>\r\n\t\t<modified>2015-03-05T22:34:03Z</modified>\r\n\t\t<unit />\r\n\t\t<up_axis>Y_UP</up_axis>\r\n\t</asset>\r\n\t<library_visual_scenes>\r\n\t\t<visual_scene id=\"RootNode\" name=\"RootNode\">");
			DebugHelper.WriteNode(streamWriter, root);
			streamWriter.Write("\r\n        </visual_scene>\r\n\t</library_visual_scenes>\r\n\t<scene>\r\n\t\t<instance_visual_scene url=\"#RootNode\" />\r\n\t</scene>\r\n</COLLADA>");
		}

		public static string[] GetLayerNames(int mask)
		{
			List<string> list = new List<string>();
			for (int i = 0; i <= 31; i++)
			{
				if ((mask & (1 << i)) != 0)
				{
					list.Add(LayerMask.LayerToName(i));
				}
			}
			return (from m in list
				select m.Trim() into m
				where m.Length > 0
				select m).ToArray<string>();
		}

		public static Texture2D LoadImage(string filePath)
		{
			filePath = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Images"), filePath);
			Texture2D texture2D = null;
			if (!File.Exists(filePath))
			{
				Console.WriteLine("File " + filePath + " does not exist");
			}
			else
			{
				byte[] array = File.ReadAllBytes(filePath);
				texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(array);
			}
			return texture2D;
		}

		public static void ShowTransformMarker(Transform parent)
		{
			Transform transform = parent.Find("TransformMarker");
			if (transform == null)
			{
				transform = new GameObject("TransformMarker").transform;
				GameObject gameObject = GameObject.CreatePrimitive((PrimitiveType)3);
				gameObject.transform.localScale = new Vector3(0.05f, 0.001f, 0.001f);
				gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
				GameObject gameObject2 = GameObject.CreatePrimitive((PrimitiveType)3);
				gameObject2.transform.localScale = new Vector3(0.001f, 0.05f, 0.001f);
				gameObject2.GetComponent<MeshRenderer>().material.color = Color.green;
				GameObject gameObject3 = GameObject.CreatePrimitive((PrimitiveType)3);
				gameObject3.transform.localScale = new Vector3(0.001f, 0.001f, 0.05f);
				gameObject3.GetComponent<MeshRenderer>().material.color = Color.blue;
				gameObject.transform.parent = transform;
				gameObject.transform.localPosition = new Vector3(0.025f, 0f, 0f);
				gameObject2.transform.parent = transform;
				gameObject2.transform.localPosition = new Vector3(0f, 0.025f, 0f);
				gameObject3.transform.parent = transform;
				gameObject3.transform.localPosition = new Vector3(0f, 0f, 0.025f);
				transform.parent = parent;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
		}

		private static void WriteNode(TextWriter writer, Transform node)
		{
			node.name.Contains("_J_");
			writer.Write("<node id=\"{0}\" name=\"{0}\" sid=\"{0}\" type=\"JOINT\">\r\n<translate sid=\"translate\">{1} {2} {3}</translate>\r\n<rotate sid=\"rotateY\">0 1 0 {5}</rotate>\r\n<rotate sid=\"rotateX\">1 0 0 {4}</rotate>\r\n<rotate sid=\"rotateZ\">0 0 1 {6}</rotate>\r\n<scale sid=\"scale\">1 1 1</scale>", new object[]
			{
				node.name,
				node.localPosition.x,
				node.localPosition.y,
				node.localPosition.z,
				node.localEulerAngles.x,
				node.localEulerAngles.y,
				node.localEulerAngles.z
			});
			for (int i = 0; i < node.childCount; i++)
			{
				DebugHelper.WriteNode(writer, node.GetChild(i));
			}
			writer.Write("</node>");
		}

		public static string GetHierarchyPath(Transform self)
		{
			string text = self.gameObject.name;
			Transform transform = self.parent;
			while (transform != null)
			{
				text = transform.name + "/" + text;
				transform = transform.parent;
			}
			return text;
		}

		public static void PrintHierarchyPath(Transform self)
		{
			Debug.LogWarning(DebugHelper.GetHierarchyPath(self));
		}
	}
}
