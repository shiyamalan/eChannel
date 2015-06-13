﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eChannel.Models;
namespace eChannel.Controllers
{
    public class PatientController : Controller
    {

        public ActionResult Dashboard()
        {
            if (Session["isDoctor"] != null && (bool)Session["isDoctor"])
            {
                return RedirectToAction("Dashboard", "Doctor");
            }
            else if (Session["isDoctor"] == null)
            {
                return RedirectToAction("LoginPatient", "User");
            }
            return View();
        }

        public ActionResult Settings()
        {
            if (Session["isDoctor"] != null && (bool)Session["isDoctor"])
            {
                return RedirectToAction("Dashboard", "Doctor");
            }
            else if (Session["isDoctor"] == null)
            {
                return RedirectToAction("LoginPatient", "User");
            }

            if (Request.HttpMethod.Equals("POST"))
            {
                Patient OldExisting = DBContext.GetInstance().FindOneInPatient("patient_id", Session["userID"].ToString());

                OldExisting.FirstName = Request.Form["firstName"];
                OldExisting.LastName = Request.Form["lastName"];
                OldExisting.PhoneNumber = Request.Form["phone"];
                OldExisting.Birthdate = Request.Form["birthdate"].Equals("") ? new DateTime(0001, 01, 01) : Convert.ToDateTime(Request.Form["birthdate"]);
                OldExisting.Gender = Request.Form["gender"];
                HttpPostedFileBase file = Request.Files["picture"];

                if (file != null && file.ContentLength > 0)
                {
                    System.IO.Stream fileStream = file.InputStream;
                    byte[] data = new byte[file.ContentLength];
                    fileStream.Read(data, 0, data.Length);
                    fileStream.Close();
                    OldExisting.Picture = data;
                }
                DBContext.GetInstance().UpdatePatient(OldExisting);
                Session["picture"] = "data:image/png;base64," + Convert.ToBase64String(OldExisting.Picture);
                ViewData["success"] = 1;
            }

            Patient existing = DBContext.GetInstance().FindOneInPatient("patient_id", Session["userID"].ToString());
            ViewData["patient"] = existing;
            return View();
        }

        public PartialViewResult AddChannel()
        {

            if (Request.HttpMethod.Equals("POST"))
            {
                Channel newChannel = new Channel()
                {
                    WorkID = Convert.ToInt32(Request.Form["work_id"]),
                    PatientID = (int)Session["userID"],
                    SpecID = Convert.ToInt32(Request.Form["spec_id"]),
                    ServiceID = Convert.ToInt32(Request.Form["service_id"]),
                    ChannelNumber = (DBContext.GetInstance().FindOneInDoctorSchedule(Convert.ToInt32(Request.Form["work_id"])).PatientApplied) + 1,
                    Reason = Convert.ToString(Request.Form["reason"]),
                    ChannelRating = 0,
                    ChannelComments = ""
                };
                DBContext.GetInstance().CreateChannel(newChannel);
                ViewData["success"] = 1;

            }

            return PartialView();
        }

        public PartialViewResult MyChannels()
        {
            List<PatientChannel> patientChannels = DBContext.GetInstance().FindAllInPatientChannel((int)Session["userID"]);
            ViewData["patient_channels"] = patientChannels;
            return PartialView();
        }

        public ActionResult GetChannel(string channelID)
        {
            if (Request.HttpMethod.Equals("POST"))
            {
                int postChannelID = Convert.ToInt32(Request.Form["channel_id"]);
                int rating = Convert.ToInt32(Request.Form["rating"]);
                string comment = Request.Form["comment"];
                Channel channel = DBContext.GetInstance().FindOneInChannel("channel_id", postChannelID.ToString());
                channel.ChannelRating = rating;
                channel.ChannelComments = comment;
                DBContext.GetInstance().UpdateChannel(channel);
                return null;
            }
            else
            {
                Channel channel = DBContext.GetInstance().FindOneInChannel("channel_id", channelID);
                return Json(channel, JsonRequestBehavior.AllowGet);
            }

            
        }
    }
}