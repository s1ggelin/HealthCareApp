using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthCareWebb.Pages.Dashboard
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminDashboardModel : PageModel
    {
        public string Username { get; private set; }

        public void OnGet()
        {
            Username = User.Identity.Name;
        }
    }
}

//Possibility to select timeslots
//Collect data in an array of objects
//Verify that data is collected properly