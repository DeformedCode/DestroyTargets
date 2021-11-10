using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBomb : ATargetBehavior
{
    [Header("Explosion zone properties")]
    [SerializeField] private Vector3 _explosionZoneSize = new Vector3(2, 2, 2);
    public Vector3 ExplosionZoneSize => _explosionZoneSize;
    [Range(0.5f, 5)] [SerializeField] private float _explosionTimeInSec = 1;
    public float ExplosionTimeInSec => _explosionTimeInSec;

    public override void TakeCnock()
    {
        _cnockCount--;
        if (_cnockCount <= 0)
        {
            OnTakeScorePointsEvent();
            OnTakeDamageEvent();

            OnDestroyEvent();
            Deactivate();
        }
    }

    public override void TargetHasFallen()
    {
        OnTakeDamageEvent();

        Deactivate();
    }

}
