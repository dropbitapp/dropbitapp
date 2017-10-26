using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Public.Models
{
    public class ContactEntity : TableEntity
    {
        public ContactEntity(string name, string email)
        {
            this.PartitionKey = name;
            this.RowKey = email;
        }

        public ContactEntity() { }
        
        public string Organization { get; set; }
    }
}