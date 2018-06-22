using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Persistence.BusinessLogicEnums
{
    // values from ReportIdentifier table, enums represent static values in table which map report name to integer value. 
    public enum PersistReportType
    {
        Default,
        Storage,
        Production,
        Processing
    }

    // values from ReportPart table, enums represent static values in table which map report parts to integer value. 
    public enum PersistReportPart
    {
        Default,
        Part1,
        Part2,
        Part3,
        Part4,
        Part5,
        Part6
    }

    // values from ReportRow table, enums represent static values in table which map row names to an integer value. 
    public enum PersistReportRow
    {
        Default,
        OnHandFirstOfMonth,
        DepositedInBulkStorage,
        ReturnedToBulkStorage,
        TotalLines_1_Through_5,
        Taxpaid,
        UseOfTheUnitedStates,
        HospitalScientificEducationalUse,
        Export,
        TransferToForeignTradeZone,
        TransferToCMBW,
        UseAsSuppliesOnVesselsAndAircraft,
        TransferToBondedWinery,
        TransferToCBW,
        ResearchDevelopmentOrTesting,
        TransferredToProcessingAccount,
        TransferredToProductionAccount,
        TransferredToOtherBondedPremises,
        Destroyed,
        Line_21_EmptyField,
        OtherLosses,
        OnHandEndOfMonthLine,
        TotalLines_7_Through_23,
        TaxPayment,
        UseOfUS,
        HospitalScientificOrEducationalUse,
        UseAsSuppliesOnVesselsorAircraft,
        UseInWineProduction,
        EnteredInProcessingAccount,
        EnteredForTransferInBond,
        EnteredInStorageAccount,
        WithdrawnForResearchDevelopmentOrTesting,
        Line_13_Empty,
        ProducedTotalLines_1_Through_13,
        ReceivedForRedistillation,
        Line_16_Empty,
        PhysicalInventoryEndOfCalendarQuarterRecievedForRedistillation,
        PhysicalInventoryEndOfCalendarQuarterUnfinishedSpirits,
        Grain,
        Fruit,
        Molasses,
        EthylSulfate,
        EthyleneGas,
        SulphiteLiquors,
        FromRedistillation,
        Bourbon,
        Corn,
        Rye,
        Light,
        GrapeBrandy,
        AllOtherBrandy,
        NeutralGrapeBrandy,
        AllOtherNeutralBrandy,
        KindOfSpirits
    }

    // values from ReportColumn table, enums represent static values in table which map column names to an integer value. 
    public enum PersistReportColumn
    {
        Default,
        WhiskyUnder160,
        WhiskyOver160,
        BrandyUnder170,
        BrandyOver170,
        Rum,
        Gin,
        Vodka,
        AlcoholUnder190,
        AlcoholOver190,
        Other,
        Wine,
        Total,
        ProofGallons,
        ProofGallonsNewCooperage,
        ProofGallonsUsedCooperage,
        ProofGallonsDepositedInTanks
    }

    /// <summary>
    /// we need this enum here to avoid using magic numbers 
    /// in tests and other workflows
    /// The enum values are matching rows in SpiritTypeReporting tables
    /// </summary>
    public enum ReportSpiritTypes
    {
        Default,
        WhiskyUnder160 = 1,
        WhiskyOver160 = 2,
        BrandyUnder170 = 3,
        BrandyOver170 = 4,
        Rum = 5,
        Gin = 6,
        Vodka = 7,
        AlcoholUnder190 = 8,
        AlcoholOver190 = 9,
        Other = 10,
        Wine = 11,
        Total = 12
    }
    /// <summary>
    /// This enum represents values that are used in Production report part 2 through 4
    /// </summary>
    public enum ReportMaterialKinds
    {
        Default,
        Grain                      = 75,
        Fruit                      = 76,
        Molasses                   = 77,
        EthylSulfate               = 78,
        EthyleneGas                = 79,
        SulphiteLiquor             = 80,
        FromRedistillation         = 81,
        Bourbon_New_Cooperage      = 82,
        Bourbon_Used_Cooperage     = 83,
        Bourbon_Deposited_in_Tanks = 84,
        Corn_New_Cooperage         = 85,
        Corn_Used_Cooperage        = 86,
        Corn_Deposited_in_Tanks    = 87,
        Rye_New_Cooperage          = 88,
        Rye_Used_Cooperage         = 89,
        Rye_Deposited_in_Tanks     = 90,
        Light_New_Cooperage        = 91,
        Light_Used_Cooperage       = 92,
        Light_Deposited_in_Tanks   = 93,
        GrapeBrandy                = 94,
        AllOtherBrandy             = 95,
        NeutralGrapeBrandy         = 96,
        AllOtherNeutralBrandy      = 97
    }
}