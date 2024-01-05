using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SUIFW;
public class SelectUI : BaseUI
{
    void Awake()
    {
        currentUIType.mode = UIFormShowMode.ReverseChange;
        RigisterBtnOnClick("BtnConfirm", go => Debug.Log("进入主城"));
        RigisterBtnOnClick("btnClose", btnClose);
    }

    public void btnClose(GameObject go) {
        Debug.Log("btnClose");
        CloseUI();
    }

    void Update()
    {
        
    }
}
