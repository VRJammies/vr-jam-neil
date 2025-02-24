using System.Collections.Generic;
using UnityEngine;
using VRJammies.Framework.Core.Health;

namespace VRJammies.Framework.Core.Boss
{
    public class ProjectileSpitter : AttackBase
    {
        [SerializeField] private Animator animator;
        [SerializeField]
        private GameObject _target;
        [SerializeField]
        private GameObject _projectilePrefab;
        [SerializeField]
        private List<GameObject> _projectileList;
        [SerializeField]
        private Transform _output;
        [SerializeField]
        private float _force = 10;
        [SerializeField]
        private bool _isActive = false;

        private static readonly int AttackState = Animator.StringToHash("Attack");
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

        [SerializeField] private PlayerFinder playerFinder;


        private bool _shouldPool = false;

        [SerializeField]
        private float _attackSpeed = 0.125f;
        private float _timer = 0f;
        private bool canAttack;


        private void Start()
        {
            if (!_projectilePrefab) Debug.LogWarning(this.name + " has no projectile prefab assigned!");
            if(!_output) Debug.LogWarning(this.name + " has no projectile output assigned!");
            if (!playerFinder)
            {
                Debug.LogWarning(this.name + " has no player finder assigned!");
            }
            else
            {
                playerFinder.OnPlayerFound += OnPlayerFound;
                playerFinder.OnPlayerLost += OnPlayerLost;
            }
            
        }

        private void Update()
        {
            if (_isActive && _target)
            {
                transform.LookAt(_target.transform);

                _timer += Time.deltaTime;
                if (_timer >= _attackSpeed)
                {
                    canAttack = true;
                }
            }
            else
            {
                _timer = 0;
                canAttack = false;
            }
        }

        private void SpawnProjectile()
        {
            if (_shouldPool)
            {
                bool didSpawn = false;

                // Check the projectile list for inactive projectiles
                foreach (var projectile in _projectileList)
                {
                    if (!projectile.activeSelf)
                    {
                        // If you found an inactive object, use that and tick of didSpawn as true
                        projectile.transform.position = this.transform.position;
                        projectile.SetActive(true);
                        ShootProjectile(projectile);
                        didSpawn = true;
                        break;
                    }
                }

                // If after the loop there was no inactive object, spawn a new one
                if (!didSpawn)
                {
                    var projectile = Instantiate(_projectilePrefab, _output.position, _output.rotation);
                    _projectileList.Add(projectile);
                    ShootProjectile(projectile);
                }
            }
            else
            {
                var projectile = Instantiate(_projectilePrefab, _output.position, _output.rotation);
                ShootProjectile(projectile);
            }
        }

        private void ShootProjectile(GameObject projectile)
        {
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            rb.velocity = Vector3.zero;

            projectile.transform.position = _output.position;
            projectile.transform.rotation = _output.rotation;
            projectile.GetComponent<DamageColliderProjectile>().SetSpawner(this.gameObject);
            projectile.SetActive(true);
            var direction = (_target.transform.position - _output.transform.position).normalized;
            rb.velocity = direction * _force;
            projectile.transform.parent = null;            
            onAttack.Invoke();
            animator.SetBool(IsAttacking, false);
            OnDoneAttacking();
        }

        public override bool CanAttack()
        {
            return canAttack;
        }

        public override void Attack()
        {
            animator.SetBool(IsAttacking, true);
            SpawnProjectile();
            _timer = 0;
        }

        private void OnPlayerFound(Player.Player player)
        {
            _target = player.gameObject;
        }

        private void OnPlayerLost(Player.Player player)
        {
            _target = null;
        }
    }
}
