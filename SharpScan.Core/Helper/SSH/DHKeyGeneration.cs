//
// DHKeyGeneration.cs: Defines the different key generation methods.
//
// Author:
//	Pieter Philippaerts (Pieter@mentalis.org)
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
//

namespace Org.Mentalis.Security.Cryptography
{
    /// <summary>
    /// Defines the different Diffie-Hellman key generation methods.
    /// </summary>
    public enum DHKeyGeneration
    {
        /// <summary>
        /// [TODO] you first randomly select a prime Q of size 160 bits, then choose P randomly among numbers like
        /// Q*R+1 with R random. Then you go along with finding a generator G which has order exactly Q. The private
        /// key X is then a number modulo Q.
        /// [FIPS 186-2-Change1 -- http://csrc.nist.gov/publications/fips/]
        /// </summary>
        // see RFC2631 [http://www.faqs.org/rfcs/rfc2631.html]
        //DSA,
        /// <summary>
        /// Returns dynamically generated values for P and G. Unlike the Sophie Germain or DSA key generation methods,
        /// this method does not ensure that the selected prime offers an adequate security level.
        /// </summary>
        Random,

        /// <summary>
        /// Returns dynamically generated values for P and G. P is a Sophie Germain prime, which has some interesting
        /// security features when used with Diffie Hellman.
        /// </summary>
        //SophieGermain,
        /// <summary>
        /// Returns values for P and G that are hard coded in this library. Contrary to what your intuition may tell you,
        /// using these hard coded values is perfectly safe.
        /// The values of the P and G parameters are taken from 'The OAKLEY Key Determination Protocol' [RFC2412].
        /// This is the prefered key generation method, because it is very fast and very safe.
        /// Because this method uses fixed values for the P and G parameters, not all bit sizes are supported.
        /// The current implementation supports bit sizes of 768, 1024 and 1536.
        /// </summary>
        Static
    }
}