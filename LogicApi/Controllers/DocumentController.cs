﻿using DocumentManagement.Data.Common;
using DocumentManagement.Data.Common.Extensions;
using DocumentManagement.Data.Exceptions;
using DocumentManagement.Data.Models;
using DocumentManagement.Data.UnitsOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
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
            var invalidFileNames = new List<InfoDto>();
            try
            {
                foreach (var formFile in files)
                {
                    var fileName = Path.GetFileName(formFile.FileName.Trim('"'));
                    if (!fileName.IsValidFileName())
                    {
                        invalidFileNames.Add(new InfoDto(fileName + "is invalid"));

                        continue;
                    }
                    await using var stream = formFile.OpenReadStream();

                    Document document;

                    document = await _unitOfWork.documentRepository.SaveFile(
                        stream,
                        fileName);

                }

                return Ok(invalidFileNames);

            }
            catch (UserException ex)
            {
                return StatusCode(500, new InfoDto(ex.Message));
            }
            catch
            {
                return StatusCode(500, new InfoDto("Document could not be uploaded"));
            }
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> Get(int fileId)
        {
            try
            {
                var document = await _unitOfWork.documentRepository.Get(fileId);
                document.NumberOfDownloads++;
                await _unitOfWork.SaveChangesAsync();
                var content = await _unitOfWork.documentRepository.GetContentByte(document);
                var contentType = MimeMapping.MimeUtility.GetMimeMapping($"export{Path.GetExtension(document.FilePath)}");
                return File(content, contentType, Path.GetFileName(document.FilePath));
            }
            catch (UserException ex)
            {
   
                return StatusCode(500, new InfoDto(ex.Message));
            }
        }

        [HttpGet("{documentId}")]
        public async Task<IActionResult> PublishDocumentAsync(int documentId)
        {
            try
            {
                var publishDocument = await _unitOfWork.documentRepository.PublishDocument(documentId);
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
                var publishDocuments = await _unitOfWork.documentRepository.GetPublishedDocument();
                if(publishDocuments == null || publishDocuments.Count == 0)
                {
                    return NotFound("Public Documents not found");
                }
                return Ok(publishDocuments.ToDtoArray());
            }
            catch (UserException ex)
            {

                return StatusCode(500, new InfoDto(ex.Message));
            }
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> Delete(int fileId)
        {
            try
            {
                await _unitOfWork.documentRepository.Delete(fileId);
                return Ok();
            }
            catch (UserException ex)
            {
                return StatusCode(500, new InfoDto(ex.Message));
            }
        }
    }
}