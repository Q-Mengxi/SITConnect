using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASAssignment
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        protected void Page_Load(object sender, EventArgs e)
        {
            string username = Session["LoggedIn"].ToString();
        }
        protected void submit_Click(object sender, EventArgs e)
        {
            string user = Session["LoggedIn"].ToString();
            string current = Curr_Pass.Text.ToString().Trim();
            string newpass = New_Password.Text.ToString().Trim();
            string confirm = New_Password.Text.ToString().Trim();


            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash(user);
            string dbSalt = getDBSalt(user);
            Response.Redirect("Registration.aspx");
        }
        protected void Cancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Userpage.aspx");
        }
        protected string getDBHash(string userid)
        {
            string h = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordHash FROM Account WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            System.Diagnostics.Debug.WriteLine("Userid getdbhash " + userid);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            {
                                h = reader["PasswordHash"].ToString();
                                System.Diagnostics.Debug.WriteLine("Wrong get hash " + h);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Null");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return h;
        }
        protected string getDBSalt(string userid)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select Passwordsalt FROM Account WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            System.Diagnostics.Debug.WriteLine("Userid getdbsalt " + userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Passwordsalt"] != null)
                        {
                            if (reader["Passwordsalt"] != DBNull.Value)
                            {
                                s = reader["Passwordsalt"].ToString();
                                System.Diagnostics.Debug.WriteLine("wrong getdbsalt " + s);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }
    }

    
}