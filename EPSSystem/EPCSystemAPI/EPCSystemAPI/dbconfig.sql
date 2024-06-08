-- Creating users table
CREATE TABLE [dbo].[users] (
    [id]       INT           IDENTITY (1, 1) NOT NULL,
    [username] VARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);

-- Creating devices table
CREATE TABLE [dbo].[devices] (
    [id]             INT             IDENTITY (1, 1) NOT NULL,
    [userid]         INT             NOT NULL,
    [devicename]     VARCHAR (255)   NULL,
    [deviceType]     VARCHAR (255)   NULL,
    [powerType]      VARCHAR (255)   NULL,
    [emissionFactor] DECIMAL (10, 4) NULL,
    [location]       VARCHAR (255)   NULL,
    [createdat]      DATETIME        NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    FOREIGN KEY ([userid]) REFERENCES [dbo].[users] ([id])
);

-- Creating electricity production table
CREATE TABLE [dbo].[electricity_production] (
    [id]             INT             IDENTITY (1, 1) NOT NULL,
    [productiontime] DATETIME        NOT NULL,
    [amountWh]       DECIMAL (10, 2) NOT NULL,
    [deviceid]       INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    FOREIGN KEY ([deviceid]) REFERENCES [dbo].[devices] ([id])
);

-- Creating Certificates table
CREATE TABLE [dbo].[Certificates] (
    [Id]                      INT             IDENTITY (1, 1) NOT NULL,
    [UserId]                  INT             NOT NULL,
    [ElectricityProductionId] INT             NOT NULL,
    [CreatedAt]               DATETIME        NOT NULL,
    [Volume]                  DECIMAL (18, 2) NOT NULL,
    [CurrentVolume]           DECIMAL (18, 2) NOT NULL,
    [ParentCertificateId]     INT             NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[users] ([id]),
    FOREIGN KEY ([ElectricityProductionId]) REFERENCES [dbo].[electricity_production] ([id]),
    FOREIGN KEY ([ParentCertificateId]) REFERENCES [dbo].[Certificates] ([Id])
);

-- Creating TransferEvents table
CREATE TABLE [dbo].[_TransferEvents] (
    [Id]                        INT          IDENTITY (1, 1) NOT NULL,
    [BundleId]                  INT          NULL,
    [FromUserId]                INT          NOT NULL,
    [ToUserId]                  INT          NOT NULL,
    [Volume]                    DECIMAL (18) NOT NULL,
    [Electricity_production_id] INT          NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Creating produce events table
CREATE TABLE [dbo].[produce_events] (
    [id]                      INT      IDENTITY (1, 1) NOT NULL,
    [deviceid]                INT      NOT NULL,
    [electricityproductionid] INT      NOT NULL,
    [productiontime]          DATETIME NOT NULL,
    [event_id]                INT      NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    FOREIGN KEY ([deviceid]) REFERENCES [dbo].[devices] ([id]),
    FOREIGN KEY ([electricityproductionid]) REFERENCES [dbo].[electricity_production] ([id])
);

-- Creating events table
CREATE TABLE [dbo].[events] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [event_type]   NVARCHAR (50) NOT NULL,
    [reference_id] INT           NOT NULL,
    [user_id]      INT           NOT NULL,
    [timestamp]    DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Creating TradeEvents table
CREATE TABLE [dbo].[TradeEvents] (
    [Id]         INT             IDENTITY (1, 1) NOT NULL,
    [FromUserId] INT             NOT NULL,
    [ToUserId]   INT             NOT NULL,
    [Volume]     DECIMAL (18, 2) NOT NULL,
    [Currency]   VARCHAR (50)    NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Creating TransformEvents table
CREATE TABLE [dbo].[TransformEvents] (
    [Id]                      INT             IDENTITY (1, 1) NOT NULL,
    [UserId]                  INT             NOT NULL,
    [BundleId]                INT             NULL,
    [TransformedVolume]       DECIMAL (18, 2) NOT NULL,
    [TransformationTimestamp] DATETIME        NOT NULL,
    [RootCertificateId]       INT             NOT NULL,
    [NewCertificateId]        INT             NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([RootCertificateId]) REFERENCES [dbo].[Certificates] ([Id]),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[users] ([id])
);

-- Creating UserBalanceView
CREATE VIEW UserBalanceView AS
SELECT
    UserId,
    ElectricityProductionId,
    SUM(CurrentVolume) AS Balance
FROM
    Certificates
GROUP BY
    UserId,
    ElectricityProductionId;

-- Creating UserEventView
CREATE VIEW UserEventView AS
SELECT 
    e.Id AS EventId,
    e.event_type AS EventType,
    e.reference_id AS ReferenceId,
    e.user_id AS UserId,
    e.timestamp AS Timestamp,
    CASE 
        WHEN e.event_type = 'PRODUCTION' THEN ep.amountWh 
        WHEN e.event_type = 'TRANSFER' AND t.FromUserId = e.user_id THEN -t.Volume 
        WHEN e.event_type = 'TRANSFER' AND t.ToUserId = e.user_id THEN t.Volume 
    END AS Value,
    ep.id AS ElectricityProductionId,
    t.Id AS CertificateId
FROM 
    events e
LEFT JOIN 
    produce_events p ON e.reference_id = p.id AND e.event_type = 'PRODUCTION'
LEFT JOIN 
    electricity_production ep ON p.electricityproductionid = ep.id
LEFT JOIN 
    _TransferEvents t ON e.reference_id = t.Id AND e.event_type = 'TRANSFER'
UNION ALL
SELECT 
    e.Id AS EventId,
    e.event_type AS EventType,
    e.reference_id AS ReferenceId,
    t.ToUserId AS UserId,
    e.timestamp AS Timestamp,
    t.Volume AS Value,
    NULL AS ElectricityProductionId,
    t.Id AS CertificateId
FROM 
    events e
LEFT JOIN 
    _TransferEvents t ON e.reference_id = t.Id AND e.event_type = 'TRANSFER'
WHERE
    e.event_type = 'TRANSFER';