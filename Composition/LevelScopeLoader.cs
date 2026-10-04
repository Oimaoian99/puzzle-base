using System;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Level;
using UnityEngine;
using VContainer;

namespace Puzzle.Composition
{
    /// <summary>
    /// Level loading and scope orchestrator implemented in the Composition layer.
    /// Manages the VContainer child LifetimeScope for the active level.
    /// Guarantees that level-scoped objects are 100% disposed and leak-free
    /// when switching levels, restarting, or returning to Home.
    /// </summary>
    public class LevelScopeLoader : ILevelLoader, IDisposable
    {
        private readonly IObjectResolver _rootResolver;
        private readonly ILevelDataProvider _dataProvider;
        private readonly LevelValidator _validator;
        private readonly IPuzzleVariantRegistry _variantRegistry;
        private IScopedObjectResolver _activeScope;
        private LevelId _activeLevelId;

        public bool IsLoading { get; private set; }
        public LevelId ActiveLevelId => _activeLevelId;
        public IScopedObjectResolver ActiveScope => _activeScope;
        public bool HasActiveScope => _activeScope != null;

        [Inject]
        public LevelScopeLoader(IObjectResolver rootResolver)
            : this(
                rootResolver,
                rootResolver != null && rootResolver.TryResolve<ILevelDataProvider>(out var provider) ? provider : null,
                rootResolver != null && rootResolver.TryResolve<LevelValidator>(out var validator) ? validator : new LevelValidator(),
                rootResolver != null && rootResolver.TryResolve<IPuzzleVariantRegistry>(out var registry) ? registry : null)
        {
        }

        public LevelScopeLoader(
            IObjectResolver rootResolver,
            ILevelDataProvider dataProvider,
            LevelValidator validator = null,
            IPuzzleVariantRegistry variantRegistry = null)
        {
            _rootResolver = rootResolver ?? throw new ArgumentNullException(nameof(rootResolver));
            _dataProvider = dataProvider;
            _validator = validator ?? new LevelValidator();
            _variantRegistry = variantRegistry;
        }

        public void LoadLevel(LevelId id, Action<ILevelRuntime> onLoaded, Action<string> onError)
        {
            if (id.IsEmpty)
            {
                onError?.Invoke("Cannot load level with an empty ID.");
                return;
            }

            IsLoading = true;

            try
            {
                // 1. Fetch authored LevelData from repository/provider if present
                LevelData levelData = null;
                if (_dataProvider != null)
                {
                    if (!_dataProvider.TryGetLevelData(id, out levelData) || levelData == null)
                    {
                        IsLoading = false;
                        onError?.Invoke($"Level '{id}' not found in level repository.");
                        return;
                    }

                    // 2. Validate authored LevelData before building scope
                    var validation = _validator.Validate(levelData);
                    if (!validation.IsValid)
                    {
                        IsLoading = false;
                        onError?.Invoke($"Level '{id}' failed validation:\n{validation}");
                        return;
                    }
                }

                // 3. Resolve variant logic type from registry if configured
                Type puzzleLogicType = null;
                if (levelData != null && !string.IsNullOrEmpty(levelData.VariantId) && _variantRegistry != null)
                {
                    _variantRegistry.TryGetLogicType(levelData.VariantId, out puzzleLogicType);
                }

                // 4. Dispose any previously active child level scope
                UnloadLevel(_activeLevelId);

                // 5. Create a child lifetime scope containing level-scoped services
                _activeScope = _rootResolver.CreateScope(builder =>
                {
                    if (levelData != null)
                    {
                        LevelLifetimeScope.ConfigureLevelServices(
                            builder,
                            id,
                            levelData.StartingMoves,
                            levelData,
                            null,
                            puzzleLogicType);
                    }
                    else
                    {
                        LevelLifetimeScope.ConfigureLevelServices(builder, id);
                    }
                });

                _activeLevelId = id;
                IsLoading = false;

                // 6. Resolve the newly scoped ILevelRuntime
                var runtime = _activeScope.Resolve<ILevelRuntime>();
                onLoaded?.Invoke(runtime);
            }
            catch (Exception ex)
            {
                IsLoading = false;
                onError?.Invoke($"Failed to create level scope for '{id}': {ex.Message}");
            }
        }

        public void UnloadLevel(LevelId id = default)
        {
            if (_activeScope != null)
            {
                _activeScope.Dispose();
                _activeScope = null;
            }
            _activeLevelId = LevelId.Empty;
        }

        public void Dispose()
        {
            UnloadLevel(_activeLevelId);
        }
    }
}
