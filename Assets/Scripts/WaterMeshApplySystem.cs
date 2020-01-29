using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

public class WaterMeshApplySystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	private VertexAttributeDescriptor[] _layout;

	private WaterMeshSystem _waterMeshSystem;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc
		{
			All = new[] {ComponentType.ReadOnly<Initialized>(), ComponentType.ReadOnly<WaterMeshComponent>(), ComponentType.ReadOnly<MeshFilter>()}
		};

		_entityQuery = GetEntityQuery(queryDesc);

		_layout = new[]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Normal,   VertexAttributeFormat.Float16, 2),
			new VertexAttributeDescriptor(VertexAttribute.Tangent,  VertexAttributeFormat.UNorm8,  4),
			new VertexAttributeDescriptor(VertexAttribute.Color,    VertexAttributeFormat.UNorm8,  4),
		};
	}

	protected override void OnStartRunning()
	{
		_waterMeshSystem = EntityManager.World.GetExistingSystem<WaterMeshSystem>();
	}

	protected override void OnUpdate()
	{
		var meshFilters = _entityQuery.ToComponentArray<MeshFilter>();
		var waterParts  = _entityQuery.ToComponentDataArray<WaterMeshComponent>(Allocator.TempJob);

		NativeArray<WaterMeshSystem.VertexData> vertsList = _waterMeshSystem.Vertices.AsArray();

		for (var i = 0; i < meshFilters.Length; i++)
		{
			var meshFilter = meshFilters[i];

			var mesh = meshFilter.sharedMesh;

			var vertexCount = (waterParts[i].PointsRange.y - waterParts[i].PointsRange.x) * 2;

			mesh.SetVertexBufferParams(vertexCount, _layout);

			if (vertsList.Length == 0)
			{
				break;
			}

			var slice = new NativeSlice<WaterMeshSystem.VertexData>(vertsList, waterParts[i].PointsRange.x * 2, vertexCount);

			var verts = new NativeArray<WaterMeshSystem.VertexData>(vertexCount, Allocator.TempJob);

			slice.CopyTo(verts);

			mesh.SetVertexBufferData(verts, 0, 0, vertexCount);

			verts.Dispose();
		}

		waterParts.Dispose();
	}
}