# dropbitdistill
## Instructions
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
