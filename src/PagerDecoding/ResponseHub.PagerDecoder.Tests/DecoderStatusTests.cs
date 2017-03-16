using Enivate.ResponseHub.DataAccess.MongoDB;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices;
using Enivate.ResponseHub.PagerDecoder.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Enivate.ResponseHub.PagerDecoder.Tests
{

	public class DecoderStatusTests : IClassFixture<UnityCollectionFixture>
	{

		//[Trait("Category", "Decoder status tests")]
		//[Fact(DisplayName = "Can send invalid message warning email.")]
		public async Task CanSendInvalidMessageWarningEmail()
		{

			DecoderStatusRepository repo = new DecoderStatusRepository();

			// Clear the existing decoder status
			await repo.ResetDecoderStatus();

			// Add some invalid messages
			for (int i = 0; i < 11; i++)
			{
				await repo.AddInvalidMessage(DateTime.UtcNow.AddMinutes(i * -1), "*U*U*U*??)");
			}

			DecoderStatusService decoderService = new DecoderStatusService();
			await decoderService.CheckInvalidMessages();

		}

	}
}
