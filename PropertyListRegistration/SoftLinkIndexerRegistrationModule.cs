using System;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.TypeScanner;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.Compatibility;
using EPiServer.SpecializedProperties;

namespace PropertyListRegistration
{
    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class SoftLinkIndexerRegistrationModule: IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var typeScannerLookup = context.Container.GetInstance<ITypeScannerLookup>();
            var types = typeScannerLookup.AllTypes;
            foreach (var type in types)
            {
                var hasAttributes = type.GetCustomAttributes(typeof(PropertyDefinitionTypePlugInAttribute), true).Any();
                if (hasAttributes == false)
                {
                    continue;
                }

                // PropertyContentReferenceList already has indexer implementation
                if (type == typeof(PropertyContentReferenceList))
                {
                    continue;
                }
                
                var baseType = type.BaseType;
                while (baseType != null)
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(PropertyList<>))
                    {
                        context.Services.Configure(c =>
                        {
                            RegisterIndexer(c, baseType);
                        });

                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
        }

        private static void RegisterIndexer(ConfigurationBuilder c, Type baseType)
        {
            var genericArgument = baseType.GetGenericArguments().SingleOrDefault();
            c.For(typeof(IPropertySoftLinkIndexer)).Use(typeof(GenericListIndexer<>).MakeGenericType(genericArgument));
        }
    }
}