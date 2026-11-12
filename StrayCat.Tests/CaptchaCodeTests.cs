using StrayCat.BLL;
using Xunit;

namespace StrayCat.Tests
{
    public class CaptchaCodeTests
    {
        [Fact]
        public void Create_uses_four_characters_from_the_alphabet()
        {
            for (int n = 0; n < 40; n++)
            {
                string code = CaptchaCode.Create();
                Assert.Equal(CaptchaCode.Length, code.Length);
                Assert.True(CaptchaCode.IsAlphabet(code));
                for (int i = 0; i < code.Length; i++)
                {
                    Assert.True(CaptchaCode.Alphabet.IndexOf(code[i]) >= 0);
                }
            }
        }

        [Fact]
        public void Matches_is_case_sensitive_and_trims_the_typed_value()
        {
            Assert.True(CaptchaCode.Matches("Ab1Z", " Ab1Z "));
            Assert.False(CaptchaCode.Matches("Ab1Z", "ab1z"));
            Assert.False(CaptchaCode.Matches("Ab1Z", "Ab1"));
            Assert.False(CaptchaCode.Matches(null, "Ab1Z"));
            Assert.False(CaptchaCode.Matches("", "Ab1Z"));
            Assert.False(CaptchaCode.IsAlphabet("ab_1"));
        }
    }
}
