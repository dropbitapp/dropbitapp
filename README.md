# Dropbit Technologies
Dropbit is an open-source distillery management application. Track, audit, and report your monthly operational reports to the TTB with peace of mind.

## Prerequisites
- git clone https://github.com/dropbitapp/dropbitapp.git
- Create an empty SQL Server database
- Add connections.config file to the root of the WebApp project
> 
```
<connectionStrings>
  <add name="DistilDBContext"
       providerName="System.Data.SqlClient" 
       connectionString="Data Source=SERVERNAME;Initial Catalog=DATABASENAME;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False" />
</connectionStrings>
```
- Open solution in Visual Studio
- Run update-database in Package Console Manager
- Create default account
  - Uncomment [AllowAnonymous] attribute for following actions in AccountController.cs
    - POST: /Account/Register
    - GET: /Account/Register
  - Start Debugging and navigate to /Account/Register using your favorite browser
  - Create new user
  - Comment back [AllowAnonymous] attributes for /Account/Register actions
  - Insert into the following database tables SSMS
    - Distiller
    - DistillerDetail
    - AspNetUserToDistiller (map newly created user to distiller)
- Login to as newly created user

## Deploying in Azure

Dropbit was developed for deployment into the Azure cloud. You can deploy these samples directly through the Azure Portal.

To deploy the using the Azure Portal, by ensuring you have an Azure subscription, an SQL Server along with the credentials for master.

Once the service is deployed, ensure you can navigate to the login page and use the credendials you created earlier to login.
