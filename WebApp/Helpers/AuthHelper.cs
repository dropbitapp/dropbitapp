using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApp.Models;

namespace WebApp.Helpers
{
    public static class AuthHelper
    {
        /// <summary>
        /// Checks if user doesn't exist, if so, creates a new ApplicationUser
        /// </summary>
        public static async Task CheckCreateUser(DistilDBContext _db, Logger _logger, ApplicationUserManager UserManager, ApplicationUser user, string password, uint distillerId)
        {
            var existingUser = await UserManager.FindByNameAsync(user.UserName);
            if (existingUser == null)
            {
                var userCreationSucceeded = false;
                try
                {
                    var result = await UserManager.CreateAsync(user, password);
                    if (result.Succeeded)
                        userCreationSucceeded = true;
                    else
                        throw new Exception(string.Concat(result.Errors));
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to create default admin user: {0}", ex.ToString());
                }

                // Insert into AspNetUserToDistiller only after successfully creating a user
                if (userCreationSucceeded)
                {
                    // Find newly created user
                    var newUser = await UserManager.FindByNameAsync(user.UserName);

                    try
                    {
                        _db.AspNetUserToDistiller.Add(new AspNetUserToDistiller { DistillerID = Convert.ToInt32(distillerId), UserId = newUser.Id });
                        _db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        // Something is wrong with the connection to the db, log and attempt to revert user creation
                        _logger.Error("Failed to insert into AspNetUserToDistiller table: \n\t{0}", ex.ToString());
                        await UserManager.DeleteAsync(newUser);
                    }
                }
            }
        }
    }
}