using System.Reflection;

namespace UGCFHotFix
{
    class HotFixTool
    {
        /// <summary>
        /// 获取指定对象的指定私有字段的值
        /// </summary>
        /// <param name="instance">指定的对象</param>
        /// <param name="filedName">指定的私有字段名</param>
        /// <returns>指定字段的值</returns>
        public static object GetPrivateField(object instance, string filedName)
        {
            FieldInfo fi = instance.GetType().GetField(filedName, BindingFlags.NonPublic | BindingFlags.Instance);
            return fi.GetValue(instance);
        }

        /// <summary>
        /// 设置指定对象的指定私有字段的值
        /// </summary>
        /// <param name="instance">指定的对象</param>
        /// <param name="filedName">指定的私有字段名</param>
        /// <param name="value">要设置的值</param>
        public static void SetPrivateField(object instance, string filedName, object value)
        {
            FieldInfo fi = instance.GetType().GetField(filedName, BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(instance, value);
        }

        /// <summary>
        /// 执行指定对象的指定私有函数
        /// </summary>
        /// <param name="instance">指定的对象</param>
        /// <param name="methodName">指定的私有函数名</param>
        /// <param name="value">函数的所有参数</param>
        /// <returns>函数的返回值</returns>
        public static object InvokePrivateMethod(object instance, string methodName, params object[] value)
        {
            MethodInfo mi = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return mi.Invoke(instance, value);
        }
    }
}
