using UnityEngine;

using Engine.Manager;

using UnityEngine.Assertions;

using CodeStage.AntiCheat.ObscuredTypes;

using DG.Tweening;

using Engine.Components;
using IEntityComponent = Engine.Components.IEntityComponent;

namespace Engine.Entities {
    
    public class Entity : MonoBehaviour, IEntity {
        public EntityType Type { get; set; }

        public IndexTree Index { get; } = new IndexTree();

        public IEntityManager EntityManager { get; set; }

        public ObscuredBool IsAlive { get; private set; } = true;

        public ObscuredBool Immortal { get; set; } = false;

        private readonly ComponentContainer _componentContainer = new ComponentContainer();

        private DamageDealer _damageDealer;
        private bool _damageDealerCached;

        // Optimization: Cache component to avoid expensive GetComponent calls in hot paths (e.g. Collision)
        public DamageDealer DamageDealer {
            get {
                if (!_damageDealerCached) {
                    _damageDealer = GetComponent<DamageDealer>();
                    _damageDealerCached = true;
                }
                return _damageDealer;
            }
        }

        private DamageReceiver _damageReceiver;
        private bool _damageReceiverCached;

        public DamageReceiver DamageReceiver {
            get {
                if (!_damageReceiverCached) {
                    _damageReceiver = GetComponent<DamageReceiver>();
                    _damageReceiverCached = true;
                }
                return _damageReceiver;
            }
        }

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