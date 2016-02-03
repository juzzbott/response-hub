using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common
{
	public class HashGenerator
	{

		/// <summary>
		/// Generates a SHA256 hash based on the input string.
		/// </summary>
		/// <param name="inputString">The input string to hash using the SHA256 hash algorithm</param>
		/// <param name="iterations">The number of iterations to run on the hash.</param>
		/// <returns>The byte array containing the hashed data.</returns>
		public static byte[] GetSha256Hash(string inputString, int hashIterations)
		{
			HashAlgorithm algorithm = SHA256.Create();   //or use SHA1.Create();

			// Compute the has bytes
			byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));

			for (int i = 0; i < hashIterations; i++)
			{
				hashBytes = algorithm.ComputeHash(hashBytes);
			}

			// return the hashed bytes.
			return hashBytes;
		}

		/// <summary>
		/// Generates a string representation SHA256 hash based on the input string.
		/// </summary>
		/// <param name="inputString">The input string to hash using the SHA256 hash algorithm</param>
		/// <param name="iterations">The number of iterations to run on the hash.</param>
		/// <returns>The string representation of the hashed data.</returns>
		public static string GetSha256HashString(string inputString, int iterations)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetSha256Hash(inputString, iterations))
			{
				sb.Append(b.ToString("X2"));
			}

			return sb.ToString();
		}
	

	}
}
