using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallWall.Web.Models
{
    public class ContactSummary
    {
        /// <summary>
        /// The title description for the contact. Usually their First and Last name.
        /// </summary>
        public string Title { get; set; }

        public string[] Tags { get; set; }
        public string PrimaryAvatar { get; set; }
    }
}