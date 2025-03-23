#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    [NoReflectionBaking]
    public class PrefabInstantiatorCached : IPrefabInstantiator
    {
        private readonly IPrefabInstantiator _subInstantiator;

        private GameObject _gameObject;

        public PrefabInstantiatorCached(IPrefabInstantiator subInstantiator)
        {
            _subInstantiator = subInstantiator;
        }

        public List<TypeValuePair> ExtraArguments => _subInstantiator.ExtraArguments;

        public Type ArgumentTarget => _subInstantiator.ArgumentTarget;

        public GameObjectCreationParameters GameObjectCreationParameters =>
            _subInstantiator.GameObjectCreationParameters;

        public UnityEngine.Object GetPrefab(InjectContext context)
        {
            return _subInstantiator.GetPrefab(context);
        }

        public GameObject Instantiate(InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            // We can't really support arguments if we are using the cached value since
            // the arguments might change when called after the first time
            Assert.IsEmpty(args);

            if (_gameObject != null)
            {
                injectAction = null;
                return _gameObject;
            }

            _gameObject = _subInstantiator.Instantiate(context, new List<TypeValuePair>(), out injectAction);
            return _gameObject;
        }
    }
}

#endif