using System;

using System.IO;

using System.Net.Http;

using System.Net.Http.Headers;

using System.Text;

using System.Text.Json;

using System.Threading.Tasks;

using System.Collections.Generic;



namespace ReactCRM.Services

{

    /// <summary>

    /// Servicio para integración con Dropbox File Requests API

    /// </summary>

    public class DropboxService

    {

        private static DropboxService _instance;

        private static readonly object _lock = new object();



        private readonly HttpClient _httpClient;

        private string _accessToken;



        // ============================================

        // CONFIGURACIÓN - EDITAR ESTOS VALORES

        // ============================================

        private const string APP_KEY = "nza5q5q034l2wdp";

        private const string APP_SECRET = "h9tww3an4ipjb7x";

        // Token de acceso (obtener desde Dropbox App Console)

        private const string DEFAULT_ACCESS_TOKEN = "sl.u.AGFceO_FZVs6ZWq1ddreRz7MmO1mhwlhKrrtxkmHqMLGsutwX2ZTwRO9iNsT6pTgKlR5u_rih-UzUz9bIkUS31-vZG1eI9Y0_CJBB5DoexqloY9pya4yZobYMdIa002isfXaHY7GT6BFBI8aw7aA5Ciytd9aGBwZ7II8PRpurSv6dLj1vVzI4qcJJalWS7x_79AxE0MgUjaCggO0DyK6YAuSdlLw8FE32Tch-Wgjx_snpvZNVr1JFAIj1t42MpAgVQ8CbXDaob-mjI-RahzH4Pnp3ns8uHYbNaCQ5s8VTg4wzJB2A_ht2pyzVo6mv_Cl5KDgEZCpxHGUxEctWxboJJkhvd0sKyj4lVO0yNAXc1K8-Pb6YGNzi7YPepLGOItZJzkaBDKA7fPiPe6AdRdatUcn5lkQbaRExhKe7vMRVOmJhfiIlDR2_aivth0QxaRbiObWr9dVFUiGXbnksxPSJo3K9mf8Oc_j_FdGrvPs1K1m-oAZncAz0vv5UOOgVSmHNbUvAFIeFMclgxAcAOeUXC-un4b9MyFdtShmxtThzMb-UIDpDnZmXrx0eT-THqxC9uFobKh1Yl9FV7PvOhrZdJsgtop7GtwavoiSg7Hea1FrfrTzNJ4F_tvlcYam9FgUJ4CqSboFtSQWzP705GTZ2GfMj-RHOy3G8TumJd21cMpaTeS5A_na-t7mE91f4pgxTH-ZCmxghhSlqQuY4IVYBNIiAjQCbqQh2V8kJI3RJK006WPlibX3DVsp3H6lFtIcwVIcwWUSAhK-awTwaGrRJmraX6CA1LjJ9X2uczhZTCE54wQUnrtBtB48gK0A2QP9thSXYeADnpXyQdrMhHAmzZfxu80kAqY-JcT7y6UX6H2gssA_dtXwwRx9PhHFS0gHEGCZtvmQ5K5HmA_i9OQTj1n8gUeT2XrsUiXVZ8eLqTwHf2TomWpk7xLScacNAtfmQe_9VbEoKeVFCPYvI2lrUwHeU3yFBXrYgPqOxvgLHj7MxMq4QJ-vsBWCA34s0Wllxm-S0m31f6Of4j1EDnlmYOegynjjHW-ccnbZdXbfoCiOs1sVISsR4tavhzUtxR_UzgiJUBO0UJGq-ROk2G8ISB0Zeig-llVeAigNk9Anhg3eIW8crGm_UudSMkt4Xbm8sK5ZccorRBTKxEZlSOtptnsHSCPURENiTr8o0_xg6wxmDhbmrfsHWnQO1RmhXowHWPx_8qptlrSfMkBwvLxmOhy0J4yytDc2EWFw9B13qu953iEn-RFUXFiqCmMN7sTlCHjJrzGBlXSyi7yw_nhsZA4T-nAcNgXqrQgxiHFkTAccADwuJ7qH7m4bclINCdpsVd95LqQ0UO9EAJYsvkOgApeOaRv_kdhlS3786IjaPDjrCS-Tstu4FF772mAUk9lw6Sbr8cko0Am-kghj8bBVAvm0FQO--dhvBVgN7u5X8F5N7g";



        // Carpeta base en Dropbox donde se guardarán los archivos

        private const string DROPBOX_BASE_FOLDER = "/CRM_Client_Uploads";



        // Carpeta local donde se descargarán los archivos

        public static string LocalDownloadFolder = Path.Combine(

            AppDomain.CurrentDomain.BaseDirectory, "ClientFiles");



        public static DropboxService Instance

        {

            get

            {

                if (_instance == null)

                {

                    lock (_lock)

                    {

                        _instance = _instance ?? new DropboxService();

                    }

                }

                return _instance;

            }

        }



        private DropboxService()

        {

            _httpClient = new HttpClient();

            _accessToken = DEFAULT_ACCESS_TOKEN;



            // Crear carpeta local si no existe

            if (!Directory.Exists(LocalDownloadFolder))

            {

                Directory.CreateDirectory(LocalDownloadFolder);

            }

        }



        /// <summary>

        /// Configura el token de acceso

        /// </summary>

        public void SetAccessToken(string token)

        {

            _accessToken = token;

        }



        /// <summary>

        /// Crea un File Request en Dropbox para un cliente

        /// </summary>

        public async Task<DropboxFileRequest> CreateFileRequest(int clientId, string clientName)

        {

            // Validar que el token esté configurado

            if (string.IsNullOrEmpty(_accessToken) || _accessToken == "YOUR_ACCESS_TOKEN")

            {

                throw new Exception(

                    "Dropbox access token not configured.\n\n" +

                    "Please configure your Dropbox access token in Services/DropboxService.cs\n" +

                    "See DROPBOX_UPLOAD_README.md for setup instructions.");

            }



            var url = "https://api.dropboxapi.com/2/file_requests/create";



            // Crear carpeta única para el cliente

            var folderPath = $"{DROPBOX_BASE_FOLDER}/{clientId}_{SanitizeFolderName(clientName)}";



            var requestBody = new

            {

                title = $"Upload files for {clientName}",

                destination = folderPath,

                open = true

            };



            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");



            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Authorization =

                new AuthenticationHeaderValue("Bearer", _accessToken);



            var response = await _httpClient.PostAsync(url, content);

            var responseJson = await response.Content.ReadAsStringAsync();



            if (!response.IsSuccessStatusCode)

            {

                // Parse error for better messaging

                string errorMessage = ParseDropboxError(responseJson);

                throw new Exception(errorMessage);

            }



            var result = JsonSerializer.Deserialize<DropboxFileRequestResponse>(responseJson,

                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });



            return new DropboxFileRequest

            {

                Id = result.Id,

                Url = result.Url,

                Title = result.Title,

                Destination = folderPath,

                Created = result.Created,

                IsOpen = result.IsOpen

            };

        }



        /// <summary>

        /// Parsea errores de Dropbox API para mensajes más claros

        /// </summary>

        private string ParseDropboxError(string responseJson)

        {

            try

            {

                using (JsonDocument doc = JsonDocument.Parse(responseJson))

                {

                    var root = doc.RootElement;



                    // Check for missing scope error

                    if (root.TryGetProperty("error_summary", out JsonElement errorSummary))

                    {

                        string summary = errorSummary.GetString();



                        if (summary.Contains("missing_scope") && summary.Contains("file_requests"))

                        {

                            return "Dropbox API Error: Missing required permission 'file_requests.write'\n\n" +

                                   "Your Dropbox access token doesn't have the required permissions.\n\n" +

                                   "To fix this:\n" +

                                   "1. Go to https://www.dropbox.com/developers/apps\n" +

                                   "2. Select your app\n" +

                                   "3. Go to 'Permissions' tab\n" +

                                   "4. Enable these permissions:\n" +

                                   "   - files.metadata.read\n" +

                                   "   - files.metadata.write\n" +

                                   "   - files.content.read\n" +

                                   "   - files.content.write\n" +

                                   "   - file_requests.read\n" +

                                   "   - file_requests.write\n" +

                                   "5. Click 'Submit'\n" +

                                   "6. Go to 'Settings' tab\n" +

                                   "7. Generate a NEW access token\n" +

                                   "8. Update the token in Services/DropboxService.cs\n\n" +

                                   "See DROPBOX_UPLOAD_README.md for detailed instructions.";

                        }



                        return $"Dropbox API Error: {summary}\n\nSee DROPBOX_UPLOAD_README.md for configuration help.";

                    }

                }

            }

            catch

            {

                // If parsing fails, return raw error

            }



            return $"Dropbox API Error: {responseJson}";

        }



        /// <summary>

        /// Obtiene un File Request existente por ID

        /// </summary>

        public async Task<DropboxFileRequest> GetFileRequest(string requestId)

        {

            var url = "https://api.dropboxapi.com/2/file_requests/get";



            var requestBody = new { id = requestId };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");



            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Authorization =

                new AuthenticationHeaderValue("Bearer", _accessToken);



            var response = await _httpClient.PostAsync(url, content);

            var responseJson = await response.Content.ReadAsStringAsync();



            if (!response.IsSuccessStatusCode)

            {

                return null;

            }



            var result = JsonSerializer.Deserialize<DropboxFileRequestResponse>(responseJson,

                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });



            return new DropboxFileRequest

            {

                Id = result.Id,

                Url = result.Url,

                Title = result.Title,

                Destination = result.Destination,

                Created = result.Created,

                IsOpen = result.IsOpen,

                FileCount = result.FileCount

            };

        }



        /// <summary>

        /// Cierra (desactiva) un File Request

        /// </summary>

        public async Task<bool> CloseFileRequest(string requestId)

        {

            var url = "https://api.dropboxapi.com/2/file_requests/update";



            var requestBody = new { id = requestId, open = false };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");



            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Authorization =

                new AuthenticationHeaderValue("Bearer", _accessToken);



            var response = await _httpClient.PostAsync(url, content);

            return response.IsSuccessStatusCode;

        }



        /// <summary>

        /// Lista archivos en una carpeta de Dropbox

        /// </summary>

        public async Task<List<DropboxFile>> ListFiles(string folderPath)

        {

            var url = "https://api.dropboxapi.com/2/files/list_folder";



            var requestBody = new { path = folderPath, recursive = false };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");



            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Authorization =

                new AuthenticationHeaderValue("Bearer", _accessToken);



            var response = await _httpClient.PostAsync(url, content);

            var responseJson = await response.Content.ReadAsStringAsync();



            if (!response.IsSuccessStatusCode)

            {

                // Parse and throw specific error

                string errorMessage = ParseDropboxError(responseJson);

                throw new Exception($"Failed to list files in Dropbox folder:\n{errorMessage}");

            }



            var result = JsonSerializer.Deserialize<DropboxListFolderResponse>(responseJson,

                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });



            var files = new List<DropboxFile>();

            foreach (var entry in result.Entries ?? new List<DropboxEntry>())

            {

                if (entry.Tag == "file")

                {

                    files.Add(new DropboxFile

                    {

                        Id = entry.Id,

                        Name = entry.Name,

                        PathDisplay = entry.PathDisplay,

                        Size = entry.Size,

                        ServerModified = entry.ServerModified

                    });

                }

            }



            return files;
        }

        /// <summary>

        /// Busca archivos recursivamente en una carpeta y subcarpetas

        /// </summary>

        public async Task<List<DropboxFile>> ListFilesRecursive(string folderPath)

        {

            var url = "https://api.dropboxapi.com/2/files/list_folder";



            var requestBody = new { path = folderPath, recursive = true };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");



            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Authorization =

                new AuthenticationHeaderValue("Bearer", _accessToken);



            var response = await _httpClient.PostAsync(url, content);

            var responseJson = await response.Content.ReadAsStringAsync();



            if (!response.IsSuccessStatusCode)

            {

                string errorMessage = ParseDropboxError(responseJson);

                throw new Exception($"Failed to list files recursively:\n{errorMessage}");

            }



            var result = JsonSerializer.Deserialize<DropboxListFolderResponse>(responseJson,

                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });



            var files = new List<DropboxFile>();

            foreach (var entry in result.Entries ?? new List<DropboxEntry>())

            {

                if (entry.Tag == "file")

                {

                    files.Add(new DropboxFile

                    {

                        Id = entry.Id,

                        Name = entry.Name,

                        PathDisplay = entry.PathDisplay,

                        Size = entry.Size,

                        ServerModified = entry.ServerModified

                    });

                }

            }



            return files;

        }



        /// <summary>

        /// Descarga un archivo de Dropbox

        /// </summary>

        public async Task<string> DownloadFile(string dropboxPath, string localFolder)

        {

            var url = "https://content.dropboxapi.com/2/files/download";



            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);



            var apiArg = JsonSerializer.Serialize(new { path = dropboxPath });

            request.Headers.Add("Dropbox-API-Arg", apiArg);



            var response = await _httpClient.SendAsync(request);



            if (!response.IsSuccessStatusCode)

            {

                throw new Exception("Failed to download file from Dropbox");

            }



            // Obtener nombre del archivo del header

            var fileName = Path.GetFileName(dropboxPath);



            // Crear carpeta local si no existe

            if (!Directory.Exists(localFolder))

            {

                Directory.CreateDirectory(localFolder);

            }



            var localPath = Path.Combine(localFolder, fileName);



            // Evitar sobrescribir archivos existentes

            int counter = 1;

            var baseName = Path.GetFileNameWithoutExtension(fileName);

            var extension = Path.GetExtension(fileName);

            while (File.Exists(localPath))

            {

                localPath = Path.Combine(localFolder, $"{baseName}_{counter}{extension}");

                counter++;

            }



            using (var fileStream = File.Create(localPath))

            {

                await response.Content.CopyToAsync(fileStream);

            }



            return localPath;

        }



        /// <summary>

        /// Sincroniza archivos de un cliente (descarga nuevos archivos de Dropbox)

        /// </summary>

        public async Task<List<string>> SyncClientFiles(int clientId, string clientName, string dropboxFolder)

        {

            var downloadedFiles = new List<string>();



            // Carpeta local del cliente - usar la misma estructura que ClientFilesForm

            // clientfiles/{clientId}-{clientName}/

            var clientFolder = Path.Combine(LocalDownloadFolder,
                Path.GetFileName(FilePathService.GetOrMigrateClientFolderPath(clientId, clientName)));



            // Crear carpeta si no existe

            if (!Directory.Exists(clientFolder))

            {

                Directory.CreateDirectory(clientFolder);

            }



            // Listar archivos en Dropbox

            var dropboxFiles = await ListFiles(dropboxFolder);



            foreach (var file in dropboxFiles)

            {

                // Generar nombre único con timestamp para evitar conflictos (igual que ClientFilesForm)

                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                string uniqueFileName = $"{timestamp}_{file.Name}";

                var localPath = Path.Combine(clientFolder, uniqueFileName);



                // Verificar si el archivo original (sin timestamp) ya existe

                bool alreadyExists = false;

                if (Directory.Exists(clientFolder))

                {

                    var existingFiles = Directory.GetFiles(clientFolder, $"*{file.Name}");

                    alreadyExists = existingFiles.Length > 0;

                }



                // Solo descargar si no existe localmente

                if (!alreadyExists)

                {

                    try

                    {

                        // Descargar archivo

                        var downloaded = await DownloadFileWithName(file.PathDisplay, clientFolder, uniqueFileName);

                        downloadedFiles.Add(downloaded);

                    }

                    catch (Exception ex)

                    {

                        System.Diagnostics.Debug.WriteLine($"Error downloading {file.Name}: {ex.Message}");

                    }

                }

            }



            return downloadedFiles;

        }



        /// <summary>

        /// Descarga un archivo de Dropbox con un nombre específico

        /// </summary>

        private async Task<string> DownloadFileWithName(string dropboxPath, string localFolder, string fileName)

        {

            var url = "https://content.dropboxapi.com/2/files/download";



            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);



            var apiArg = JsonSerializer.Serialize(new { path = dropboxPath });

            request.Headers.Add("Dropbox-API-Arg", apiArg);



            var response = await _httpClient.SendAsync(request);



            if (!response.IsSuccessStatusCode)

            {

                throw new Exception("Failed to download file from Dropbox");

            }



            // Crear carpeta local si no existe

            if (!Directory.Exists(localFolder))

            {

                Directory.CreateDirectory(localFolder);

            }



            var localPath = Path.Combine(localFolder, fileName);



            using (var fileStream = File.Create(localPath))

            {

                await response.Content.CopyToAsync(fileStream);

            }



            return localPath;

        }



        private string SanitizeFolderName(string name)

        {

            // Remover caracteres inválidos para nombres de carpeta

            var invalid = Path.GetInvalidFileNameChars();

            var sanitized = new StringBuilder();



            foreach (char c in name)

            {

                if (Array.IndexOf(invalid, c) < 0)

                    sanitized.Append(c);

                else

                    sanitized.Append('_');

            }



            return sanitized.ToString().Trim();

        }

    }



    #region DTOs



    public class DropboxFileRequest

    {

        public string Id { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Destination { get; set; }

        public DateTime Created { get; set; }

        public bool IsOpen { get; set; }

        public int FileCount { get; set; }

    }



    public class DropboxFile

    {

        public string Id { get; set; }

        public string Name { get; set; }

        public string PathDisplay { get; set; }

        public long Size { get; set; }

        public DateTime ServerModified { get; set; }

    }



    // Response models for Dropbox API

    internal class DropboxFileRequestResponse

    {

        public string Id { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Destination { get; set; }  // Dropbox returns this as a string path

        public DateTime Created { get; set; }

        public bool IsOpen { get; set; }

        public int FileCount { get; set; }

    }



    internal class DropboxListFolderResponse

    {

        public List<DropboxEntry> Entries { get; set; }

        public bool HasMore { get; set; }

        public string Cursor { get; set; }

    }



    internal class DropboxEntry

    {

        public string Tag { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string PathDisplay { get; set; }

        public long Size { get; set; }

        public DateTime ServerModified { get; set; }

    }



    #endregion

}