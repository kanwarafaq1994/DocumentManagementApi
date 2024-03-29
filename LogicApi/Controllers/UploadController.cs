using DocumentManagement.Data.Common;
using DocumentManagement.Data.Exceptions;
using DocumentManagement.Data.Models;
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
    public class UploadController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public UploadController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> Upload(List<IFormFile> files)
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

                    document = await _unitOfWork._documentRepository.SaveFile(
                        stream,
                        fileName);

                }

                return Ok(invalidFileNames);

            }
            catch (UserException ex)
            {
                return StatusCode(500, new InfoDto(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new InfoDto("Document could not be uploaded"));
            }
        }

        [HttpGet("{fileId}")]
        //[ServiceFilter(typeof(OwnDocumentAttribute))]
        public async Task<IActionResult> Get(int fileId)
        {
            try
            {
                var document = await _unitOfWork._documentRepository.Get(fileId);
                var content = await _unitOfWork._documentRepository.GetContentByte(document);
                var contentType = MimeMapping.MimeUtility.GetMimeMapping($"export{Path.GetExtension(document.FilePath)}");
                return File(content, contentType, Path.GetFileName(document.FilePath));
            }
            catch (UserException ex)
            {
   
                return StatusCode(500, new InfoDto(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new InfoDto("Document could not be loaded"));
            }
        }

        [HttpDelete("{fileId}")]
       // [ServiceFilter(typeof(OwnDocumentAttribute))]
        public async Task<IActionResult> Delete(int fileId)
        {
            try
            {
                await _unitOfWork._documentRepository.Delete(fileId);
                return Ok();
            }
            catch (UserException ex)
            {
                return StatusCode(500, new InfoDto(ex.Message));
            }
            catch (Exception e)
            {
                return StatusCode(500, new InfoDto("Document could not be deleted"));
            }
        }
    }
}
