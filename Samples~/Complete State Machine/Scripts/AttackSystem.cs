using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial class AttackSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((in AttackWindow attackWindow) =>
        {
            var value = attackWindow.IsOpen ? 1 : 0;
            Debug.Log(FixedString.Format("Attack Window is: {0}", value));
        }).Schedule();
    }
}