using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Enemy : UI_Base
{
    private UITransform _initialTransform = new UITransform(new Vector3(30f, -150f, 0f), 500f, 800f);
    private Vector3 _enemyFocusPos;

    public override void Init()
    {
        SetEnemyTransform(_initialTransform);
    }

    public void SetEnemyTransform(UITransform enemyTransform)
    {
        _enemyFocusPos = MapGenerator.Instance.GetCenterPosition()
           + new Vector3(
               -enemyTransform.OffsetFromTarget * Mathf.Sin(enemyTransform.Rotation.y * Mathf.Deg2Rad),
               0f,
               -enemyTransform.OffsetFromTarget * Mathf.Cos(enemyTransform.Rotation.y * Mathf.Deg2Rad));

        // Event Camera is meaningless
        transform.localEulerAngles = enemyTransform.Rotation;
        transform.position = _enemyFocusPos - transform.forward * enemyTransform.DistanceFromTarget;
    }
}
