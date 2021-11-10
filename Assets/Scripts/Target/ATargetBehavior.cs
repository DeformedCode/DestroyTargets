using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody), typeof(MeshRenderer))]
public abstract class ATargetBehavior : MonoBehaviour, IPointerDownHandler
{
    public event System.Action<ATargetBehavior> DeactivateEvent;
    public event System.Action<ATargetBehavior, Color> DestroyEvent;
    public event System.Action<Vector3, Color> HitEvent;
    public event System.Action<int> TakeScorePointsEvent;
    public event System.Action<int> TakeDamageEvent;

    [SerializeField] private uint _changeableMaterialIndex = 0;

    [Header("Properties of random")]
    [Range(0, 100)] [SerializeField] private int _minScorePoints = 1;
    [Range(0, 100)] [SerializeField] private int _maxScorePoints = 1;
    [Space]
    [Range(1, 10)] [SerializeField] private int _minCnockCount = 1;
    [Range(1, 10)] [SerializeField] private int _maxCnockCount = 1;
    [Space]
    [Range(0, 100)] [SerializeField] private int _minDamage = 1;
    [Range(0, 100)] [SerializeField] private int _maxDamage = 1;

    [Header("Speed limits")]
    [Range(1, 5)] [SerializeField] private float _minSpeed = 1;
    public float MinSpeed => _minSpeed;

    [Range(1, 5)] [SerializeField] private float _maxSpeed = 3;
    public float MaxSpeed => _maxSpeed;

    public DestroyedTarget DestroyedTarget { get; set; }


    protected Transform _transform;
    protected MeshRenderer _meshRenderer;
    protected SphereCollider _sphereCollider;
    protected Rigidbody _rigidbody;
    public Rigidbody Rigidbody => _rigidbody;

    protected int _receivedPoints;
    protected int _cnockCount;
    protected int _damage;


    private void Awake()
    {
        _transform = transform;
        _sphereCollider = GetComponent<SphereCollider>();
        _rigidbody = GetComponent<Rigidbody>();

        _rigidbody.useGravity = false;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;


        _meshRenderer = GetComponent<MeshRenderer>();

        if (_changeableMaterialIndex > _meshRenderer.materials.Length)
            _changeableMaterialIndex = (uint)_meshRenderer.materials.Length - 1;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Block"))
        {
            _rigidbody.velocity = Vector3.Reflect(_rigidbody.velocity, collision.contacts[0].normal * 1.05f);
            _rigidbody.velocity += collision.contacts[0].normal;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TakeCnock();
        HitEvent?.Invoke(eventData.pointerCurrentRaycast.worldPosition, _meshRenderer.materials[_changeableMaterialIndex].color);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeactivateZone"))
            TargetHasFallen();
        else if (other.CompareTag("ExplosionZone"))
        {
            OnTakeScorePointsEvent();
            OnDestroyEvent();
            Deactivate();
        }
    }



    public abstract void TakeCnock();
    public abstract void TargetHasFallen();


    public virtual void SetRandomProperties()
    {
        _receivedPoints = Random.Range(_minScorePoints, _maxScorePoints);
        _cnockCount = Random.Range(_minCnockCount, _maxCnockCount);
        _damage = Random.Range(_minDamage, _maxDamage);

        _meshRenderer.materials[_changeableMaterialIndex].color = Random.ColorHSV(0, 1, 0.8f, 1, 0.8f, 1);
    }

    protected void OnTakeScorePointsEvent()
    {
        TakeScorePointsEvent?.Invoke(_receivedPoints);
    }
    protected void OnTakeDamageEvent()
    {
        TakeDamageEvent?.Invoke(_damage);
    }

    protected void Deactivate()
    {
        _rigidbody.velocity = Vector3.zero;

        gameObject.SetActive(false);
        DeactivateEvent?.Invoke(this);

    }
    protected void OnDestroyEvent()
    {
        DestroyEvent?.Invoke(this, _meshRenderer.materials[_changeableMaterialIndex].color);
    }

}
