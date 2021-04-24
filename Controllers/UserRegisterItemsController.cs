using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPNETAOP_WebServer.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ASPNETAOP_WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegisterItemsController : ControllerBase
    {
        private readonly UserRegisterContext _context;

        public UserRegisterItemsController(UserRegisterContext context)
        {
            _context = context;
        }

        // GET: api/UserRegisterItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRegisterItem>>> GetUserRegisterItems()
        {
            return await _context.UserRegisterItems.ToListAsync();
        }

        // GET: api/UserRegisterItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserRegisterItem>> GetUserRegisterItem(long id)
        {
            var userRegisterItem = await _context.UserRegisterItems.FindAsync(id);

            if (userRegisterItem == null)
            {
                return NotFound();
            }

            return userRegisterItem;
        }

        // PUT: api/UserRegisterItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserRegisterItem(long id, UserRegisterItem userRegisterItem)
        {
            if (id != userRegisterItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(userRegisterItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserRegisterItemExists(id))
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

        // Give a standart role to the newly added user
        private void AddUserRole(int UserID)
        {
            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                sqlconn.Open();
                string sql = "insert into UserRoles(UserID, Roleid) values(@ID,@Role)";
                using (SqlCommand cmd = new SqlCommand(sql, sqlconn))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = UserID;
                    cmd.Parameters.Add("@Role", SqlDbType.Int).Value = 2;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // POST: api/UserRegisterItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserRegisterItem>> PostUserRegisterItem(UserRegisterItem userRegisterItem)
        {
            String connection = "Server=DESKTOP-II1M7LK;Database=AccountDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            using (SqlConnection sqlconn = new SqlConnection(connection))
            {
                sqlconn.Open();
                string sql = "insert into AccountInfo(Username, Usermail, Userpassword) values(@Username,@Usermail,@Userpassword)";
                using (SqlCommand cmd = new SqlCommand(sql, sqlconn))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = userRegisterItem.Username;
                    cmd.Parameters.Add("@Usermail", SqlDbType.NVarChar).Value = userRegisterItem.Usermail;
                    cmd.Parameters.Add("@Userpassword", SqlDbType.NVarChar).Value = userRegisterItem.Userpassword;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }

            _context.UserRegisterItems.Add(userRegisterItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserRegisterItem), new { id = userRegisterItem.Id }, userRegisterItem);
        }

        // DELETE: api/UserRegisterItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserRegisterItem(long id)
        {
            var userRegisterItem = await _context.UserRegisterItems.FindAsync(id);
            if (userRegisterItem == null)
            {
                return NotFound();
            }

            _context.UserRegisterItems.Remove(userRegisterItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserRegisterItemExists(long id)
        {
            return _context.UserRegisterItems.Any(e => e.Id == id);
        }
    }
}
