# Resource Stack - [Background Job Service](https://msazure.visualstudio.com/One/_wiki/wikis/One.wiki/20671/Resource-Stack-(BJS))

<!-- TOC depthfrom:2 -->

- [Resource Stack - Background Job Service](#resource-stack---background-job-service)
  - [What is BJS?](#what-is-bjs)
    - [Features](#features)
  - [Contents of this example](#contents-of-this-example)
  - [Caveats](#caveats)
  - [Links](#links)
  - [Pre-reqs](#pre-reqs)
  - [How to run this tutorial](#how-to-run-this-tutorial)
  - [Exploring different state-stores](#exploring-different-state-stores)
    - [1. Azure Storage](#1-azure-storage)
    - [2. Cosmos DB](#2-cosmos-db)
    - [3. SQL Server](#3-sql-server)
      - [Full Admin](#full-admin)
      - [Least privilege](#least-privilege)
      - [SQL Cleanup](#sql-cleanup)
    - [4. In-Memory](#4-in-memory)

<!-- /TOC -->
## What is BJS?
BJS is a battle tested framework to schedule and run jobs. Originating from AzureUX-ResourceStack It's used by the PowerApps to run their large amount of background jobs.

### Features

- Job scheduling
- Job sequencing using DAGs
- Repeat logic for high flexibility on scheduling
- Multiple options for retry strategies (different back-off strategies, max retries, etc.)
- Support for checkpoint and continuation
- High scalability
- Simple interface
- Does not require any additional processes for queue grooming or consistency checks

## Contents of this example

This example builds two executables.

| Name       | Description                                                                        |
| ---------- | ---------------------------------------------------------------------------------- |
| BJS-Admin  | Schedule and query various types of jobs.                                          |
| BJS-Worker | Runtime environment for jobs, also contains system job to ensure data consistency. |

Another component needed is CosmosDB, for testing the local emulator is sufficient.

```mermaid
flowchart LR
    Admin --> |Schedule new jobs| CosmosDB
    Admin --> |Retrieve status of jobs| CosmosDB
    Worker --> |Check and run jobs| CosmosDB
    Worker --> |Schedule system jobs| CosmosDB
    CosmosDB[(fa:fa-database CosmosDB)]
    Worker(fa:fa-cog Worker)
    Admin(fa:fa-play Admin)
```

## Caveats

1. BJS requires a specific consistency configuration in CosmosDB.
    > ### Cosmos DB consistency model
    > BJS depends on Read-Your-Writes consistency model to function properly. By default, Cosmos DB account is configured with weaker Session consistency. Please reconfigure this to at least Bounded-Staleness. Failure to update Cosmos DB consistency setting will result in failures and intermittent job execution delays.
2. BJS is a DotNet library, no other languages are supported.

## Links
- [BJS One.wiki](https://msazure.visualstudio.com/One/_wiki/wikis/One.wiki/20671/Resource-Stack-(BJS))
- [BJS OneBranch](https://msazure.visualstudio.com/DefaultCollection/One/_git/AzureUX-ResourceStack?path=/src/common/jobs&version=GBmaster)
- [BJS in PowerApps](https://eng.ms/docs/cloud-ai-platform/business-applications-and-platform/bap-dataverse/dv-infrastructure/coreservices-microservices-infrastructure/powerapps-coreservices-wiki/librariesandsdks/backgroundjobservicebjs/bjsincoreframework)

## Pre-reqs

- Visual Studio Community
- Intall Cosmos DB Emulator on a Windows machine [from here](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21#download-the-emulator)

## How to run this tutorial

1. Run the Cosmos DB Emulator - browsable at `https://localhost:8081/_explorer/index.html`
   
   ![Cosmos DB key](_imgs/cosmos-db.png)

2. Put the Primary Key into `App.config` of both `BJS-Admin` and `BJS-Job` - so both can connect:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="documentServiceEndpoint" value="https://localhost:8081"/>
    <add key="documentAuthorizationKey" value="C2y6yDj...=="/>
  </appSettings>
</configuration>
```

3. Run both apps in seperate windows:

    ![Run Both](_imgs/run-both.png)

4. `BJS-Admin` spins up `Jobs`, `BJS-Job` executes them:

    ![Both are running](_imgs/both-running.png)

5. Cosmos DB will store execution state:

    ![Job State](_imgs/job-state.png)

    ![Job Trigger](_imgs/job-trigger.png)

6. All Jobs are done:

    ![Work done](_imgs/work-done.png)
   

## Exploring different state-stores
### 1. Azure Storage

`TODO`

---

### 2. Cosmos DB

See above.

---

### 3. SQL Server

#### Full Admin

Set this in the `App.config`
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="sqlServerConnectionString" value="Server=localhost;Database=msdb;Trusted_Connection=True;" />
  </appSettings>
</configuration>
```

#### Least privilege

> You must enable [mixed-mode](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/change-server-authentication-mode?view=sql-server-ver16) for the below demo to work

The script below contains the add and removal permissions:

```sql
----------------------------
-- ADD PERMISSIONS AND LOGIN
----------------------------

USE [master];
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'bjs')
BEGIN
	-- Create server login
	USE [master]; CREATE LOGIN bjs WITH PASSWORD = 'password123!';

	-- Create database user
	USE [msdb]; CREATE USER bjs FOR LOGIN bjs;

	-- Grant database roles
	USE [msdb]; GRANT CREATE TABLE TO bjs;
	USE [msdb]; GRANT CONTROL, EXECUTE, ALTER ANY SCHEMA TO bjs;
	USE [msdb]; GRANT CREATE TYPE TO bjs;
	USE [msdb]; ALTER ROLE db_datareader ADD MEMBER bjs;
	USE [msdb]; ALTER ROLE db_datawriter ADD MEMBER bjs;

END
GO

---------------------
-- REMOVE PERMISSIONS
---------------------

USE [msdb];
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'bjs')
BEGIN

	-- Drop database roles
	USE [msdb]; DENY CREATE TABLE TO bjs;
	USE [msdb]; DENY CONTROL, EXECUTE, ALTER ANY SCHEMA TO bjs;
	USE [msdb]; DENY CREATE TYPE TO bjs;
	USE [msdb]; ALTER ROLE db_datareader DROP MEMBER bjs;
	USE [msdb]; ALTER ROLE db_datawriter DROP MEMBER bjs;
	
	-- Drop database user
	USE [msdb]; DROP USER bjs;
END

---------------
-- REMOVE LOGIN
---------------
USE [master];
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'bjs')
BEGIN
	-- Kill connections
	DECLARE @SessionID INT;
	DECLARE @SQL NVARCHAR(MAX);

	-- Create a temporary table to store session IDs
	USE master; CREATE TABLE #ActiveSessions (SessionID INT);

	-- Insert active session IDs into the temporary table
	USE master; INSERT INTO #ActiveSessions (SessionID) SELECT session_id FROM sys.dm_exec_sessions WHERE login_name = 'bjs';

	-- Loop through and kill each active session
	WHILE EXISTS (SELECT 1 FROM #ActiveSessions)
	BEGIN
		USE master;
		SELECT TOP 1 @SessionID = SessionID FROM #ActiveSessions;
		SET @SQL = 'KILL ' + CAST(@SessionID AS NVARCHAR(10));
		EXEC sp_executesql @SQL;
		DELETE FROM #ActiveSessions WHERE SessionID = @SessionID;
	END

	-- Drop the temporary table
	USE master; DROP TABLE #ActiveSessions;

	-- Drop the login
	USE master; DROP LOGIN bjs;
END
GO
```

Update `App.config`:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="sqlServerConnectionString" value="Server=localhost;Database=msdb;User Id=bjs;Password=password123!" />
  </appSettings>
</configuration>
```


#### SQL Cleanup

And, cleanup logic for tables generated:

```sql
USE msdb;

-----------------------
-- VIEW
-----------------------

-- Only run SELECT if table exists.
IF EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dt' 
    AND TABLE_NAME = 'arcJobDefinitions'
)
BEGIN
	-- SELECT specific columns
    SELECT RowKey
		,TIMESTAMP
		,JobPartition
		,JobId
		,Callback
		,CurrentLocation
		,STATE
		,ExecutionState
		,StartTime
		,RetryCount
		,Timeout
	FROM dt.arcJobDefinitions
	WHERE JobPartition = 'MY:5FOLD:5FSQL2019';
END

-----------------------
-- CLEANUP
-----------------------

-- Tables -------------

DECLARE @TableName NVARCHAR(255);
DECLARE @SchemaName NVARCHAR(255);
DECLARE @DropSQL NVARCHAR(MAX);

DECLARE TableCursor CURSOR FOR
SELECT TABLE_NAME, TABLE_SCHEMA
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND (TABLE_SCHEMA = 'dq' OR TABLE_SCHEMA = 'dt')
  ORDER BY TABLE_NAME ASC;

OPEN TableCursor;

FETCH NEXT FROM TableCursor INTO @TableName, @SchemaName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @DropSQL = 'DROP TABLE [' + @SchemaName + '].[' + @TableName + '];';
    EXEC sp_executesql @DropSQL;
	PRINT 'Table [' + @SchemaName + '].[' + @TableName + '];' + 'dropped successfully.';

    FETCH NEXT FROM TableCursor INTO @TableName, @SchemaName;
END;

CLOSE TableCursor;
DEALLOCATE TableCursor;

-- Types -------------

IF EXISTS (
    SELECT 1
    FROM sys.types
    WHERE name = 'arcJobDefinitionsOperationType'
)
BEGIN
    -- Drop the type if it exists
    DROP TYPE [dt].[arcJobDefinitionsOperationType];
    PRINT 'Type "arcJobDefinitionsOperationType" dropped successfully.';
END

-----------------------
-- VERIFY
-----------------------
SELECT * FROM msdb.sys.objects
WHERE is_ms_shipped=0
```


---

### 4. In-Memory

`TODO`
