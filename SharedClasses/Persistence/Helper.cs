using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SharedClasses.Persistence
{
    public static class Helper
    {
        public static StringBuilder BuildRequestString(object request, char separator)
        {

            var contentString = new StringBuilder();
            var objectProperties = DictionaryOfPropertiesFromInstance(request);

            foreach (var key in objectProperties.Keys)
            {
                contentString.Append(objectProperties[key]);
                contentString.Append(separator);
            }
            return contentString;
            throw new NotImplementedException();
        }
        public static Dictionary<string, string> DictionaryOfPropertiesFromInstance<T>(T InstanceOfAType)
        {
            if (InstanceOfAType == null) return null;
            Type TheType = InstanceOfAType.GetType();
            PropertyInfo[] Properties = TheType.GetProperties();
            Dictionary<string, string> PropertiesMap = new Dictionary<string, string>();
            foreach (PropertyInfo Prop in Properties)
            {
                var val = "";
                if (Prop.PropertyType == typeof(bool))
                {
                    val = (bool)TheType.GetProperty(Prop.Name).GetValue(InstanceOfAType) == true ? "1" : "0";
                }
                else
                {
                    val = TheType.GetProperty(Prop.Name).GetValue(InstanceOfAType) != null ? TheType.GetProperty(Prop.Name).GetValue(InstanceOfAType).ToString() : "";

                }
                PropertiesMap.Add(Prop.Name, val);
            }
            return PropertiesMap;
        }
        public static Field GetField(string key, List<Field> addInfo)
        {

            var exist = addInfo.FirstOrDefault(x => x.Value.ToLower() == key.ToLower());
            if (exist != null)
            {
                return exist;
            }
            return null;

        }
        public static string GetFormatDictionnary(string key, List<Field> addInfo)
        {
            var exist = addInfo.FirstOrDefault(x => x.Value.ToLower() == key.ToLower());
            if (exist != null && !string.IsNullOrEmpty(exist.Format))
            {
                return exist.Format;
            }
            return null;
        }
        public static T TryParse<T>(string input, int length,ref int index)
        {
            return TryParse<T>(input, length, "",ref index);
        }

        public static T TryParse<T>(string input, int length, string format, ref int index)
        {
            T result = default(T);

            if (input.Length > length && index<=0)
            {
                throw new ArgumentOutOfRangeException(input, new ArgumentOutOfRangeException(), "Field out of range length");
            }
            else
            {
            if (!string.IsNullOrEmpty(input))
            {
                if (result is DateTime && format.Length > 1)
                    result = (T)(object)DateTime.ParseExact((string)input, format, null);
                else
                {
                    var r = result = (T)Convert.ChangeType(input, typeof(T));
                }
            }
                
            }
            index--;
            return result;
        }
        public static T TryParseDecimal<T>(string input,int length, char separator,char toReplace,ref int index)
        {
            var result = "";
            if(string.IsNullOrEmpty(input))
            {
                 result = input.Replace(toReplace, separator);
            }

            return TryParse<T>(result,length,ref index);
        }

        

        public static string[] ResponseSplitedBySeparator(string msg, char separator)
        {
            return msg.Split(separator);
        }
    }
}
