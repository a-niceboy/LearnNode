

using System.Collections.Generic;
/**
*Copyright(C) 2019 by DefaultCompany
*All rights reserved.
*FileName:     SysDefine.cs
*Author:       wy
*Version:      1.0
*UnityVersion：2018.4.26f1
*Date:         2021-02-25
*Description:   UI框架配置
* 1.系统常量
* 2.全局性方法
* 3.系统枚举类型
* 4.委托定义
*History:
*/
namespace SUIFW {
    public enum UIKey {
        NONE,
        CANVAS,
        LOGIN,
        SELECT
    }

    public static class UIConfig {
        /// <summary>
        /// 路径常量
        /// </summary>
        /// 
        public static Dictionary<UIKey, string> Path;
        static UIConfig() {
            Path = new Dictionary<UIKey, string>();

            Path[UIKey.CANVAS] = "Canvas";
            Path[UIKey.LOGIN] = "UIPrefabs/LoginUI";
            Path[UIKey.SELECT] = "SelectUI";
        }
        /// <summary>
        /// 标签常量
        /// </summary>
        public static class Tag {
            public const string canvas = "Canvas";
        }
        /// <summary>
        /// 节点常量
        /// </summary>
        public static class Node {
            public const string normal = "Normal";
            public const string @fixed = "Fixed";
            public const string popUp = "PopUp";
            public const string ScriptsMgr = "ScriptsMgr";
        }
    }
}
