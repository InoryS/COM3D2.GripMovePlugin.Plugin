using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200000C RID: 12
	internal class DoubleSidePlane
	{
		// Token: 0x06000038 RID: 56 RVA: 0x0000255E File Offset: 0x0000075E
		public static GameObject Create(GameObject go, Material mat, float zScale)
		{
			return DoubleSidePlane.Create(go, mat, 1U, 1U, zScale);
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00004CF0 File Offset: 0x00002EF0
		public static GameObject Create(GameObject go, Material mat, uint divX, uint divY, float zScale)
		{
			int num = (int)(divX + 1U);
			int num2 = (int)(divY + 1U);
			float num3 = 1f / divX;
			float num4 = 1f / divY;
			Vector3[] array = new Vector3[num * num2 * 2];
			Vector2[] array2 = new Vector2[num * num2 * 2];
			int[] array3 = new int[divX * divY * 6U * 2U];
			int num5 = 0;
			int num6 = 0;
			for (int i = 0; i < 2; i++)
			{
				float num7 = 0.5f;
				float num8 = 1f;
				float num9 = (float)((i == 0) ? (-1) : 1);
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
			Mesh mesh = new Mesh();
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
}
