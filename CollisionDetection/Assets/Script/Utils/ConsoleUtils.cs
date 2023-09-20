using LitJson;
using System;
using UnityEngine;

namespace Game
{
    public class ConsoleUtils
    {
        private static string PrintDic(dynamic dic)
        {
            string res = "{ ";
            foreach (dynamic data in dic)
            {
                string key = data.Key.ToString();
                string valueName = data.Value.GetType().Name;
                if (valueName.IndexOf("Dictionary") != -1)
                {
                    res += key + ":" + PrintDic(data.Value) + ",";
                }
                else if (valueName.IndexOf("List") != -1)
                {
                    res += key + ":" + PrintList(data.Value) + ",";
                }
                else
                {
                    res += key + ":" + data.Value.ToString() + ",";
                }
            }
            res = res.Substring(0, res.Length - 1) + " }";
            return res;
        }

        private static string PrintList(dynamic list)
        {
            string res = "[ ";
            for(int i = 0,len = list.Count; i < len; i++)
            {
                string valueName = list[i].GetType().Name;
                if (valueName.IndexOf("Dictionary") != -1)
                {
                    res += PrintDic(list[i]) + ",";
                }
                else if (valueName.IndexOf("List") != -1)
                {
                    res += PrintList(list[i]) + ",";
                }
                else
                {
                    res += list[i].ToString() + ",";
                }
            }
            res = res.Substring(0,res.Length - 1) + " ]";
            return res;
        }
        
        /// <summary>
        /// 打印到控制台
        /// </summary>
        /// <param name="objs"></param>
        public static void Log(params object[] objs)
        {
            string str = string.Empty;
            bool isWrap = false;
            foreach (object obj in objs)
            {
                if (obj == null) continue;
                Type type = obj.GetType();
                string extra = string.Empty;
                if (type.Name.IndexOf("[]") != -1)
                {
                    extra = ",\r\n[ ";
                    Array objArray = (Array)obj;
                    int i = 0;
                    int len = objArray.Length;
                    foreach (object data in objArray)
                    {
                        extra += data.ToString() + (i == len - 1 ? string.Empty : " , ");
                        i++;
                    }

                    extra += " ]";
                    str += extra;
                    isWrap = true;
                }
                else if (type == typeof(JsonData))
                {
                    string json = ((JsonData)obj).ToJson();
                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else if (type.Name.IndexOf("Dictionary") != -1)
                {
                    string json = PrintDic(obj);

                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else if (type.Name.IndexOf("List") != -1)
                {
                    string json = PrintList(obj);

                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else
                {
                    if (isWrap)
                    {
                        isWrap = false;
                        str += ",\r\n";
                    }
                    str += obj.ToString() + " ";
                }
            }
            Debug.Log(str);
        }

        public static void Warn(params object[] objs)
        {
            string str = string.Empty;
            bool isWrap = false;
            foreach (object obj in objs)
            {
                Type type = obj.GetType();
                string extra = string.Empty;
                if (type.Name.IndexOf("[]") != -1)
                {
                    extra = ",\r\n[ ";
                    Array objArray = (Array)obj;
                    int i = 0;
                    int len = objArray.Length;
                    foreach (object data in objArray)
                    {
                        extra += data.ToString() + (i == len - 1 ? string.Empty : " , ");
                        i++;
                    }

                    extra += " ]";
                    str += extra;
                    isWrap = true;
                }
                else if (type == typeof(JsonData))
                {
                    string json = ((JsonData)obj).ToJson();
                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else if (type.Name.IndexOf("Dictionary") != -1)
                {
                    string json = PrintDic(obj);

                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else if (type.Name.IndexOf("List") != -1)
                {
                    string json = PrintList(obj);

                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else
                {
                    if (isWrap)
                    {
                        isWrap = false;
                        str += "\r\n";
                    }
                    str += obj.ToString() + " ";
                }
            }
            Debug.LogWarning(str);
        }

        public static void Error(params object[] objs)
        {
            string str = string.Empty;
            bool isWrap = false;
            foreach (object obj in objs)
            {
                Type type = obj.GetType();
                string extra = string.Empty;
                if (type.Name.IndexOf("[]") != -1)
                {
                    extra = ",\r\n[ ";
                    Array objArray = (Array)obj;
                    int i = 0;
                    int len = objArray.Length;
                    foreach (object data in objArray)
                    {
                        extra += data.ToString() + (i == len - 1 ? string.Empty : " , ");
                        i++;
                    }

                    extra += " ]";
                    str += extra;
                    isWrap = true;
                }
                else if (type == typeof(JsonData))
                {
                    string json = ((JsonData)obj).ToJson();
                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else if (type.Name.IndexOf("Dictionary") != -1)
                {
                    string json = PrintDic(obj);

                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else if (type.Name.IndexOf("List") != -1)
                {
                    string json = PrintList(obj);

                    str += ",\r\n" + json;
                    isWrap = true;
                }
                else
                {
                    if (isWrap)
                    {
                        isWrap = false;
                        str += "\r\n";
                    }
                    str += obj.ToString() + " ";
                }
            }
            Debug.LogError(str);
        }
    }
}

