using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetStandart : ATargetBehavior
{

    public override void TakeCnock()
    {
        _cnockCount--;
        if (_cnockCount <= 0)
        {
            OnTakeScorePointsEvent();

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
