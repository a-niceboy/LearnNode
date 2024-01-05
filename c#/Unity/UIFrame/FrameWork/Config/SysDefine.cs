/**
 *Copyright(C) 2019 by DefaultCompany
 *All rights reserved.
 *FileName:     SysDefine.cs
 *Author:       why
 *Version:      1.0
 *UnityVersion：2017.2.2f1
 *Date:         2019-05-10
 *Description:   UI框架核心参数
 * 1.系统常量
 * 2.全局性方法
 * 3.系统枚举类型
 * 4.委托定义
 *History:
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SUIFW {
    #region UI系统枚举类型
    public enum UILifeType {
        DestroyImmediate,
        DestroyLater,
        DontDestroy
    }

    /// <summary>
    /// UI窗体(位置)类型
    /// </summary>
    public enum UIFormType {
        /// <summary>
        /// 普通窗体
        /// </summary>
        Normal,
        /// <summary>
        /// 固定窗体
        /// </summary>
        Fixed,
        /// <summary>
        /// 弹出窗体
        /// </summary>
        PopUp,
    }

    /// <summary>
    /// UI窗体的显示类型
    /// </summary>
    public enum UIFormShowMode {
        /// <summary>
        /// 普通
        /// </summary>
        Normal,
        /// <summary>
        /// 反向切换 
        /// </summary>//按照相反方向切换过去 原路弹回来
        ReverseChange,
        /// <summary>
        /// 隐藏其他 
        /// </summary>
        HideOther
    }

    /// <summary>
    /// UI窗体透明度类型
    /// </summary>
    public enum UIFormLucenyType {
        /// <summary>
        /// 完全透明，不能穿透
        /// </summary>
        Lucency,
        /// <summary>
        /// 半透明，不能穿透
        /// </summary>
        Translucence,
        /// <summary>
        /// 低透明度，不能穿透
        /// </summary>
        ImPenetrable,
        /// <summary>
        /// 可以穿透
        /// </summary>
        Pentrate
    }
    #endregion
}