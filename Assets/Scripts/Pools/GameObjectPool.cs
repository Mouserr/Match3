using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pools
{
	public class GameObjectPool<T> where T : Component
	{
		private readonly Transform _container;
		private readonly T _prefab;
		private readonly int _addingCount;
		private readonly Stack<T> _instances;

		public GameObjectPool(Transform container, T prefabObj, int startCount, int addingCount = 5)
		{
			_prefab = prefabObj;
			this._addingCount = addingCount;
			this._container = container;
			_instances = new Stack<T>(startCount);
			AddInstances(startCount);
		}

		public T GetObject()
		{
			if (_instances.Count == 0)
			{
				AddInstances(_addingCount);
			}
			return _instances.Pop();
		}

		public void ReleaseObject(T objectToRelease)
		{
			objectToRelease.gameObject.SetActive(false);
			_instances.Push(objectToRelease);
		}

		public void ClearPull()
		{
			while (_instances.Count > 0)
			{
				GameObject.Destroy(_instances.Pop());
			}
		}


		private void AddInstances(int count)
		{
			for (int i = 0; i < count; i++)
			{
				T instance = GameObject.Instantiate(_prefab, _container);
				instance.gameObject.SetActive(false);
				var autoReturn = instance.GetComponent<AutoReturnToPool>();
				if (autoReturn)
				{
					autoReturn.OnComplete += () => ReleaseObject(instance);
				}
				_instances.Push(instance);
			}
		}

	}
}