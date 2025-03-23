using System.Collections.Generic;
using UnityEngine;

namespace Zenject.ReflectionBaking
{
    public class ZenjectReflectionBakingSettings : ScriptableObject
    {
        [SerializeField] private bool _isEnabledInBuilds = true;

        [SerializeField] private bool _isEnabledInEditor = false;

        [SerializeField] private bool _allGeneratedAssemblies = true;

        [SerializeField] private List<string> _includeAssemblies = null;

        [SerializeField] private List<string> _excludeAssemblies = null;

        [SerializeField] private List<string> _namespacePatterns = null;

        public List<string> NamespacePatterns => _namespacePatterns;

        public List<string> IncludeAssemblies => _includeAssemblies;

        public List<string> ExcludeAssemblies => _excludeAssemblies;

        public bool IsEnabledInEditor => _isEnabledInEditor;

        public bool IsEnabledInBuilds => _isEnabledInBuilds;

        public bool AllGeneratedAssemblies => _allGeneratedAssemblies;
    }
}