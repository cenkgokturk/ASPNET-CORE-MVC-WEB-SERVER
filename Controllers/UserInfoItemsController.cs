using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPNETAOP_WebServer.Models;
using Microsoft.Data.SqlClient;

namespace ASPNETAOP_WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoItemsController : ControllerBase
    {
        private readonly UserInfoContext _context;

        public UserInfoItemsController(UserInfoContext context)
        {
            _context = context;
        }

        // GET: api/UserInfoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInfoItem>>> GetUserInfoItems()
        {
            return await _context.UserInfoItems.ToListAsync();
        }

        // GET: api/UserInfoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInfoItem>> GetUserInfoItem(Guid id)
        {
            var userInfoItem = await _context.UserInfoItems.FindAsync(id);

            if (userInfoItem == null)
            {
                return NotFound();
            }

            return userInfoItem;
        }

        // PUT: api/UserInfoItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserInfoItem(Guid id, UserInfoItem userInfoItem)
        {
            if (id != userInfoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(userInfoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserInfoItemExists(id))
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

        private void AddUserSessions(UserInfoItem userInfoItem)
        {
            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                DateTime thisDay = DateTime.Now;
                //Date format is 30/3/2020 12:00 AM

                string sqlQuerySession = "insert into AccountSessions(UserId, SessionID, LoginDate) values ('" + userInfoItem.UserID + "', '" + userInfoItem.Id + "', '" + thisDay + "' )";
                using (SqlCommand sqlcommCookie = new SqlCommand(sqlQuerySession, sqlconn))
                {
                    sqlconn.Open();
                    sqlcommCookie.ExecuteNonQuery();
                }
            }
        }

        // POST: api/UserInfoItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserInfoItem>> PostUserInfoItem(UserInfoItem userInfoItem)
        {
            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                string sqlquery = "select AI.Userpassword, AI.UserID, AI.Username, UR.Roleid  from AccountInfo AI, UserRoles UR where AI.UserID = UR.UserID AND AI.Usermail = '" + userInfoItem.Usermail + "' ";
                using (SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn))
                {
                    sqlconn.Open();
                    SqlDataReader reader = sqlcomm.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            // Password correct - Successful Login
                            if (reader.GetString(0).Equals(userInfoItem.Userpassword))
                            {

                                userInfoItem.UserID = reader.GetInt32(1);
                                userInfoItem.Username = reader.GetString(2);
                                userInfoItem.UserRole = reader.GetInt32(3);

                                DateTime thisDay = DateTime.Now;
                                userInfoItem.LoginDate = thisDay;

                                // Add the user session for logging
                                AddUserSessions(userInfoItem);
                            }
                        }
                    }

                    reader.Close();
                }
            }

            _context.UserInfoItems.Add(userInfoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserInfoItem), new { id = userInfoItem.Id }, userInfoItem);
        }

        // DELETE: api/UserInfoItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserInfoItem(Guid id)
        {
            var userInfoItem = await _context.UserInfoItems.FindAsync(id);
            if (userInfoItem == null)
            {
                return NotFound();
            }

            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                string sqlquery = "DELETE FROM AccountSessions WHERE UserID = '" + userInfoItem.UserID + "' ";
                using (SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn))
                {
                    sqlconn.Open();
                    sqlcomm.ExecuteNonQuery();
                }
            }

            _context.UserInfoItems.Remove(userInfoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserInfoItemExists(Guid id)
        {
            return _context.UserInfoItems.Any(e => e.Id == id);
        }
    }
}
