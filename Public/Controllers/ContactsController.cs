using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using Public.Models;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Public.Data;

namespace Public.Controllers
{
    public class ContactsController : ApiController
    {
        // POST: api/Contacts
        [ResponseType(typeof(ContactDTO))]
        public async Task<IHttpActionResult> PostContact(ContactDTO contact)
        {
            if (!ModelState.IsValid ||
                contact.Name == null ||
                contact.Name.Length > 50 ||
                contact.Name.Length < 1 ||
                contact.Email == null ||
                contact.Email.Length < 1 ||
                contact.Email.Length > 100)
            {
                return BadRequest();
            }

            const string secret = "***REMOVED***";
            WebClient client = new WebClient();
            var reply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, contact.Response));
            var captchaResponse = JsonConvert.DeserializeObject<RecaptchaResponse>(reply);

            if (!captchaResponse.Success)
            {
                return BadRequest();
            }

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("contacts");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // Create a new customer entity
            ContactEntity contactEntity = new ContactEntity(contact.Name, contact.Email);
            contactEntity.Organization = contact.Organization;

            if (contactEntity == null)
            {
                return BadRequest();
            }

            // Create the TableOperation object that inserts the contact entity.
            TableOperation insertOperation = TableOperation.Insert(contactEntity);

            // Execute the insert operation.
            await table.ExecuteAsync(insertOperation);

            return Ok();
        }
    }
}