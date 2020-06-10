# Dropbit Technologies
Dropbit is an open-source distillery management application. Track, audit, and report your monthly operational reports to the TTB with peace of mind.

## Prerequisites
- git clone https://github.com/dropbitapp/dropbitapp.git
- Create an empty SQL Server database
- Add connections.config file to the root of the WebApp/WebAppTests projects
> 
```
<connectionStrings>
  <add name="DistilDBContext"
       providerName="System.Data.SqlClient" 
       connectionString="Data Source=SERVERNAME;Initial Catalog=DATABASENAME;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False" />
</connectionStrings>
```
- Open solution in Visual Studio 2015/2017
- Run update-database in Package Console Manager
- Launch the application using the debugger (The application may take short time to initialize during first launch)
- Login with default user credentials
  - Username: admin@dropbit.io
  - Password: P@ssword1!
- Click Run All from the Test Explorer tab to verify the setup
  - NOTE: Due to a regression, 49 tests are currently green and 24 are red

## Deploying in Azure

Dropbit was developed for deployment into the Azure cloud. You can deploy these samples directly through the Azure Portal.

To deploy the using the Azure Portal, by ensuring you have an Azure subscription, an SQL Server along with the credentials for master.

Once the service is deployed, ensure you can navigate to the login page and use the credendials you created earlier to login.
