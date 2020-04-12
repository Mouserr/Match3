﻿using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(FindMatchesSystem))]
	[UpdateBefore(typeof(DestroyMatchGroupsSystem))]
	public class GravitySwitcherPlacementSystem : JobComponentSystem
	{
		private Entity[] _gravitySwitchPrefabs;

		public void Init(Entity[] gravitySwitchPrefabs)
		{
			_gravitySwitchPrefabs = gravitySwitchPrefabs;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			var prefabs = new NativeArray<Entity>(_gravitySwitchPrefabs, Allocator.TempJob);
			Entities
				.WithoutBurst()
				.ForEach((Entity entity, DynamicBuffer<MatchGroupElement> matchGroup) =>
				{
					if (matchGroup.Length >= 4)
					{
						var ballEntity = matchGroup[UnityEngine.Random.Range(0, matchGroup.Length)].BallEntity;
						var color = EntityManager.GetComponentData<Color>(ballEntity).Value;
						var cellEntity = EntityManager.GetComponentData<CellLink>(ballEntity).Value;
						ecb.AddComponent(cellEntity, new Spawner { Prefab = prefabs[color], Offset = new float3(0, 0, -1)});
						ecb.AddComponent<SpawnCount>(cellEntity);
						ecb.AddComponent(cellEntity, new SpawnLimit { Value = 1 });
					}
				}).Run();

			prefabs.Dispose();
			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}
	}
}