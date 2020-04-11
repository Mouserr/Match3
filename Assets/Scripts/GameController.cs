using System.Collections.Generic;
using Assets.Scripts.Components;
using Assets.Scripts.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
		private Camera _camera;
		[SerializeField]
		private GameObject _selection;

		private Field _field;
		private Entity[] _ballEntityPrefabs;
		
		private void Start()
		{
			_ballEntityPrefabs = new Entity[_ballPrefabs.Length];
			var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
			for (var i = 0; i < _ballPrefabs.Length; i++)
			{
				_ballEntityPrefabs[i] = (GameObjectConversionUtility.ConvertGameObjectHierarchy(_ballPrefabs[i], settings));
			}

			_manager = World.DefaultGameObjectInjectionWorld.EntityManager;

			CreateField();
			InitSystems(World.DefaultGameObjectInjectionWorld);
		}

		private void InitSystems(World world)
		{
			world.GetOrCreateSystem<ShowSelectionSystem>().Init(_selection);
			world.GetOrCreateSystem<InputSystem>().Init(_camera, _field);
			world.GetOrCreateSystem<SpawnSystem>().Init(_ballEntityPrefabs);
			world.GetOrCreateSystem<FallSystem>().Init(_field);
			world.GetOrCreateSystem<SwapSystem>().Init(_field);
		}

		private void CreateField()
		{
			_field = new Field(_size.x, _size.y, _cellSize, _fieldLeftBottomCorner.position);
			_field.Gravity = new int2(0, -1);
			for (int x = 0; x < _size.x; x++)
			{
				for (int y = 0; y < _size.y; y++)
				{
					var entity = _manager.CreateEntity();
					_manager.AddComponentData(entity, new Translation { Value = _field.GetWorldPosition(x, y) });
					_manager.AddComponentData(entity, new GridPosition { Value = new int2(x, y) });
					_field.SetCell(x, y, entity);
					if (y == _size.y - 1)
					{
						_manager.AddComponent<Spawner>(entity);
					}
				}
			}
		}
	}
}