using System;
using System.Collections.Generic;
using RestSharp;
using System.Linq;
using System.Threading.Tasks;
using RestAdaptersCommon;
using Newtonsoft.Json;
using KLogMonitor;
using System.Reflection;
using AdapterClients.IngestTransformation.Models;
using ApiObjects;

namespace AdapterClients.IngestTransformation
{
	public class IngestTransformationAdapterClient : BaseAdapterClient<IngestProfile>
	{
		private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
		private readonly IngestProfile _IngestProfile;
		private readonly RestClient _Client;

		public IngestTransformationAdapterClient(IngestProfile profile) : base(profile)
		{
			_IngestProfile = profile;
			_Client = new RestClient(profile.TransformationAdapterUrl);
		}

		/// <summary>
		/// Set the configuration for the adapter
		/// </summary>
		public override eAdapterStatus SetConfiguration()
		{
			return Task.Run(() => SetConfigurationAsync()).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Set the configuration for the adapter
		/// </summary>
		public async Task<eAdapterStatus> SetConfigurationAsync()
		{
			try
			{
				var settingsDict = _IngestProfile.Settings.ToDictionary(k => k.Key, v => v.Value);
				var requestPayload = new SetConfigurationRequest
				{
					Configuration = settingsDict,
					IngestProfileId = _IngestProfile.Id,
					GroupId = _IngestProfile.GroupId,
				};
				requestPayload.Signature = requestPayload.CalculateSignature(_IngestProfile.TransformationAdapterSharedSecret);


				var request = new RestRequest("SetConfiguration", Method.POST);
				request.AddJsonBody(requestPayload);

				var response = await _Client.MakeRequestAsync(request, _IngestProfile.GroupId);
				var responseObj = JsonConvert.DeserializeObject<BaseAdapterResponse>(response.Content);

				ValidateAdapterResponse(responseObj?.ResponseStatus);

				return (eAdapterStatus)responseObj.ResponseStatus.Code;

			}
			catch (Exception e)
			{
				_Logger.Error($"Error while SetConfiguration to ingest transformation adapter profileId:[{_IngestProfile.Id}], groupId:[{_IngestProfile.GroupId}]", e);
				throw;
			}
		}


		/// <summary>
		/// Sends transformation request to the ingest transformation adapater
		/// </summary>
		/// <param name="fileUrl">the source file url that the adaater should download from</param>
		/// <returns>The transformed xml string</returns>
		public string Transform(string fileUrl)
		{
			return Task.Run(() => TransformAsync(fileUrl)).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Sends transformation request to the ingest transformation adapater
		/// </summary>
		/// <param name="fileUrl">the source file url that the adaater should download from</param>
		/// <returns>The transformed xml string</returns>
		public async Task<string> TransformAsync(string fileUrl)
		{
			var requestPayload = new TransformationRequest
			{
				IngestProfileId = _IngestProfile.Id,
				GroupId = _IngestProfile.GroupId,
				FileUrl = fileUrl
			};
			requestPayload.Signature = requestPayload.CalculateSignature(_IngestProfile.TransformationAdapterSharedSecret);


			var request = new RestRequest("Transform", Method.POST);
			request.AddJsonBody(requestPayload);

			var response = await _Client.MakeRequestAsync(request, _IngestProfile.GroupId);
			var responseObj = JsonConvert.DeserializeObject<TransformationResponse>(response.Content);
			var transformedXmlTv = responseObj.XmlTv;
			
			ValidateAdapterResponse(responseObj?.ResponseStatus, () => {
				var retryResponse = Transform(fileUrl);
				transformedXmlTv = retryResponse;
			});

			return transformedXmlTv;
		}


	}




}
