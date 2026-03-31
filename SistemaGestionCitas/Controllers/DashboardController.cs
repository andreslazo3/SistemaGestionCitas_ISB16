using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionCitas.Data;
using SistemaGestionCitas.Models;

namespace SistemaGestionCitas.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrador"))
                return View("DashboardAdmin", await GetDashboardAdmin());
            else
                return View("DashboardUsuario", await GetDashboardUsuario());
        }

        private async Task<DashboardAdminViewModel> GetDashboardAdmin()
        {
            var totalUsuarios = _userManager.Users.Count();
            var totalClientes = await _context.Clientes.CountAsync();
            var totalServiciosActivos = await _context.Servicios.CountAsync(s => s.Activo);
            var totalServiciosInactivos = await _context.Servicios.CountAsync(s => !s.Activo);
            var totalCitas = await _context.Citas.CountAsync();
            var citasProgramadas = await _context.Citas.CountAsync(c => c.Estado == EstadoCita.Programada);
            var citasCanceladas = await _context.Citas.CountAsync(c => c.Estado == EstadoCita.Cancelada);

            // Top 3 servicios más solicitados
            var topServicios = await _context.Citas
                .GroupBy(c => c.Servicio!.NombreServicio)
                .Select(g => new ServicioPopularViewModel
                {
                    NombreServicio = g.Key,
                    TotalCitas = g.Count()
                })
                .OrderByDescending(s => s.TotalCitas)
                .Take(3)
                .ToListAsync();

            return new DashboardAdminViewModel
            {
                TotalUsuarios = totalUsuarios,
                TotalClientes = totalClientes,
                TotalServiciosActivos = totalServiciosActivos,
                TotalServiciosInactivos = totalServiciosInactivos,
                TotalCitas = totalCitas,
                CitasProgramadas = citasProgramadas,
                CitasCanceladas = citasCanceladas,
                TopServicios = topServicios
            };
        }

        private async Task<DashboardUsuarioViewModel> GetDashboardUsuario()
        {
            var userId = _userManager.GetUserId(User);

            var totalCitas = await _context.Citas.CountAsync(c => c.UsuarioId == userId);
            var citasActivas = await _context.Citas.CountAsync(c => c.UsuarioId == userId && c.Estado == EstadoCita.Programada);
            var citasCanceladas = await _context.Citas.CountAsync(c => c.UsuarioId == userId && c.Estado == EstadoCita.Cancelada);

            // Próximas citas ordenadas por fecha
            var proximasCitas = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Where(c => c.UsuarioId == userId
                         && c.Estado == EstadoCita.Programada
                         && c.Fecha >= DateTime.Today)
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .Take(5)
                .ToListAsync();

            return new DashboardUsuarioViewModel
            {
                TotalCitas = totalCitas,
                CitasActivas = citasActivas,
                CitasCanceladas = citasCanceladas,
                ProximasCitas = proximasCitas
            };
        }
    }

    // ViewModels del Dashboard
    public class DashboardAdminViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalClientes { get; set; }
        public int TotalServiciosActivos { get; set; }
        public int TotalServiciosInactivos { get; set; }
        public int TotalCitas { get; set; }
        public int CitasProgramadas { get; set; }
        public int CitasCanceladas { get; set; }
        public List<ServicioPopularViewModel> TopServicios { get; set; } = new();
    }

    public class DashboardUsuarioViewModel
    {
        public int TotalCitas { get; set; }
        public int CitasActivas { get; set; }
        public int CitasCanceladas { get; set; }
        public List<Cita> ProximasCitas { get; set; } = new();
    }

    public class ServicioPopularViewModel
    {
        public string NombreServicio { get; set; } = string.Empty;
        public int TotalCitas { get; set; }
    }
}