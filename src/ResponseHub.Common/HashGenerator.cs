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

		#region SHA256

		/// <summary>
		/// Generates a SHA256 hash based on the input string.
		/// </summary>
		/// <param name="inputString">The input string to hash using the SHA256 hash algorithm</param>
		/// <param name="iterations">The number of iterations to run on the hash.</param>
		/// <returns>The byte array containing the hashed data.</returns>
		public static byte[] GetSha256Hash(string inputString, int hashIterations)
		{
			return GetHash(SHA256.Create(), inputString, hashIterations);
		}

		/// <summary>
		/// Generates a string representation SHA256 hash based on the input string.
		/// </summary>
		/// <param name="inputString">The input string to hash using the SHA256 hash algorithm</param>
		/// <param name="iterations">The number of iterations to run on the hash.</param>
		/// <returns>The string representation of the hashed data.</returns>
		public static string GetSha256HashString(string inputString, int iterations)
		{
			return GetHashString(SHA256.Create(), inputString, iterations);
		}

		#endregion

		#region SHA1

		/// <summary>
		/// Generates a SHA1 hash based on the input string.
		/// </summary>
		/// <param name="inputString">The input string to hash using the SHA1 hash algorithm</param>
		/// <param name="iterations">The number of iterations to run on the hash.</param>
		/// <returns>The byte array containing the hashed data.</returns>
		public static byte[] GetSha1Hash(string inputString, int hashIterations)
		{
			return GetHash(SHA1.Create(), inputString, hashIterations);
		}

		/// <summary>
		/// Generates a string representation SHA1 hash based on the input string.
		/// </summary>
		/// <param name="inputString">The input string to hash using the SHA1 hash algorithm</param>
		/// <param name="iterations">The number of iterations to run on the hash.</param>
		/// <returns>The string representation of the hashed data.</returns>
		public static string GetSha1HashString(string inputString, int iterations)
		{
			return GetHashString(SHA1.Create(), inputString, iterations);
		}

		#endregion

		#region Base generators

		/// <summary>
		/// Generates a string representation hash based on the specified hash algorithm and the input string.
		/// </summary>
		/// <param name="inputString">The input string to hash using the SHA256 hash algorithm</param>
		/// <param name="iterations">The number of iterations to run on the hash.</param>
		/// <returns>The string representation of the hashed data.</returns>
		public static string GetHashString(HashAlgorithm algorithm, string inputString, int iterations)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(algorithm, inputString, iterations))
			{
				sb.Append(b.ToString("X2"));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Generates a hash based on the specified hash algorithm and the input string.
		/// </summary>
		/// <param name="inputString">The input string to hash using the specified hash algorithm</param>
		/// <param name="iterations">The number of iterations to run on the hash.</param>
		/// <returns>The byte array containing the hashed data.</returns>
		public static byte[] GetHash(HashAlgorithm algorithm, string inputString, int hashIterations)
		{

			// Compute the has bytes
			byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));

			for (int i = 0; i < hashIterations; i++)
			{
				hashBytes = algorithm.ComputeHash(hashBytes);
			}

			// return the hashed bytes.
			return hashBytes;
		}

		#endregion

	}
}
