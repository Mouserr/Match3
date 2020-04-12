using System.Collections.Generic;
using Assets.Scripts.Components;
using Assets.Scripts.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Color = Assets.Scripts.Components.Color;

namespace Assets.Scripts
{
	public class GameController : MonoBehaviour
	{
		private EntityManager _manager;

		[SerializeField]
		private Vector2Int _size;
		[SerializeField]
		private float _cellSize;
		[SerializeField]
		private Transform _fieldLeftBottomCorner;
		[SerializeField]
		private GameObject[] _ballPrefabs;
		[SerializeField]
		private GameObject[] _gravitySwitchPrefabs;
		[SerializeField]
		private Transform _destroyBallPrefab;
		[SerializeField]
		private Transform _gravitySwitcherEffectPrefab;
		[SerializeField]
		private Camera _camera;
		[SerializeField]
		private GameObject _selection;

		private Field _field;
		private Entity[] _ballEntityPrefabs;
		private Entity[] _gravitySwitchEntityPrefabs;

		private void Start()
		{
			_manager = World.DefaultGameObjectInjectionWorld.EntityManager;

			ConvertPrefabs();
			CreateField();
			InitSystems(World.DefaultGameObjectInjectionWorld);
		}

		private void ConvertPrefabs()
		{
			_ballEntityPrefabs = new Entity[_ballPrefabs.Length];
			var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
			for (var i = 0; i < _ballPrefabs.Length; i++)
			{
				_ballEntityPrefabs[i] = (GameObjectConversionUtility.ConvertGameObjectHierarchy(_ballPrefabs[i], settings));
				_manager.AddComponentData(_ballEntityPrefabs[i], new Color {Value = i});
			}

			_gravitySwitchEntityPrefabs = new Entity[_gravitySwitchPrefabs.Length];
			for (var i = 0; i < _gravitySwitchPrefabs.Length; i++)
			{
				_gravitySwitchEntityPrefabs[i] = (GameObjectConversionUtility.ConvertGameObjectHierarchy(_gravitySwitchPrefabs[i], settings));
				_manager.AddComponentData(_gravitySwitchEntityPrefabs[i], new Color { Value = i });
			}
		}

		private void InitSystems(World world)
		{
			world.GetOrCreateSystem<ShowSelectionSystem>().Init(_selection);
			world.GetOrCreateSystem<InputSystem>().Init(_camera, _field);
			world.GetOrCreateSystem<BorderSpawnSystem>().Init(_ballEntityPrefabs);
			world.GetOrCreateSystem<FallSystem>().Init(_field);
			world.GetOrCreateSystem<SwapSystem>().Init(_field);
			world.GetOrCreateSystem<FindMatchesSystem>().Init(_field);
			world.GetOrCreateSystem<DestroyBallsSystem>().Init(_destroyBallPrefab);
			world.GetOrCreateSystem<GravitySwitcherPlacementSystem>().Init(_gravitySwitchEntityPrefabs);

			var stateEntity = _manager.CreateEntity(typeof(SystemState));
			_manager.AddComponentData(stateEntity, new Gravity { Value = new int2(0, -1) });
		}

		private void CreateField()
		{
			_field = new Field(_size.x, _size.y, _cellSize, _fieldLeftBottomCorner.position);
			for (int x = 0; x < _size.x; x++)
			{
				for (int y = 0; y < _size.y; y++)
				{
					var entity = _manager.CreateEntity();
					_manager.AddComponentData(entity, new Translation { Value = _field.GetWorldPosition(x, y) });
					_manager.AddComponentData(entity, new GridPosition { Value = new int2(x, y) });
					_field.SetCell(x, y, entity);
					if (y == 0 || y == _field.Height - 1)
					{
						_manager.AddComponentData(entity, new Border{ RequiredGravity = new int2(0, y == 0 ? 1 : -1) });
						_manager.AddComponentData(entity, new Spawner { Offset = new float3(0, y == 0 ? -1 : 1, 0) });
						_manager.AddComponent<SpawnCount>(entity);
					}
				}
			}
		}
	}
}