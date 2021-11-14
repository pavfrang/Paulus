using System;
using System.Diagnostics.CodeAnalysis;

[assembly: CLSCompliant(true)]
namespace Paulus.Algebra
{
    public static class Constants
    {
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PI")]
        public const double PI = 3.14159265358979;
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PIMULT2")]
        public const double PIMULT2 = 2.0 * PI;
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PIDIV")]
        public const double PIDIV2 = PI / 2.0;
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TORAD")]
        public const double TORAD = PI / 180.0;
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TODEG")]
        public const double TODEG = 180.0 / PI;
    }
}
