using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyedTarget : MonoBehaviour
{
    public event System.Action<DestroyedTarget> DeactivateEvent;

    [SerializeField] private uint _changeableMaterialIndex = 0;
    public uint ChangeableMaterialIndex => _changeableMaterialIndex;

    public float DisableCollisionOverTime { get; set; }
    public float DeactivateOverTime { get; set; }
 
    public Transform[] PartsTransforms { get; private set; }

    private void OnEnable()
    {
        StartCoroutine(DisableCollisionOnDestryedFragments());

        StartCoroutine(Deactivate());
    }

    private void Awake()
    {
        PartsTransforms = GetComponentsInChildren<Transform>();

        MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
        if (_changeableMaterialIndex > mesh.materials.Length)
            _changeableMaterialIndex = mesh.materials.Length == 0 ? (uint)mesh.materials.Length : (uint)mesh.materials.Length - 1;
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(DeactivateOverTime);

        DeactivateEvent?.Invoke(this);
        gameObject.SetActive(false);
    }
    private IEnumerator DisableCollisionOnDestryedFragments()
    {
        yield return new WaitForSeconds(DisableCollisionOverTime);

        MeshCollider[] parts = GetComponentsInChildren<MeshCollider>();
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].enabled = false;
        }
    }
}
