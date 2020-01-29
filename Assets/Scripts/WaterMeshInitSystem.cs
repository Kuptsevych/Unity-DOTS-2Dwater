using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class WaterMeshInitSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	private Vector3[][] _vertices;

	private int _pointIndex;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc
		{
			None = new[] {ComponentType.ReadOnly<Initialized>()},
			All  = new[] {ComponentType.ReadOnly<WaterMeshComponent>(), ComponentType.ReadOnly<MeshFilter>()}
		};

		_entityQuery = GetEntityQuery(queryDesc);
	}

	protected override void OnUpdate()
	{
		var waterParts = _entityQuery.ToComponentDataArray<WaterMeshComponent>(Allocator.TempJob);
		var entities   = _entityQuery.ToEntityArray(Allocator.TempJob);

		MeshFilter[] meshes = _entityQuery.ToComponentArray<MeshFilter>();

		EntityArchetype waterPointArchetype = EntityManager.CreateArchetype(typeof(WaterPoint));

		for (var i = 0; i < waterParts.Length; i++)
		{
			_vertices = new Vector3[waterParts.Length][];

			WaterMeshComponent waterMeshComponent = waterParts[i];

			waterMeshComponent.PartId = i;

			MeshFilter meshFilter = meshes[i];

			int pointCount = Mathf.FloorToInt(waterMeshComponent.Size.x / waterMeshComponent.Step);

			meshFilter.mesh = GenerateWaterMesh(pointCount, waterMeshComponent.Step, waterMeshComponent.Size.y,
				waterMeshComponent.BottomColor,             waterMeshComponent.TopColor);

			_vertices[i] = new Vector3[meshFilter.mesh.vertexCount];

			float height = meshFilter.mesh.vertices[1].y;

			waterMeshComponent.PointsRange = new int2(_pointIndex, _pointIndex + pointCount);

			_pointIndex += pointCount;

			for (int j = 0; j < pointCount; j++)
			{
				Entity entity = EntityManager.CreateEntity(waterPointArchetype);

				var waterPoint = new WaterPoint {PartId = i, TargetHeight = height};

				EntityManager.SetComponentData(entity, waterPoint);
			}

			waterParts[i] = waterMeshComponent;

			PostUpdateCommands.AddComponent(entities[i], new Initialized());
		}

		_entityQuery.CopyFromComponentDataArray(waterParts);

		entities.Dispose();
		waterParts.Dispose();
	}

	private Mesh GenerateWaterMesh(int pointsCount, float waterStep, float height, Color bottomColor, Color topColor)
	{
		var mesh = new Mesh();

		int verticesCount = pointsCount * 2;

		var vertices = new Vector3[verticesCount];

		var uvs = new Vector2[verticesCount];

		int trianglesCount = (verticesCount) * 3;

		var triangles = new int[trianglesCount];

		var colors = new Color[verticesCount];

		for (int i = 0; i < verticesCount; i++)
		{
			float y = -height * 0.5f;

			if (i % 2f > 0)
			{
				y         += height;
				colors[i] =  topColor;
			}
			else
			{
				colors[i] = bottomColor;
			}

			float x = Mathf.Floor(i * 0.5f) * waterStep - 0.5f * waterStep * pointsCount;

			vertices[i] = new Vector3(x, y, 0);
		}

		for (int i = 0; i < verticesCount; i++)
		{
			uvs[i] = new Vector2(vertices[i].x / pointsCount, vertices[i].y / height);
		}

		for (int i = 0; i < pointsCount - 1; i++)
		{
			int index = i * 6;

			int idx = i * 2;

			triangles[index]     = idx;
			triangles[index + 1] = idx + 1;
			triangles[index + 2] = idx + 3;

			triangles[index + 3] = idx + 3;
			triangles[index + 4] = idx + 2;
			triangles[index + 5] = idx;
		}

		mesh.Clear();


		mesh.vertices  = vertices;
		mesh.triangles = triangles;
		mesh.uv        = uvs;
		mesh.colors    = colors;

		mesh.RecalculateNormals();

		return mesh;
	}
}