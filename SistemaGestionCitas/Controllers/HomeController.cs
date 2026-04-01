using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionCitas.Data;
using SistemaGestionCitas.Models;


namespace SistemaGestionCitas.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> Stats()
        {
            var userId = _userManager.GetUserId(User);
            var esAdmin = User.IsInRole("Administrador");

            var clientes = await _context.Clientes.CountAsync();
            var servicios = await _context.Servicios.CountAsync(s => s.Activo);
            var citas = esAdmin
                ? await _context.Citas.CountAsync(c => c.Estado == EstadoCita.Programada)
                : await _context.Citas.CountAsync(c => c.UsuarioId == userId && c.Estado == EstadoCita.Programada);
            var usuarios = _userManager.Users.Count();

            return Json(new { clientes, servicios, citas, usuarios });
        }

        public IActionResult Privacy() => View();
    }
}
