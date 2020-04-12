using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateInGroup(typeof(MatchLogicGroup))]
	public class BorderSpawnSystem : JobComponentSystem
	{
		private Entity[] _ballPrefabs;
		private EntityQuery _systemStateGroup;

		public void Init(Entity[] ballPrefabs)
		{
			_ballPrefabs = ballPrefabs;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>(), ComponentType.ReadOnly<Gravity>(), ComponentType.Exclude<Delay>());
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (_systemStateGroup.IsEmptyIgnoreFilter)
			{
				return default;
			}

			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			var prefabs = new NativeArray<Entity>(_ballPrefabs, Allocator.TempJob);
			var gravityArray = _systemStateGroup.ToComponentDataArray<Gravity>(Allocator.TempJob);
			var gravity = gravityArray[0];

			Entities
				.WithNone<Delay>()
				.WithNone<BallLink>()
				.ForEach((Entity entity, in Spawner spawner, in Translation translation, in Border border) =>
				{
					if (!border.RequiredGravity.Equals(gravity.Value))
					{
						return;
					}

					var prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
					SpawnSystem.SpawnBall(ecb, prefab, translation, spawner, entity);
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			prefabs.Dispose();
			gravityArray.Dispose();
			return default;
		}

	}
}