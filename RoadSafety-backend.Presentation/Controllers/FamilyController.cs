using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.UseCases.Family;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FamilyController(CreateFamilyUseCase createFamilyUseCase)
{
    [HttpPost]
    public async Task<IActionResult> CreateFamily()
}
