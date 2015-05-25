using CallWall.Web.Domain;
using NUnit.Framework;

namespace CallWall.Web.Core.Tests
{
    [TestFixture]
    public class ContactEmailHandleFixture
    {
        [TestCase("bill@googlemail.com", "bill@gmail.com")]
        [TestCase("bill@GoogleMail.com", "bill@gmail.com")]
        public void Given_a_GoogleMail_address_When_normalized_Then_the_domain_should_be_returned_as_gmail(string input, string expected)
        {
            var sut = new ContactEmailAddress(input, "foo");
            var actual = sut.NormalizedHandle();

            CollectionAssert.AreEqual(new[]{expected}, actual);
        }

        [TestCase("bill+spam@gmail.com", "bill@gmail.com")]
        [TestCase("bill+spam@GoogleMail.com", "bill@gmail.com")]
        public void Given_a_GoogleMail_address_When_normalized_Then_the_plusSign_and_alias_suffix_is_removed(string input, string expected)
        {
            var sut = new ContactEmailAddress(input, "foo");
            var actual = sut.NormalizedHandle();

            CollectionAssert.AreEqual(new[] { expected }, actual);
        }
        
        [TestCase("b.ill@gmail.com", "bill@gmail.com")]
        [TestCase("b.i.l.l@GoogleMail.com", "bill@gmail.com")]
        public void Given_a_GoogleMail_address_When_normalized_Then_the_periods_in_alias_are_removed(string input, string expected)
        {
            var sut = new ContactEmailAddress(input, "foo");
            var actual = sut.NormalizedHandle();

            CollectionAssert.AreEqual(new[] { expected }, actual);
        }
    }
}