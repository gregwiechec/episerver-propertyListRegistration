using System.Collections.Generic;
using EPiServer.Cms.Shell.UI.ObjectEditing.EditorDescriptors;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using PropertyListRegistration.demo.Models.Blocks;

namespace PropertyListRegistration.demo.Models.Pages
{
    [ContentType(GUID = "78A2FA19-D142-4FEA-9022-8D66A7D1C641")]
    public class ArticlePage: PageData
    {
        public virtual ContentReference Link { get; set; }

        public virtual IList<ContentReference> ContentReferences { get; set; }

        [EditorDescriptor(EditorDescriptorType = typeof(CollectionEditorDescriptor<Contact>))]
        public virtual IList<Contact> Contacts { get; set; }
    }
}