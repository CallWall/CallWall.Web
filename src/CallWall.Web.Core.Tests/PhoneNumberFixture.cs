using System.Linq;
using CallWall.Web.Domain;
using NUnit.Framework;

namespace CallWall.Web.Core.Tests
{
    [TestFixture]
    public class ContactPhoneNumberFixture
    {
        [TestCase("0212543824", "0212543824")]
        [TestCase("(021) 254 3824", "0212543824")]
        [TestCase("021-254 3824", "0212543824")]
        [TestCase("021-254-3824", "0212543824")]
        public void Given_a_local_phoneNumbers_Should_return_just_the_normalized_local_number(string input, string expected)
        {
            var sut = new ContactPhoneNumber(input, "test");
            Assert.AreEqual(expected, sut.NormalizedHandle().Single());
        }
        
        [TestCase("+64211234567",   "+64211234567", "0211234567")]
        [TestCase("+64 21 123 4567", "+64211234567", "0211234567")]
        [TestCase("+64-21-123-4567", "+64211234567", "0211234567")]
        [TestCase("+64 21 123 4567", "+64211234567", "0211234567")]

        [TestCase("0064211234567",    "+64211234567", "0211234567")]
        [TestCase("(00)64211234567",  "+64211234567", "0211234567")]
        [TestCase("(00) 64211234567", "+64211234567", "0211234567")]
        [TestCase("(0064) 211234567", "+64211234567", "0211234567")]
        [TestCase("0064 21 123 4567", "+64211234567", "0211234567")]
        [TestCase("0064-21-123-4567", "+64211234567", "0211234567")]
        [TestCase("0064 21 123 4567", "+64211234567", "0211234567")]
        public void Given_a_fully_qualified_international_phoneNumbers_Should_return_both_GSM_and_normailized_local_number(string input, string expectedInternational, string expectedLocal)
        {
            var sut = new ContactPhoneNumber(input, "test");
            var expected = new[] {expectedInternational, expectedLocal};
            CollectionAssert.AreEqual(expected, sut.NormalizedHandle());
        }
    }
}
