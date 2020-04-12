using Assets.Scripts.Components;
using Assets.Scripts.Pools;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(DestroyMatchGroupsSystem))]
	[UpdateInGroup(typeof(MatchLogicGroup))]
	public class DestroyBallsSystem : ComponentSystem
	{
		private GameObjectPool<Transform> _effectsPool;
		private EntityQuery _destroyedBallsGroup;
		private EntityQuery _systemStateGroup;

		public void Init(Transform destroyEffectPrefab)
		{
			_effectsPool = new GameObjectPool<Transform>(new GameObject("DestroyEffects").transform, destroyEffectPrefab, 5);
			_destroyedBallsGroup = GetEntityQuery(ComponentType.ReadOnly<Destroyed>(), ComponentType.ReadOnly<CellLink>(), ComponentType.ReadOnly<Translation>());
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>());
		}

		protected override void OnUpdate()
		{
			if (_destroyedBallsGroup.IsEmptyIgnoreFilter)
			{
				return;
			}

			var translations = _destroyedBallsGroup.ToComponentDataArray<Translation>(Allocator.TempJob);
			var cellLinks = _destroyedBallsGroup.ToComponentDataArray<CellLink>(Allocator.TempJob);

			for (int i = 0; i < translations.Length; i++)
			{
				EntityManager.RemoveComponent<BallLink>(cellLinks[i].Value);
				var effect = _effectsPool.GetObject();
				effect.position = translations[i].Value;
				effect.gameObject.SetActive(true);
			}

			if (translations.Length > 0)
			{
				EntityManager.AddComponentData(_systemStateGroup.GetSingletonEntity(), new Delay {Value = 0.5f});
			}

			translations.Dispose();
			cellLinks.Dispose();
		}
	}
}