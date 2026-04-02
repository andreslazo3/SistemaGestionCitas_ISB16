using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionCitas.Data;
using SistemaGestionCitas.Models;

namespace SistemaGestionCitas.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CitasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Citas
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var esAdmin = User.IsInRole("Administrador");

            var citas = _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Include(c => c.Usuario)
                .AsQueryable();

            // Usuario solo ve sus propias citas
            if (!esAdmin)
                citas = citas.Where(c => c.UsuarioId == userId);

            return View(await citas.OrderBy(c => c.Fecha).ThenBy(c => c.Hora).ToListAsync());
        }

        // GET: Citas/Create
        public async Task<IActionResult> Create()
        {
            await CargarSelectLists();
            return View();
        }

        // POST: Citas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,ServicioId,Fecha,Hora")] Cita cita)
        {
            
            ModelState.Remove("Usuario");
            ModelState.Remove("UsuarioId");
            ModelState.Remove("Cliente");
            ModelState.Remove("Servicio");

            // Regla: no se pueden crear citas en fechas pasadas
            if (cita.Fecha.Date < DateTime.Today)
                ModelState.AddModelError("Fecha", "No se pueden registrar citas en fechas pasadas.");

            // Regla: no se pueden crear citas con servicios inactivos
            var servicio = await _context.Servicios.FindAsync(cita.ServicioId);
            if (servicio != null && !servicio.Activo)
                ModelState.AddModelError("ServicioId", "El servicio seleccionado está inactivo.");

            if (ModelState.IsValid)
            {
                cita.UsuarioId = _userManager.GetUserId(User)!;
                cita.Estado = EstadoCita.Programada;
                _context.Add(cita);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Cita registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarSelectLists(cita.ClienteId, cita.ServicioId);
            return View(cita);
        }

        // GET: Citas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            // Regla: cita cancelada no puede modificarse
            if (cita.Estado == EstadoCita.Cancelada)
            {
                TempData["Error"] = "Una cita cancelada no puede modificarse.";
                return RedirectToAction(nameof(Index));
            }

            // Usuario solo puede editar sus propias citas
            if (!User.IsInRole("Administrador") && cita.UsuarioId != _userManager.GetUserId(User))
                return Forbid();

            await CargarSelectLists(cita.ClienteId, cita.ServicioId);
            return View(cita);
        }

        // POST: Citas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,ServicioId,Fecha,Hora,UsuarioId,Estado")] Cita cita)
        {
            if (id != cita.Id) return NotFound();

            // Limpiar errores de navegación
            ModelState.Remove("Usuario");
            ModelState.Remove("Cliente");
            ModelState.Remove("Servicio");

            var citaOriginal = await _context.Citas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (citaOriginal == null) return NotFound();

            if (citaOriginal.Estado == EstadoCita.Cancelada)
            {
                TempData["Error"] = "Una cita cancelada no puede modificarse.";
                return RedirectToAction(nameof(Index));
            }

            if (!User.IsInRole("Administrador") && citaOriginal.UsuarioId != _userManager.GetUserId(User))
                return Forbid();

            if (cita.Fecha.Date < DateTime.Today)
                ModelState.AddModelError("Fecha", "No se pueden registrar citas en fechas pasadas.");

            var servicio = await _context.Servicios.FindAsync(cita.ServicioId);
            if (servicio != null && !servicio.Activo)
                ModelState.AddModelError("ServicioId", "El servicio seleccionado está inactivo.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cita);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Cita actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Citas.Any(e => e.Id == cita.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await CargarSelectLists(cita.ClienteId, cita.ServicioId);
            return View(cita);
        }

        // POST: Citas/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            if (cita.Estado == EstadoCita.Cancelada)
            {
                TempData["Error"] = "Esta cita ya está cancelada.";
                return RedirectToAction(nameof(Index));
            }

            // Usuario solo puede cancelar sus propias citas
            if (!User.IsInRole("Administrador") && cita.UsuarioId != _userManager.GetUserId(User))
                return Forbid();

            cita.Estado = EstadoCita.Cancelada;
            _context.Update(cita);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Cita cancelada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            if (!User.IsInRole("Administrador") && cita.UsuarioId != _userManager.GetUserId(User))
                return Forbid();

            return View(cita);
        }

        // Método privado para cargar los dropdowns
        private async Task CargarSelectLists(int clienteId = 0, int servicioId = 0)
        {
            ViewBag.Clientes = new SelectList(
                await _context.Clientes.OrderBy(c => c.NombreCompleto).ToListAsync(),
                "Id", "NombreCompleto", clienteId);

            // Solo servicios activos en el dropdown
            ViewBag.Servicios = new SelectList(
                await _context.Servicios.Where(s => s.Activo).OrderBy(s => s.NombreServicio).ToListAsync(),
                "Id", "NombreServicio", servicioId);
        }
    }
}