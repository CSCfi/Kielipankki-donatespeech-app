using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Text;
using System.Threading;

using Recorder.Models;

namespace Recorder.Services
{
    public class RecorderApi : IRecorderApi
    {
        private IAppConfiguration appConfiguration;

        public const string ThemeUrlPath = "theme";
        public const string InitUploadUrlPath = "init-upload";  // for POST request
        public const string ConfigurationUrlPath = "configuration";

        private static JsonSerializerSettings scheduleJsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        private static JsonSerializerSettings themeJsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        private static JsonSerializerSettings uploadDescriptionJsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        private HttpClient client;

        public RecorderApi(IAppConfiguration appConfiguration)
        {
            client = new HttpClient();
            this.appConfiguration = appConfiguration;
        }

        public async Task<UploadDescription> InitUploadAsync(Recording recording, RecordingMetadata metadata)
        {
            var uri = new Uri($"{appConfiguration.RecorderApiUrl}/{InitUploadUrlPath}");

            // The recordings are stored in the Documents folder. We only save the filename in the database
            // (because deleting and reinstalling the app may cause the full path to change), so we need to
            // construct the full pathname again:
            //var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string fileName = Path.Combine(documentsFolder, recording.FileName);

            var fileName = Path.GetFileName(recording.FileName);

            JObject payload = new JObject(
                new JProperty("filename", fileName),
                new JProperty("metadata", metadata.ToJsonObject())
            );

            try
            {
                string payloadString = payload.ToString();
                Debug.WriteLine($"JSON payload = '{payloadString}'");
                Debug.WriteLine($"JSON payload length = {payloadString.Length} characters");
                var content = new StringContent(payloadString, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage()
                {
                    RequestUri = uri,
                    Method = HttpMethod.Post,
                    Content = content
                };
                request.Headers.Add("x-api-key", appConfiguration.RecorderApiKey);

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UploadDescription>(responseContent, uploadDescriptionJsonSerializerSettings);
                }
                else
                {
                    Debug.WriteLine($"Post request to {uri.ToString()} failed, status: {response.StatusCode}, reason: {response.ReasonPhrase}");
                    throw new Exception("Failed to init upload");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"              ERROR {ex.Message}");
                throw new Exception("Failed to init upload", ex);
            }
        }

        public async Task<List<Theme>> GetAllThemesAsync()
        {
            var uri = new Uri($"{appConfiguration.RecorderApiUrl}/{ThemeUrlPath}");
            return await PerformGetRequest<List<Theme>>(uri, themeJsonSerializerSettings);
        }

        public async Task<Schedule> GetScheduleAsync(string scheduleId)
        {
            var uri = new Uri($"{appConfiguration.RecorderApiUrl}/{ConfigurationUrlPath}/{scheduleId}");
            return await PerformGetRequest<Schedule>(uri, scheduleJsonSerializerSettings);
        }

        public async Task<List<Schedule>> GetAllSchedulesAsync()
        {
            var uri = new Uri(string.Format("{0}/{1}", appConfiguration.RecorderApiUrl, ConfigurationUrlPath));
            return await PerformGetRequest<List<Schedule>>(uri, scheduleJsonSerializerSettings);
        }

        private HttpRequestMessage CreateGetRequest(Uri uri)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };
            request.Headers.Add("x-api-key", appConfiguration.RecorderApiKey);
            //request.Headers.Add("x-app-version", Constants.ScheduleVersion.ToString());
            return request;
        }

        private async Task<T> PerformGetRequest<T>(Uri uri, JsonSerializerSettings serializerSettings)
        {
            try
            {
                var request = CreateGetRequest(uri);
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    T data = JsonConvert.DeserializeObject<T>(content, serializerSettings);
                    return data;
                }
                else
                {
                    Debug.WriteLine($"Get request to {uri.ToString()} failed, status: {response.StatusCode}, reason: {response.ReasonPhrase}");
                    throw new Exception("Network request failed");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Get request to {uri.ToString()} failed, exception: {ex.Message}", uri.ToString());
                throw new Exception("Network request failed", ex);
            }
        }

        public async Task<bool> UploadRecordingAsync(string filePath, string url, string contentType)
        {
            int lastSlashPosition = filePath.LastIndexOf('/');
            string fileNamePart = filePath.Substring(lastSlashPosition + 1);

            Debug.WriteLine($"UploadRecordingAsync: about to start uploading file '{fileNamePart}' with a PUT request");
            bool success = false;
            try
            {
                StreamContent strm = new StreamContent(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                strm.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                Debug.WriteLine($"Stream content headers --> Content-Type: '{strm.Headers.ContentType}'");
                HttpResponseMessage responseMessage = await client.PutAsync(url, strm);
                Debug.WriteLine($"Upload response: status={responseMessage.StatusCode} description={responseMessage.ReasonPhrase}");
                success = responseMessage.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception uploading file, message = '{e.Message}'");
                success = false;
            }

            return success;
        }

        public async Task<bool> DeleteRecordingAsync(string recId)
        {
            Debug.WriteLine($"About to delete recording '{recId}'");

            var uri = new Uri($"{appConfiguration.RecorderApiUrl}/{recId}");
            var result = false;
            try
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = uri,
                    Method = HttpMethod.Delete,
                };
                request.Headers.Add("x-api-key", appConfiguration.RecorderApiKey);

                var response = await client.SendAsync(request);
                result = response.StatusCode == HttpStatusCode.OK;
                Debug.WriteLine($"DELETE {recId} - response = {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"              ERROR {ex.Message}");
            }

            return result;
        }
    }
}
