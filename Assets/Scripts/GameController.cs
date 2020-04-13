using Assets.Scripts.Components;
using Assets.Scripts.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
		private int2 _size;
		[SerializeField]
		private float _cellSize;
		[SerializeField]
		private float _fallSpeed;
		[SerializeField]
		private float _delayOnDestroy;
		[SerializeField]
		private Transform _fieldLeftBottomCorner;
		[SerializeField]
		private GameObject[] _ballPrefabs;
		[SerializeField]
		private GameObject[] _gravitySwitchPrefabs;
		[SerializeField]
		private Transform[] _destroyBallPrefabs;
		[SerializeField]
		private Transform _gravitySwitcherEffectPrefab;
		[SerializeField]
		private Camera _camera;
		[SerializeField]
		private GameObject _selection;

		private void Start()
		{
			_manager = World.DefaultGameObjectInjectionWorld.EntityManager;
			StartGame();
		}

		private void StartGame()
		{
			AdjustCamera();
			ConvertPrefabs();
			_field = new Field(_size.x, _size.y, _cellSize, _fieldLeftBottomCorner.position, _manager);
			InitSystems(World.DefaultGameObjectInjectionWorld);
		}

		private void AdjustCamera()
		{
			_camera.transform.position = _cellSize * new Vector3((_size.x - _cellSize) / 2f, (_size.y - _cellSize) / 2f, _camera.transform.position.z);
			_camera.orthographicSize = math.max(_size.x / _camera.aspect, _size.y) * _cellSize / 2f;
		}

		private void ConvertPrefabs()
		{
			var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
			_ballEntityPrefabs = CreateEntityPrefabs(_ballPrefabs, settings);
			_gravitySwitchEntityPrefabs = CreateEntityPrefabs(_gravitySwitchPrefabs, settings);
		}

		private void InitSystems(World world)
		{
			world.GetOrCreateSystem<BorderSpawnSystem>().Init(_ballEntityPrefabs, _cellSize, _fallSpeed);
			world.GetOrCreateSystem<LimitedSpawnSystem>().Init(_cellSize, _fallSpeed);
			world.GetOrCreateSystem<FindMatchesSystem>().Init(_field);
			world.GetOrCreateSystem<FallSystem>().Init(_field);
			world.GetOrCreateSystem<GravitySwitcherPlacementSystem>().Init(_gravitySwitchEntityPrefabs);
			world.GetOrCreateSystem<DestroyBallsSystem>().Init(_destroyBallPrefabs, _delayOnDestroy);
			world.GetOrCreateSystem<ShowSelectionSystem>().Init(_selection);
			world.GetOrCreateSystem<InputSystem>().Init(_camera, _field);
			world.GetOrCreateSystem<SwapSystem>().Init(_field);

			var stateEntity = _manager.CreateEntity(typeof(SystemState));
			_manager.AddComponentData(stateEntity, new Gravity { Value = new int2(0, -1) });
		}

		private Entity[] CreateEntityPrefabs(GameObject[] ballPrefabs, GameObjectConversionSettings settings)
		{
			var ballEntityPrefabs = new Entity[ballPrefabs.Length];
			for (var i = 0; i < _ballPrefabs.Length; i++)
			{
				ballEntityPrefabs[i] = (GameObjectConversionUtility.ConvertGameObjectHierarchy(ballPrefabs[i], settings));
				_manager.AddComponentData(ballEntityPrefabs[i], new Color {Value = i});
				_manager.AddComponentData(ballEntityPrefabs[i], new Speed {Value = _fallSpeed});
			}

			return ballEntityPrefabs;
		}
	}
}