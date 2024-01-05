using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SUIFW;
public class LoginUI : BaseUI
{
    void Awake()
    {
        RigisterBtnOnClick("btnLogin", LoginSys);
    }

    public void LoginSys(GameObject go) {
        Debug.Log("LoginSys");
        OpenUI(UIKey.SELECT);
    }

    void Update()
    {
        
    }
}
