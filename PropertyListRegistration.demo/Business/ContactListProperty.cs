using EPiServer.PlugIn;
using PropertyListRegistration.demo.Models.Blocks;

namespace PropertyListRegistration.demo.Business
{
    [PropertyDefinitionTypePlugIn]
    public class ContactListProperty : PropertyListBase<Contact>
    {
    }
}