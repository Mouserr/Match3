﻿using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Pools
{
	public class AutoReturnToPool : MonoBehaviour
	{
		private Coroutine _aliveCheckCoroutine;
		public event Action OnComplete;

		void OnEnable()
		{
			if (_aliveCheckCoroutine != null)
			{
				StopCoroutine(_aliveCheckCoroutine);
			}

			_aliveCheckCoroutine = StartCoroutine(CheckIfAlive());
		}

		IEnumerator CheckIfAlive()
		{
			ParticleSystem ps = this.GetComponent<ParticleSystem>();

			while (ps != null && ps.IsAlive(true))
			{
				yield return new WaitForSeconds(0.5f);
			}

			OnComplete?.Invoke();
		}
	}
}