using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class WaterMeshSystem : JobComponentSystem
{
	private EntityQuery _pointsQuery;

	[NativeDisableParallelForRestriction] public NativeList<VertexData> Vertices = new NativeList<VertexData>(4, Allocator.Persistent);

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct VertexData
	{
		public Vector3 pos;
		public ushort  normalX, normalY;
		public Color32 tangent;
		public Color32 color;
	}

	protected override void OnCreate()
	{
		_pointsQuery = GetEntityQuery(ComponentType.ReadOnly<WaterPoint>());
	}

	[BurstCompile]
	struct WaterPointJob : IJobChunk
	{
		public ArchetypeChunkComponentType<WaterPoint> WaterPointType;

		[NativeDisableParallelForRestriction] public NativeList<VertexData> Vertices;

		[DeallocateOnJobCompletion] public NativeArray<float>   WaterSteps;
		[DeallocateOnJobCompletion] public NativeArray<int>     WaterPointsCount;
		[DeallocateOnJobCompletion] public NativeArray<Color32> TopColors;
		[DeallocateOnJobCompletion] public NativeArray<Color32> BottomColors;

		private int pointIndex;
		private int pointIndexStart;

		public float Height;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var points = chunk.GetNativeArray(WaterPointType);

			for (var i = 0; i < points.Length; i++)
			{
				var waterPoint = points[i];

				float y = -Height;

				float waterStep = WaterSteps[waterPoint.PartId];

				if (pointIndex + 1 == WaterPointsCount[waterPoint.PartId])
				{
					pointIndexStart += WaterPointsCount[waterPoint.PartId];
				}

				float x = (pointIndex - pointIndexStart) * waterStep - 0.5f * waterStep * WaterPointsCount[waterPoint.PartId];

				pointIndex++;

				Vertices.Add(new VertexData
				{
					pos   = new Vector3(x, y, 0),
					color = BottomColors[waterPoint.PartId]
				});

				Vertices.Add(new VertexData
				{
					pos   = new Vector3(x, y + waterPoint.Height, 0),
					color = TopColors[waterPoint.PartId]
				});
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		var waterPointType = GetArchetypeChunkComponentType<WaterPoint>();

		var partsQuery = GetEntityQuery(typeof(Initialized), typeof(WaterMeshComponent));

		int partsCount = partsQuery.CalculateEntityCount();

		var steps           = new NativeArray<float>(partsCount, Allocator.TempJob);
		var partPointsCount = new NativeArray<int>(partsCount, Allocator.TempJob);

		var topColors    = new NativeArray<Color32>(partsCount, Allocator.TempJob);
		var bottomColors = new NativeArray<Color32>(partsCount, Allocator.TempJob);

		var parts = partsQuery.ToComponentDataArray<WaterMeshComponent>(Allocator.TempJob);

		for (int i = 0; i < partsCount; i++)
		{
			var part = parts[i];

			steps[i] = part.Step;

			partPointsCount[i] = part.PointsRange.y - part.PointsRange.x + 1;

			topColors[i]    = part.TopColor;
			bottomColors[i] = part.BottomColor;
		}

		parts.Dispose();

		Vertices.Clear();

		var job = new WaterPointJob
		{
			Vertices         = Vertices,
			WaterPointType   = waterPointType,
			Height           = 1f,
			WaterSteps       = steps,
			WaterPointsCount = partPointsCount,
			TopColors        = topColors,
			BottomColors     = bottomColors
		};

		return job.Schedule(_pointsQuery, inputDependencies);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Vertices.Dispose();
	}
}