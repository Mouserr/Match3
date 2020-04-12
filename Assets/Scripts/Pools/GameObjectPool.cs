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
			_addingCount = addingCount;
			_container = container;
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
				Object.Destroy(_instances.Pop().gameObject);
			}
		}

		private void AddInstances(int count)
		{
			for (int i = 0; i < count; i++)
			{
				T instance = Object.Instantiate(_prefab, _container);
				instance.gameObject.SetActive(false);
				var completionCheck = instance.GetComponent<ICompletionCheck>();
				if (completionCheck != null)
				{
					completionCheck.OnComplete += () => ReleaseObject(instance);
				}
				_instances.Push(instance);
			}
		}

	}
}