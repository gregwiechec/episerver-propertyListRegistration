using System.Collections.Generic;
using EPiServer.Core;

namespace PropertyListRegistration.demo.Models.Blocks
{
    public class Contact
    {
        public string Name { get; set; }
        public ContentReference Link { get; set; }
        public PageReference PageLink { get; set; }
        public IList<ContentReference> Assistants { get; set; }
    }
}