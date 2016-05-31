using System.Text.RegularExpressions;
using Hammock.Specifications;

namespace Hammock.Validation
{
    public class ValidEmailSpecification : HammockSpecification<string>
    {
        // Accepts names, i.e. John Smith <john@johnsmith.com>
        private static readonly Regex _names =
            new Regex(
                @"\w*<([-_a-z0-9'+*$^&%=~!?{}]+(?:\.[-_a-z0-9'+*$^&%=~!?{}]+)*@(?:(?![-.])[-a-z0-9.]+(?<![-.])\.[a-z]{2,6}|\d{1,3}(?:\.\d{1,3}){3})(?::\d+)?)>",
#if !SL4 && !MonoTouch
                RegexOptions.Compiled |
#endif
                 RegexOptions.IgnoreCase
                );

        // Just an email address
        private static readonly Regex _explicit =
            new Regex(
                @"^[-_a-z0-9'+*$^&%=~!?{}]+(?:\.[-_a-z0-9'+*$^&%=~!?{}]+)*@(?:(?![-.])[-a-z0-9.]+(?<![-.])\.[a-z]{2,6}|\d{1,3}(?:\.\d{1,3}){3})(?::\d+)?$",
#if !SL4 && !MonoTouch
                RegexOptions.Compiled |
#endif
                 RegexOptions.IgnoreCase
                );

        public override bool IsSatisfiedBy(string instance)
        {
            var result = _explicit.IsMatch(instance) || _names.IsMatch(instance);

            return result;
        }
    }
}
