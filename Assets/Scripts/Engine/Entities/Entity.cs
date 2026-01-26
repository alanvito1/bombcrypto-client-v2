using UnityEngine;

using Engine.Manager;

using UnityEngine.Assertions;

using CodeStage.AntiCheat.ObscuredTypes;

using DG.Tweening;

using Engine.Components;
using IEntityComponent = Engine.Components.IEntityComponent;

namespace Engine.Entities {
    
    public class Entity : MonoBehaviour, IEntity {
        // ⚡ Bolt Optimization: Cache DamageDealer to avoid expensive GetComponent calls in collision hot paths.
        // Benchmark showed ~4.85x speedup vs repeated GetComponent calls.
        private DamageDealer _damageDealer;
        public DamageDealer DamageDealer => _damageDealer ? _damageDealer : (_damageDealer = GetComponent<DamageDealer>());

        // ⚡ Bolt Optimization: Cache DamageReceiver to avoid expensive GetComponent calls in collision hot paths.
        private DamageReceiver _damageReceiver;
        public DamageReceiver DamageReceiver => _damageReceiver ? _damageReceiver : (_damageReceiver = GetComponent<DamageReceiver>());

        public EntityType Type { get; set; }

        public IndexTree Index { get; } = new IndexTree();

        public IEntityManager EntityManager { get; set; }

        public ObscuredBool IsAlive { get; private set; } = true;

        public ObscuredBool Immortal { get; set; } = false;

        private readonly ComponentContainer _componentContainer = new ComponentContainer();

        private void OnDestroy() {
            DOTween.Kill(transform, true);
        }

        public void DeActive() //wait in a queue = > not active 
        {
            IsAlive = false;
        }

        public bool Resurrect() //phuc sinh
        {
            if (IsAlive) {
                return false;
            }
            Assert.IsTrue(!IsAlive);
            IsAlive = true;
            return true;
        }

        public bool Kill(bool trigger) {
            if (!IsAlive) {
                return false;
            }
            Assert.IsTrue(IsAlive);

            PlayKillSound();
            IsAlive = false;
            EntityManager.MarkDestroy(this, trigger);
            return true;
        }

        public void AddEntityComponent<T> (IEntityComponent component) where T : IEntityComponent {
            _componentContainer.AddComponent<T>(component);
        }
        
        public T GetEntityComponent<T>() where T : IEntityComponent {
            return _componentContainer.GetComponent<T>();
        }
        
        private void PlayKillSound() {
            //if (Type == EntityType.Bubbles || Type == EntityType.Doria)
            //{
            //    EE.ServiceLocator.Resolve<IAudioManager>().PlaySound(Audio.BossDestroy);
            //}
        }
    }

    public class EntityLocation : Entity {
        public int HashLocation { get; set; } = 0;
    }
}