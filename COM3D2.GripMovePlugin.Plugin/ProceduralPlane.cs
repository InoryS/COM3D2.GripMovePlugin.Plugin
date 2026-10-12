using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralPlane : MonoBehaviour
{
	private const int DEFAULT_X_SEGMENTS = 10;

	private const int DEFAULT_Y_SEGMENTS = 10;

	private const int MIN_X_SEGMENTS = 1;

	private const int MIN_Y_SEGMENTS = 1;

	private const float DEFAULT_WIDTH = 1f;

	private const float DEFAULT_HEIGHT = 1f;

	public int xSegments = 10;

	public int ySegments = 10;

	public Vector2 topLeftOffset = Vector2.zero;

	public Vector2 topRightOffset = Vector2.zero;

	public Vector2 bottomLeftOffset = Vector2.zero;

	public Vector2 bottomRightOffset = Vector2.zero;

	public float distance = 1f;

	public float width = 1f;

	public float height = 1f;

	public float angleSpan = 160f;

	public float curviness;

	private MeshFilter meshFilter;

	private Mesh modelMesh;

	private int numVertexColumns;

	private int numVertexRows;

	public void AssignDefaultShader()
	{
		gameObject.GetComponent<MeshRenderer>().sharedMaterial =
			new Material(Shader.Find("Unlit/Texture")) { color = Color.white };
	}

	public void Rebuild()
	{
		modelMesh = new Mesh { name = "ProceduralPlaneMesh" };
		meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = modelMesh;
		if (xSegments < 1)
		{
			xSegments = 1;
		}

		if (ySegments < 1)
		{
			ySegments = 1;
		}

		numVertexColumns = xSegments + 1;
		numVertexRows = ySegments + 1;
		int num = numVertexColumns * numVertexRows;
		var array = new Vector3[num];
		var array2 = new Vector2[num];
		int[] array3 = new int[xSegments * ySegments * 2 * 3];
		float num2 = width / xSegments;
		float num3 = height / ySegments;
		float num4 = 1f / xSegments;
		float num5 = 1f / ySegments;
		float num6 = -width / 2f;
		float num7 = -width / 2f;
		float num8 = angleSpan * 0.017453292f;
		float num9 = Screen.width / (float)Screen.height;
		for (int i = 0; i < numVertexRows; i++)
		{
			for (int j = 0; j < numVertexColumns; j++)
			{
				int num10 = 0;
				Vector3 vector;
				vector = new Vector3(
					j * num2 + num6 +
					bottomLeftOffset.x * (numVertexColumns - 1 - j) / (numVertexColumns - 1) *
					(numVertexRows - 1 - i) / (numVertexRows - 1) +
					bottomRightOffset.x * j / (numVertexColumns - 1) * (numVertexRows - 1 - i) /
					(numVertexRows - 1) +
					topLeftOffset.x * (numVertexColumns - 1 - j) / (numVertexColumns - 1) * i /
					(numVertexRows - 1) +
					topRightOffset.x * j / (numVertexColumns - 1) * i / (numVertexRows - 1),
					i * num3 + num7 +
					bottomLeftOffset.y * (numVertexColumns - 1 - j) / (numVertexColumns - 1) *
					(numVertexRows - 1 - i) / (numVertexRows - 1) +
					bottomRightOffset.y * j / (numVertexColumns - 1) * (numVertexRows - 1 - i) /
					(numVertexRows - 1) +
					topLeftOffset.y * (numVertexColumns - 1 - j) / (numVertexColumns - 1) * i /
					(numVertexRows - 1) +
					topRightOffset.y * j / (numVertexColumns - 1) * i / (numVertexRows - 1) -
					(height - 1f) / 2f + num10, distance);
				float num11 = Mathf.Lerp(num9 * height * vector.x,
					Mathf.Cos(1.5707964f - vector.x * num8) * distance, Mathf.Clamp01(curviness));
				float num12 = Mathf.Sin(1.5707964f - vector.x * num8 * Mathf.Clamp01(curviness));
				int num13 = i * numVertexColumns + j;
				array[num13] = new Vector3(num11, vector.y, num12);
				if (curviness > 1f)
				{
					float num14 = curviness - 1f;
					array[num13] = Vector3.Lerp(array[num13], array[num13].normalized * distance,
						Mathf.Clamp01(num14));
				}

				array2[num13] = new Vector2(j * num4, i * num5);
				if (i != 0 && j < numVertexColumns - 1)
				{
					int num15 = (i - 1) * xSegments * 6 + j * 6;
					array3[num15] = i * numVertexColumns + j;
					array3[num15 + 1] = i * numVertexColumns + j + 1;
					array3[num15 + 2] = (i - 1) * numVertexColumns + j;
					array3[num15 + 3] = (i - 1) * numVertexColumns + j;
					array3[num15 + 4] = i * numVertexColumns + j + 1;
					array3[num15 + 5] = (i - 1) * numVertexColumns + j + 1;
				}
			}
		}

		modelMesh.Clear();
		modelMesh.vertices = array;
		modelMesh.uv = array2;
		modelMesh.triangles = array3;
		modelMesh.RecalculateNormals();
		modelMesh.RecalculateBounds();
	}

	public float TransformX(float x)
	{
		float num = Screen.width / (float)Screen.height;
		float num2 = angleSpan * 0.017453292f;
		float num3 = 1f;
		return Mathf.Lerp(num * height * x, Mathf.Cos(1.5707964f - x * (num2 / num3)) * distance,
			curviness);
	}

	public float TransformZ(float x)
	{
		float num = Screen.width / (float)Screen.height;
		float num2 = angleSpan * 0.017453292f;
		return Mathf.Sin(1.5707964f - x * num2 * curviness);
	}
}
