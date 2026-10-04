using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RhythmArmy.Core.Save
{
    public static class MiniJson
    {
        public static string Serialize(object obj)
        {
            var sb = new StringBuilder();
            SerializeValue(obj, sb);
            return sb.ToString();
        }

        private static void SerializeValue(object value, StringBuilder sb)
        {
            if (value == null)
            {
                sb.Append("null");
                return;
            }

            var type = value.GetType();

            string str = value as string;
            if (str != null)
            {
                sb.Append("\"").Append(str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r")).Append("\"");
                return;
            }

            if (value is bool)
            {
                sb.Append(((bool)value) ? "true" : "false");
                return;
            }

            if (value is int || value is long || value is float || value is double || value is decimal)
            {
                sb.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                return;
            }

            if (type.IsEnum)
            {
                sb.Append("\"").Append(value.ToString()).Append("\"");
                return;
            }

            IDictionary dict = value as IDictionary;
            if (dict != null)
            {
                sb.Append("{");
                bool first = true;
                foreach (DictionaryEntry kv in dict)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append("\"").Append(kv.Key).Append("\":");
                    SerializeValue(kv.Value, sb);
                }
                sb.Append("}");
                return;
            }

            IList list = value as IList;
            if (list != null)
            {
                sb.Append("[");
                bool first = true;
                foreach (var item in list)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    SerializeValue(item, sb);
                }
                sb.Append("]");
                return;
            }

            // Complex Object
            sb.Append("{");
            bool firstObj = true;
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var f in fields)
            {
                if (!firstObj) sb.Append(",");
                firstObj = false;
                sb.Append("\"").Append(f.Name).Append("\":");
                SerializeValue(f.GetValue(value), sb);
            }
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                if (!p.CanRead || !p.CanWrite) continue;
                if (!firstObj) sb.Append(",");
                firstObj = false;
                sb.Append("\"").Append(p.Name).Append("\":");
                SerializeValue(p.GetValue(value, null), sb);
            }
            sb.Append("}");
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default(T);
            var parsed = JsonParser.Parse(json);
            return (T)ConvertParsedObject(parsed, typeof(T));
        }

        private static object ConvertParsedObject(object parsed, Type targetType)
        {
            if (parsed == null) return null;

            if (targetType == typeof(string)) return parsed.ToString();
            if (targetType == typeof(int)) return Convert.ToInt32(parsed);
            if (targetType == typeof(long)) return Convert.ToInt64(parsed);
            if (targetType == typeof(float)) return Convert.ToSingle(parsed);
            if (targetType == typeof(double)) return Convert.ToDouble(parsed);
            if (targetType == typeof(bool)) return Convert.ToBoolean(parsed);
            if (targetType.IsEnum) return Enum.Parse(targetType, parsed.ToString(), true);

            List<object> list = parsed as List<object>;
            if (list != null)
            {
                if (targetType.IsArray)
                {
                    var elemType = targetType.GetElementType();
                    var array = Array.CreateInstance(elemType, list.Count);
                    for (int i = 0; i < list.Count; i++)
                    {
                        array.SetValue(ConvertParsedObject(list[i], elemType), i);
                    }
                    return array;
                }
                if (typeof(IList).IsAssignableFrom(targetType))
                {
                    var instance = (IList)Activator.CreateInstance(targetType);
                    var elemType = targetType.IsGenericType ? targetType.GetGenericArguments()[0] : typeof(object);
                    foreach (var item in list)
                    {
                        instance.Add(ConvertParsedObject(item, elemType));
                    }
                    return instance;
                }
            }

            Dictionary<string, object> dict = parsed as Dictionary<string, object>;
            if (dict != null)
            {
                if (typeof(IDictionary).IsAssignableFrom(targetType))
                {
                    var instance = (IDictionary)Activator.CreateInstance(targetType);
                    var keyType = targetType.GetGenericArguments()[0];
                    var valType = targetType.GetGenericArguments()[1];
                    foreach (KeyValuePair<string, object> kv in dict)
                    {
                        instance.Add(ConvertParsedObject(kv.Key, keyType), ConvertParsedObject(kv.Value, valType));
                    }
                    return instance;
                }

                var instanceObj = Activator.CreateInstance(targetType);
                var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var f in fields)
                {
                    object val;
                    if (dict.TryGetValue(f.Name, out val))
                    {
                        f.SetValue(instanceObj, ConvertParsedObject(val, f.FieldType));
                    }
                }
                var props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var p in props)
                {
                    if (!p.CanWrite) continue;
                    object val;
                    if (dict.TryGetValue(p.Name, out val))
                    {
                        p.SetValue(instanceObj, ConvertParsedObject(val, p.PropertyType), null);
                    }
                }
                return instanceObj;
            }

            return Convert.ChangeType(parsed, targetType);
        }

        private class JsonParser
        {
            private readonly string _json;
            private int _index;

            private JsonParser(string json)
            {
                _json = json;
                _index = 0;
            }

            public static object Parse(string json)
            {
                return new JsonParser(json).ParseValue();
            }

            private void SkipWhitespace()
            {
                while (_index < _json.Length && char.IsWhiteSpace(_json[_index]))
                {
                    _index++;
                }
            }

            private object ParseValue()
            {
                SkipWhitespace();
                if (_index >= _json.Length) return null;

                char c = _json[_index];
                if (c == '{') return ParseObject();
                if (c == '[') return ParseArray();
                if (c == '"') return ParseString();
                if (char.IsDigit(c) || c == '-') return ParseNumber();
                if (c == 't' || c == 'f') return ParseBool();
                if (c == 'n') return ParseNull();

                throw new FormatException(string.Format("Unexpected character: {0} at index {1}", c, _index));
            }

            private Dictionary<string, object> ParseObject()
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _index++; // Skip '{'
                SkipWhitespace();

                if (_index < _json.Length && _json[_index] == '}')
                {
                    _index++;
                    return dict;
                }

                while (_index < _json.Length)
                {
                    SkipWhitespace();
                    string key = ParseString();
                    SkipWhitespace();
                    if (_index < _json.Length && _json[_index] == ':') _index++; // Skip ':'
                    object val = ParseValue();
                    dict[key] = val;

                    SkipWhitespace();
                    if (_index < _json.Length && _json[_index] == ',')
                    {
                        _index++;
                        continue;
                    }
                    if (_index < _json.Length && _json[_index] == '}')
                    {
                        _index++;
                        break;
                    }
                }

                return dict;
            }

            private List<object> ParseArray()
            {
                var list = new List<object>();
                _index++; // Skip '['
                SkipWhitespace();

                if (_index < _json.Length && _json[_index] == ']')
                {
                    _index++;
                    return list;
                }

                while (_index < _json.Length)
                {
                    object val = ParseValue();
                    list.Add(val);

                    SkipWhitespace();
                    if (_index < _json.Length && _json[_index] == ',')
                    {
                        _index++;
                        continue;
                    }
                    if (_index < _json.Length && _json[_index] == ']')
                    {
                        _index++;
                        break;
                    }
                }

                return list;
            }

            private string ParseString()
            {
                _index++; // Skip opening '"'
                var sb = new StringBuilder();

                while (_index < _json.Length)
                {
                    char c = _json[_index++];
                    if (c == '"') return sb.ToString();
                    if (c == '\\' && _index < _json.Length)
                    {
                        char next = _json[_index++];
                        switch (next)
                        {
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            default: sb.Append(next); break;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                return sb.ToString();
            }

            private object ParseNumber()
            {
                int start = _index;
                while (_index < _json.Length && (char.IsDigit(_json[_index]) || _json[_index] == '.' || _json[_index] == '-'))
                {
                    _index++;
                }

                string str = _json.Substring(start, _index - start);
                if (str.Contains("."))
                {
                    return double.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
                }
                return long.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }

            private bool ParseBool()
            {
                if (_json.Substring(_index).StartsWith("true", StringComparison.OrdinalIgnoreCase))
                {
                    _index += 4;
                    return true;
                }
                if (_json.Substring(_index).StartsWith("false", StringComparison.OrdinalIgnoreCase))
                {
                    _index += 5;
                    return false;
                }
                throw new FormatException("Invalid boolean literal at index " + _index);
            }

            private object ParseNull()
            {
                if (_json.Substring(_index).StartsWith("null", StringComparison.OrdinalIgnoreCase))
                {
                    _index += 4;
                    return null;
                }
                throw new FormatException("Invalid null literal at index " + _index);
            }
        }
    }
}
