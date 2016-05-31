using NUnit.Framework;
using TableDescriptor.Tests.Models;

namespace TableDescriptor.Tests
{
    [TestFixture]
    public class DescriptorTests
    {
        [Test]
        public void Simple_user_has_an_identity()
        {
            var descriptor = SimpleDescriptor.Create<SimpleUser>();

            Assert.IsNotNull(descriptor);
            AssertIdentityIsPresent(descriptor);

            Assert.AreEqual("Id", descriptor.Identity.Property.Name);
            Assert.AreEqual(typeof(int), descriptor.Identity.Property.Type);

            Assert.AreEqual(1, descriptor.All.Count, "All");
            Assert.AreEqual(0, descriptor.Insertable.Count, "Insertable");
        }

        [Test]
        public void Simple_inherited_user_has_an_identity()
        {
            var descriptor = SimpleDescriptor.Create<SimpleUserInherited>();

            Assert.IsNotNull(descriptor);
            AssertIdentityIsPresent(descriptor);

            Assert.AreEqual("Id", descriptor.Identity.Property.Name);
            Assert.AreEqual(typeof(int), descriptor.Identity.Property.Type);

            Assert.AreEqual(1, descriptor.All.Count, "All");
            Assert.AreEqual(0, descriptor.Insertable.Count, "Insertable");
        }
        
        [Test]
        public void Through_tables_are_given_two_assigned_keys_rather_than_an_identity()
        {
            var descriptor = SimpleDescriptor.Create<ThroughTable>();
            Assert.IsNotNull(descriptor);
            Assert.IsNull(descriptor.Identity);
            
            Assert.AreEqual(2, descriptor.All.Count, "All");
            Assert.AreEqual(2, descriptor.Insertable.Count, "Insertable");
            Assert.AreEqual(0, descriptor.Computed.Count, "Computed");
            Assert.AreEqual(2, descriptor.Keys.Count, "Keys");
            Assert.AreEqual(2, descriptor.Assigned.Count, "Assigned");
        }

        [Test]
        public void Transients_are_ignored()
        {
            var descriptor = SimpleDescriptor.Create<HasTransient>();
            Assert.IsNotNull(descriptor);
            AssertIdentityIsPresent(descriptor);

            Assert.AreEqual(2, descriptor.All.Count, "All");                    // Id, Email, (EmailTrimmed is transient)
            Assert.AreEqual(1, descriptor.Insertable.Count, "Insertable");      // Email (Id is identity)
        }

        [Test]
        public void Getting_and_setting_works_against_an_accessor()
        {
            var user = new SimpleUser();
            var descriptor = SimpleDescriptor.Create<SimpleUser>();
            var accessor = descriptor[0].Property;
            accessor.Set(user, 5);
            var value = accessor.Get(user);
            Assert.AreEqual(5, value);
        }

        [Test]
        public void Identity_attribute_overrides_multiple_keys()
        {
            var descriptor = SimpleDescriptor.Create<MultipleKeysButIdentityForced>();
            Assert.IsNotNull(descriptor.Identity);
            Assert.AreEqual(2, descriptor.Keys.Count);
        }
        
        private static void AssertIdentityIsPresent(Descriptor descriptor)
        {
            Assert.IsNotNull(descriptor.Identity, "Identity is not present");
            Assert.AreEqual(1, descriptor.Computed.Count, "Computed");
            Assert.AreEqual(1, descriptor.Keys.Count, "Keys");
        }
    }
}
