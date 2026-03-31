using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaGestionCitas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.ToList();
            var modelo = new List<UsuarioViewModel>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                modelo.Add(new UsuarioViewModel
                {
                    Id = u.Id,
                    Email = u.Email!,
                    Nombre = u.UserName!,
                    Rol = roles.FirstOrDefault() ?? "Sin rol",
                    Activo = !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.Now
                });
            }

            return View(modelo);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(new[] { "Administrador", "Usuario" });
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearUsuarioViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = modelo.Email,
                    Email = modelo.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, modelo.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, modelo.Rol);
                    TempData["Exito"] = "Usuario creado correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = new SelectList(new[] { "Administrador", "Usuario" }, modelo.Rol);
            return View(modelo);
        }

        // GET: Usuarios/Edit/id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var modelo = new EditarUsuarioViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                Nombre = user.UserName!,
                Rol = roles.FirstOrDefault() ?? "Usuario",
                Activo = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.Now
            };

            ViewBag.Roles = new SelectList(new[] { "Administrador", "Usuario" }, modelo.Rol);
            return View(modelo);
        }

        // POST: Usuarios/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditarUsuarioViewModel modelo)
        {
            if (id != modelo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                user.UserName = modelo.Email;
                user.Email = modelo.Email;

                // Actualizar estado activo/inactivo
                if (modelo.Activo)
                    await _userManager.SetLockoutEndDateAsync(user, null);
                else
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    // Actualizar rol
                    var rolesActuales = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, rolesActuales);
                    await _userManager.AddToRoleAsync(user, modelo.Rol);

                    TempData["Exito"] = "Usuario actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = new SelectList(new[] { "Administrador", "Usuario" }, modelo.Rol);
            return View(modelo);
        }

        // GET: Usuarios/Delete/id
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var modelo = new UsuarioViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                Nombre = user.UserName!,
                Rol = roles.FirstOrDefault() ?? "Sin rol",
                Activo = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.Now
            };

            return View(modelo);
        }

        // POST: Usuarios/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Evitar eliminar el admin principal
            if (user.Email == "admin@sistema.com")
            {
                TempData["Error"] = "No se puede eliminar el administrador principal del sistema.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);
            TempData["Exito"] = "Usuario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModels necesarios (podés moverlos a la carpeta Models si preferís)
    public class UsuarioViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class CrearUsuarioViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El correo es obligatorio")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La contraseña es obligatoria")]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = string.Empty;
    }

    public class EditarUsuarioViewModel
    {
        public string Id { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El correo es obligatorio")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Display(Name = "Activo")]
        public bool Activo { get; set; }
    }
}