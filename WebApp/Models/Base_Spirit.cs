using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WebApp.Models
{
    /// <summary>
    /// Stores serial numbers that are assigned to a purchase or production record at the time of gauging
    /// </summary>
    public class GaugeSerial
    {
        [Key]
        public int GaugeSerialID { get; set; }
        [Required]
        public string Serial { get; set; }
        [Required]
        public int RecordID { get; set; }
        [Required]
        public int RecordType { get; set; } // Possible values: 1 for purchase or 2 for Production
    }

    /// <summary>
    /// TaxWithdrawn table contains records of how much Proof Gallons have been sold for associated Production record.
    /// One thing to note that the table may contain more than one Tax Withdrawal for a particular Production record
    /// in cases when particular bottling record is not sold right away
    /// </summary>
    public class TaxWithdrawn
    {
        [Key]
        public int TaxWithdrawnID { get; set; }
        [Column(Order =  1), ForeignKey ("Production")]
        public int ProductionID { get; set; }
        public float Value { get; set; }
        public DateTime DateOfSale { get; set; } // date when the sale of the spirit has happened
        public DateTime DateRecorded { get; set; } // date when tax witdrawal was entered in the system
        public virtual Production Production { get; set; }
    }

    /// <summary>
    /// ProductionContents Records contains the list of records for a particular production record. 
    /// For example, A production Distil record can have fermentation record with that many gallons that went into it
    /// </summary>
    public class ProductionContent
    {
        [Key]
        public int ProductionContentID { get; set; }
        [Column(Order = 1), ForeignKey ("Production")]
        public int ProductionID { get; set; }
        public int RecordID { get; set; } // this could be either Production or Purchase ID. It is determined by ProductionRecord column in the table.
        [Column(Order = 4), ForeignKey("ContentField")]
        public int ContentFieldID { get; set; }
        public float ContentValue { get; set; }
        public bool isProductionComponent { get; set; } // this is boolean value indicating whether RecordID is a production record or not.

        public virtual ContentField ContentField { get; set; }
        public virtual Production Production { get; set; }
    }

    /// <summary>
    /// ContentField table contains various content classifications 
    /// For example, Fermented Quantity, or Raw Material Quantity or Grape Quantity etc. Whatever quantity is applicable for a given production record
    /// </summary>
    public class ContentField
    {
        [Key]
        public int ContentFieldID { get; set; }
        public string ContentFieldName { get; set; } // ex: DistillateGallons or BotanicalAniseLbs or just Botanical or Anise or AniselLbs
    }

    /// <summary>
    /// ProductionReportMaterialCategory table contains categories from Production report, part 6 
    /// for easy mapping with Material Kinds during the automation
    /// </summary>
    public class ProductionReportMaterialCategory
    {
        [Key]
        public int ProductionReportMaterialCategoryID { get; set; }
        public string MaterialCategoryName { get; set; } // exmaple: Grain and Grain Products or Fruit and Fruit Products, etc
    }

    /// <summary>
    /// ProdRepMatCat2MaterialKind table contains mappings between ProductionReportMaterialCategory and MaterialKindReporting
    /// </summary>
    public class ProdRepMatCat2MaterialKind
    {
        [Key]
        public int ProdRepMatCat2MaterialKindID { get; set; }
        [Column(Order =1), ForeignKey("MaterialKindReporting")]
        public int MaterialKindReportingID { get; set; }
        [Column(Order =2), ForeignKey("ProductionReportMaterialCategory")]
        public int ProductionReportMaterialCategoryID { get; set; }

        public virtual ProductionReportMaterialCategory ProductionReportMaterialCategory { get; set; }
        public virtual MaterialKindReporting MaterialKindReporting { get; set; }
    }

    /// <summary>
    /// Production4Reporting table is used to keep track of the original production
    /// quantities, for reprorting purposes. 
    /// </summary>
    public class Production4Reporting
    {
        [Key]
        public int Production4ReportingID { get; set; }
        [Column(Order = 1), ForeignKey("Production")]
        public int ProductionID { get; set; }
        public float Weight { get; set; } // original weight of the production record
        public float Volume { get; set; } // original volume of the production record
        public float Proof { get; set; } // original Proof of the production record
        public float Alcohol { get; set; } // original Alcohol of the production record
        public bool Redistilled { get; set; } // this flag is needed to indicate whether this production records is a re-distillation of the existing record

        public virtual Production Production { get; set; }
    }

    /// <summary>
    /// Purchase4Reporting table is used to keep track of the original purchase quantities
    /// quantities, for reprorting purposes. 
    /// </summary>
    public class Purchase4Reporting
    {
        [Key]
        public int Purchase4ReportingID { get; set; }
        [Column(Order = 1), ForeignKey("Purchase")]
        public int PurchaseID { get; set; }
        public float Weight { get; set; }
        public float Volume { get; set; }
        public float Proof { get; set; }
        public float Alcohol { get; set; }
        public bool Redistilled { get; set; } // this flag is needed to indicate whether this production records is a re-distillation of the existing record

        public virtual Purchase Purchase { get; set; }
    }

    public class AspNetUserToDistiller
    {
        [Key]
        public int AspNetUserToDistillerID { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required Column(Order = 2), ForeignKey("Distiller")]
        public int DistillerID { get; set; }

        public virtual Distiller Distiller { get; set; }
    }

    public class ProductionToSpiritTypeReporting
    {
        [Key, Column(Order = 0), ForeignKey("Production")]
        public int ProductionID { get; set; }

        [Key, Column(Order = 1), ForeignKey("SpiritTypeReporting")]
        public int SpiritTypeReportingID { get; set; }

        [Key, Column(Order = 2), ForeignKey("MaterialKindReporting")]
        public int MaterialKindReportingID { get; set; }

        public virtual Production Production { get; set; }
        public virtual SpiritTypeReporting SpiritTypeReporting { get; set; }
        public virtual MaterialKindReporting MaterialKindReporting { get; set; }
    }

    public class PurchaseToSpiritTypeReporting
    {
        [Key, Column(Order = 0), ForeignKey("Purchase")]
        public int PurchaseID { get; set; }

        [Key, Column(Order = 1), ForeignKey("SpiritTypeReporting")]
        public int SpiritTypeReportingID { get; set; }

        public virtual Purchase Purchase { get; set; }
        public virtual SpiritTypeReporting SpiritTypeReporting { get; set; }
    }

    /// <summary>
    /// SpiritType2MaterialKindReporting table maps SpiritTypeReportingID to MaterialKindReportingID primary keys from 
    /// tables that are used in Reporting
    /// </summary>
    public class SpiritType2MaterialKindReporting
    {
        [Key, Column(Order = 0), ForeignKey("SpiritTypeReporting")]
        public int SpiritTypeReportingID { get; set; }
        [Key, Column(Order = 1), ForeignKey("MaterialKindReporting")]
        public int MaterialKindReportingID { get; set; }

        public virtual SpiritTypeReporting SpiritTypeReporting { get; set; }
        public virtual MaterialKindReporting MaterialKindReporting { get; set; }
    }

    /// <summary>
    /// MaterialKindReporting table used in distillation to indicate material kind for reportin (e.g grape brandy, rye, fruit)
    /// </summary>
    public class MaterialKindReporting
    {
        [Key]
        public int MaterialKindReportingID { get; set; }
        public string MaterialKindName { get; set; }
        [Required Column(Order = 2), ForeignKey("Distiller")]
        public int DistillerID { get; set; }

        public virtual Distiller Distiller { get; set; }
    }

    /// <summary>
    /// SpiritTypeReporting table used in ditilation to indicate product type for reporting (e.g whiskye,brandy,rum)
    /// </summary>
    public class SpiritTypeReporting
    {
        [Key]
        public int SpiritTypeReportingID { get; set; }
        public string ProductTypeName { get; set; } // this column contains reportColumnNames ex. BrandyUnder170
        public string ProductTypeFullName { get; set; } // this column contains readable name ex:Brandy
    }

    public class Distiller
    {
        [Key]
        public int DistillerID { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Holds records of article (Wine or Spirits) destuction
    /// </summary>
    public class Destruction
    {
        [Key]
        public int DestructionID { get; set; }
        [Required]
        public int RecordID { get; set; } // PurchaseID or ProductionID
        [Required]
        public string WorkflowType { get; set; } // Purchase or Production
        public string RepresentativeName { get; set; } // The name and title of the proprietor’s representative who accomplished or supervised the destruction
        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime StartTime { get; set; } // Destruction start time
        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime EndTime { get; set; } // Destruction end time
        [Required]
        public string DestructionMethod { get; set; } // Manner of the destruction
        [Required]
        public bool Withdrawn { get; set; } // A statement of whether or not the spirits had previously been withdrawn and returned to bond
        [Required]
        public float Volume { get; set; }
        [Required]
        public float Weight { get; set; }
        [Required]
        public float AlcoholContent { get; set; }
        [Required]
        public float ProofGallons { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public class DistillerDetail
    {
        [Key]
        public int DistillerDetailID { get; set; }
        [ForeignKey("Distiller")]
        public int DistillerID { get; set; }
        public string EIN { get; set; }
        public string DSP { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }

        public virtual Distiller Distiller { get; set; }
    }

    public class Purchase
    {
        [Key]
        public int PurchaseID { get; set; }
        public string PurchaseName { get; set; } // purchase batch name
        public int PurchaseTypeID { get; set; } // id of the PurchaseType ID - Foreign Key
        public int MaterialDictID { get; set; } // this needs to be setup as a foreing key
        public float Price { get; set; }
        public int VendorID { get; set; } // this needs to be setup as a foreing key
        public int VolumeID { get; set; }
        public int WeightID { get; set; }
        public int AlcoholID { get; set; }
        public int ProofID { get; set; }
        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime PurchaseDate { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }
        [Required, ForeignKey("State")]
        public int StateID { get; set; } // states are used to track the current state of the production. We can now see the state the production is at any moment
        [Required, ForeignKey("Status")]
        public int StatusID { get; set; } // status values {Active, Processing, Processed, Deleted, Archived, Destroyed}
        [ForeignKey("Distiller")]
        public int DistillerID { get; set; } // foreign key
        public string BurningDownMethod { get; set; } // this column is used to indicate how we are burning down this particular purchase, in terms of inventory. So far, two possible values are {volume, wight}
        public bool Gauged { get; set; } // this value is showing whether the particular record has been gauged so it shows up in reporting

        public virtual Distiller Distiller { get; set; }
        public virtual State State { get; set; }
        public virtual Status Status { get; set; }
    }

    public class UnitOfMeasurement
    {
        [Key]
        public int UnitOfMeasurementID { get; set; }
        [Required]
        public string Name { get; set; }
    }

    /// <summary>
    /// ProductionToPurchase table contains many to many relationships between Production and Purchases
    /// </summary>
    public class ProductionToPurchase
    {
        [Key]
        public int ProductionToPurchaseID { get; set; }
        public int PurchaseID { get; set; }
        public int ProductionID { get; set; }
    }

    /// <summary>
    /// Purchase History table keeps track of changes that happen to a particular purchase record. 
    /// Trigger is doing the job of table updates
    /// </summary>
    public class PurchaseHist
    {
        [Key]
        public  int PurchaseHistID { get; set; }
        public int PurchaseID { get; set; }
        public int F2HID { get; set; }
        public string F2HValue { get; set; } // here, we are casting everything to a string, even though it is of different types.
        [Required]
        public DateTime UpdateDate { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    /// <summary>
    /// PurchaseHistory table is kept as a flat table to keep track of the changes for each Purchase record
    /// </summary>
    public class PurchaseHistory
    {
        [Key]
        public int PurchaseHistoryID { get; set; }
        public int PurchaseID { get; set; }
        public string PurchaseName { get; set; } // purchase batch name
        public float Price { get; set; }
        public string Vendor { get; set; } 
        public float Volume { get; set; }
        public float Weight { get; set; }
        public float Alcohol { get; set; }
        public float Proof { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime PurchaseDate { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }
        public string State { get; set; } 
        public string Status { get; set; }
        public bool Gauged { get; set; } // this value is showing whether the particular record has been gauged so it shows up in reporting
        public int UserID { get; set; } // user that made the change
        [Column(TypeName = "datetime2")]
        public DateTime UpdateDate { get; set; }
        public string Storage { get; set; }
        public string RecordName { get; set; } // name of raw material used in this purchase
        public int SpiritTypeReportingID { get; set; }
    }

    /// <summary>
    /// Production History table keeps track of changes that happen to a particular production record. 
    /// Trigger is doing the job of table updates
    /// </summary>
    public class ProductionHist
    {
        [Key]
        public int ProductionHistID { get; set; }
        public int ProductionID { get; set; }
        public int F2HID { get; set; }
        public string F2HValue { get; set; } // here, we are casting everything to a string, even though it is of different types.
        [Required]
        public DateTime UpdateDate { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    /// <summary>
    /// ProductionHistory tabe is a flat table that contains keeps track of changes to the production record
    /// </summary>
    public class ProductionHistory
    {
        [Key]
        public int ProductionHistoryID { get; set; }
        public int ProductionID { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime UpdateDate { get; set; }
        public string ProductionName { get; set; } // production batch name
        [Column(TypeName = "datetime2")]
        public DateTime ProductionStartTime { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime ProductionEndTime { get; set; }
        public float Volume { get; set; }
        public float Weight { get; set; }
        public float Alcohol { get; set; }
        public float Proof { get; set; }   
        public string Status { get; set; }
        public string State { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }
        public int UserID { get; set; }
        public bool Gauged { get; set; } // this value is showing whether the particular record has been gauged so it shows up in reporting
        public string Storage { get; set; }
        public string BurningDownMethod { get; set; } // this is used in distillation when at the time of burning down used materials. Currently, possible values are {"weight", "volume"}
        public string UsedMats { get; set; } // this field contains record ids that went into production id in question. Blueprint of value to be inserted: {ID, NewVal, Proof, DistillableOrigin, BurningDownMethod}{ID, NewVal, Proof, DistillableOrigin, BurningDownMethod}
        public string SpiritCutName { get; set; }
        public string BlendingAdditives { get; set; } // this field contains blending additives that go into production (for now, we only do this for Blending). Blueprint for value to be inserted: {RawMaterialId, RawMaterialName, RawMaterialQuantity, UnitOfMeasurement}
        public string BottlingInfo { get; set; } // this field contains botlling information that go into production. Blueprint for value to be inserted: {CaseCapacity, BottleCapacity, CaseQuantity, BottleQuantity}
        public int SpiritTypeReportingID { get; set; }
        public int MaterialKindReportingID { get; set; }
        public float TaxedProof { get; set; } // Proof value indicated as taxable. For now, it is used to enable reporting processing. In  other words, how much alcohol has been sold.
        public System.DateTime WithdrawalDate { get; set; } // This is meant to track when the sale has happened
    }

    /// <summary>
    /// F2H short for Field2History table contains field names from other tables. 
    /// The Ids for the respective field names are used in History tables
    /// </summary>
    public class F2H
    {
        [Key]
        public int F2HID { get; set; }
        public string FName { get; set; }
    }

    /// <summary>
    /// State is meant to broadcast the state of the material in Distill production. Possible values are: 
    /// Fermentable
    /// Fermented
    /// Distilled
    /// Blended
    /// Bottled
    /// Supply
    /// Additive
    /// These values are not meant to tell the user production status but in state particular material is.
    /// An example could be Grape.It's possible state can be as follows: Fermentable(when the fruit is raw) -> Fermented -> Distilled -> Blended - >Bottled
    /// </summary>
    public class State
    {
        [Key]
        public int StateID { get; set; }
        public string Name { get; set; } // States: {Fermentable, Fermented, Distilled, Blended, Bottled, Supply, Additive} These, technically are exactly the same as Purchase types, at this point
    }

    /// <summary>
    /// Status is meant to broadcast material administrative/production status
    /// Active
    /// Processing
    /// Processed
    /// Deleted
    /// Archived
    /// Destroyed
    /// As can be seen from value names they represent what the current status of a particular record is. 
    /// They could be actively processing, be processed or simply, be deleted.Example: when the material is has just been bought or made,
    /// it's status is Active, when it's quantity started being used but has not been used the entire quantity, its status is Processing.
    /// When all of the quantity is being used up, the status becomes Processed.
    /// </summary>
    public class Status
    {
        [Key]
        public int StatusID { get; set; }
        public string Name { get; set; } // status values {Purchased, InProd, Finished, Cancelled, Closed, Deleted, Archived}
    }

    /// <summary>
    /// Weight table contains VolumeByWeight values
    /// </summary>
    public class Weight
    {
        [Key]
        public int WeightID { get; set; }
        public float Value { get; set; }
    }

    /// <summary>
    /// VolumeID table contains Quanityties that are expressed in Gallons
    /// </summary>
    public class Volume
    {
        [Key]
        public int VolumeID { get; set; }
        public float Value { get; set; }
    }

    /// <summary>
    /// StorageToRecord maps Purchases materials to storage
    /// </summary>
    public class StorageToRecord
    {
        [Key]
        public int StorageToRecordID { get; set; }
        [ForeignKey("Storage")]
        public int StorageID { get; set; }
        public int RecordId { get; set; } // this could be a production id or a purchase id
        public string TableIdentifier { get; set; } // table identifier values: {prod, pur}

        public virtual Storage Storage { get; set; }
    }

    /// <summary>
    /// Storage table contains Storage information
    /// </summary>
    public class Storage
    {
        [Key]
        public int StorageID { get; set; }
        public string Name { get; set; }
        public float Capacity { get; set; }
        public string SerialNumber { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }
        [ForeignKey("Distiller")]
        public int DistillerID { get; set; }

        public virtual Distiller Distiller { get; set; }
    }

    /// <summary>
    /// StorageState keeps track of availability states for Storages
    /// True - avaiabe
    /// False - not available
    /// </summary>
    public class StorageState
    {
        [Key]
        public int StorageID { get; set; } // this needs to be setup as a foreing key
        public bool Available { get; set; } // true - avaiable, false - not avaiable
    }

    /// <summary>
    /// Table that contains differences between Blending and Bottling records when Blending record flows into Bottling record.
    /// This is important for two reasons. First, it is needed in government reporting. Second, it is needed for potential data mining in the future.
    /// </summary>
    public class GainLoss
    {
        [Key]
        public int GainLossID { get; set; }
        public bool Type { get; set; } // true - gain, false - loss
        public float Quantity { get; set; }
        public DateTime DateRecorded { get; set; }
        public int BlendedRecordId { get; set; } //  since we can ony bottle one blend at a time, we expect it to be Blended record ID
        public int BottledRecordId { get; set; } //  since we can ony bottle one blend at a time, we expect it to be Bottled record ID
    }

    /// <summary>
    /// Vendor table keeps record of vendor(seller) information
    /// </summary>
    public class Vendor
    {
        [Key]
        public int VendorID { get; set; }
        [Required]
        public string Name { get; set; }
        [ForeignKey("Distiller")]
        public int DistillerID { get; set; }

        public virtual Distiller Distiller { get; set; }
    }

    public class VendorDetail
    {
        [Key]
        public int VendorDetailID { get; set; }
        [ForeignKey("Vendor")]
        public int VendorID { get; set; } // this needs to be setup as a foreing key
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }

        public virtual Vendor Vendor { get; set; }
    }

    /// <summary>
    /// MaterialDict table stores Material names that users creates in the dictionary workflows
    /// </summary>
    public class MaterialDict
    {
        [Key]
        public int MaterialDictID { get; set; }
        public string Name { get; set; } // Material Name could be grapes, pomace, Sugar, Vodka, Honey, Anise, etc...
        public int UnitOfMeasurementID { get; set; }
        [MaxLength (1024)]
        public string Note { get; set; }
        [ForeignKey("Distiller")]
        public int DistillerID { get; set; }

        public virtual Distiller Distiller { get; set; }
    }

    /// <summary>
    /// MaterialDict2MaterialCategory keeps relationship between Material and 
    /// Materail Category(the actual category table is named ProductionReportMaterialCategory
    /// since it was created for reproting purposes) The assignment happens during the purchase materials workflow.
    /// we are expected to allow assigning Material Categories for Fermentable and Fermented workflows
    /// </summary>
    public class MaterialDict2MaterialCategory
    {
        [Key ForeignKey("MaterialDict") Column(Order = 0)]
        public int MaterialDictID { get; set; }
        [ForeignKey("ProductionReportMaterialCategory") Column(Order =1)]
        public int ProductionReportMaterialCategoryID { get; set; }

        public virtual MaterialDict MaterialDict { get; set; }
        public virtual ProductionReportMaterialCategory ProductionReportMaterialCategory { get; set; }
    }

    /// <summary>
    /// MaterialType contans MaterialDict and MaterialType names mappings
    /// Non-Clustered Index here
    /// </summary>
    public class MaterialType
    {
        [Key]
        public int MaterialTypeID { get; set; }
        public string Name { get; set; } // fermentabe, fermented, distilled, supply, additive
        public int MaterialDictID { get; set; } // this needs to be setup as a foreign key
    }

    /// <summary>
    /// PurchaseType table contains mappings of Purchase type Ids to Purchase Types.
    /// </summary>
    public class PurchaseType
    {
        [Key]
        public int PurchaseTypeID { get; set; } 
        public string Name { get; set; } // fermentabe, fermented, distilled, supply, additive
    }

    public class Alcohol
    {
        [Key]
        public int AlcoholID { get; set; }
        public float Value { get; set; }
    }

    public class SpiritCut
    {
        [Key]
        public int SpiritCutID { get; set; }
        [Required ForeignKey("Distiller")]
        public int DistillerId { get; set; }
        [Required]
        public string Name { get; set; }

        public virtual Distiller Distiller { get; set; }
    }

    public class Proof
    {
        [Key]
        public int ProofID { get; set; }
        public float Value { get; set; }
    }

    public class Production
    {
        [Key]
        public int ProductionID { get; set; }
        public string ProductionName { get; set; } // production batch name
        [Column(TypeName = "datetime2")]
        public DateTime ProductionDate { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime ProductionStartTime { get; set; } // Ilias requested it. This will help him to understand how much time he spends on distilling {heads, hearts, tails}
        [Column(TypeName = "datetime2")]
        public DateTime ProductionEndTime { get; set; } // Ilias requested it. This will help him to understand how much time he spends on distilling {heads, hearts, tails}
        public int VolumeID { get; set; }
        public int WeightID { get; set; }
        public int AlcoholID { get; set; }
        public int ProofID { get; set; }
        public int ProductionTypeID { get; set; } // integer type for the type of production
        public int StatusID { get; set; }
        public int StateID { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }
        [ForeignKey("Distiller")]
        public int DistillerID { get; set; }
        public string BurningDownMethod { get; set; } // possible values {"weight","volume"}
        public bool Gauged { get; set; } // this value is showing whether the particular record has been gauged so it shows up in reporting

        public virtual Distiller Distiller { get; set; }
    }

    /// <summary>
    /// ProductionToSpirit maps productionID to spiritID. (1 to 1)
    /// </summary>
    public class ProductionToSpirit
    {
        [Key]
        public int ProductionToSpiritID { get; set; }
        public int SpiritID { get; set; }
        [ForeignKey("Production")]
        public int ProductionID { get; set; }

        public virtual Production Production { get; set; }
    }

    /// <summary>
    /// public class ProductionToSpiritCut maps productionID to spiritCutID.
    /// </summary>
    public class ProductionToSpiritCut
    {
        [Key]
        public int ProductionToSpiritCutID { get; set; }
        public int SpiritCutID { get; set; }
        [ForeignKey("Production")]
        public int ProductionID { get; set; }

        public virtual Production Production { get; set; }
    }

    /// <summary>
    ///  Contains spirit name (Uzo, Vodka, Gin, etc) that are managed in dicitonary and are used in production
    /// </summary>
    public class Spirit
    {
        [Key]
        public int SpiritID { get; set; }
        [ForeignKey("Distiller")]
        public int DistillerID { get; set; }
        [Required]
        public string Name { get; set; }
        [MaxLength(1024)]
        public string Note { get; set; }

        public virtual Distiller Distiller { get; set; }
    }

    /// <summary>
    /// Blended Component history table keeps track of Blended Component records 
    /// as we can't use ProductionHist table because we end up having one to many relationship
    /// </summary>
    public class BlendedComponentHist
    {
        [Key]
        public int BlendedComponentHistID { get; set; }
        public int ProductionID { get; set; } // this is the actual newly created blending production record to which everything is being tight to
        public string FieldName { get; set; } // column name in BottlingInfo
        public string FieldValue { get; set; } // stringified value from BottlingInfo table
        [Column(TypeName = "datetime2")]
        public DateTime UpdateDate { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
    /// <summary>
    /// BlendedCompenent table keeps records of Blended Record and respective records that went into this
    /// particuial blend. These could be Additives or any other materials
    /// </summary>
    public class BlendedComponent
    {
        [Key]
        public int BlendedComponentID { get; set; }
        [ForeignKey("Production")]
        public int ProductionID { get; set; } // this is the actual newly created blending production record to which everything is being tight to
        [Required]
        public int RecordId { get; set; } // this record Id is meant to be an id of rawMaterial of type Additive
        public float Quantity { get; set; }
        public string UnitOfMeasurement { get; set; }

        public virtual Production Production { get; set; }
    }

    public class BottlingInfoHist
    {
        [Key]
        public int BottlingInfoHistID { get; set; }
        public int ProductionID { get; set; } // newly created production id
        public string FieldName { get; set; } // column name in BottlingInfo
        public string FieldValue { get; set; } // stringified value from BottlingInfo table
        [Column(TypeName = "datetime2")]
        public DateTime UpdateDate { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public class BottlingInfo
    {
        [Key]
        public int BottlingInfoID { get; set; }
        public int ProductionID { get; set; } // newly created production id
        public int CaseCapacity { get; set; }
        public float BottleVolume { get; set; } // the volume of the bottle in bottling record
        public float CaseQuantity { get; set; } // number of records in bottlnig record
        public float BottleQuantity { get; set; } // number of bottles in bottlnig record
    }

    /// <summary>
    /// BottlingFillTest table contains bottling fill test proof records associated with bottling production ID
    /// </summary>
    public class FillTest
    {
        [Key]
        public int FillTestID { get; set; }
        [Column(Order = 1), ForeignKey("Production")]
        public int ProductionID { get; set; }
        public float ProofGallons { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime FillTestDate { get; set; }

        public virtual Production Production { get; set; }
    }
    /// <summary>
    /// ProductionType table maps production ID with the type of production
    /// </summary>
    public class ProductionType
    {
        [Key]
        public int ProductionTypeID { get; set; }
        public string Name { get; set; } // possible values {Fermentation, Distillation, Blending, Bottling}
    }

    public class DistilDBContext : DbContext
    {
        public DbSet<GaugeSerial> GaugeSerial { get; set; }
        public DbSet<TaxWithdrawn> TaxWithdrawn { get; set; }
        public DbSet<ProductionContent> ProductionContent { get; set; }
        public DbSet<ContentField> ContentField { get; set; }
        public DbSet<ProdRepMatCat2MaterialKind> ProdRepMatCat2MaterialKind { get; set; }
        public DbSet<ProductionReportMaterialCategory> ProductionReportMaterialCategory { get; set; }
        public DbSet<ProductionToSpiritTypeReporting> ProductionToSpiritTypeReporting { get; set; }
        public DbSet<PurchaseToSpiritTypeReporting> PurchaseToSpiritTypeReporting { get; set; }
        public DbSet<Production4Reporting> Production4Reporting { get; set; }
        public DbSet<Purchase4Reporting> Purchase4Reporting { get; set; }
        public DbSet<AspNetUserToDistiller> AspNetUserToDistiller { get; set; }
        public DbSet<Destruction> Destruction { get; set; }
        public DbSet<Distiller> Distiller { get; set; }
        public DbSet<DistillerDetail> DistillerDetail { get; set; }
        public DbSet<Purchase> Purchase { get; set; }
        public DbSet<UnitOfMeasurement> UnitOfMeasurement { get; set; }
        public DbSet<ProductionToPurchase> ProductionToPurchase { get; set; }
        public DbSet<PurchaseHist> PurchaseHist { get; set; } // todo: this needs to be removed after new purchase history is implemented and is working
        public DbSet<PurchaseHistory> PurchaseHistory { get; set; }
        public DbSet<ProductionHist> ProductionHist { get; set; } // todo: this needs to be removed after new production history is implemented and is working
        public DbSet<ProductionHistory> ProductionHistory { get; set; }
        public DbSet<F2H> F2H { get; set; }
        public DbSet<State> State { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Weight> Weight { get; set; }
        public DbSet<Volume> Volume { get; set; }
        public DbSet<StorageToRecord> StorageToRecord { get; set; }
        public DbSet<Storage> Storage { get; set; }
        public DbSet<StorageState> StorageState { get; set; }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<GainLoss> GainLoss { get; set; }
        public DbSet<VendorDetail> VendorDetail { get; set; }
        public DbSet<MaterialDict> MaterialDict { get; set; }
        public DbSet<MaterialType> MaterialType { get; set; }
        public DbSet<PurchaseType> PurchaseType { get; set; }
        public DbSet<Alcohol> Alcohol { get; set; }
        public DbSet<SpiritCut> SpiritCut { get; set; }
        public DbSet<Proof> Proof { get; set; }
        public DbSet<Production> Production { get; set; }
        public DbSet<Spirit> Spirit { get; set; }
        public DbSet<BlendedComponent> BlendedComponent { get; set; }
        public DbSet<BlendedComponentHist> BlendedComponentHist { get; set; }
        public DbSet<BottlingInfo> BottlingInfo { get; set; }
        public DbSet<BottlingInfoHist> BottlingInfoHist { get; set; }
        public DbSet<ProductionType> ProductionType { get; set; }
        public DbSet<ProductionToSpiritCut> ProductionToSpiritCut { get; set; }
        public DbSet<ProductionToSpirit> ProductionToSpirit { get; set; }
        public DbSet<SpiritTypeReporting> SpiritTypeReporting { get; set; }
        public DbSet<MaterialKindReporting> MaterialKindReporting { get; set; }
        public DbSet<SpiritType2MaterialKindReporting> SpiritType2MaterialKindReporting { get; set; }
        public DbSet<MaterialDict2MaterialCategory> MaterialDict2MaterialCategory { get; set; }
        public DbSet<FillTest> FillTest { get; set; }

        // Remove table name pluralization before context(table) creation. Called only once when context(table) is created.
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}