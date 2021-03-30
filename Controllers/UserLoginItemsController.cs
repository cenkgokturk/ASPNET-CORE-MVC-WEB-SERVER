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

            await Task.Delay(200);

            //Compare current time with the last accessed time
            if(userLoginItem != null)
            {
                DateTime timeAccessed = DateTime.Now;
                TimeSpan span = timeAccessed.Subtract(userLoginItem.LoginDate);
            }
            

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

        //Retrieve the UserID of the given User 
        private int GetUserID(String Usermail)
        {
            int UserID = -1;

            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                string sqlquery = "select UserID  from AccountInfo where Usermail = '" + Usermail + "' ";
                using (SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn))
                {
                    sqlconn.Open();
                    SqlDataReader reader = sqlcomm.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            UserID = reader.GetInt32(0);
                        }
                    }

                    sqlconn.Close();
                }
            }
            return UserID;
        }

        // Add a new entity to the UserRoles with the given user
        private void AddUserRole(int UserID)
        {
            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                // Number 2 for the Roleid indicates RegularUser
                string sqlquery = "insert into UserRoles(UserID, Roleid) values ('" + UserID + "', 2)";
                using (SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn))
                {
                    sqlconn.Open();
                    sqlcomm.ExecuteNonQuery();
                }
            }
        }

        private void AddUserSessions(UserLoginItem userLoginItem)
        {
            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                DateTime thisDay = DateTime.Now;
                //Date format is 30/3/2020 12:00 AM
                //Number at the end indicates 0 for Logged Out & 1 for Logged in
                string sqlQuerySession = "insert into AccountSessions(UserId, LoginDate, IsLoggedIn) values ('" + userLoginItem.UserID + "', '" + thisDay + "', 1 )";
                using (SqlCommand sqlcommCookie = new SqlCommand(sqlQuerySession, sqlconn))
                {
                    sqlconn.Open();
                    sqlcommCookie.ExecuteNonQuery();
                }
            }
        }

        // POST: api/UserLoginItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserLoginItem>> PostUserLoginItem(UserLoginItem userLoginItem)
        {
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
                            // Password correct - Successful Login
                            if (reader.GetString(0).Equals(userLoginItem.Userpassword))
                            {
                                userLoginItem.isUserLoggedIn = 1;

                                userLoginItem.UserID = reader.GetInt32(1);
                                userLoginItem.Username = reader.GetString(2);
                                userLoginItem.UserRole = reader.GetInt32(3);

                                DateTime thisDay = DateTime.Now;
                                userLoginItem.LoginDate = thisDay;

                                // Add the user session for logging
                                AddUserSessions(userLoginItem);
                            }   
                        }
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

            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                string sqlquery = "DELETE FROM AccountSessions WHERE UserID = '" + userLoginItem.UserID + "' ";
                using (SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn))
                {
                    sqlconn.Open();
                    sqlcomm.ExecuteNonQuery();
                }
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
