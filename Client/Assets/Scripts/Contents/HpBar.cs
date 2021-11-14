using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField]
    Transform _hpBar = null;

    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0.0f, 1.0f);
        _hpBar.localScale = new Vector3(ratio, 1.0f, 1.0f);
    }
}
