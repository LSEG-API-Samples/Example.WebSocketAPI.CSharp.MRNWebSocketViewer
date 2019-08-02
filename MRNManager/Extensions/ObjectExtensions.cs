using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MarketDataWebSocket.Models.Enum;
using WebsocketAdapter;

namespace MarketDataWebSocket.Extensions
{
    public static class ObjectExtensions
    {
        public static T ToObject<T>(this IDictionary<string, object> source)
            where T : class, new()
        {
            var tmpObject = new T();
            var tmpObjectType = tmpObject.GetType();

            foreach (var item in source)
            {
                try
                {
                    var propertyInfo = tmpObjectType.GetProperty(item.Key);

                    if (propertyInfo == null) continue;
                    if(propertyInfo.PropertyType==typeof(MrnTypeEnum))
                        propertyInfo.SetValue(tmpObject,
                            Convert.ChangeType((MrnTypeEnum)Enum.Parse(typeof(MrnTypeEnum), (string)item.Value, true), propertyInfo.PropertyType), null);
                    else if (propertyInfo.PropertyType == typeof(byte[]))
                    {
                        var tmpValue = (item.Value == null) ? null : MarketDataUtils.StringToByteArray((string)item.Value);
                        propertyInfo.SetValue(tmpObject,
                            Convert.ChangeType(tmpValue, propertyInfo.PropertyType), null);
                    }
                    else
                    {
                        propertyInfo.SetValue(tmpObject,
                            Convert.ChangeType(item.Value, propertyInfo.PropertyType), null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }

            return tmpObject;
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
}
