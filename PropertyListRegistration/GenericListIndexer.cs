using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;

namespace PropertyListRegistration
{
    //registered automatically through SoftLinkIndexerRegistrationModule
    //[ServiceConfiguration(typeof(IPropertySoftLinkIndexer))] 
    public class GenericListIndexer<T> : IPropertySoftLinkIndexer<IList<T>>
    {
        private readonly ServiceAccessor<ContentSoftLinkIndexer> _contentSoftLinkIndexer;

        public GenericListIndexer(ServiceAccessor<ContentSoftLinkIndexer> contentSoftLinkIndexer)
        {
            // Have to use ServiceLocator to get ContentSoftLinkIndexer because of bi-directional dependencies
            _contentSoftLinkIndexer = contentSoftLinkIndexer;
        }

        public IEnumerable<SoftLink> ResolveReferences(IList<T> propertyValue, IContent owner)
        {
            // add all ContentReferences properties
            var softLinks = IndexContentReferenceProperties(propertyValue, owner).ToList();

            // add all properties that has implementation for IPropertySoftLinkIndexer
            var contentSoftLinkIndexer = this._contentSoftLinkIndexer();
            foreach (var p in propertyValue)
            {
                var test = new FakeContent(p) {ContentLink = owner.ContentLink};
                softLinks.AddRange(contentSoftLinkIndexer.GetLinks(test));
            }
            return softLinks;
        }

        private IEnumerable<SoftLink> IndexContentReferenceProperties(IEnumerable<T> propertyValue, IContent owner)
        {
            var contentReferences = GetContentReferences(propertyValue);
            var contentReferenceListIndexer = FindContentReferenceIndexer();
            var result = contentReferenceListIndexer.ResolveReferences(contentReferences, owner);
            return result;
        }

        private IPropertySoftLinkIndexer<IList<ContentReference>> FindContentReferenceIndexer()
        {
            var propertySoftLinkIndexers = ServiceLocator.Current.GetAllInstances<IPropertySoftLinkIndexer>();

            foreach (var propertySoftLinkIndexer in propertySoftLinkIndexers)
            {
                var renderInterfaces = propertySoftLinkIndexer.GetType().GetInterfaces().Where(t => t.Name.Equals(typeof(IPropertySoftLinkIndexer<>).Name));
                foreach (var renderInterface in renderInterfaces)
                {
                    var genericArgument = renderInterface.GetGenericArguments().SingleOrDefault();
                    if (genericArgument == typeof(IList<ContentReference>))
                    {
                        return (IPropertySoftLinkIndexer<IList<ContentReference>>) propertySoftLinkIndexer;
                    }
                }
            }
            throw new ArgumentException("Cannot find implementation for IList<ContentReference> indexer");
        }

        private static List<ContentReference> GetContentReferences(IEnumerable<T> propertyValue)
        {
            var contentReferences = new List<ContentReference>();
            foreach (var item in propertyValue)
            {
                foreach (var propertyInfo in item.GetType().GetProperties())
                {
                    if (!propertyInfo.PropertyType.IsAssignableFrom(typeof(ContentReference)))
                    {
                        continue;
                    }
                    var value = propertyInfo.GetValue(item) as ContentReference;
                    if (value == null)
                    {
                        continue;
                    }
                    contentReferences.Add(value);
                }
            }
            return contentReferences;
        }


        private class FakeContent : BasicContent
        {
            private readonly object _instance;

            public FakeContent(object instance)
            {
                _instance = instance;
            }

            public override PropertyDataCollection Property
            {
                get
                {
                    var properties = _instance.GetType().GetProperties();

                    var propertyDataCollection = new PropertyDataCollection();

                    foreach (var propertyInfo in properties)
                    {
                        var rawProperty = new FakePropertyData(propertyInfo.PropertyType)
                        {
                            Value = propertyInfo.GetValue(_instance),
                            IsDynamicProperty = false,
                            Name = propertyInfo.Name
                        };
                        propertyDataCollection.Add(rawProperty);
                    }

                    return propertyDataCollection;
                }
            }
        }

        private class FakePropertyData : PropertyData
        {
            public FakePropertyData(Type propertyValueType)
            {
                PropertyValueType = propertyValueType;
            }

            public override object Value { get; set; }

            public override PropertyDataType Type => PropertyDataType.Json;

            public override Type PropertyValueType { get; }

            protected override void SetDefaultValue()
            {
            }

            public override PropertyData ParseToObject(string value)
            {
                throw new NotImplementedException();
            }

            public override void ParseToSelf(string value)
            {
                throw new NotImplementedException();
            }

            public override bool IsNull => this.Value == null;
        }
    }
}