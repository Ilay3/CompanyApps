using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Core.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompanyApp.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class SoftwareLicenseController : Controller
    {
        private readonly ISoftwareLicenseService _softwareLicenseService;
        private readonly IComputerService _computerService;

        public SoftwareLicenseController(ISoftwareLicenseService softwareLicenseService, IComputerService computerService)
        {
            _softwareLicenseService = softwareLicenseService;
            _computerService = computerService;
        }

        // GET: /SoftwareLicense
        [HttpGet]
        [HttpGet("Index")]
        [Route("~/SoftwareLicense")]

        public async Task<IActionResult> Index()
        {
            var softwareLicenses = await _softwareLicenseService.GetAllSoftwareLicensesAsync();
            return View(softwareLicenses);
        }

        // GET: /SoftwareLicense/Details/5
        [HttpGet("Details/{id}")]

        public async Task<IActionResult> Details(int id)
        {
            var softwareLicense = await _softwareLicenseService.GetSoftwareLicenseByIdAsync(id);
            if (softwareLicense == null)
            {
                return NotFound();
            }

            return View(softwareLicense);
        }

        // GET: /SoftwareLicense/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new SoftwareLicenseDto
            {
                PurchaseDate = DateTime.Now,
                Seats = 1,
                LicenseType = "Perpetual"
            });
        }

        // POST: /SoftwareLicense/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SysAdmin")]

        public async Task<IActionResult> Create(SoftwareLicenseDto softwareLicenseDto)
        {
            if (ModelState.IsValid)
            {
                await _softwareLicenseService.CreateSoftwareLicenseAsync(softwareLicenseDto);
                return RedirectToAction(nameof(Index));
            }
            return View(softwareLicenseDto);
        }

        // GET: /SoftwareLicense/Edit/5
        [HttpGet("Edit/{id}")]
        [Authorize(Roles = "SysAdmin,Manager")]

        public async Task<IActionResult> Edit(int id)
        {
            var softwareLicense = await _softwareLicenseService.GetSoftwareLicenseByIdAsync(id);
            if (softwareLicense == null)
            {
                return NotFound();
            }
            return View(softwareLicense);
        }

        // POST: /SoftwareLicense/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SysAdmin")]

        public async Task<IActionResult> Edit(int id, SoftwareLicenseDto softwareLicenseDto)
        {
            if (id != softwareLicenseDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _softwareLicenseService.UpdateSoftwareLicenseAsync(softwareLicenseDto);
                return RedirectToAction(nameof(Index));
            }
            return View(softwareLicenseDto);
        }

        // GET: /SoftwareLicense/Delete/5
        [HttpGet("Delete/{id}")]

        public async Task<IActionResult> Delete(int id)
        {
            var softwareLicense = await _softwareLicenseService.GetSoftwareLicenseByIdAsync(id);
            if (softwareLicense == null)
            {
                return NotFound();
            }
            return View(softwareLicense);
        }

        // POST: /SoftwareLicense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SysAdmin")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _softwareLicenseService.DeleteSoftwareLicenseAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /SoftwareLicense/Assign
        [HttpGet("Assign/{id}")]
        public async Task<IActionResult> Assign(int id)
        {
            var softwareLicense = await _softwareLicenseService.GetSoftwareLicenseByIdAsync(id);
            if (softwareLicense == null)
            {
                return NotFound();
            }

            // Получаем все офисы для выпадающего списка
            var offices = await _computerService.GetComputersByOfficeIdAsync(1); // Здесь используется заглушка, в реальном приложении нужно получить список офисов

            var assignDto = new AssignSoftwareDto
            {
                SoftwareLicenseId = id,
                InstallationDate = DateTime.Now
            };

            ViewBag.Computers = new SelectList(offices, "Id", "Model");
            ViewBag.SoftwareName = softwareLicense.Name;

            return View(assignDto);
        }

        // POST: /SoftwareLicense/Assign
        [HttpPost("Assign/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignSoftwareDto assignDto)
        {
            if (ModelState.IsValid)
            {
                await _softwareLicenseService.AssignSoftwareToComputersAsync(assignDto);
                return RedirectToAction(nameof(Details), new { id = assignDto.SoftwareLicenseId });
            }

            var softwareLicense = await _softwareLicenseService.GetSoftwareLicenseByIdAsync(assignDto.SoftwareLicenseId);
            ViewBag.SoftwareName = softwareLicense.Name;

            var computers = await _computerService.GetComputersByOfficeIdAsync(1); // Заглушка
            ViewBag.Computers = new SelectList(computers, "Id", "Model");

            return View(assignDto);
        }

        // POST: /SoftwareLicense/Unassign
        [HttpPost("Unassign/{softwareLicenseId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SysAdmin")]

        public async Task<IActionResult> Unassign(int computerId, int softwareLicenseId)
        {
            await _softwareLicenseService.UnassignSoftwareFromComputerAsync(computerId, softwareLicenseId);
            return RedirectToAction(nameof(Details), new { id = softwareLicenseId });
        }

        // GET: /SoftwareLicense/ForComputer/5

        [HttpGet("ForComputer/{id}")]
        public async Task<IActionResult> ForComputer(int id)
        {
            var softwareLicenses = await _softwareLicenseService.GetSoftwareLicensesByComputerIdAsync(id);
            var computer = await _computerService.GetComputerByIdAsync(id);

            ViewBag.ComputerId = id;
            ViewBag.ComputerModel = computer?.Model;

            return View(softwareLicenses);
        }

        // GET: /SoftwareLicense/Expiring
       
        [HttpGet("Expiring")]
        [Authorize(Roles = "SysAdmin")]

        public async Task<IActionResult> Expiring(int days = 30)
        {
            var expiringSoftware = await _softwareLicenseService.GetExpiringSoftwareLicensesAsync(days);
            ViewBag.Days = days;
            return View(expiringSoftware);
        }
    }
}