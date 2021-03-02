/**
 *Copyright(C) 2019 by DefaultCompany
 *All rights reserved.
 *FileName:     BaseUI.cs
 *Author:       wy
 *Version:      1.0
 *UnityVersion：2017.2.2f1
 *Date:         2019-05-10
 *Description:  UI窗体父类
 *定义UI窗体的父类
 * 有四个生命周期
 * 1.Display显示状态
 * 2.Hiding隐藏状态
 * 3.ReDisplay再显示状态
 * 4.Freeze冻结状态 就是弹出窗体后面的窗体冻结
 *History:
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SUIFW {
    //窗体类型
    public class UIType {
        /// <summary>
        /// 是否清空"栈集合"
        /// </summary>
        public bool isClearStack = false;
        /// <summary>
        /// UI窗体(位置)类型
        /// </summary>
        public UIFormType type = UIFormType.Normal;
        /// <summary>
        ///UI窗体显示类型
        /// </summary>
        public UIFormShowMode mode = UIFormShowMode.Normal;
        /// <summary>
        /// UI窗体透明度类型
        /// </summary>
        public UIFormLucenyType lucenyType = UIFormLucenyType.Lucency;

    }

    public class BaseUI : MonoBehaviour {
        public UIType currentUIType { get; set; } = new UIType();

        public UIKey uikey { get; set; }

        #region 窗体的四种状态
        /// <summary>
        /// 显示状态
        /// </summary>
        public virtual void ActiveTrue() {
            gameObject.SetActive(true);
        }
        /// <summary>
        /// 隐藏状态
        /// </summary>
        public virtual void ActiveFalse() {

            gameObject.SetActive(false);
        }
        /// <summary>
        /// 重新显示状态
        /// </summary>
        public virtual void ReActiveTrue() {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 冻结状态
        /// </summary>
        public virtual void Freeze() {
            gameObject.SetActive(true);
        }
        #endregion

        #region 封装子类常用方法
        /// <summary>
        /// 注册按钮事件
        /// </summary>
        protected void RigisterBtnOnClick(string btnName, EventTriggerListener.VoidDelegate del) {
            Transform btn = UnityHelper.Find(gameObject.transform, btnName);
            EventTriggerListener.Get(btn?.gameObject).onClick = del;
        }

        /// <summary>
        /// 打开UI窗体
        /// </summary>
        /// <param name="UIName"></param>
        protected virtual void OpenUI(UIKey UIName) {
            UIManager.instance.ShowUI(UIName);
        }

        /// <summary>
        /// 关闭UI窗体
        /// </summary>
        public virtual void CloseUI() {
            string UIName = GetType().ToString();
            UIManager.instance.CloseUI(uikey);
        }
        #endregion
    }
}