using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRPatients.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MRPatients.Controllers
{
    [Authorize(Roles = "administrators")]
    public class MRRoleController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;

        public MRRoleController(RoleManager<IdentityRole> roleManager,
                                        UserManager<IdentityUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRole model)
        {
            if(!String.IsNullOrEmpty(model.RoleName))
            {
                if (ModelState.IsValid)
                {
                    IdentityRole role = new IdentityRole { Name = model.RoleName.Trim() };

                    var result = await roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        TempData["message"] = "role added: " + model.RoleName;
                        return RedirectToAction("ListRole");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            TempData["message"] = error.Description;
                            break;
                        }
                        return RedirectToAction("ListRole");
                    }
                }
            }
            else
            {
                TempData["message"] = "The proposed role name is null, empty or just blanks";
                return RedirectToAction("ListRole");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListRole()
        {
            var roles = roleManager.Roles.OrderBy(n => n.Name);
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveUserInRole(string removeUserId, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Error meesage
                ViewBag.ErrorMessage = $"Role with id = {roleId} cannot be found";
            }

            var user = await userManager.FindByIdAsync(removeUserId);

            if (role.Name.ToUpper().Equals("ADMINISTRATORS") && user.UserName.Equals(User.Identity.Name))
            {
                TempData["message"] = "You can NOT delete yourself in the 'administrators' role";
                return RedirectToAction("EditUsersInRole", new { roleId = roleId });
            }
            else
            {
                IdentityResult result = await userManager.RemoveFromRoleAsync(user, role.Name);
                if (result.Succeeded)
                {
                    TempData["message"] = "user " + user.UserName + " successfully removed from '" + role.Name + "'";
                    return RedirectToAction("EditUsersInRole", new { roleId = roleId });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        TempData["message"] = error.Description;
                        break;
                    }
                }
            }

            return RedirectToAction("EditUsersInRole", new { roleId = roleId });
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Error meesage
                ViewBag.ErrorMessage = $"Role with id = {roleId} cannot be found";
            }

            var model = new List<UserRole>();
            var notInRoleUsers = new List<UserRole>();

            foreach (var user in userManager.Users)
            {
                var userrole = new UserRole
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    UserEmail = user.Email
                };
                
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userrole.IsSelected = true;
                    model.Add(userrole);
                }
                else
                {
                    userrole.IsSelected = false;
                    notInRoleUsers.Add(userrole);
                }
            }

            List<SelectListItem> item = notInRoleUsers.ConvertAll(a =>
            {
                return new SelectListItem()
                {
                    Text = a.UserName,
                    Value = a.UserId,
                    Selected = false
                };
            });

            ViewData["NotInRoleUsers"] = item.OrderBy(t => t.Text);
            ViewData["roleId"] = roleId;
            ViewData["roleName"] = role.Name;
            List<UserRole> SortedList = model.OrderBy(o => o.UserName).ToList();
            return View(SortedList);

        }

        [HttpPost]
        public async Task<IActionResult> AddUserInRole(string addUserId, string roleId)
        {
            if (!String.IsNullOrEmpty(addUserId))
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    // Error meesage
                    ViewBag.ErrorMessage = $"Role with id = {roleId} cannot be found";
                }

                var user = await userManager.FindByIdAsync(addUserId);
                IdentityResult result = await userManager.AddToRoleAsync(user, role.Name);

                if (result.Succeeded)
                {
                    TempData["message"] = "user " + user.UserName + " successfully added to '" + role.Name + "'";
                    return RedirectToAction("EditUsersInRole", new { roleId = roleId });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        TempData["message"] = error.Description;
                        break;
                    }
                    return RedirectToAction("EditUsersInRole", new { roleId = roleId });
                }
            }
            else
            {
                TempData["message"] = "User is Required";
            }
            
            return RedirectToAction("EditUsersInRole", new { roleId = roleId });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Error meesage
                ViewBag.ErrorMessage = $"Role with id = {roleId} cannot be found";
            }
            var model = new DeleteRole
            {
                Id = role.Id,
                RoleName = role.Name
            };

            if (!role.Name.ToUpper().Equals("ADMINISTRATORS"))
            {
                int usersInRoleCount = 0;
                foreach (var user in userManager.Users)
                {
                    if (await userManager.IsInRoleAsync(user, role.Name))
                    {
                        usersInRoleCount++;
                        model.Users.Add(user.UserName);
                    }
                }

                if (usersInRoleCount == 0)
                {
                    var result = await roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        TempData["message"] = role.Name + " role successfully Deleted";
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            TempData["message"] = error.Description;
                            break;
                        }
                    }
                    return RedirectToAction("ListRole");
                }
            }
            else
            {
                TempData["message"] = "administrators cannot be deleted";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleId, string continueDelete)
        {
            if(continueDelete.Equals("Y"))
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    // Error meesage
                    ViewBag.ErrorMessage = $"Role with id = {roleId} cannot be found";
                }
                else
                {
                    if (!role.Name.ToUpper().Equals("ADMINISTRATORS"))
                    {
                        var result = await roleManager.DeleteAsync(role);

                        if (result.Succeeded)
                        {
                            TempData["message"] = role.Name + " role successfully Deleted";
                            return RedirectToAction("ListRole");
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                TempData["message"] = error.Description;
                                break;
                            }
                        }
                    }
                    else
                    {
                        TempData["message"] = "administrators cannot be deleted";
                    }
                }
            }

            return RedirectToAction("ListRole");
        }
    }
}
