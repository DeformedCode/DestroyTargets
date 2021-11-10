using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(TargetDestroyer), typeof(Particles))]
public class TargetsGenerator : MonoBehaviour
{
    [SerializeField] GameAndScoreManager gameAndScoreManager;

    [System.Serializable]
    public sealed class Target
    {
        public ATargetBehavior TargetPrefab;
        public DestroyedTarget DestroyedTargetPrefab;
        [Range(0, 100)] public int MinCapacityInPool;

        [HideInInspector] public int RandomWeight;

    }
    [SerializeField] private Target[] _targetsPrefabs;
    [Space]
    [SerializeField] private Transform _targetsContainer;


    [Header("Beginning and end points of the spawn line")]
    [SerializeField] private Vector3 _startSpawnPoint;
    [SerializeField] private Vector3 _endSpawnPoint;

    [Header("Direction")]
    [SerializeField] private Vector3 _targetDirection = new Vector3(0, -1, 0);
    [Header("Add random turn")]
    [SerializeField] private Vector3 _rotateAxis = new Vector3(0, 0, -1);
    [Range(0, 70)]
    [SerializeField] private float _maxRandomAngle;

    [Header("SpawnDelay")]
    [Range(0.1f, 5)] [SerializeField] private float _spawnDelayInSeconds = 1;

    [Header("Increasing difficulty over time")]
    [SerializeField] AnimationCurve _speedingUpCurve;
    [Range(1, 1000)] [SerializeField] private float _takingTimeForSpeedUp = 1;
    [Space]
    [SerializeField] AnimationCurve _increaseSpawnCurve;
    [Range(1, 1000)] [SerializeField] private float _takingTimeForIncreaseSpawn = 1;

    private TargetsPool[] _targetsPools;
    private TargetDestroyer _targetDestroyer;
    private Particles _particles;

    private int _weightSum;

    private void Awake()
    {
        if (_targetsContainer == null)
            _targetsContainer = gameObject.transform;

        int weightSum = 0;
        for (int i = 0; i < _targetsPrefabs.Length; i++)
        {
            weightSum += _targetsPrefabs[i].MinCapacityInPool;
            _targetsPrefabs[i].RandomWeight = weightSum;
        }
        _weightSum = weightSum;

        _targetDestroyer = GetComponent<TargetDestroyer>();
        _particles = GetComponent<Particles>();

    }
    private void Start()
    {

        PoolInitialization();

        StartCoroutine(SpawnTimer());
    }

    private ATargetBehavior RandomizeTarget()
    {
        int random = Mathf.RoundToInt(Random.Range(0, _weightSum));
        for (int i = 0; i < _targetsPrefabs.Length; i++)
        {
            if (random <= _targetsPrefabs[i].RandomWeight)
                return _targetsPools[i].GiveTarget();

        }
        return null;
    }

    private void PoolInitialization()
    {
        _targetsPools = new TargetsPool[_targetsPrefabs.Length];
        for (int i = 0; i < _targetsPools.Length; i++)
        {
            _targetsPools[i] = new TargetsPool(_targetsPrefabs[i]);
            if (_targetsPrefabs[i].TargetPrefab != null)
            {
                _targetsPools[i].NeedMoreTargets += InstantiateTarget;

                if (_targetsPrefabs[i].DestroyedTargetPrefab != null)
                    _targetsPools[i].NeedMoreDestroyedTargets += InstantiateDestroyedTarget;
                else
                    Debug.LogError("TargetDestroyedPrefab == Null");
            }
            else
            {
                Debug.LogError("TargetPrefab == Null");
            }

        }
    }


    private async void InstantiateDestroyedTarget(DestroyedTarget destroyedTarget, int count, TargetsPool targetsPool)
    {
        for (int i = 0; i <= count; i++)
        {
            DestroyedTarget instantiatedDestroyedTarget = Instantiate(destroyedTarget, _targetsContainer);

            instantiatedDestroyedTarget.DeactivateEvent += targetsPool.TakeDestroyedTarget;

            targetsPool.TakeDestroyedTarget(instantiatedDestroyedTarget);

            instantiatedDestroyedTarget.gameObject.SetActive(false);

            await Task.Yield();
        }
    }

    private async void InstantiateTarget(ATargetBehavior targetBehavior, int count, TargetsPool targetsPool)
    {
        for (int i = 0; i <= count; i++)
        {
            ATargetBehavior instantiatedTarget = Instantiate(targetBehavior, _targetsContainer);

            instantiatedTarget.DeactivateEvent += targetsPool.TakeTarget;

            SubscribeToDestroyer(instantiatedTarget);
            SubscribeToParticles(instantiatedTarget);

            if (gameAndScoreManager != null)
            {
                instantiatedTarget.TakeDamageEvent += gameAndScoreManager.TakeDamage;
                instantiatedTarget.TakeScorePointsEvent += gameAndScoreManager.AddScorePoints;

                if (instantiatedTarget is TargetHeal)
                {
                    TargetHeal heal = (TargetHeal)instantiatedTarget;
                    heal.TakeHealEvent += gameAndScoreManager.TakeHeal;
                }
            }


            instantiatedTarget.gameObject.SetActive(false);
            targetsPool.TakeTarget(instantiatedTarget);

            await Task.Yield();
        }

    }
    private void SubscribeToParticles(ATargetBehavior targetBehavior)
    {
        targetBehavior.DestroyEvent += _particles.DestroyExplosion;
        targetBehavior.HitEvent += _particles.Hit;

    }
    private void SubscribeToDestroyer(ATargetBehavior targetBehavior)
    {
        targetBehavior.DestroyEvent += _targetDestroyer.Destroy;

        if(targetBehavior is TargetBomb)
            targetBehavior.DestroyEvent += _targetDestroyer.Explosion;
    }

    public void SpawnTargetAlongLine()
    {
        ATargetBehavior spawnedTarget = RandomizeTarget();
        if (spawnedTarget != null)
        {
            spawnedTarget.gameObject.SetActive(true);

            spawnedTarget.SetRandomProperties();

            Vector3 spawnPoint = new Vector3(Random.Range(_startSpawnPoint.x, _endSpawnPoint.x), Random.Range(_startSpawnPoint.y, _endSpawnPoint.y), Random.Range(_startSpawnPoint.z, _endSpawnPoint.z));
            spawnedTarget.transform.position = spawnPoint;

            Vector3 currentDirection = Quaternion.AngleAxis(Random.Range(-_maxRandomAngle, _maxRandomAngle), _rotateAxis) * _targetDirection;
            currentDirection = Vector3.Normalize(currentDirection);

            float speed = Random.Range(spawnedTarget.MinSpeed, spawnedTarget.MaxSpeed);
            speed += speed * _speedingUpCurve.Evaluate(Time.time / _takingTimeForSpeedUp);

            spawnedTarget.Rigidbody.AddRelativeTorque(Random.Range(-20, 20), Random.Range(-20, 20), Random.Range(-20, 20));
            spawnedTarget.Rigidbody.AddForce(currentDirection * speed, ForceMode.VelocityChange);

        }
    }
    private IEnumerator SpawnTimer()
    {
        while (true)
        {
            SpawnTargetAlongLine();

            float currentDelay = _spawnDelayInSeconds - (_spawnDelayInSeconds * _increaseSpawnCurve.Evaluate(Time.time / _takingTimeForIncreaseSpawn) / 2);
            yield return new WaitForSeconds(currentDelay);
        }
    }

}
