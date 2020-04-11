using System;
using System.Collections.Generic;
using Assets.Scripts.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	public class SpawnSystem : JobComponentSystem
	{
		private Entity[] _ballPrefabs;

		public void Init(Entity[] ballPrefabs)
		{
			_ballPrefabs = ballPrefabs;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			var prefabs = new NativeArray<Entity>(_ballPrefabs, Allocator.TempJob);
			Entities
				.WithNone<Delay>()
				.WithNone<BallLink>()
				.ForEach((Entity entity, ref Spawner spawner, in Translation translation) =>
				{
					var instance = ecb.Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)]);

					ecb.SetComponent(instance, new Translation { Value = translation.Value });
					ecb.AddComponent(instance, new CellLink { Value = entity });
					ecb.AddComponent(entity, new BallLink {Value =  instance});
					ecb.AddComponent(entity, new Delay { Value = 0.2f});
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			prefabs.Dispose();
			return default;
		}
	}
}