using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Drawing;

namespace ASAssignment
{
    public partial class Registration : System.Web.UI.Page
    {
        string MYDBConnectionString =
        System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;
        int att = 0;
        string time = "0";

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        private bool checkPassword(string password)
        {
            int score = 0;

            if (password.Length < 8)
            {
                return false;
            }
            else
            {
                score = 1;
            }
            if (Regex.IsMatch(password, "[a-z]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[A-Z]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[ 0-9 ]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[^a-zA-Z0-9 ]"))
            {
                score++;
            }
            if (score == 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool validateinput()
        {

            lblMessage.Text = String.Empty;


            if (Firstname.Text == "")
            {
                lblMessage.Text += "First name cannot be empty! <br/>";
            }
            if (Lastname.Text == "")
            {
                lblMessage.Text += "Last name cannot be empty! <br/>";
            }
            if (Ccno.Text == "")
            {
                lblMessage.Text += "Credit Card Number cannot be empty! <br/>";
            }
            if (Ccno.Text.Length != 16)
            {
                lblMessage.Text += "Credit Card Number is Invalid! <br/>";
            }
            if (Emaila.Text == "")
            {
                lblMessage.Text += "Email address cannot be empty! <br/>";
            }
            if (tb_password.Text == "")
            {
                lblMessage.Text += "Password cannot be empty <br/>";
            }
            if (Confirmedpassword.Text == "")
            {
                lblMessage.Text += "Confirmed password cannot be empty! <br/>";
            }
            if (tb_password.Text != Confirmedpassword.Text)
            {
                lblMessage.Text += "Different password enter for confirm password! <br/>";
            }
            if (Dateob.Text == "")
            {
                lblMessage.Text += "Date cannot be empty!" + "<br/>";
            }
            if (String.IsNullOrEmpty(lblMessage.Text))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //protected string checkemail(string id)
        //{
        //    string accountid = "";
        //    SqlConnection connection = new SqlConnection(MYDBConnectionString);
        //    string sql = "select Id FROM Account WHERE Email=@paraemail";
        //    SqlCommand command = new SqlCommand(sql, connection);
        //    command.Parameters.AddWithValue("@paraemail", id);
        //    try
        //    {
        //        connection.Open();
        //        using (SqlDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                if (reader["Id"] != null)
        //                {
        //                    if (reader["Id"] != DBNull.Value)
        //                    {
        //                        accountid = reader["Id"].ToString();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //    finally { connection.Close(); }
        //    return accountid;
        //}
    
        private string checkemail(string email)
        {
            string a = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select Email from Account where Email=@paraEmail";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@paraEmail", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Email"] != null)
                        {
                            if (reader["Email"] != DBNull.Value)
                            {
                                a = reader["Email"].ToString();
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
            return a;
        }

        public void createAccount()
        {
            try
            {
                    using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO Account VALUES(@paraFname, @paraLname, @paraCcno, @paraEmail, @paraPasswordsalt, @paraPasswordHash, @paraDateOfBirth, @paraIV, @paraKey, @paraAttempts, @paraLockedOutTime)"))
                        {
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                System.Diagnostics.Debug.WriteLine(Firstname.Text.Trim());
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.AddWithValue("@paraFname", Firstname.Text.Trim());
                                cmd.Parameters.AddWithValue("@paraLname", Lastname.Text.Trim());
                                cmd.Parameters.AddWithValue("@paraCcno", encryptData(Ccno.Text.Trim()));
                                cmd.Parameters.AddWithValue("@paraEmail", Emaila.Text.Trim());
                                cmd.Parameters.AddWithValue("@paraPasswordsalt", salt);
                                cmd.Parameters.AddWithValue("@paraPasswordHash", finalHash);
                                cmd.Parameters.AddWithValue("@paraDateOfBirth", Dateob.Text.Trim());
                                cmd.Parameters.AddWithValue("@paraIV", Convert.ToBase64String(IV));
                                cmd.Parameters.AddWithValue("@paraKey", Convert.ToBase64String(Key));
                                cmd.Parameters.AddWithValue("@paraAttempts", att);
                                cmd.Parameters.AddWithValue("@paraLockedOutTime", time);
                                cmd.Connection = con;
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        protected void btn_Submit_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(tb_password.Text);
            System.Diagnostics.Debug.WriteLine(Confirmedpassword.Text);
            string emailid = Emaila.Text.ToString();
            if (validateinput())
            {
                if (checkemail(emailid)==null)
                {
                    if (checkPassword(tb_password.Text))
                    {
                        //string pwd = get value from your Textbox
                        string pwd = tb_password.Text.ToString().Trim(); ;
                        //Generate random "salt"
                        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                        byte[] saltByte = new byte[8];
                        //Fills array of bytes with a cryptographically strong sequence of random values.
                        rng.GetBytes(saltByte);
                        salt = Convert.ToBase64String(saltByte);
                        SHA512Managed hashing = new SHA512Managed();
                        string pwdWithSalt = pwd + salt;
                        byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwd));
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                        finalHash = Convert.ToBase64String(hashWithSalt);
                        RijndaelManaged cipher = new RijndaelManaged();
                        cipher.GenerateKey();
                        Key = cipher.Key;
                        IV = cipher.IV;
                        createAccount();
                        Response.Redirect("Login.aspx");
                    }
                }
                else
                {
                    lblMessage.Text += "Email address has already existed! Please Enter a new one.";
                }
            }
            
        }

        protected void Log_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx");
        }

        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0,
               plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return cipherText;
        }

    }
}