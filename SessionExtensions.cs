using System;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace PTM2._0.Extensions
{
    /// <summary>
    /// Session扩展方法，支持布尔值存储
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// 向Session中存储布尔值
        /// </summary>
        public static void SetBool(this ISession session, string key, bool value)
        {
            session.SetString(key, value.ToString());
        }

        /// <summary>
        /// 从Session中获取布尔值
        /// </summary>
        public static bool GetBool(this ISession session, string key)
        {
            if (session.GetString(key) is string value)
            {
                return bool.TryParse(value, out bool result) ? result : false;
            }
            return false;
        }

        /// <summary>
        /// 从Session中获取枚举值
        /// </summary>
        public static T GetEnum<T>(this ISession session, string key) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"{typeof(T)} 必须是枚举类型");
            }

            if (session.GetString(key) is string value)
            {
                return (T)Enum.Parse(typeof(T), value);
            }
            return default;
        }

        /// <summary>
        /// 向Session中存储枚举值
        /// </summary>
        public static void SetEnum<T>(this ISession session, string key, T value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"{typeof(T)} 必须是枚举类型");
            }
            session.SetString(key, value.ToString());
        }
    }
}