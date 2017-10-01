using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;

namespace GridMvc.Controllers
{
    public class ContactsController : Controller
    {
        //
        // GET: /Contacts/

        public ActionResult Index()
        {
            List<Contact> allContacts = null;
            using (TestBaseEntities dc = new TestBaseEntities())
            {
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         select new
                         {
                             a,
                             b.CountryName,
                             c.StateName
                         });
                if (v != null)
                {
                    allContacts = new List<Contact>();
                    foreach (var i in v)
                    {
                        Contact c = i.a;
                        c.CountryName = i.CountryName;
                        c.StateName = i.StateName;
                        allContacts.Add(c);
                    }
                }
            }
            return View(allContacts);
        }
        //edit and create country / state contact
        private List<Country> GetCountry()
        {
            using (TestBaseEntities dc = new TestBaseEntities())
            {
                return dc.Countries.OrderBy(a => a.CountryName).ToList();
            }
        }

        private List<State> GetState()
        {
            using (TestBaseEntities dc = new TestBaseEntities())
            {
                return dc.States.OrderBy(a => a.StateName).ToList();
            }
        }
        //Write function for return state list of selected country in json format, which we will use for cascade dropdown.
        public JsonResult GetStateList(int countryID)
        {
            using (TestBaseEntities dc = new TestBaseEntities())
            {

                return new JsonResult { Data = GetState(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        //Here I have written the below function "GetContact" into "Contacts" Controller. 
        public Contact GetContact(int contactID)
        {
            Contact contact = null;
            using (TestBaseEntities dc = new TestBaseEntities())
            {
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         where a.ContactID.Equals(contactID)
                         select new
                         {
                             a,
                             b.CountryName,
                             c.StateName
                         }).FirstOrDefault();
                if (v != null)
                {
                    contact = v.a;
                    contact.CountryName = v.CountryName;
                    contact.StateName = v.StateName;
                }
                return contact;
            }
        }
        public ActionResult Save(int id = 0)
        {
            List<Country> country = GetCountry();
            List<State> states = GetState();
            if (id > 0)
            {
                //Update
                var c = GetContact(id);
                if (c != null)
                {
                    ViewBag.Countries = new SelectList(country, "CountryID", "CountryName", c.CountryID);
                    ViewBag.States = new SelectList(GetState(), "StateID", "StateName", c.StateID);
                }
                else
                {
                    return HttpNotFound();
                }
                return View(c);
            }
            else
            {
                //Create 
                ViewBag.Countries = new SelectList(country, "CountryID", "CountryName");
                ViewBag.States = new SelectList(states, "StateID", "StateName");
                return View();
            }
        }


    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(Contact c)
        {
            string message = "";
            bool status = false;

            //Save 
            if (ModelState.IsValid)
            {
                using (TestBaseEntities dc = new TestBaseEntities())
                {
                    if (c.ContactID > 0)
                    {
                        //Update
                        var v = dc.Contacts.Where(a => a.ContactID.Equals(c.ContactID)).FirstOrDefault();
                        if (v != null)
                        {
                            v.ContactPerson = c.ContactPerson;
                            v.ContactNo = c.ContactNo;
                            v.CountryID = c.CountryID;
                            v.StateID = c.StateID;
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                    else
                    {
                        //Add new 
                        dc.Contacts.Add(c);
                    }
                    dc.SaveChanges();
                    status = true;
                    return RedirectToAction("Index");
                }
            }
            else
            {
                message = "Error! Please try again.";
                ViewBag.Countries = new SelectList(GetCountry(), "CountryID", "CountryName", c.CountryID);
                ViewBag.States = new SelectList(GetState(), "StateID", "StateName", c.StateID);
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(c);
        }
    }
}

