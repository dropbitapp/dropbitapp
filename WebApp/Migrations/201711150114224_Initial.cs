namespace WebApp.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Alcohol",
                c => new
                {
                    AlcoholID = c.Int(nullable: false, identity: true),
                    Value = c.Single(nullable: false),
                })
                .PrimaryKey(t => t.AlcoholID);

            CreateTable(
                "dbo.AspNetUserToDistiller",
                c => new
                {
                    DistillerID = c.Int(nullable: false),
                    AspNetUserToDistillerID = c.Int(nullable: false, identity: true),
                    UserId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.AspNetUserToDistillerID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.Distiller",
                c => new
                {
                    DistillerID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.DistillerID);

            CreateTable(
                "dbo.BlendedComponent",
                c => new
                {
                    BlendedComponentID = c.Int(nullable: false, identity: true),
                    ProductionID = c.Int(nullable: false),
                    RecordId = c.Int(nullable: false),
                    Quantity = c.Single(nullable: false),
                    UnitOfMeasurement = c.String(),
                })
                .PrimaryKey(t => t.BlendedComponentID)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID);

            CreateTable(
                "dbo.Production",
                c => new
                {
                    ProductionID = c.Int(nullable: false, identity: true),
                    ProductionName = c.String(),
                    ProductionDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    ProductionStartTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    ProductionEndTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    VolumeID = c.Int(nullable: false),
                    WeightID = c.Int(nullable: false),
                    AlcoholID = c.Int(nullable: false),
                    ProofID = c.Int(nullable: false),
                    ProductionTypeID = c.Int(nullable: false),
                    StatusID = c.Int(nullable: false),
                    StateID = c.Int(nullable: false),
                    Note = c.String(maxLength: 1024),
                    DistillerID = c.Int(nullable: false),
                    BurningDownMethod = c.String(),
                    Gauged = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.ProductionID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.BlendedComponentHist",
                c => new
                {
                    BlendedComponentHistID = c.Int(nullable: false, identity: true),
                    ProductionID = c.Int(nullable: false),
                    FieldName = c.String(),
                    FieldValue = c.String(),
                    UpdateDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                })
                .PrimaryKey(t => t.BlendedComponentHistID);

            CreateTable(
                "dbo.BottlingInfo",
                c => new
                {
                    BottlingInfoID = c.Int(nullable: false, identity: true),
                    ProductionID = c.Int(nullable: false),
                    CaseCapacity = c.Int(nullable: false),
                    BottleVolume = c.Single(nullable: false),
                    CaseQuantity = c.Single(nullable: false),
                    BottleQuantity = c.Single(nullable: false),
                })
                .PrimaryKey(t => t.BottlingInfoID);

            CreateTable(
                "dbo.BottlingInfoHist",
                c => new
                {
                    BottlingInfoHistID = c.Int(nullable: false, identity: true),
                    ProductionID = c.Int(nullable: false),
                    FieldName = c.String(),
                    FieldValue = c.String(),
                    UpdateDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                })
                .PrimaryKey(t => t.BottlingInfoHistID);

            CreateTable(
                "dbo.ContentField",
                c => new
                {
                    ContentFieldID = c.Int(nullable: false, identity: true),
                    ContentFieldName = c.String(),
                })
                .PrimaryKey(t => t.ContentFieldID);

            CreateTable(
                "dbo.Destruction",
                c => new
                {
                    DestructionID = c.Int(nullable: false, identity: true),
                    RecordID = c.Int(nullable: false),
                    WorkflowType = c.String(nullable: false),
                    RepresentativeName = c.String(),
                    StartTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    EndTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    DestructionMethod = c.String(nullable: false),
                    Withdrawn = c.Boolean(nullable: false),
                    Volume = c.Single(nullable: false),
                    Weight = c.Single(nullable: false),
                    AlcoholContent = c.Single(nullable: false),
                    ProofGallons = c.Single(nullable: false),
                    Note = c.String(maxLength: 1024),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                })
                .PrimaryKey(t => t.DestructionID);

            CreateTable(
                "dbo.DistillerDetail",
                c => new
                {
                    DistillerDetailID = c.Int(nullable: false, identity: true),
                    DistillerID = c.Int(nullable: false),
                    EIN = c.String(),
                    DSP = c.String(),
                    StreetAddress = c.String(),
                    City = c.String(),
                    Zip = c.String(),
                    State = c.String(),
                    Phone = c.String(),
                    Email = c.String(),
                    Note = c.String(maxLength: 1024),
                })
                .PrimaryKey(t => t.DistillerDetailID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.F2H",
                c => new
                {
                    F2HID = c.Int(nullable: false, identity: true),
                    FName = c.String(),
                })
                .PrimaryKey(t => t.F2HID);

            CreateTable(
                "dbo.FillTest",
                c => new
                {
                    ProductionID = c.Int(nullable: false),
                    FillTestID = c.Int(nullable: false, identity: true),
                    AlcoholContent = c.Single(nullable: false),
                    FillTestDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    FillVariation = c.Single(nullable: false),
                    CorrectiveAction = c.String(),
                })
                .PrimaryKey(t => t.FillTestID)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID);

            CreateTable(
                "dbo.GainLoss",
                c => new
                {
                    GainLossID = c.Int(nullable: false, identity: true),
                    Type = c.Boolean(nullable: false),
                    Quantity = c.Single(nullable: false),
                    DateRecorded = c.DateTime(nullable: false),
                    ProductionId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.GainLossID);

            CreateTable(
                "dbo.GaugeSerial",
                c => new
                {
                    GaugeSerialID = c.Int(nullable: false, identity: true),
                    Serial = c.String(nullable: false),
                    RecordID = c.Int(nullable: false),
                    RecordType = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.GaugeSerialID);

            CreateTable(
                "dbo.MaterialDict",
                c => new
                {
                    MaterialDictID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    UnitOfMeasurementID = c.Int(nullable: false),
                    Note = c.String(maxLength: 1024),
                    DistillerID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.MaterialDictID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.MaterialDict2MaterialCategory",
                c => new
                {
                    MaterialDictID = c.Int(nullable: false),
                    ProductionReportMaterialCategoryID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.MaterialDictID)
                .ForeignKey("dbo.MaterialDict", t => t.MaterialDictID)
                .ForeignKey("dbo.ProductionReportMaterialCategory", t => t.ProductionReportMaterialCategoryID, cascadeDelete: true)
                .Index(t => t.MaterialDictID)
                .Index(t => t.ProductionReportMaterialCategoryID);

            CreateTable(
                "dbo.ProductionReportMaterialCategory",
                c => new
                {
                    ProductionReportMaterialCategoryID = c.Int(nullable: false, identity: true),
                    MaterialCategoryName = c.String(),
                })
                .PrimaryKey(t => t.ProductionReportMaterialCategoryID);

            CreateTable(
                "dbo.MaterialKindReporting",
                c => new
                {
                    DistillerID = c.Int(nullable: false),
                    MaterialKindReportingID = c.Int(nullable: false, identity: true),
                    MaterialKindName = c.String(),
                })
                .PrimaryKey(t => t.MaterialKindReportingID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.MaterialType",
                c => new
                {
                    MaterialTypeID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    MaterialDictID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.MaterialTypeID);

            CreateTable(
                "dbo.ProcessingReportType",
                c => new
                {
                    ProcessingReportTypeID = c.Int(nullable: false, identity: true),
                    ProcessingReportTypeName = c.String(),
                })
                .PrimaryKey(t => t.ProcessingReportTypeID);

            CreateTable(
                "dbo.ProdRepMatCat2MaterialKind",
                c => new
                {
                    MaterialKindReportingID = c.Int(nullable: false),
                    ProductionReportMaterialCategoryID = c.Int(nullable: false),
                    ProdRepMatCat2MaterialKindID = c.Int(nullable: false, identity: true),
                })
                .PrimaryKey(t => t.ProdRepMatCat2MaterialKindID)
                .ForeignKey("dbo.MaterialKindReporting", t => t.MaterialKindReportingID, cascadeDelete: true)
                .ForeignKey("dbo.ProductionReportMaterialCategory", t => t.ProductionReportMaterialCategoryID, cascadeDelete: true)
                .Index(t => t.MaterialKindReportingID)
                .Index(t => t.ProductionReportMaterialCategoryID);

            CreateTable(
                "dbo.Production4Reporting",
                c => new
                {
                    ProductionID = c.Int(nullable: false),
                    Production4ReportingID = c.Int(nullable: false, identity: true),
                    Weight = c.Single(nullable: false),
                    Volume = c.Single(nullable: false),
                    Proof = c.Single(nullable: false),
                    Alcohol = c.Single(nullable: false),
                    Redistilled = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Production4ReportingID)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID);

            CreateTable(
                "dbo.ProductionContent",
                c => new
                {
                    ProductionID = c.Int(nullable: false),
                    ContentFieldID = c.Int(nullable: false),
                    ProductionContentID = c.Int(nullable: false, identity: true),
                    RecordID = c.Int(nullable: false),
                    ContentValue = c.Single(nullable: false),
                    isProductionComponent = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.ProductionContentID)
                .ForeignKey("dbo.ContentField", t => t.ContentFieldID, cascadeDelete: true)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID)
                .Index(t => t.ContentFieldID);

            CreateTable(
                "dbo.ProductionHist",
                c => new
                {
                    ProductionHistID = c.Int(nullable: false, identity: true),
                    ProductionID = c.Int(nullable: false),
                    F2HID = c.Int(nullable: false),
                    F2HValue = c.String(),
                    UpdateDate = c.DateTime(nullable: false),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                })
                .PrimaryKey(t => t.ProductionHistID);

            CreateTable(
                "dbo.ProductionHistory",
                c => new
                {
                    ProductionHistoryID = c.Int(nullable: false, identity: true),
                    ProductionID = c.Int(nullable: false),
                    UpdateDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    ProductionName = c.String(),
                    ProductionStartTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    ProductionEndTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    Volume = c.Single(nullable: false),
                    Weight = c.Single(nullable: false),
                    Alcohol = c.Single(nullable: false),
                    Proof = c.Single(nullable: false),
                    Status = c.String(),
                    State = c.String(),
                    Note = c.String(maxLength: 1024),
                    UserID = c.Int(nullable: false),
                    Gauged = c.Boolean(nullable: false),
                    Storage = c.String(),
                    BurningDownMethod = c.String(),
                    UsedMats = c.String(),
                    SpiritCutName = c.String(),
                    BlendingAdditives = c.String(),
                    BottlingInfo = c.String(),
                    SpiritTypeReportingID = c.Int(nullable: false),
                    MaterialKindReportingID = c.Int(nullable: false),
                    TaxedProof = c.Single(nullable: false),
                    WithdrawalDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                })
                .PrimaryKey(t => t.ProductionHistoryID);

            CreateTable(
                "dbo.ProductionToPurchase",
                c => new
                {
                    ProductionToPurchaseID = c.Int(nullable: false, identity: true),
                    PurchaseID = c.Int(nullable: false),
                    ProductionID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ProductionToPurchaseID);

            CreateTable(
                "dbo.ProductionToSpirit",
                c => new
                {
                    ProductionToSpiritID = c.Int(nullable: false, identity: true),
                    SpiritID = c.Int(nullable: false),
                    ProductionID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ProductionToSpiritID)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID);

            CreateTable(
                "dbo.ProductionToSpiritCut",
                c => new
                {
                    ProductionToSpiritCutID = c.Int(nullable: false, identity: true),
                    SpiritCutID = c.Int(nullable: false),
                    ProductionID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ProductionToSpiritCutID)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID);

            CreateTable(
                "dbo.ProductionToSpiritTypeReporting",
                c => new
                {
                    ProductionID = c.Int(nullable: false),
                    SpiritTypeReportingID = c.Int(nullable: false),
                    MaterialKindReportingID = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.ProductionID, t.SpiritTypeReportingID })
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .ForeignKey("dbo.SpiritTypeReporting", t => t.SpiritTypeReportingID, cascadeDelete: true)
                .Index(t => t.ProductionID)
                .Index(t => t.SpiritTypeReportingID);

            CreateTable(
                "dbo.SpiritTypeReporting",
                c => new
                {
                    SpiritTypeReportingID = c.Int(nullable: false, identity: true),
                    ProductTypeName = c.String(),
                    ProductTypeFullName = c.String(),
                })
                .PrimaryKey(t => t.SpiritTypeReportingID);

            CreateTable(
                "dbo.ProductionType",
                c => new
                {
                    ProductionTypeID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.ProductionTypeID);

            CreateTable(
                "dbo.Proof",
                c => new
                {
                    ProofID = c.Int(nullable: false, identity: true),
                    Value = c.Single(nullable: false),
                })
                .PrimaryKey(t => t.ProofID);

            CreateTable(
                "dbo.Purchase",
                c => new
                {
                    PurchaseID = c.Int(nullable: false, identity: true),
                    PurchaseName = c.String(),
                    PurchaseTypeID = c.Int(nullable: false),
                    MaterialDictID = c.Int(nullable: false),
                    Price = c.Single(nullable: false),
                    VendorID = c.Int(nullable: false),
                    VolumeID = c.Int(nullable: false),
                    WeightID = c.Int(nullable: false),
                    AlcoholID = c.Int(nullable: false),
                    ProofID = c.Int(nullable: false),
                    PurchaseDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    Note = c.String(maxLength: 1024),
                    StateID = c.Int(nullable: false),
                    StatusID = c.Int(nullable: false),
                    DistillerID = c.Int(nullable: false),
                    BurningDownMethod = c.String(),
                    Gauged = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.PurchaseID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .ForeignKey("dbo.State", t => t.StateID, cascadeDelete: true)
                .ForeignKey("dbo.Status", t => t.StatusID, cascadeDelete: true)
                .Index(t => t.StateID)
                .Index(t => t.StatusID)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.State",
                c => new
                {
                    StateID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.StateID);

            CreateTable(
                "dbo.Status",
                c => new
                {
                    StatusID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.StatusID);

            CreateTable(
                "dbo.Purchase4Reporting",
                c => new
                {
                    PurchaseID = c.Int(nullable: false),
                    Purchase4ReportingID = c.Int(nullable: false, identity: true),
                    Weight = c.Single(nullable: false),
                    Volume = c.Single(nullable: false),
                    Proof = c.Single(nullable: false),
                    Alcohol = c.Single(nullable: false),
                    Redistilled = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Purchase4ReportingID)
                .ForeignKey("dbo.Purchase", t => t.PurchaseID, cascadeDelete: true)
                .Index(t => t.PurchaseID);

            CreateTable(
                "dbo.PurchaseHist",
                c => new
                {
                    PurchaseHistID = c.Int(nullable: false, identity: true),
                    PurchaseID = c.Int(nullable: false),
                    F2HID = c.Int(nullable: false),
                    F2HValue = c.String(),
                    UpdateDate = c.DateTime(nullable: false),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                })
                .PrimaryKey(t => t.PurchaseHistID);

            CreateTable(
                "dbo.PurchaseHistory",
                c => new
                {
                    PurchaseHistoryID = c.Int(nullable: false, identity: true),
                    PurchaseID = c.Int(nullable: false),
                    PurchaseName = c.String(),
                    Price = c.Single(nullable: false),
                    Vendor = c.String(),
                    Volume = c.Single(nullable: false),
                    Weight = c.Single(nullable: false),
                    Alcohol = c.Single(nullable: false),
                    Proof = c.Single(nullable: false),
                    PurchaseDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    Note = c.String(maxLength: 1024),
                    State = c.String(),
                    Status = c.String(),
                    Gauged = c.Boolean(nullable: false),
                    UserID = c.Int(nullable: false),
                    UpdateDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    Storage = c.String(),
                    RecordName = c.String(),
                    SpiritTypeReportingID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.PurchaseHistoryID);

            CreateTable(
                "dbo.PurchaseToSpiritTypeReporting",
                c => new
                {
                    PurchaseID = c.Int(nullable: false),
                    SpiritTypeReportingID = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.PurchaseID, t.SpiritTypeReportingID })
                .ForeignKey("dbo.Purchase", t => t.PurchaseID, cascadeDelete: true)
                .ForeignKey("dbo.SpiritTypeReporting", t => t.SpiritTypeReportingID, cascadeDelete: true)
                .Index(t => t.PurchaseID)
                .Index(t => t.SpiritTypeReportingID);

            CreateTable(
                "dbo.PurchaseType",
                c => new
                {
                    PurchaseTypeID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.PurchaseTypeID);

            CreateTable(
                "dbo.Spirit",
                c => new
                {
                    SpiritID = c.Int(nullable: false, identity: true),
                    DistillerID = c.Int(nullable: false),
                    Name = c.String(nullable: false),
                    Note = c.String(maxLength: 1024),
                    ProcessingReportTypeID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.SpiritID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.SpiritCut",
                c => new
                {
                    SpiritCutID = c.Int(nullable: false, identity: true),
                    DistillerId = c.Int(nullable: false),
                    Name = c.String(nullable: false),
                })
                .PrimaryKey(t => t.SpiritCutID)
                .ForeignKey("dbo.Distiller", t => t.DistillerId, cascadeDelete: true)
                .Index(t => t.DistillerId);

            CreateTable(
                "dbo.SpiritType2MaterialKindReporting",
                c => new
                {
                    SpiritTypeReportingID = c.Int(nullable: false),
                    MaterialKindReportingID = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.SpiritTypeReportingID, t.MaterialKindReportingID })
                .ForeignKey("dbo.MaterialKindReporting", t => t.MaterialKindReportingID, cascadeDelete: true)
                .ForeignKey("dbo.SpiritTypeReporting", t => t.SpiritTypeReportingID, cascadeDelete: true)
                .Index(t => t.SpiritTypeReportingID)
                .Index(t => t.MaterialKindReportingID);

            CreateTable(
                "dbo.Storage",
                c => new
                {
                    StorageID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Capacity = c.Single(nullable: false),
                    SerialNumber = c.String(),
                    Note = c.String(maxLength: 1024),
                    DistillerID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.StorageID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.StorageState",
                c => new
                {
                    StorageID = c.Int(nullable: false, identity: true),
                    Available = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.StorageID);

            CreateTable(
                "dbo.StorageToRecord",
                c => new
                {
                    StorageToRecordID = c.Int(nullable: false, identity: true),
                    StorageID = c.Int(nullable: false),
                    RecordId = c.Int(nullable: false),
                    TableIdentifier = c.String(),
                })
                .PrimaryKey(t => t.StorageToRecordID)
                .ForeignKey("dbo.Storage", t => t.StorageID, cascadeDelete: true)
                .Index(t => t.StorageID);

            CreateTable(
                "dbo.TaxWithdrawn",
                c => new
                {
                    ProductionID = c.Int(nullable: false),
                    TaxWithdrawnID = c.Int(nullable: false, identity: true),
                    Value = c.Single(nullable: false),
                    DateOfSale = c.DateTime(nullable: false),
                    DateRecorded = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.TaxWithdrawnID)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID);

            CreateTable(
                "dbo.UnitOfMeasurement",
                c => new
                {
                    UnitOfMeasurementID = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false),
                })
                .PrimaryKey(t => t.UnitOfMeasurementID);

            CreateTable(
                "dbo.Vendor",
                c => new
                {
                    VendorID = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false),
                    DistillerID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.VendorID)
                .ForeignKey("dbo.Distiller", t => t.DistillerID, cascadeDelete: true)
                .Index(t => t.DistillerID);

            CreateTable(
                "dbo.VendorDetail",
                c => new
                {
                    VendorDetailID = c.Int(nullable: false, identity: true),
                    VendorID = c.Int(nullable: false),
                    StreetAddress = c.String(),
                    City = c.String(),
                    Zip = c.String(),
                    State = c.String(),
                    Phone = c.String(),
                    Email = c.String(),
                    Note = c.String(maxLength: 1024),
                })
                .PrimaryKey(t => t.VendorDetailID)
                .ForeignKey("dbo.Vendor", t => t.VendorID, cascadeDelete: true)
                .Index(t => t.VendorID);

            CreateTable(
                "dbo.Volume",
                c => new
                {
                    VolumeID = c.Int(nullable: false, identity: true),
                    Value = c.Single(nullable: false),
                })
                .PrimaryKey(t => t.VolumeID);

            CreateTable(
                "dbo.Weight",
                c => new
                {
                    WeightID = c.Int(nullable: false, identity: true),
                    Value = c.Single(nullable: false),
                })
                .PrimaryKey(t => t.WeightID);
        }

        public override void Down()
        {
            DropForeignKey("dbo.VendorDetail", "VendorID", "dbo.Vendor");
            DropForeignKey("dbo.Vendor", "DistillerID", "dbo.Distiller");
            DropForeignKey("dbo.TaxWithdrawn", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.StorageToRecord", "StorageID", "dbo.Storage");
            DropForeignKey("dbo.Storage", "DistillerID", "dbo.Distiller");
            DropForeignKey("dbo.SpiritType2MaterialKindReporting", "SpiritTypeReportingID", "dbo.SpiritTypeReporting");
            DropForeignKey("dbo.SpiritType2MaterialKindReporting", "MaterialKindReportingID", "dbo.MaterialKindReporting");
            DropForeignKey("dbo.SpiritCut", "DistillerId", "dbo.Distiller");
            DropForeignKey("dbo.Spirit", "DistillerID", "dbo.Distiller");
            DropForeignKey("dbo.PurchaseToSpiritTypeReporting", "SpiritTypeReportingID", "dbo.SpiritTypeReporting");
            DropForeignKey("dbo.PurchaseToSpiritTypeReporting", "PurchaseID", "dbo.Purchase");
            DropForeignKey("dbo.Purchase4Reporting", "PurchaseID", "dbo.Purchase");
            DropForeignKey("dbo.Purchase", "StatusID", "dbo.Status");
            DropForeignKey("dbo.Purchase", "StateID", "dbo.State");
            DropForeignKey("dbo.Purchase", "DistillerID", "dbo.Distiller");
            DropForeignKey("dbo.ProductionToSpiritTypeReporting", "SpiritTypeReportingID", "dbo.SpiritTypeReporting");
            DropForeignKey("dbo.ProductionToSpiritTypeReporting", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.ProductionToSpiritCut", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.ProductionToSpirit", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.ProductionContent", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.ProductionContent", "ContentFieldID", "dbo.ContentField");
            DropForeignKey("dbo.Production4Reporting", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.ProdRepMatCat2MaterialKind", "ProductionReportMaterialCategoryID", "dbo.ProductionReportMaterialCategory");
            DropForeignKey("dbo.ProdRepMatCat2MaterialKind", "MaterialKindReportingID", "dbo.MaterialKindReporting");
            DropForeignKey("dbo.MaterialKindReporting", "DistillerID", "dbo.Distiller");
            DropForeignKey("dbo.MaterialDict2MaterialCategory", "ProductionReportMaterialCategoryID", "dbo.ProductionReportMaterialCategory");
            DropForeignKey("dbo.MaterialDict2MaterialCategory", "MaterialDictID", "dbo.MaterialDict");
            DropForeignKey("dbo.MaterialDict", "DistillerID", "dbo.Distiller");
            DropForeignKey("dbo.FillTest", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.DistillerDetail", "DistillerID", "dbo.Distiller");
            DropForeignKey("dbo.BlendedComponent", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.Production", "DistillerID", "dbo.Distiller");
            DropForeignKey("dbo.AspNetUserToDistiller", "DistillerID", "dbo.Distiller");
            DropIndex("dbo.VendorDetail", new[] { "VendorID" });
            DropIndex("dbo.Vendor", new[] { "DistillerID" });
            DropIndex("dbo.TaxWithdrawn", new[] { "ProductionID" });
            DropIndex("dbo.StorageToRecord", new[] { "StorageID" });
            DropIndex("dbo.Storage", new[] { "DistillerID" });
            DropIndex("dbo.SpiritType2MaterialKindReporting", new[] { "MaterialKindReportingID" });
            DropIndex("dbo.SpiritType2MaterialKindReporting", new[] { "SpiritTypeReportingID" });
            DropIndex("dbo.SpiritCut", new[] { "DistillerId" });
            DropIndex("dbo.Spirit", new[] { "DistillerID" });
            DropIndex("dbo.PurchaseToSpiritTypeReporting", new[] { "SpiritTypeReportingID" });
            DropIndex("dbo.PurchaseToSpiritTypeReporting", new[] { "PurchaseID" });
            DropIndex("dbo.Purchase4Reporting", new[] { "PurchaseID" });
            DropIndex("dbo.Purchase", new[] { "DistillerID" });
            DropIndex("dbo.Purchase", new[] { "StatusID" });
            DropIndex("dbo.Purchase", new[] { "StateID" });
            DropIndex("dbo.ProductionToSpiritTypeReporting", new[] { "SpiritTypeReportingID" });
            DropIndex("dbo.ProductionToSpiritTypeReporting", new[] { "ProductionID" });
            DropIndex("dbo.ProductionToSpiritCut", new[] { "ProductionID" });
            DropIndex("dbo.ProductionToSpirit", new[] { "ProductionID" });
            DropIndex("dbo.ProductionContent", new[] { "ContentFieldID" });
            DropIndex("dbo.ProductionContent", new[] { "ProductionID" });
            DropIndex("dbo.Production4Reporting", new[] { "ProductionID" });
            DropIndex("dbo.ProdRepMatCat2MaterialKind", new[] { "ProductionReportMaterialCategoryID" });
            DropIndex("dbo.ProdRepMatCat2MaterialKind", new[] { "MaterialKindReportingID" });
            DropIndex("dbo.MaterialKindReporting", new[] { "DistillerID" });
            DropIndex("dbo.MaterialDict2MaterialCategory", new[] { "ProductionReportMaterialCategoryID" });
            DropIndex("dbo.MaterialDict2MaterialCategory", new[] { "MaterialDictID" });
            DropIndex("dbo.MaterialDict", new[] { "DistillerID" });
            DropIndex("dbo.FillTest", new[] { "ProductionID" });
            DropIndex("dbo.DistillerDetail", new[] { "DistillerID" });
            DropIndex("dbo.Production", new[] { "DistillerID" });
            DropIndex("dbo.BlendedComponent", new[] { "ProductionID" });
            DropIndex("dbo.AspNetUserToDistiller", new[] { "DistillerID" });
            DropTable("dbo.Weight");
            DropTable("dbo.Volume");
            DropTable("dbo.VendorDetail");
            DropTable("dbo.Vendor");
            DropTable("dbo.UnitOfMeasurement");
            DropTable("dbo.TaxWithdrawn");
            DropTable("dbo.StorageToRecord");
            DropTable("dbo.StorageState");
            DropTable("dbo.Storage");
            DropTable("dbo.SpiritType2MaterialKindReporting");
            DropTable("dbo.SpiritCut");
            DropTable("dbo.Spirit");
            DropTable("dbo.PurchaseType");
            DropTable("dbo.PurchaseToSpiritTypeReporting");
            DropTable("dbo.PurchaseHistory");
            DropTable("dbo.PurchaseHist");
            DropTable("dbo.Purchase4Reporting");
            DropTable("dbo.Status");
            DropTable("dbo.State");
            DropTable("dbo.Purchase");
            DropTable("dbo.Proof");
            DropTable("dbo.ProductionType");
            DropTable("dbo.SpiritTypeReporting");
            DropTable("dbo.ProductionToSpiritTypeReporting");
            DropTable("dbo.ProductionToSpiritCut");
            DropTable("dbo.ProductionToSpirit");
            DropTable("dbo.ProductionToPurchase");
            DropTable("dbo.ProductionHistory");
            DropTable("dbo.ProductionHist");
            DropTable("dbo.ProductionContent");
            DropTable("dbo.Production4Reporting");
            DropTable("dbo.ProdRepMatCat2MaterialKind");
            DropTable("dbo.ProcessingReportType");
            DropTable("dbo.MaterialType");
            DropTable("dbo.MaterialKindReporting");
            DropTable("dbo.ProductionReportMaterialCategory");
            DropTable("dbo.MaterialDict2MaterialCategory");
            DropTable("dbo.MaterialDict");
            DropTable("dbo.GaugeSerial");
            DropTable("dbo.GainLoss");
            DropTable("dbo.FillTest");
            DropTable("dbo.F2H");
            DropTable("dbo.DistillerDetail");
            DropTable("dbo.Destruction");
            DropTable("dbo.ContentField");
            DropTable("dbo.BottlingInfoHist");
            DropTable("dbo.BottlingInfo");
            DropTable("dbo.BlendedComponentHist");
            DropTable("dbo.Production");
            DropTable("dbo.BlendedComponent");
            DropTable("dbo.Distiller");
            DropTable("dbo.AspNetUserToDistiller");
            DropTable("dbo.Alcohol");
        }
    }
}
