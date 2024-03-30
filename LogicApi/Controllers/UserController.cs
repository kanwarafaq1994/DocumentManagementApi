using DocumentManagement.Data.Common;
using DocumentManagement.Data.UnitsOfWork;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DocumentManagement.Data.Common.Extensions;

namespace LogicApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthContext _authContext;

        public UserController(IUnitOfWork unitOfWork, IAuthContext authContext)
        {
            _unitOfWork = unitOfWork;
            _authContext = authContext;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserDocuments(int userId)
        {
            try
            {
                var documents = await _unitOfWork.documentRepository.GetUserDocuments(userId);
                if (documents == null || documents.Count == 0) return NotFound(new InfoDto("Documents not found"));

                return Ok(documents.ToDtoArray());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new InfoDto("Get user documents failed"));
            }
        }
    }
}
