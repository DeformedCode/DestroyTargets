using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHeal : ATargetBehavior
{
    public event System.Action<int> TakeHealEvent;

    [Header("Heal")]
    [Range(0, 5)] [SerializeField] private int _minHealPerCnock = 0;
    [Range(0, 5)] [SerializeField] private int _maxHealPerCnock = 0;

    [Range(0, 10)] [SerializeField] private int _minHealIfDestroy = 0;
    [Range(0, 10)] [SerializeField] private int _maxHealIfDestroy = 0;

    [Range(0, 10)] [SerializeField] private int _minHealIfFall = 0;
    [Range(0, 10)] [SerializeField] private int _maxHealIfFall = 0;

    private int _healPerCnock;
    private int _healIfDestroy;
    private int _healIfFall;


    public override void SetRandomProperties()
    {
        base.SetRandomProperties();

        _healPerCnock = Random.Range(_minHealPerCnock, _maxHealPerCnock);
        _healIfDestroy = Random.Range(_minHealIfDestroy, _maxHealIfDestroy);
        _healIfFall = Random.Range(_minHealIfFall, _maxHealIfFall);
    }
    public override void TakeCnock()
    {
        _cnockCount--;
        Healing(_healPerCnock);
        if (_cnockCount <= 0)
        {
            OnTakeScorePointsEvent();
            OnTakeDamageEvent();

            Healing(_healIfDestroy);
            OnDestroyEvent();
            Deactivate();
        }
    }

    public override void TargetHasFallen()
    {
        Healing(_healIfFall);

        OnTakeDamageEvent();

        Deactivate();
    }

    private void Healing(int heal)
    {
        TakeHealEvent?.Invoke(heal);
    }
}
