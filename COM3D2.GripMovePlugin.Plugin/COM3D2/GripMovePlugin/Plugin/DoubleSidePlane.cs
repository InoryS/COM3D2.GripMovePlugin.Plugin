using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

internal class DoubleSidePlane
{
	public static GameObject Create(GameObject go, Material mat, float zScale) =>
		Create(go, mat, 1U, 1U, zScale);

	public static GameObject Create(GameObject go, Material mat, uint divX, uint divY, float zScale)
	{
		int num = (int)(divX + 1U);
		int num2 = (int)(divY + 1U);
		float num3 = 1f / divX;
		float num4 = 1f / divY;
		var array = new Vector3[num * num2 * 2];
		var array2 = new Vector2[num * num2 * 2];
		int[] array3 = new int[divX * divY * 6U * 2U];
		int num5 = 0;
		int num6 = 0;
		for (int i = 0; i < 2; i++)
		{
			float num7 = 0.5f;
			float num8 = 1f;
			float num9 = i == 0 ? -1 : 1;
			float num10 = num9 * zScale;
			for (int j = 0; j < num2; j++)
			{
				float num11 = num9 * 0.5f;
				float num12 = num9 * -num3;
				float num13 = 0f;
				for (int k = 0; k < num; k++)
				{
					array[num5] = new Vector3(num11, num7, num10);
					array2[num5] = new Vector2(num13, num8);
					if (j != num2 - 1 && k != num - 1)
					{
						array3[num6++] = num5;
						array3[num6++] = num5 + 1;
						array3[num6++] = num5 + num;
						array3[num6++] = num5 + 1;
						array3[num6++] = num5 + num + 1;
						array3[num6++] = num5 + num;
					}

					num5++;
					num11 += num12;
					num13 += num3;
				}

				num7 += -num4;
				num8 += -num4;
			}
		}

		var mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		go = go ?? new GameObject();
		go.AddComponent<MeshFilter>().mesh = mesh;
		go.AddComponent<MeshRenderer>().material = mat;
		return go;
	}
}
