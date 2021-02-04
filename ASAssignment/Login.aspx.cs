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
    public partial class Login : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;


        public class MyObject
        {
            public string success { get; set; }
            public List<string> errorMessage { get; set; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        private bool validateinput()
        {

            lbMsg.Text = String.Empty;

            if (Email_Login.Text == "")
            {
                lbMsg.Text += "Email cannot be empty! <br/>";
            }
            if (Login_Password.Text == "")
            {
                lbMsg.Text += "Password cannot be empty! <br/>";
            }
            if (String.IsNullOrEmpty(lbMsg.Text))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        protected void Login_Click(object sender, EventArgs e)
        {
            if (validateinput()){
                System.Diagnostics.Debug.WriteLine("Error Here");
                if (validateCaptcha())
                {
                    System.Diagnostics.Debug.WriteLine("ERROR HERE 2");
                    string pwd = Login_Password.Text.ToString().Trim();
                    string userid = Email_Login.Text.ToString().Trim();
                    SHA512Managed hashing = new SHA512Managed();
                    string dbHash = getDBHash(userid);
                    string dbSalt = getDBSalt(userid);
                    string lockedoutime = getlockedouttime(userid);
                    int attempt = Convert.ToInt32(getAttempt(userid));
                    System.Diagnostics.Debug.WriteLine(dbHash);
                    System.Diagnostics.Debug.WriteLine(dbSalt);
                    System.Diagnostics.Debug.WriteLine(userid);
                    try
                    {
                        if (lockedoutime == "0") {
                            if (attempt < 3)
                            {
                                if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                                {
                                    System.Diagnostics.Debug.WriteLine("Wrong 2");
                                    string pwdWithSalt = pwd + dbSalt;
                                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                                    string userHash = Convert.ToBase64String(hashWithSalt);
                                    if (userHash.Equals(dbHash))
                                    {
                                        System.Diagnostics.Debug.WriteLine("Wrong Input");
                                        Session["LoggedIn"] = Email_Login.Text.Trim();
                                        string guid = Guid.NewGuid().ToString();
                                        Session["AuthToken"] = guid;
                                        Response.Cookies.Add(new HttpCookie("AuthToken", guid));
                                        Response.Redirect("Userpage.aspx", false);
                                    }
                                    else
                                    {
                                        lbMsg.Text = "Email or password is invalid. Please try again.";
                                        attempt += 1;
                                        int update;
                                        update = UpdateAttempt(userid, attempt);
                                    }

                                }
                            }
                            else if (attempt == 3)
                            {
                                lbMsg.Text = "Your account has been locked out.";
                                int update;
                                update = UpdateAttempt(userid, 0); 
                                setlockouttime(userid, DateTime.Now.AddMinutes(1).ToString());
                            }
                        }
                        else
                        {
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                    finally { }
                }
            }
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

        public bool validateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create
                (" https://www.google.com/recaptcha/api/siteverify?secret=6LdHhiUaAAAAAAU2LzudU3F596RM7viycpUgC5Dq &response=" + captchaResponse);
            //6Lf1e-QZAAAAAKl4rh7eojWK5zTD95W0MOVe3wsk secret v3
            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);

                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }
        
        protected string getAttempt(string email)
        {
            string attempt = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select Attempts FROM Account WHERE Email=@paraEmail";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@paraEmail", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Attempts"] != null)
                        {
                            if (reader["Attempts"] != DBNull.Value)
                            {
                                attempt = reader["Attempts"].ToString();
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
            return attempt;
        }

        protected int UpdateAttempt(string email, int attempt)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "Update Account SET Attempts = @paraattempt WHERE Email=@paraEmail";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@paraEmail", email);
            command.Parameters.AddWithValue("@paraattempt", attempt);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }

        }

        protected int setlockouttime(string username, string time)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "Update Account SET LockedOutTime = @paralockedouttime WHERE Email=@paraEmail";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@paraEmail", username);
            command.Parameters.AddWithValue("@paralockedouttime", time);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
        }

        protected string getlockedouttime(string username)
        {
            string lockedouttime = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select LockedOutTime FROM Account WHERE Email=@paraEmail";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@paraEmail", username);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["LockedOutTime"] != null)
                        {
                            if (reader["LockedOutTime"] != DBNull.Value)
                            {
                                lockedouttime = reader["LockedOutTime"].ToString();
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
            return lockedouttime;
        }


        protected void Register_Click(object sender, EventArgs e)
        {
            Response.Redirect("Registration.aspx");
        }
    }
}