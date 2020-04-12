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
		private Field _field;
		private EntityManager _manager;
		private Entity[] _ballEntityPrefabs;
		private Entity[] _gravitySwitchEntityPrefabs;

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

		private void Start()
		{
			_manager = World.DefaultGameObjectInjectionWorld.EntityManager;

			ConvertPrefabs();
			_field = new Field(_size.x, _size.y, _cellSize, _fieldLeftBottomCorner.position, _manager);
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
			world.GetOrCreateSystem<BorderSpawnSystem>().Init(_ballEntityPrefabs);
			world.GetOrCreateSystem<FindMatchesSystem>().Init(_field);
			world.GetOrCreateSystem<FallSystem>().Init(_field);
			world.GetOrCreateSystem<GravitySwitcherPlacementSystem>().Init(_gravitySwitchEntityPrefabs);
			world.GetOrCreateSystem<DestroyBallsSystem>().Init(_destroyBallPrefab);
			world.GetOrCreateSystem<ShowSelectionSystem>().Init(_selection);
			world.GetOrCreateSystem<InputSystem>().Init(_camera, _field);
			world.GetOrCreateSystem<SwapSystem>().Init(_field);

			var stateEntity = _manager.CreateEntity(typeof(SystemState));
			_manager.AddComponentData(stateEntity, new Gravity { Value = new int2(0, -1) });
		}
	}
}