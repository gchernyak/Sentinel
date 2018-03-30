using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Sentinel
{
    /// <summary>
    /// Class allow business logic pertaining to Cache configuration
    /// </summary>
    public static class CacheConfigurations
    {
        /// <summary>
        /// Add configuration files into memory cache
        /// </summary>
        public static void CacheConfigurationFiles()
        {
            SetupSelfUpdatingCache();
        }

        private static void SetupSelfUpdatingCache()
        {
            var uid = ConfigurationManager.AppSettings["ServiceUserID"].ToInt();
            var configs = (SentinelCacheConfigurations) ConfigurationManager.GetSection("sentinelCache");

           foreach (SentinelCacheConfigurations.FunctionElement config in configs.Functions)
            {
                var methodName = config.MethodName;



                Type tt = Type.GetType(string.Format("{0}, {1}", config.Name, config.Assembly));
                var instance = Activator.CreateInstance(tt);
                MethodInfo method = tt.GetMethod(methodName);
                var methodParams = method.GetParameters();
                var methodParamCount = methodParams.Count();
                var objArray = new object[methodParamCount];
                var paramValue = config.Parameters.Value;
                if (!string.IsNullOrEmpty(paramValue))
                {
                    // retrieving the type of acceptable parameters (XML, JSON or List)
                    var paramValueType = paramValue.StringDataType();
                    switch (paramValueType)
                    {
                        case "xml":
                            var rawXml = config.Parameters.Value.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                            var doc = XDocument.Parse(rawXml);
                            // reconstructing the objects passed in as parameters and adding them to object [] for delegate
                            if (doc.Root != null) { 
                                var index = 0;
                                foreach (var node in doc.Root.Elements())
                                {
                                    if (node.Attribute("assembly") != null && node.Attribute("namespace") != null)
                                    {
                                        var assemby = node.Attribute("assembly").Value;
                                        var nameSpace = node.Attribute("namespace").Value;
                                        var objName = node.Name;
                                        Type currentType =
                                            Type.GetType(string.Format("{0}.{1}, {2}", nameSpace, objName, assemby));
                                        try
                                        {
                                            var serializer = new XmlSerializer(currentType);
                                            objArray[index] = serializer.Deserialize(new StringReader(node.ToString())); 
                                            index++;

                                        }
                                        catch (Exception ex)
                                        {

                                            throw ex;
                                        }
                                    }
                                    else
                                    {
                                        var val = node.Value;
                                        objArray = SetParameter(val.GetType(), objArray, index, val);
                                        index++;
                                    }
                                }
                            }

                            break;
                        case "list":
                            if (methodParamCount > 0 && !string.IsNullOrEmpty(paramValue))
                            {
                                var items =
                                    paramValue.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim().Split('='));
                                var index = 0;
                                foreach (ParameterInfo parameterInfo in methodParams)
                                {
                                    var parameterType = parameterInfo.ParameterType;
                                    var currentKey = parameterInfo.Name;
                                    var incomingValue = items.Where(k => k.Contains(currentKey)).ToArray()[0][1];
                                    if (incomingValue.GetType() != parameterType)
                                    {
                                        objArray = SetParameter(parameterType, objArray, index, incomingValue);
                                    }
                                    else
                                    {
                                        objArray[index] = incomingValue;
                                    }
                                    index++;
                                }
                            }
                            break;
                        default:
                            throw new NotImplementedException(
                                "Parameter is not in an acceptable data type (XML,JSON or List)");
                    }
                }

                SentinelCache.GetGenericResults(() => method.Invoke(instance, objArray), method.Name,
                    TimeSpan.FromSeconds(config.InitialAddDelay), TimeSpan.FromMinutes(config.TemporaryBlockDuration),
                    TimeSpan.FromMinutes(config.UpdateInterval), TimeSpan.FromMinutes(config.MaximumCacheSurvival));
            }
        }

        private static object[] SetParameter(Type parameterType, object[] target, int index, string value)
        {
            var incomingType = Type.GetTypeCode(parameterType);
            try
            {
                switch (incomingType)
                {
                    case TypeCode.Boolean:
                        target[index] = Convert.ToBoolean(value);
                        break;
                    case TypeCode.Byte:
                        target[index] = Convert.ToByte(value);
                        break;
                    case TypeCode.SByte:
                        target[index] = Convert.ToSByte(value);
                        break;
                    case TypeCode.Int16:
                        target[index] = Convert.ToInt16(value);
                        break;
                    case TypeCode.UInt16:
                        target[index] = Convert.ToUInt16(value);
                        break;
                    case TypeCode.Int32:
                        target[index] = Convert.ToInt32(value);
                        break;
                    case TypeCode.UInt32:
                        target[index] = Convert.ToUInt32(value);
                        break;
                    case TypeCode.Int64:
                        target[index] = Convert.ToInt64(value);
                        break;
                    case TypeCode.UInt64:
                        target[index] = Convert.ToUInt64(value);
                        break;
                    case TypeCode.Char:
                        target[index] = Convert.ToChar(value);
                        break;
                    case TypeCode.Double:
                        target[index] = Convert.ToDouble(value);
                        break;
                    case TypeCode.Decimal:
                        target[index] = Convert.ToDecimal(value);
                        break;
                    case TypeCode.Single:
                        target[index] = Convert.ToSingle(value);
                        break;
                    default:
                        target[index] = value;
                        break;
                }
                return target;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}