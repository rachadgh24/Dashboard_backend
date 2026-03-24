using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1;
using task1.Application.Services;
using task1.Models;

namespace task1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notificationService;

        public CustomersController(ICustomerService customerService, INotificationService notificationService)
        {
            _customerService = customerService;
            _notificationService = notificationService;
        }

        [Authorize(Policy = "ViewCustomers")]
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(new ApiResponse<List<CustomerModel>> { Data = customers });
        }

        [Authorize(Policy = "ViewCustomer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Customer not found." } });
            return Ok(new ApiResponse<CustomerModel> { Data = customer });
        }

        [Authorize(Policy = "CreateCustomer")]
        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerModel? customerModel)
        {
            if (customerModel == null) return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });
            var addedCustomer = await _customerService.AddCustomerAsync(customerModel);
            var role = User.FindFirst("role")?.Value ?? string.Empty;
            if (role == UserRoles.GeneralManager || role == UserRoles.SocialMediaManager)
            {
                var name = ActionNotificationHelper.GetDisplayName(User);
                var message = ActionNotificationHelper.Format(role, name, "added a customer");
                await _notificationService.RecordAsync(message);
                return Ok(new ApiResponse<object> { Data = new { customer = addedCustomer, message } });
            }
            return Ok(new ApiResponse<CustomerModel> { Data = addedCustomer });
        }

        [Authorize(Policy = "EditCustomer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerModel? customerModel)
        {
            if (customerModel == null) return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });
            var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerModel);
            if (updatedCustomer == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Customer not found." } });
            return Ok(new ApiResponse<CustomerModel> { Data = updatedCustomer });
        }

        [Authorize(Policy = "DeleteCustomer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (!await _customerService.DeleteCustomerAsync(id)) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Customer not found." } });
            return Ok(new ApiResponse<object> { Data = null });
        }

        [Authorize(Policy = "SearchCustomers")]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? query)
        {
            var customers = await _customerService.Search(query);
            return Ok(new ApiResponse<List<CustomerModel>> { Data = customers });
        }

        [Authorize(Policy = "ViewCustomers")]
        [HttpGet("paginate")]
        public async Task<IActionResult> PaginateCustomers([FromQuery] int page = 1, [FromQuery] string? sortBy = null)
        {
            var customers = await _customerService.PaginateCustomersAsync(page, sortBy);
            return Ok(new ApiResponse<List<CustomerModel>> { Data = customers });
        }

        [Authorize(Policy = "ViewCustomers")]
        [HttpGet("count")]
        public async Task<IActionResult> GetCustomersCount()
        {
            var count = await _customerService.GetCountAsync();
            return Ok(new ApiResponse<int> { Data = count });
        }
    }
}
