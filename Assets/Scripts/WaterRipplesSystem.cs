using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class WaterRipplesSystem : ComponentSystem
{
	private EntityQuery _entityQuery;
	private EntityQuery _pointsQuery;

	protected override void OnCreate()
	{
		_entityQuery = GetEntityQuery(typeof(Initialized),
			typeof(WaterRipplesComponent), typeof(WaterMeshComponent));
		_pointsQuery = GetEntityQuery(typeof(WaterPoint));
	}

	protected override void OnUpdate()
	{
		var ripples    = _entityQuery.ToComponentDataArray<WaterRipplesComponent>(Allocator.TempJob);
		var waterParts = _entityQuery.ToComponentDataArray<WaterMeshComponent>(Allocator.TempJob);
		var points     = _pointsQuery.ToComponentDataArray<WaterPoint>(Allocator.TempJob);

		float dt = Time.DeltaTime;

		for (var i = 0; i < ripples.Length; i++)
		{
			var waterRipples = ripples[i];

			waterRipples.Timer += dt;

			if (waterRipples.Timer > 1f / waterRipples.Freq)
			{
				waterRipples.Timer = 0;

				int pointId = Random.Range(waterParts[i].PointsRange.x, waterParts[i].PointsRange.y);

				var point = points[pointId];

				point.Velocity = Random.Range(waterRipples.Power.x, waterRipples.Power.y);

				points[pointId] = point;
			}

			ripples[i] = waterRipples;
		}

		_entityQuery.CopyFromComponentDataArray(ripples);
		_pointsQuery.CopyFromComponentDataArray(points);

		waterParts.Dispose();
		ripples.Dispose();
		points.Dispose();
	}
}