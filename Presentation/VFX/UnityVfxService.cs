using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Logging;
using UnityEngine;

namespace Puzzle.Presentation.VFX
{
    /// <summary>
    /// Presentation component managing ParticleSystem instantiation and recycling.
    /// Pure C# gameplay code has zero knowledge of ParticleSystem or prefabs.
    /// If prefabs or particles are missing or fail to instantiate, it safely logs a warning without crashing.
    /// </summary>
    public class UnityVfxService : MonoBehaviour, IVfxService
    {
        [Serializable]
        public struct VfxEntry
        {
            public string EffectName;
            public ParticleSystem Prefab;
        }

        [SerializeField] private List<VfxEntry> _effectBank = new List<VfxEntry>();
        private readonly Dictionary<string, ParticleSystem> _lookup = new Dictionary<string, ParticleSystem>(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            RebuildLookup();
        }

        public void RebuildLookup()
        {
            _lookup.Clear();
            if (_effectBank != null)
            {
                foreach (var entry in _effectBank)
                {
                    if (!string.IsNullOrEmpty(entry.EffectName) && entry.Prefab != null)
                    {
                        _lookup[entry.EffectName] = entry.Prefab;
                    }
                }
            }
        }

        public void SpawnPieceVfx(string effectName, Vector3 worldPosition, PieceType pieceType = PieceType.None)
        {
            if (string.IsNullOrEmpty(effectName)) return;

            if (_lookup.TryGetValue(effectName, out var prefab) && prefab != null)
            {
                var instance = Instantiate(prefab, worldPosition, Quaternion.identity, transform);
                instance.Play();
                // Auto cleanup when done
                Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetimeMultiplier + 0.1f);
            }
            else
            {
                CoreLogger.Log($"[UnityVfxService] VFX '{effectName}' played at {worldPosition} (fallback: no prefab configured).");
            }
        }

        public void SpawnScreenVfx(string effectName)
        {
            if (string.IsNullOrEmpty(effectName)) return;

            if (_lookup.TryGetValue(effectName, out var prefab) && prefab != null)
            {
                var instance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
                instance.Play();
                Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetimeMultiplier + 0.1f);
            }
            else
            {
                CoreLogger.Log($"[UnityVfxService] Screen VFX '{effectName}' played (fallback: no prefab configured).");
            }
        }

        public void ClearAllVfx()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
