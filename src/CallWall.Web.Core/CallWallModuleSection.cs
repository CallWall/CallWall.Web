using System;
using System.ComponentModel;
using System.Configuration;

namespace CallWall.Web
{
    public class CallWallModuleSection : ConfigurationSection
    {
        public static CallWallModuleSection GetConfig()
        {
            var section = ConfigurationManager.GetSection("CallWallModules") as CallWallModuleSection;
            return section;
        }

        [ConfigurationProperty("modules")]
        [ConfigurationCollection(typeof(ModuleConfigurationCollection))]
        public ModuleConfigurationCollection Modules
        {
            get { return (ModuleConfigurationCollection)base["modules"]; }
        }
    }

    public class ModuleElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        [TypeConverter(typeof(TypeNameConverter))]
        [CallbackValidator(Type = typeof(ModuleElement), CallbackMethodName = "ValidateProviderType")]
        public Type Type
        {
            get { return (Type)this["type"]; }
        }

        public static void ValidateProviderType(object type)
        {
            if (!typeof(IModule).IsAssignableFrom((Type)type))
            {
                throw new ConfigurationErrorsException(
                    "The module type must implement the CallWall.Web.IModule interface.");
            }
        }
    }

    [ConfigurationCollection(typeof(ModuleElement), AddItemName = "module", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ModuleConfigurationCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ModuleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ModuleElement)element).Type;
        }

        public ModuleElement this[int index]
        {
            get { return (ModuleElement)BaseGet(index); }
        }

        protected override string ElementName
        {
            get { return "module"; }
        }
    }
}