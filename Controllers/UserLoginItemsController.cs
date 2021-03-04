using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPNETAOP_WebServer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System.Net.Http;
using System.Net.Http.Json;

namespace ASPNETAOP_WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginItemsController : ControllerBase
    {
        private readonly UserLoginContext _context;

        private IConfiguration _configuration;

        public UserLoginItemsController(UserLoginContext context)
        {
            _context = context;
        }

        // GET: api/UserLoginItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserLoginItem>>> GetUserLoginItems()
        {
            return await _context.UserLoginItems.ToListAsync();
        }

        // GET: api/UserLoginItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserLoginItem>> GetUserLoginItem(long id)
        {
            var userLoginItem = await _context.UserLoginItems.FindAsync(id);

            if (userLoginItem == null)
            {
                return NotFound();
            }

            return userLoginItem;
        }

        // PUT: api/UserLoginItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserLoginItem(long id, UserLoginItem userLoginItem)
        {
            if (id != userLoginItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(userLoginItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserLoginItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserLoginItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserLoginItem>> PostUserLoginItem(UserLoginItem userLoginItem)
        {
            

            int isUserLoggedIn = 0;

            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                string sqlquery = "select AI.Userpassword, AI.UserID, AI.Username, UR.Roleid  from AccountInfo AI, UserRoles UR where AI.UserID = UR.UserID AND AI.Usermail = '" + userLoginItem.Usermail + "' ";
                using (SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn))
                {
                    sqlconn.Open();
                    SqlDataReader reader = sqlcomm.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(0).Equals(userLoginItem.Userpassword)){
                                userLoginItem.isUserLoggedIn = 1; 
                            }    //Password correct - Successful Login
                            else{ userLoginItem.isUserLoggedIn = 2; }   //Password not correct - Login denied
                        }
                    }
                    else
                    {
                        userLoginItem.isUserLoggedIn = 3; //User not found - Login denied
                    }
                    reader.Close();
                }
            }

            _context.UserLoginItems.Add(userLoginItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserLoginItem), new { id = userLoginItem.Id }, userLoginItem);

        }

        // DELETE: api/UserLoginItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserLoginItem(long id)
        {
            var userLoginItem = await _context.UserLoginItems.FindAsync(id);
            if (userLoginItem == null)
            {
                return NotFound();
            }

            _context.UserLoginItems.Remove(userLoginItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserLoginItemExists(long id)
        {
            return _context.UserLoginItems.Any(e => e.Id == id);
        }
    }
}
