﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WM.Register.MD5;
using WM.Register.Models;

namespace WM.Register.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        //[Authorize]
        public ActionResult Login(srv_VNOGateWay_Merchant srv_VNOGateWay_Merchant, string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    var logined = new RegisterDbContext().Database.SqlQuery<srv_VNOGateWay_Merchant>("exec [dbo].[Login] @email, @Password", new SqlParameter("email", srv_VNOGateWay_Merchant.merchant_email), new SqlParameter("Password", Common.MD5Hash(srv_VNOGateWay_Merchant.password))).FirstOrDefault();
                    if (logined != null)
                    {
                        FormsAuthentication.SetAuthCookie(srv_VNOGateWay_Merchant.merchant_email, false);
                       
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, srv_VNOGateWay_Merchant.merchant_email, DateTime.Now, DateTime.Now.AddMinutes(30), false, JsonConvert.SerializeObject(logined));
                        string encryptForm = FormsAuthentication.Encrypt(ticket);
                        HttpCookie httpCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptForm);
                        Response.Cookies.Add(httpCookie);
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return Json(srv_VNOGateWay_Merchant, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
                //return Json(srv_VNOGateWay_Merchant, JsonRequestBehavior.AllowGet);
            }
            return Json(srv_VNOGateWay_Merchant, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Login");
            }
            else
            {
                return View();
            }
        }
        public ActionResult ChangePass()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult ChangePass(string newPass, string oldPass/*, srv_VNOGateWay_Merchant srv_VNOGateWay_Merchant*/)
        {          
            if (User.Identity.IsAuthenticated)
            {
                if(Common.MD5Hash(oldPass) == ((User as WebIPrincipal).User.password))
                {
                    var change = new RegisterDbContext().Database.SqlQuery<srv_VNOGateWay_Merchant>("exec [dbo].[ChangePasswordMerchant] @id, @NewPassword, @OldPass", new SqlParameter("id", (User as WebIPrincipal).User.id), new SqlParameter("NewPassword", Common.MD5Hash(newPass)), new SqlParameter("OldPass", Common.MD5Hash(oldPass))).FirstOrDefault();
                    FormsAuthentication.SignOut();
                    return Json(new { result = 1 });
                }
                else
                {
                    return Json("Wrong Password", JsonRequestBehavior.AllowGet);
                }
                
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }

        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(srv_VNOGateWay_Merchant model)
        {
            Guid.NewGuid();
            return View(model);
        }
    }
}