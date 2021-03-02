using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        SUIFW.UIManager.instance.ShowUI(SUIFW.UIKey.LOGIN);
    }
}
