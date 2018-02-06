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
}