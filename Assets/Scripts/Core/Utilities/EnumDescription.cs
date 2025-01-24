using System;
using System.ComponentModel;

namespace Scripts.Core
{
    public static class EnumHelper
    {
        public static string GetDescription<T>(this T enumValue) 
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }

        public static TOutput GetValue<TEnum, TOutput>(this TEnum enumValue)
            where TEnum : struct, IConvertible
            where TOutput : IConvertible
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an enumerated type");

            var value = Convert.ChangeType(enumValue, typeof(TOutput));
            return (TOutput)value;
        }

    }
}