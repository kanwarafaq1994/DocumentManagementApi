﻿using DocumentManagement.Data.Common;
using DocumentManagement.Data.Common.Extensions;
using DocumentManagement.Data.Exceptions;
using DocumentManagement.Data.UnitsOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LogicApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public DocumentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadAsync(List<IFormFile> files)
        {
            var fileStatusMessages = new List<InfoDto>();
            try
            {
                foreach (var formFile in files)
                {
                    var fileName = Path.GetFileName(formFile.FileName.Trim('"'));
                    if (!fileName.IsValidFileName())
                    {
                        fileStatusMessages.Add(new InfoDto(fileName + " is invalid"));
                        continue;
                    }

                    try
                    {
                        using (var stream = formFile.OpenReadStream())
                        {
                            var (document, uploaded) = await _unitOfWork.DocumentRepository.SaveFile(stream, fileName);

                            if (!uploaded)
                            {
                                fileStatusMessages.Add(new InfoDto(fileName + " is already uploaded"));
                            }
                            else
                            {
                                fileStatusMessages.Add(new InfoDto(fileName + " uploaded successfully"));

                                string previewImagePath = await _unitOfWork.DocumentRepository.GeneratePreviewImage(document.FilePath, fileName);

                                if (previewImagePath != null)
                                {
                                    document.PreviewImagePath = previewImagePath;
                                    await _unitOfWork.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    catch (UserException ex)
                    {
                        fileStatusMessages.Add(new InfoDto($"Error uploading {fileName}: {ex.Message}"));
                    }
                }

                // Check if there are any error messages, if not, return success
                if (fileStatusMessages.Any())
                {
                    return Ok(fileStatusMessages);
                }
                else
                {
                    return Ok(new InfoDto("All documents uploaded successfully!"));
                }
            }
            catch (UserException ex)
            {
                return StatusCode(500, new InfoDto("Document could not be uploaded: " + ex.Message));
            }
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetAsync(int fileId)
        {
            try
            {
                var document = await _unitOfWork.DocumentRepository.Get(fileId);
                if (document == null)
                {
                    return NotFound(new InfoDto("Document not found"));
                }
                document.NumberOfDownloads++;
                var content = await _unitOfWork.DocumentRepository.GetContentByte(document);
                var contentType = MimeMapping.MimeUtility.GetMimeMapping($"export{Path.GetExtension(document.FilePath)}");
                await _unitOfWork.SaveChangesAsync();
                return File(content, contentType, Path.GetFileName(document.FilePath));
            }
            catch (Exception ex)
            {

                return StatusCode(500, new InfoDto(ex.Message));
            }
        }

        [HttpGet("Publish/{documentId}")]
        public async Task<IActionResult> PublishDocumentAsync(int documentId)
        {
            try
            {
                var publishDocument = await _unitOfWork.DocumentRepository.PublishDocument(documentId);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new InfoDto("Document Published successfully"));
            }
            catch (UserException ex)
            {

                return StatusCode(500, new InfoDto(ex.Message));
            }
        }

        [HttpGet("GetPublishDocumentAsync")]
        public async Task<IActionResult> GetPublishDocumentAsync()
        {
            try
            {
                var publishDocuments = await _unitOfWork.DocumentRepository.GetPublishedDocument();
                if (publishDocuments == null || publishDocuments.Count == 0)
                {
                    return NotFound("Public Documents not found or it is already expired!");
                }
                return Ok(publishDocuments.ToDtoArray());
            }
            catch (UserException ex)
            {

                return StatusCode(500, new InfoDto(ex.Message));
            }
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteAsyncs(int fileId)
        {
            try
            {
                await _unitOfWork.DocumentRepository.Delete(fileId);
                return Ok(new InfoDto("Document Deleted Successfully!"));
            }
            catch (UserException ex)
            {
                return StatusCode(500, new InfoDto(ex.Message));
            }
        }
    }
}
