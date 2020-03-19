using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataStore.ConfigOptions;
using DataStore.Contracts;
using DataStore.FileHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
// ReSharper disable UseObjectOrCollectionInitializer

namespace DataStore.Controllers
{
    //[Authorize]
    //[Route("api/[controller]")]
    [Route("api/")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITempFileHelper _tempFileHelper;
        private readonly IDataRepository _dataRepository;
        private readonly IFileTypeScanner _fileTypeScanner;
        private readonly IFileVirusScanner _fileVirusScanner;
        private readonly IFileHasher _fileHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NodeAppSettingsOptions _appSettingsOptions;

        private const int _maxUserInputLength = 500;

        public FilesController(
            ILogger logger,
            ITempFileHelper tempFileHelper,
            IDataRepository dataRepository, 
            IFileTypeScanner fileTypeScanner, 
            IFileVirusScanner fileVirusScanner, 
            IFileHasher fileHasher, 
            IHttpContextAccessor httpContextAccessor, 
            IOptions<NodeAppSettingsOptions> appSettingsOptions)
        {
            _logger = logger;
            _tempFileHelper = tempFileHelper;
            _dataRepository = dataRepository;
            _fileTypeScanner = fileTypeScanner;
            _fileVirusScanner = fileVirusScanner;
            _fileHasher = fileHasher;
            _httpContextAccessor = httpContextAccessor;
            _appSettingsOptions = appSettingsOptions.Value;
        }
        
        // GET api/values/5
        [HttpGet("Files/{id}")]
        public IActionResult Get(string id)
        {
            var outputStream = new MemoryStream();
            try
            {
                _logger.Information("Received FilesController.GET request {id}", id);

                if (IdIsNotValid(id))
                    return BadRequest("id not valid");

                var fileInfo = _dataRepository.GetBlob(id, outputStream);

                if (fileInfo == null)
                {
                    return NotFound();
                }

                outputStream.Position = 0;

                if(fileInfo.MimeType == "image/jpeg")
                    return File(outputStream, fileInfo.MimeType);

                return File(outputStream, "application/octet-stream", $"{fileInfo.Filename}");
            }
            catch
            {
                outputStream.Dispose();
                throw;
            }
        }

        //[Authorize(Policy = "AdminsOnly")]
        [HttpPost("Files/Upload")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            try
            {
                _logger.Information("Received Post request {fileName} length:{fileLength}kb user:{user}", file.FileName, file.Length, User.Identity.Name);
                long size = file.Length;

                if (file.Length <= 0) return BadRequest("file must not be empty");

                var rand = new Random();
                var tempFilePath = $"{_appSettingsOptions.TempFileDirectory}{rand.Next()}.tmp";
                using (var tempFileStream = _tempFileHelper.Create(tempFilePath))
                {
                    await file.CopyToAsync(tempFileStream);
                }

                var (fileTypeIsOk, fileType) = _fileTypeScanner.CheckFileType(tempFilePath);
                if (!fileTypeIsOk)
                {
                    _logger.Warning("Detected file type {detectedFileType} is not allowed, original filename is {fileName}", fileType, file.Name);

                    _tempFileHelper.Delete(tempFilePath);

                    return BadRequest($"File type {fileType} is not allowed");
                }

                if (!file.FileName.ToLower().EndsWith(fileType.ToLower()))
                {
                    _logger.Warning("Detected file type {detectedFileType} does not match supplied filename {fileName}", fileType, file.Name);

                    _tempFileHelper.Delete(tempFilePath);

                    return BadRequest($"File type {fileType} is not allowed");
                }

                var (fileContainsVirus, detail) = _fileVirusScanner.ScanFile(tempFilePath);
                if (fileContainsVirus)
                {
                    _logger.Warning("Detected vulnerability in file {fileType} {fileName} {detail}", fileType, file.Name, detail);

                    _tempFileHelper.Delete(tempFilePath);

                    return BadRequest("File is corrupt");
                }

                var hash = _fileHasher.CalculateHash(tempFilePath);
                string blobId;
                bool created;
                using (var stream = new FileStream(tempFilePath, FileMode.Open))
                {
                    var postResult = _dataRepository.PostBlob(stream, file.FileName, hash);
                    blobId = postResult.BlobId;
                    created = postResult.Created;
                }

                _tempFileHelper.Delete(tempFilePath);

                var uri = GenerateGetUri(blobId, "Upload");

                if (created)
                    return Created(uri, new { uri, blobId, size, hash });

                return Ok(new { uri, blobId, size, hash });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured in Post {@Exception}", ex);
                return new StatusCodeResult(500);
            }
        }

        private Uri GenerateGetUri(string blobId, string stringToReplaceWithId)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var uriBuilder = new UriBuilder();
            // ReSharper disable once PossibleNullReferenceException
            uriBuilder.Scheme = request.Scheme;
            uriBuilder.Host = request.Host.Host;
            if (request.Host.Port.HasValue)
                uriBuilder.Port = request.Host.Port.Value;
            uriBuilder.Path = request.Path.ToString().Replace(stringToReplaceWithId, blobId);
            var uri = uriBuilder.Uri;
            return uri;
        }

        // DELETE api/values/5
        //[Authorize(Policy = "AdminsOnly")]
        [HttpDelete("Files/Delete/{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _logger.Information("Received Delete request {id}", id);

                if (IdIsNotValid(id))
                    return BadRequest("id not valid");

                if (_dataRepository.BlobExists(id))
                {
                    _dataRepository.DeleteBlob(id);
                }

                return Ok($"blob {id} deleted");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured in Delete {@Exception}", ex);
                return new StatusCodeResult(500);
            }
            
        }

        //[Authorize(Policy = "AdminsOnly")]
        [HttpPost("Metadata/Post")]
        public IActionResult PostMetadata(string id, string metadataPropertyName, string metadataPropertyValue)
        {
            try
            {
                _logger.Information("Received PostMetadata request {id} {metadataPropertyName} {metadataPropertyValue}", id, metadataPropertyName, metadataPropertyValue);

                if (IdIsNotValid(id))
                    return BadRequest("id not valid");

                if (DodgyInputFoundIn(metadataPropertyName))
                    return BadRequest("metadataPropertyName not valid");

                if (DodgyInputFoundIn(metadataPropertyValue))
                    return BadRequest("metadataPropertyValue not valid");

                _dataRepository.PostMetadata(id, metadataPropertyName, metadataPropertyValue);

                return new StatusCodeResult(201);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Exception occured in PostMetaData {@Exception}", ex);
                return new StatusCodeResult(500);
            }
        }

        [HttpGet("Metadata/Get")]
        public IActionResult GetMetadata(string id)
        {
            try
            {
                _logger.Information("Received GetMetadata request {id}", id);

                if (IdIsNotValid(id))
                    return BadRequest("id not valid");

                var metaData = _dataRepository.GetMetadata(id).Select(x => new {x.Name, x.Value });

                return Ok(metaData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured in GetMetaData {@Exception}", ex);
                return new StatusCodeResult(500);
            }
        }

        [HttpGet("Metadata/GetByName")]
        public IActionResult GetMetadata(string id, string metadataPropertyName)
        {
            try
            {
                _logger.Information("Received GetMetadataByName request {id}", id);

                if (IdIsNotValid(id))
                    return BadRequest("id not valid");

                if (DodgyInputFoundIn(metadataPropertyName))
                    return BadRequest("metadataPropertyName not valid");

                var metaData = _dataRepository.GetMetadata(id, metadataPropertyName).Select(x => x.Value);

                return Ok(metaData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured in GetMetaDataByName {@Exception}", ex);
                return new StatusCodeResult(500);
            }
        }

        //[Authorize(Policy = "AdminsOnly")]
        [HttpDelete("Metadata/Delete/{id}")]
        public IActionResult DeleteMetadata(string id, string metadataPropertyName)
        {
            try
            {
                _logger.Information("Received DeleteMetadata request {id}", id);

                if (IdIsNotValid(id))
                    return BadRequest("id not valid");

                if (DodgyInputFoundIn(metadataPropertyName))
                    return BadRequest("metadataPropertyName not valid");

                var affectedRecords = _dataRepository.DeleteMetadata(id, metadataPropertyName);
                return Ok($"{affectedRecords} metadata records deleted");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured in DeleteMetadata {@Exception}", ex);
                return new StatusCodeResult(500);
            }
        }

        [HttpGet("Files/FindIdsViaMetadata")]
        public IActionResult FindBlobIdsViaMetadata(string metadataPropertyName, string metadataPropertyValue)
        {
            try
            {
                _logger.Information("Received FindBlobIdsViaMetadata request ");

                if (DodgyInputFoundIn(metadataPropertyName))
                    return BadRequest("metadataPropertyName not valid");

                if (DodgyInputFoundIn(metadataPropertyValue))
                    return BadRequest("metadataPropertyValue not valid");

                var metaData = _dataRepository.FindBlobIdsViaMetadata(metadataPropertyName, metadataPropertyValue, false);

                var response = metaData.Select(x => new
                    {
                        BlobRef = x,
                        Uri = GenerateGetUri(x, "FindIdsViaMetadata")
                    });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured in FindBlobIdsViaMetadata {@Exception}", ex);
                return new StatusCodeResult(500);
            }
        }

        [HttpGet("Files/GetAll")]
        public IActionResult GetAllBlobRefs(int itemsPerPage, int page)
        {
            try
            {
                _logger.Information("Received GetAllBlobRefs request {itemsPerPage} {page}", itemsPerPage, page);

                // Handle zero based confusion
                var zeroBasedPage = page - 1;
                if (zeroBasedPage < 0)
                    zeroBasedPage = 0;

                var totalItems = _dataRepository.GetCountOfAllBlobReferences();
                var totalPagesAtCurrentSize = Math.Ceiling(decimal.Divide(totalItems, itemsPerPage));

                var items = _dataRepository.GetAllBlobReferences(itemsPerPage, zeroBasedPage);

                var response = new
                {
                    Summary = new
                    {
                        TotalItems = totalItems,
                        ItemsPerPage = itemsPerPage,
                        CurrentPage = $"{page} of {totalPagesAtCurrentSize}" 
                    },
                    Data = items.Select(x => new
                    {
                        x.Id,
                        x.Filename,
                        x.Length,
                        x.UploadDate,
                        x.MimeType,
                        Uri = GenerateGetUri(x.Id, "GetAll")
                    })
                };

                return Ok(response);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Exception occured in GetAllBlobRefs {@Exception}", ex);
                return new StatusCodeResult(500);
            }
        }

        private bool IdIsNotValid(string id)
        {
            // ReSharper disable once UnusedVariable
            return !Guid.TryParse(id, out _);
        }

        private bool DodgyInputFoundIn(string input)
        {
            if (input.Length > _maxUserInputLength)
                return true;

            var regex = new Regex(@"^[a-zA-Z0-9_.-]+$");
            return !regex.IsMatch(input);
        }
    }    
}
