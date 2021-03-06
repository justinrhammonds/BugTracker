﻿using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Web;

namespace BugTracker.Helpers
{
    public static class HelperExtensions
    {
        public static bool Update<T>(this ApplicationDbContext db, T item, params string[] changedPropertyNames) where T : class, new()
        {
            db.Set<T>().Attach(item);
            foreach (var propertyName in changedPropertyNames)
            {
                // If we can't find the property, this line will throw an exception, 
                //which is good because we want to know about it
                db.Entry(item).Property(propertyName).IsModified = true;
            }
            return true;
        }

        public static async Task RefreshAuthentication(this HttpContextBase context, ApplicationUser user)
        {
            context.GetOwinContext().Authentication.SignOut();
            await context.GetOwinContext().Get<ApplicationSignInManager>().SignInAsync(user, isPersistent: false, rememberBrowser: false);
        }
    }
}