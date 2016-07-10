using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mojang.Minecraft
{
    public class Chat : IPackageField
    {
        public static Chat Empty;
        public static Chat EmptyText;
        private static readonly JavaScriptSerializer m_JavaScriptSerializer = new JavaScriptSerializer();


        static Chat()
        {
            m_JavaScriptSerializer.RegisterConverters(new[] { new ExpandoJsonConverter() });
            Empty = new Chat();
            EmptyText = new Chat("{\"text\":\"\"}");
        }


        //{clickEvent:{action:run_command,value:"/say 1"},text:"/say 1"}

        public dynamic Root => _InnerJson;

        private dynamic _InnerJson = new ExpandoObject();


        public static bool TryParse(string s, out Chat result)
        {
            try
            {
                result = Parse(s);
            }
            catch (FormatException)
            {
                result = new Chat();
                return false;
            }
            return true;
        }


        public static Chat Parse(string s)
        {
            try
            {
                return new Chat(s);
            }
            catch (ArgumentException)
            {
                throw new FormatException();
            }
        }

        public Chat()
        {

        }


        public Chat(string jsonStr)
        {
            try
            {
                if (string.Empty.Equals(jsonStr))
                {
                    _InnerJson = new ExpandoObject();
                }
                else
                {
                    _InnerJson = ToExpandoJson(jsonStr);
                }
                //_InnerJson = ToExpandoJson(jsonStr) ?? new ExpandoObject();
            }
            catch (ArgumentException)
            {
                _InnerJson = new ExpandoObject();
                //throw new ArgumentException("无效的json");
            }
        }


        

        public static string ToJsonString(ExpandoObject expandoObject) =>
            m_JavaScriptSerializer.Serialize(expandoObject);

        public static dynamic ToExpandoJson(string jsonString) =>
            m_JavaScriptSerializer.Deserialize<ExpandoObject>(jsonString);


        private class ExpandoJsonConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes => new[] { typeof(ExpandoObject) };

            /// <summary>
            /// 反序列化字典
            /// </summary>
            /// <param name="dictionary">作为名称/值对存储的属性数据的 IDictionary&ltTKey-TValue&gt实例</param>
            /// <param name="type">所生成对象的类型</param>
            /// <param name="serializer">JavaScriptSerializer实例</param>
            /// <returns>反序列化的对象</returns>
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                return ConvertIDictionaryObject<ExpandoObject>(dictionary);
            }

            /// <summary>
            /// 序列化对象
            /// </summary>
            /// <param name="obj">要序列化的对象</param>
            /// <param name="serializer">负责序列化的对象</param>
            /// <returns>一个对象，包含表示该对象数据的键/值对</returns>
            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                return ConvertIDictionaryObject<Dictionary<string, object>>(obj);
                //return JsonDictionaryFromExpandoJson(obj);
            }

            private static dynamic ConvertIDictionaryObject<TTarget>(dynamic IDictionary)
                where TTarget : new()
            {
                //如果是ICollection<object> 或 IDictionary<string, object>则遍历
                if (IDictionary is IDictionary<string, object>)
                {
                    //创建一个ExpandoObject
                    var e = (IDictionary<string, object>)new TTarget();
                    //如果是字典，则获取其全部的KeyValuePair,并构建在此类型中
                    foreach (var keyValuePair in IDictionary)
                    {
                        e.Add(keyValuePair.Key, ConvertIDictionaryObject<TTarget>(keyValuePair.Value));
                    }
                    return e;
                }
                else if (IDictionary is ArrayList)
                {
                    var list = new List<dynamic>();

                    foreach (var atom in IDictionary)
                    {
                        var expando = (dynamic)null;
                        if (atom is IDictionary<string, object>)
                        {
                            expando = ConvertIDictionaryObject<TTarget>(atom);
                        }
                        else
                        {
                            expando = atom;
                        }
                        list.Add(expando);
                    }
                    return list.ToArray();
                }
                return IDictionary;
            }
        }


        public override string ToString()
            => ToJsonString(_InnerJson as ExpandoObject);

        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            var jsonStr = new VarintPrefixedUTF8String();
            (jsonStr as IPackageField).FromFieldMatcher(fieldMatcher);
            if (string.Empty.Equals(jsonStr) || jsonStr == "\"\"")
            {
                _InnerJson = new ExpandoObject();
            }
            else
            {
                _InnerJson = ToExpandoJson(jsonStr);
            }
        }

        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            var jsonStr = new VarintPrefixedUTF8String(ToJsonString(_InnerJson));
            //TODO:空牌子解释
            if (jsonStr == "{}")
                jsonStr = "\"\"";
            (jsonStr as IPackageField).AppendIntoPackageMaker(packageMaker);
        }

        public static implicit operator string (Chat json)
            => json.ToString();


        public static implicit operator Chat(string str)
            => new Chat(str);
    }
}

