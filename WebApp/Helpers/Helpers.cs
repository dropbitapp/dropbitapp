using System;
using System.Collections.Generic;

namespace WebApp.Helpers
{
    #region Reporting Objects
    /* This sections contains object that are used for reporting workflows*/

     public class ProductionReportingObject
    {
        public ReportHeader Header { get; set; }
        public List<ProdReportPart1> Part1 { get; set; }
        public List<ProdReportParts2Through5> Part2Through5 { get; set; }
        public List<ProdReportPart6> ProdReportPart6 { get; set; }
    }

    public class StorageReport
    {
        public ReportHeader Header { get; set; }
        public List<StorageReportCategory> ReportBody { get; set; }
    }
    
    public class ReportHeader
    {
        public string ProprietorName { get; set; }
        public string EIN { get; set; }
        public string ReportDate { get; set; }
        public string PlantAddress { get; set; }
        public string DSP { get; set; }
    }

    public class ProdReportPart1
    {
        public string SpiritCatName { get; set; }
        public float ProccessingAcct { get; set; } // stands for Entered In Proccessing Account
        public float SorageAcct { get; set; } // stands for Entered In Storage Account
        public float ProducedTotal { get; set; } // stands for Produced total (on report, lines 1 through 13) for 170 & Under
        public float Recd4Redistil { get; set; } // stands for Received for Re-Distillation(on report, for this period, invlcuding values from storage + processing) for 170 & Under
        public int SpiritTypeReportingID { get; set; }
        public float UnfinishedSpiritsEndOfQuarter { get; set; }
        public float RedistilledEndOfQuarter { get; set; }
    }

    public class ProdReportParts2Through5
    {
        public string KindOfMaterial { get; set; }
        public float ProofGallons { get; set; }
        public int MaterialKindReportingID { get; set; }
        public int SpiritTypeReportingID { get; set; } // we need this here so we can associate Material Kind with Spirit Type if needed.
        bool NewCoop { get; set; } // new cooperage - used only in whiskey production
        bool UsedCoop { get; set; } // used cooperage - used only in whiskey production
        bool Tanks { get; set; } // tanks- used only in whiskey production
    }

    public class ProdReportPart6
    {
        public int ProductionID { get; set; }
        public string KindOfMaterial { get; set; }
        public float Weight { get; set; }
        public float Volume { get; set; }
        public int ProdReportMaterialCategoryID { get; set; } // this will help us to determine in which category in Production report Part 6 to put custom Materials
    }

    public class ProcessingReportingObject
    {
        public ReportHeader Header { get; set; }
        public ProcessReportingPart1 Part1 { get; set; }
        public ProcessReportingPart2 Part2 { get; set; }
        public List<ProcessReportingPart4> Part4List { get; set; }
    }

    public class ProcessReportingPart1
    {
        public string BulkIngredients { get; set; } // either wine or spirit
        public float OnHandFirstofMonth { get; set; }
        public float Recd4Process { get; set; } // Line 2 for Spirit of Part 1received for Processing, for spirit this value comes from production and/or storage
        public float WineMixedWithSpirit { get; set; }
        public float Dumped4Processing { get; set; }
        public float Gains { get; set; }
        public float AmtBottledPackaged { get; set; }
        public float Transf2Prod4Redistil { get; set; } // transferred to Production for re-distillation
        public float Destroyed { get; set; }
        public float Used4Redistil { get; set; }
        public float Losses { get; set; }
        public float OnHandEndofMonth { get; set; }
    }   

    public class ProcessReportingPart2
    {
        public string FinishedProduct { get; set; } // either bottled or packaged
        public float OnHandFirstofMonth { get; set; }
        public float AmtBottledPackaged { get; set; }
        public float Recd4Process { get; set; } 
        public float InventoryOverage { get; set; }
        public float Transf2Prod4Redistil { get; set; } // transferred to Production for re-distillation
        public float Destroyed { get; set; }
        public float Dumped4Processing { get; set; }
        public float RecordedLosses { get; set; }
        public float InventoryShortage { get; set; }
        public float OnHandEndofMonth { get; set; }
        public float TaxWithdrawn { get; set; }
    }

    public class ProcessReportingPart4
    {
        public string ProcessingSpirits { get; set; } // either BulkSpiritDumped or Bottled
        public float AlcoholNeutral { get; set; }
        public float BlendedStraightWhiskey { get; set; }
        public float BlendedWhiskeyWithNeutral { get; set; }
        public float BlendedWhiskeyWithLight { get; set; } // line 51 of Processing report
        public float BlendedLightWhiskey { get; set; } // line 52 of Processing report
        public float BlendedOtherWhiskey { get; set; }
        public float ImportedWhiskeyScotch { get; set; }
        public float ImportedWhiskeyCanadian { get; set; }
        public float ImportedWhiskeyIrish { get; set; }
        public float DomesticWhiskey160Under { get; set; }
        public float DomesticWhiskeyOver160 { get; set; }
        public float Brandy170Under { get; set; }
        public float BrandyOver170 { get; set; }
        public float RumPuertoRican { get; set; }
        public float RumVirginIslands { get; set; }
        public float RumDomestic { get; set; }
        public float RumOtherImported { get; set; }
        public float Gin { get; set; }
        public float Vodka { get; set; }
        public float Liqueur { get; set; }
        public float Cocktail { get; set; }
        public float Tequila { get; set; }
        public string ProcessingReportTypeName { get; set; }
        public int ProcessingTypeID { get; set; }
        public int StateID { get; set; }
    }

    public class StorageReportCategory
    {
        public string CategoryName { get; set; }
        public float r1_OnHandFirstOfMonth { get; set; }
        public float r2_DepositedInBulkStorage { get; set; }
        public float r4_ReturnedToBulkStorage { get; set; }
        public float r6_TotalLines1Through5 { get; set; }
        public float r7_TaxPaid { get; set; }
        public float r17_TransferredToProcessingAccount { get; set; }
        public float r18_TransferredToProductionAccount { get; set; }
        public float r19_TransferredToOtherBondedPremises { get; set; }
        public float r20_Destroyed { get; set; }
        public float r22_OtherLosses { get; set; }
        public float r23_OnHandEndOfMonth { get; set; }
        public float r24_Lines7Through23 { get; set; }
    }
    #endregion

    /// <summary>
    /// This object is used as a transport for data to be displayed in Fermentable Production workflow to display Fermentable list. 
    /// Please updare comments if you starting to use it for something else
    /// </summary>
    public class PurMatObject
    {
        public int PurchaseId { get; set; }
        public string PurchaseBatchName { get; set; }
        public int RawMaterialId { get; set; }
        public string MaterialName { get; set; }
        public float VBW { get; set; }
        public float QtyGal { get; set; }
        public string BurningDownMethod { get; set; }
        public string DistillableOrigin { get; set; }
    }

    /// <summary>
    /// I am using this object as a transport in returning data to some Production workflows, such as Distilling.
    /// They are being displayed in List boxes
    /// </summary>
    public class ProdObjectConcise
    {
        public int PurchaseId { get; set; }
        public int ProductionId { get; set; }
        public string BatchName { get; set; }
        public int RecordId { get; set; }
        public string DistillableOrigin { get; set; }
        public int DistillableOriginID { get; set; }
        public string DistillMethod { get; set; }
        public float Quantity { get; set; }
        public float VolumeByWeight { get; set; }
        public float AlcoholContent { get; set; }
        public float ProofGallon { get; set; }
        public int SpiritId { get; set; }
        public string SpiritName { get; set; }
        public string Note { get; set; }
        public string BurningDownMethod { get; set; } // this is used in distillation when at the time of burning down used materials. Currently, possible values are {"lb", "gal"}
    }

    /// <summary>
    /// This object is used during creation of the production workflow, for example, Distillation. 
    /// When we need to send quantity information about used material, we need to send information about old and new quantity.
    /// </summary>
    public class ObjInfo4Burndwn
    {
        public int ID { get; set; }
        public float OldVal { get; set; }
        public float NewVal { get; set; }
        public float Proof { get; set; }
        public string DistillableOrigin { get; set; } // {"pur", "prod"} values used to in AvaiableFor[Distillation, Blending or Bottling].
        public string BurningDownMethod { get; set; } // this is used in distillation when at the time of burning down used materials. Currently, possible values are {"weight", "volume"}
    }

    /// <summary>
    /// ProductionObject is used when being transferred between client and backend
    /// </summary>
    public class ProductionObject
    {
        public int PurchaseId { get; set; }
        public int ProductionId { get; set; }
        public string BatchName { get; set; }
        public string ProductionType { get; set; } // this value is set on the client side {Fermentation, Distill, Blending, Bottling}
        public int ProductionTypeId { get; set; }
        public string RecordName { get; set; }
        public string DistilledFrom { get; set; } // two possible values {"F", "D"}. Indicates what this distil is made from.(Fermented or another Distill)
        public List<int> RecordId { get; set; } // grisha - todo: remove this attribute after we migrate to v3 since we are using RecordIds of type Dictionary
        public Dictionary<int, string> RecordIds { get; set; } // 1st param: id of the material being used for production. 2nd param: value used to in AvaiableFor[Distillation, Blending or Bottling] {"pur", "prod"}. todo: update front end  and also viewing query to transport this info to front-end
        public List<ObjInfo4Burndwn> UsedMats { get; set; } // this attribute is used to keep old/new values and a string containing value that is currently sent using RecordIds. todo: remove RecordIds from all workflows since it is being taken care of with this attribute
        public int SpiritCutId { get; set; }
        public string SpiritCutName { get; set; }
        public System.DateTime ProductionDate { get; set; }
        public System.DateTime ProductionStart { get; set; }
        public System.DateTime ProductionEnd { get; set; }
        public float Quantity { get; set; }
        public float VolumeByWeight { get; set; }
        public float AlcoholContent { get; set; }
        public float ProofGallon { get; set; }
        public List<StorageObject> Storage { get; set; }
        public int StorageId { get; set; }
        public string StorageName { get; set; }
        public int SpiritId { get; set; }
        public string SpiritName { get; set; }
        public string StatusName { get; set; }
        public List<BlendingAdditive> BlendingAdditives { get; set; }
        public BottlingObject BottlingInfo { get; set; }
        public List<FillTestObject> FillTestList { get; set; }
        public List<int> PurchaseIdList { get; set; }
        public int SpiritTypeReportingID { get; set; }
        public int MaterialKindReportingID { get; set; }
        public string Note { get; set; }
        public float TaxedProof { get; set; } // Proof value indicated as taxable. For now, it is used to enable reporting processing. In  other words, how much alcohol has been sold.
        public System.DateTime WithdrawalDate { get; set; } // This is meant to track when the sale has happened
        public bool Gauged { get; set; }
    }

    /// <summary>
    /// BottlingObject object is used to store and transfer Bottling 
    /// specific data between client and server.
    /// </summary>
    public class BottlingObject
    {
        public int CaseCapacity { get; set; }
        public float BottleCapacity { get; set; }
        public float CaseQuantity { get; set; }
        public int BottleQuantity { get; set; }
    }

    /// <summary>
    /// SpiritObject is representation of 
    /// the Spirit table in the DB
    /// </summary>
    public class SpiritObject
    {
        public int SpiritId { get; set; }
        public string SpiritName { get; set; }
        public string Note { get; set; }
    }

    /// <summary>
    /// DeleteRecord is a generic object which defines
    /// a dictionary item which must be deleted
    /// </summary>
    public class DeleteRecordObject
    {
        public int DeleteRecordID { get; set; }
        public string DeleteRecordType { get; set; }
    }

    /// <summary>
    /// It destroys things!
    /// </summary>
    public class DestructionObject
    {
        public int DestructionID { get; set; }
        public int RecordID { get; set; } // PurchaseID or ProductionID
        public string WorkflowType { get; set; } // Production or Purchase
        public string RepresentativeName { get; set; } // The name and title of the proprietor’s representative who accomplished or supervised the destruction.
        public string BatchType { get; set; } // Fermented, Distilled, Fermentation, Distillation, Blending, or Bottling
        public string BatchName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string DestructionMethod { get; set; } // Manner of the destruction
        public bool Withdrawn { get; set; } // A statement of whether or not the spirits had previously been withdrawn and returned to bond
        public int QuantityGalID { get; set; }
        public int VolumeByWeightLBID { get; set; }
        public int AlcoholID { get; set; }
        public int ProofID { get; set; }
        public float Quantity { get; set; }
        public float VolumeByWeight { get; set; }
        public float AlcoholContent { get; set; }
        public float ProofGallons { get; set; }
        public string Note { get; set; }
    }

    /// <summary>
    /// PurchaseObject is used when being transferred between client and backend
    /// </summary>
    public class PurchaseObject
    {
        public int PurchaseId { get; set; }
        public string PurBatchName { get; set; }
        public string PurchaseType { get; set; } // this value is set on the client side {Fermentable, Fermented, Distill, Additive, Supply}
        public int RecordId { get; set; }
        public string RecordName { get; set; }
        public int RawMaterialId { get; set; }
        public System.DateTime PurchaseDate { get; set; }
        public float Quantity { get; set; }
        public float VolumeByWeight { get; set; }
        public float AlcoholContent{ get; set; }
        public float ProofGallon { get; set; }
        public int UnitOfMeasurementId { get; set; }
        public string UnitOfMeasurementName { get; set; }
        public float Price { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public List<StorageObject> Storage { get; set; }
        public int StorageId { get; set; }
        public string StorageName { get; set; }
        public string Note { get; set; }
        public int StateID { get; set; }
        public string State { get; set; }
        public int StatusID { get; set; }
        public string Status { get; set; }
        public int SpiritTypeReportingID { get; set; }
        public bool Gauged { get; set; }
    }

     /// <summary>
    /// BlendingAdditive is the object that contains Additive information for 
    /// a given Blending record that we are either creating or viewing
    /// </summary>
    public class BlendingAdditive
    {
        public int BlendingAdditiveId { get; set; }
        public int RawMaterialId { get; set; } // this is Additive object
        public float RawMaterialQuantity { get; set; }
        public string RawMaterialName { get; set; }
        public string UnitOfMeasurement { get; set; }
    }

    /// <summary>
    /// Unit object is used when being transferred between client and backend
    /// </summary>
    public class UnitObject
    {
        public int UnitOfMeasurementId { get; set; }
        public string UnitName { get; set; }
    }

    /// <summary>
    /// Vendor Object is used when being transferred between client and backend
    /// </summary>
    public class VendorObject
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string Note { get; set; }
    }

    /// <summary>
    /// Storage Object is used when being transferred between client and backend
    /// </summary>
    public class StorageObject
    {
        public int StorageId { get; set; }
        public string StorageName { get; set; }
        public string SerialNumber { get; set; }
        public float Capacity { get; set; }
        public string Note { get; set; }
    }

    /// <summary>
    /// SpiritCutObject is used when being transferred between client and backend
    /// </summary>
    public class SpiritCutObject
    {
        public int SpiritCutId { get; set; }
        public string SpiritCutName { get; set; }
    }

    /// <summary>
    /// Raw Material Object is used as a blueprint object when transferred between client and backend 
    /// </summary>
    public class RawMaterialObject
    {
        public int RawMaterialId { get; set; }
        public string RawMaterialName { get; set; }
        public string Note { get; set; }
        public PurchaseMaterialBooleanTypes PurchaseMaterialTypes { get; set; }
        public string UnitType { get; set; }
        public int UnitTypeId { get; set; }
        public int MaterialCategoryID { get; set; } // this here is used for creating an associations between Materials used in Distillation an Material Categories that are needed for production report part 6.
    }

    /// <summary>
    /// MaterialCategory object is used for Production Report part 6, on page 2. We also need it to send information about
    /// categories to Dictionary Fermentable and Fermented workflows
    /// </summary>
    public class MaterialCategory
    {
        public int MaterialCategoryID { get; set; }
        public string MaterialCategoryName { get; set; }
    }

    /// <summary>
    /// Additive Object is used as a blueprint object when transferred between client and backend 
    /// </summary>
    public class AdditiveObject
    {
        public int AdditiveTypeId { get; set; }
        public string AdditiveName { get; set; }
        public string Note { get; set; }
    }

    /// <summary>
    /// Supply Object is used as a blueprint object when transferred between client and backend 
    /// </summary>
    public class SupplyObject
    {
        public int SupplyId { get; set; }
        public string SupplyName { get; set; }
        public string Note { get; set; }
    }

    /// <summary>
    /// Note Object is used as a blueprint object when transferred between client and backend 
    /// </summary>
    public class NoteObject
    { 
        public int NoteId { get; set; }
        public int RecordId { get; set; }
        public int IdentifierId { get; set; }
        public string NoteValue { get; set; }
    }

    /// <summary>
    /// PurchaseMaterialType Object is the collection of PurchaseMaterialTypes with bool values associated with it and used as an attribute in RawMaterialObject
    /// </summary>
    public class PurchaseMaterialBooleanTypes
    {
        public bool Fermentable { get; set; }
        public bool Fermented { get; set; }
        public bool Distilled { get; set; }
        public bool Supply { get; set; }
        public bool Additive { get; set; }
    }

    /// <summary>
    /// Helper Class to act as a dictionary items but which allows duplicate ids
    /// through the usage of Lists. So, something like List<CollidableDictionaryItem> will serve my purposes
    /// </summary>
    public class CollidableDictionaryItem
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public class MaterialKindObject
    {
        public int MaterialKindID { get; set; }
        public int SpiritTypeReportingID { get; set; }
        public string MaterialKindName { get; set; }
    }

    public class SpiritToKindListObject
    {
        public int SpiritTypeReportingID { get; set; }
        public string ProductTypeName { get; set; }
        public List<MaterialKindObject> MaterialKindObject { get; set; }
        
    }
    /// <summary>
    /// FillTestObject object is used to store fill test data (proof and date) during bottling 
    /// </summary>
    public class FillTestObject
    {
        public float FillProof { get; set; }
        public DateTime FillDate { get; set; }
    }
}