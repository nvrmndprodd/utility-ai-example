using System.Collections.Generic;

namespace Zenject
{
    public class ListPool<T> : StaticMemoryPool<List<T>>
    {
        private static ListPool<T> _instance = new();

        public ListPool()
        {
            OnDespawnedMethod = OnDespawned;
        }

        public static ListPool<T> Instance => _instance;

        private void OnDespawned(List<T> list)
        {
            list.Clear();
        }
    }
}