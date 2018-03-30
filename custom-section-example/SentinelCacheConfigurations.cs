using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Http.Description;
using System.Xml;

namespace Sentinel
{
    public sealed class SentinelCacheConfigurations : ConfigurationSection
    {
        [
            ConfigurationProperty("functions", IsDefaultCollection = true),
            ConfigurationCollection(typeof (FunctionCollection), AddItemName = "function",
                ClearItemsName = "clear", RemoveItemName = "remove")
        ]
        public FunctionCollection Functions
        {
            get { return this["functions"] as FunctionCollection; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class FunctionCollection : ConfigurationElementCollection
        {
            public override ConfigurationElementCollectionType CollectionType
            {
                get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
            }

            public FunctionElement this[int index]
            {
                get { return (FunctionElement) BaseGet(index); }
                set
                {
                    if (BaseGet(index) != null)
                        BaseRemoveAt(index);
                    BaseAdd(index, value);
                }
            }

            public void Add(FunctionElement element)
            {
                BaseAdd(element);
            }

            public void Clear()
            {
                BaseClear();
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new FunctionElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((FunctionElement) element).MethodName;
            }

            public void Remove(FunctionElement element)
            {
                BaseRemove(element.MethodName);
            }

            public void Remove(string name)
            {
                BaseRemove(name);
            }

            public void RemoveAt(int index)
            {
                BaseRemoveAt(index);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public class FunctionElement : ConfigurationElement
        {
            /// <summary>
            /// 
            /// </summary>
            public FunctionElement()
            {
            }
            /// <summary>
            /// 
            /// </summary>
            [ConfigurationProperty("assembly", IsRequired = true, DefaultValue = "System.Web")]
            public string Assembly
            {
                get { return (string) this["assembly"]; }
                set { this["assembly"] = value; }
            }
            /// <summary>
            /// 
            /// </summary>
            [ConfigurationProperty("name", IsRequired = false, DefaultValue = "System.Web")]
            public string Name
            {
                get { return (string)this["name"]; }
                set { this["name"] = value; }
            }
            /// <summary>
            /// 
            /// </summary>
            [ConfigurationProperty("methodName", IsRequired = true, DefaultValue = "Null")]
            public string MethodName
            {
                get { return (string) this["methodName"]; }
                set { this["methodName"] = value; }
            }
            /// <summary>
            /// 
            /// </summary>
            [ConfigurationProperty("initialAddDelay", DefaultValue = ".05", IsRequired = false)]
            public double InitialAddDelay
            {
                get
                { return (double)this["initialAddDelay"]; }
                set
                { this["initialAddDelay"] = value; }
            }
            /// <summary>
            /// 
            /// </summary>
            [ConfigurationProperty("temporaryBlockDuration", DefaultValue = ".025", IsRequired = false)]
            public double TemporaryBlockDuration
            {
                get
                { return (double)this["temporaryBlockDuration"]; }
                set
                { this["temporaryBlockDuration"] = value; }
            }

            /// <summary>
            /// The interval that you want the cache to refresh itself
            /// </summary>
            [ConfigurationProperty("updateInterval", DefaultValue = "1", IsRequired = true)]
            public double UpdateInterval
            {
                get
                { return (double)this["updateInterval"]; }
                set
                { this["updateInterval"] = value; }
            }
            /// <summary>
            /// This is the absolute expiration of the "value" cache
            /// </summary>
            [ConfigurationProperty("maximumSurvival", DefaultValue = "1", IsRequired = true)]
            public double MaximumCacheSurvival
            {
                get
                { return (double)this["maximumSurvival"]; }
                set
                { this["maximumSurvival"] = value; }
            }
            /// <summary>
            /// 
            /// </summary>
            [ConfigurationProperty("parameters", DefaultValue = null, IsKey = true)]
            public SignatureElement Parameters
            {
                get
                {
                    return (SignatureElement)this["parameters"];
                }
                set
                { this["parameters"] = value; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SignatureElement : ConfigurationElement
        {
            public string Value { get; set; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="s"></param>
            protected override void DeserializeElement(XmlReader reader, bool s)
            {
                Value = reader.ReadElementContentAs(typeof(string), null) as string;
            }
        }
    }
}