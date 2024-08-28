using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace AutoCADLoader.Utility
{
    public static class JSONFunctions
    {
        private const int _timeoutSeconds = 20;

        private static HttpClient _httpClient = new HttpClient();

        public static string? GetJsonFromServer(string url, int timeoutSeconds = _timeoutSeconds)
        {
            EventLogger.Log("Getting Json from server: " + url, EventLogEntryType.Information);

            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            Task<HttpResponseMessage> getResponse;
            try
            {
                getResponse = Task.Run(() => _httpClient.GetAsync(url, cts.Token));
                getResponse.Wait();
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                return null;
            }
            catch (AggregateException)
            {
                return null;
            }
            catch (Exception ex) when (
            ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

            if (getResponse != null)
            {

                HttpResponseMessage response = getResponse.Result;
                EventLogger.Log("Received response from server " + response.IsSuccessStatusCode, EventLogEntryType.Information);


                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        string json = response.Content.ReadAsStringAsync().Result;
                        return json;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public static string? GetJsonFromLocalFile(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    return null;
                }

                using (StreamReader r = new StreamReader(jsonFilePath))
                {
                    string json = r.ReadToEnd();
                    return json;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void UpdateJson(string sourceJsonFilepath, string destinationJsonFilepath, bool checkIfNewer = true)
        {
            if (string.IsNullOrWhiteSpace(sourceJsonFilepath) || string.IsNullOrWhiteSpace(destinationJsonFilepath))
                return;

            try
            {
                FileInfo sourceJson = new FileInfo(sourceJsonFilepath);
                if (!sourceJson.Exists)
                    return;

                FileInfo destinationJson = new FileInfo(destinationJsonFilepath);
                if (destinationJson.Exists)
                {
                    if (checkIfNewer && sourceJson.LastWriteTime < destinationJson.LastWriteTime)
                        return;
                }
                else
                {
                    Directory.CreateDirectory(destinationJson.DirectoryName);
                }

                sourceJson.CopyTo(destinationJson.FullName, true);
            }
            catch
            {
                // Prevents program crash but could add logging here
            }
        }

        public static bool SaveJsonToLocalFile(string json, string folderPath, string fileName)
        {
            string jsonAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    try
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    catch
                    {
                        return false;
                    }
                }

                using (StreamWriter file = File.CreateText(Path.Combine(folderPath, fileName)))
                {
                    file.Write(json);
                    return true;
                }
            }
            catch
            {
                // Prevents program crash but could add logging here
                return false;
            }
        }
    }
}
