using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASAssignment
{
    public partial class Userpage : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    Response.Redirect("Login.aspx", false);
                }
                else
                {
                    lblMessage.Text = "Congratulations ! You have logged in sucessfully!";
                    lblMessage.ForeColor = System.Drawing.Color.White;
                    BtnLogout.Visible = true;
                }
            }
            else
            {
                Response.Redirect("Login.aspx");
            }

        }

        protected void ChangePassword(object sender, EventArgs e)
        {
            string guid = Session["AuthToken"].ToString();
            Response.Cookies.Add(new HttpCookie("AuthToken", guid));
            Response.Redirect("ChangePassword.aspx", false);
        }

            protected void LogoutMe(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            Response.Redirect("~/Login", false);

            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }
            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }
        }
    }
}