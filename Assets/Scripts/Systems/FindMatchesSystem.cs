using System.Collections.Generic;
using Assets.Scripts.Components;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[AlwaysUpdateSystem]
	[UpdateAfter(typeof(CheckMovementsCompleteSystem))]
	[UpdateInGroup(typeof(MatchLogicGroup))]
	public class FindMatchesSystem : JobComponentSystem
	{
		private Field _field;
		private EntityQuery _swapQuery;
		private EntityQuery _emptyCellsQuery;
		private EntityQuery _systemStateGroup;
		private EntityQuery _movementsQuery;

		public void Init(Field field)
		{
			_field = field;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>(), ComponentType.ReadOnly<NeedsToCheck>(), ComponentType.Exclude<Delay>());
			_swapQuery = GetEntityQuery(ComponentType.ReadOnly<Swap>());
			_emptyCellsQuery = GetEntityQuery(ComponentType.ReadOnly<GridPosition>(), ComponentType.Exclude<BallLink>());
			_movementsQuery = GetEntityQuery(ComponentType.ReadOnly<Destination>());
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (_systemStateGroup.IsEmptyIgnoreFilter || !_emptyCellsQuery.IsEmptyIgnoreFilter || !_movementsQuery.IsEmptyIgnoreFilter)
			{
				return default;
			}

			var groups = new List<List<Entity>>();

			var tempGroup = new List<Entity>();

			for (var x = 0; x < _field.Width; x++)
			{
				var prevColor = -1;
				for (var y = 0; y < _field.Height; y++)
				{
					CollectGroups(x, y, ref prevColor, tempGroup, groups);
				}

				CheckTempGroup(tempGroup, groups);
			}

			for (var y = 0; y < _field.Height; y++)
			{
				var prevColor = -1;
				for (var x = 0; x < _field.Width; x++)
				{
					CollectGroups(x, y, ref prevColor, tempGroup, groups);
				}

				CheckTempGroup(tempGroup, groups);
			}

			if (!_swapQuery.IsEmptyIgnoreFilter)
			{
				var swapResult = _swapQuery.GetSingletonEntity();
				EntityManager.AddComponentData(swapResult, new SwapResult { Success = groups.Count > 0 });
			}

			foreach (var group in groups)
			{
				var matchGroupEntity = EntityManager.CreateEntity();
				var matchGroup = EntityManager.AddBuffer<MatchGroupElement>(matchGroupEntity);
				for (int i = 0; i < group.Count; i++)
				{
					matchGroup.Add(new MatchGroupElement { BallEntity = group[i] });
				}
			}

			var systemState = _systemStateGroup.GetSingletonEntity();
			EntityManager.RemoveComponent<NeedsToCheck>(systemState);
			if (groups.Count == 0)
			{
				EntityManager.AddComponent<Interactable>(systemState);
			}

			return default;
		}

		private void CollectGroups(int x, int y, ref int prevColor, List<Entity> tempGroup, List<List<Entity>> groups)
		{
			var entity = _field.GetCell(x, y);
			var ballEntity = EntityManager.GetComponentData<BallLink>(entity).Value;
			var curColor = EntityManager.GetComponentData<Color>(ballEntity).Value;
			if (prevColor != curColor)
			{
				CheckTempGroup(tempGroup, groups);
			}

			tempGroup.Add(ballEntity);
			prevColor = curColor;
		}

		private static void CheckTempGroup(List<Entity> tempGroup, List<List<Entity>> groups)
		{
			if (tempGroup.Count >= 3)
			{
				groups.Add(new List<Entity>(tempGroup));
			}

			tempGroup.Clear();
		}
	}
}