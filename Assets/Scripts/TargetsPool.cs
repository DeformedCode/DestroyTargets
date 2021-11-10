using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(TargetsGenerator))]
public class TargetsPool 
{
    public event System.Action<ATargetBehavior, int, TargetsPool> NeedMoreTargets;
    public event System.Action<DestroyedTarget, int, TargetsPool> NeedMoreDestroyedTargets;

    private TargetsGenerator.Target _target;
    private Stack<ATargetBehavior> _targets;

    private Stack<DestroyedTarget> _destroyedTargets;
    private Vector3[] _destroyedTargetsPartPosition;

    public TargetsPool(TargetsGenerator.Target target)
    {
        _targets = new Stack<ATargetBehavior>();
        _destroyedTargets = new Stack<DestroyedTarget>();
        _target = target;

        NeedMoreTargets?.Invoke(target.TargetPrefab, target.MinCapacityInPool, this);

        NeedMoreDestroyedTargets?.Invoke(_target.DestroyedTargetPrefab, Mathf.FloorToInt(target.MinCapacityInPool * 1.2f), this);

        Transform[] destroyedTargetsPartTransform = target.DestroyedTargetPrefab.GetComponentsInChildren<Transform>();
        _destroyedTargetsPartPosition = new Vector3[destroyedTargetsPartTransform.Length];
        for (int i = 0; i < destroyedTargetsPartTransform.Length; i++)
        {
            _destroyedTargetsPartPosition[i] = destroyedTargetsPartTransform[i].position;
        }

    }

    public ATargetBehavior GiveTarget()
    {
        if (_destroyedTargets.Count == 0 && _targets.Count == 0)
        {
            NeedMoreDestroyedTargets?.Invoke(_target.DestroyedTargetPrefab, Mathf.FloorToInt(_target.MinCapacityInPool / 10), this);
            NeedMoreTargets?.Invoke(_target.TargetPrefab, Mathf.FloorToInt(_target.MinCapacityInPool / 10), this);
        }
        else if (_destroyedTargets.Count == 0)
        {
            NeedMoreDestroyedTargets?.Invoke(_target.DestroyedTargetPrefab, Mathf.FloorToInt(_target.MinCapacityInPool / 10), this);
        }
        else if (_targets.Count == 0)
        {
            NeedMoreTargets?.Invoke(_target.TargetPrefab, Mathf.FloorToInt(_target.MinCapacityInPool / 10), this);
        }
        else
        {
            ATargetBehavior targetBehavior = _targets.Pop();
            targetBehavior.DestroyedTarget = _destroyedTargets.Pop();


            if (_destroyedTargets.Count == 0)
                NeedMoreDestroyedTargets?.Invoke(_target.DestroyedTargetPrefab, Mathf.FloorToInt(_target.MinCapacityInPool / 10), this);
            else if (_targets.Count == 0)
                NeedMoreTargets?.Invoke(_target.TargetPrefab, Mathf.FloorToInt(_target.MinCapacityInPool / 10), this);


            return targetBehavior;
        }

        return null;
    }

    public void TakeTarget(ATargetBehavior target)
    {
        _targets.Push(target);
        target.gameObject.SetActive(false);

        if(target.DestroyedTarget != null)
        {
            TakeDestroyedTarget(target.DestroyedTarget);
            target.DestroyedTarget = null;
        }
    }
    public async void TakeDestroyedTarget(DestroyedTarget destroyedTarget)
    {
        destroyedTarget.gameObject.SetActive(false);
        Transform[] destroyedTargetTransforms = destroyedTarget.PartsTransforms;
        for (int i = 0; i < _destroyedTargetsPartPosition.Length; i++)
        {
            destroyedTargetTransforms[i].localPosition = _destroyedTargetsPartPosition[i];
            await Task.Yield();
        }
        _destroyedTargets.Push(destroyedTarget);
    }

}
