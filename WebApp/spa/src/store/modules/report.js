/* eslint no-plusplus: ["error", { "allowForLoopAfterthoughts": true }] */
import axios from 'axios';

export default {
  namespaced: true,
  state: {
    processing: null,
    storage: null,
    production: null,
  },
  mutations: {
    updateProcessing(state, processing) {
      state.processing = processing; // eslint-disable-line no-param-reassign
    },
    updateStorage(state, storage) {
      state.storage = storage; // eslint-disable-line no-param-reassign
    },
    updateProduction(state, production) {
      state.production = production; // eslint-disable-line no-param-reassign
    },
  },
  actions: {
    getProcessing({ commit }, payload) {
      return axios.get('/Reporting/GetProcessingReport', {
        params: {
          startOfReporting: payload.start.toJSON(),
          endOfReporting: payload.end.toJSON(),
        },
      })
        .then(response => commit('updateProcessing', response.data))
        .catch(console.error);
    },
    getStorage({ commit }, payload) {
      return axios.get('/Reporting/GetStorageReport', {
        params: {
          startOfReporting: payload.start.toJSON(),
          endOfReporting: payload.end.toJSON(),
        },
      })
        .then(response => commit('updateStorage', response.data))
        .catch(console.error);
    },
    getProduction({ commit }, payload) {
      return axios.get('/Reporting/GetProductionReport', {
        params: {
          startOfReporting: payload.start.toJSON(),
          endOfReporting: payload.end.toJSON(),
        },
      })
        .then(response => commit('updateProduction', response.data))
        .catch(console.error);
    },
  },
  getters: {
    processingPart1Spirit(state) {
      if (state.processing.Part1.BulkIngredients === 'spirit') {
        return state.processing.Part1;
      }
      return null;
    },
    processingPart1Wine(state) {
      if (state.processing.Part1.BulkIngredients === 'wine') {
        return state.processing.Part1;
      }
      return null;
    },
    processingPart2Bottled(state) {
      if (state.processing.Part2.FinishedProduct === 'bottled') {
        return state.processing.Part2;
      }
      return null;
    },
    processingPart2Packaged(state) {
      if (state.processing.Part2.FinishedProduct === 'packaged') {
        return state.processing.Part2;
      }
      return null;
    },
    processingPart4BulkSpiritDumped(state) {
      for (let i = 0; i < state.processing.Part4List.length; i++) {
        if (state.processing.Part4[i].ProcessingSpirits === 'bulkSpiritDumped') {
          return state.processing.Part4[i];
        }
      }
      return null;
    },
    processingPart4Bottled(state) {
      for (let i = 0; i < state.processing.Part4List.length; i++) {
        if (state.processing.Part4[i].ProcessingSpirits === 'bottled') {
          return state.processing.Part4[i];
        }
      }
      return null;
    },
    storageWhiskyUnder160(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'WhiskyUnder160') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageWhiskyOver160(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'WhiskyOver160') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageBrandyUnder170(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'BrandyUnder170') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageBrandyOver170(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'BrandyOver170') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageRum(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'Rum') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageGin(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'Gin') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageVodka(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'Vodka') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storage190AndOver(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'AlcoholUnder190') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageUnder190(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'AlcoholOver190') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageOther(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'Other') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageWine(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'Wine') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    storageTotal(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if (state.storage.ReportBody[i].CategoryName === 'Total') {
          return state.storage.ReportBody[i];
        }
      }
      return null;
    },
    productionPart1WhiskyUnder160(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'WhiskyUnder160') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1WhiskyOver160(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'WhiskyOver160') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1BrandyUnder170(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'BrandyUnder170') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1BrandyOver170(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'BrandyOver170') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1Rum(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'Rum') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1Gin(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'Gin') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1Vodka(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'Vodka') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1190AndOver(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'AlcoholUnder190') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1Under190(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'AlcoholOver190') {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1Other(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName !== 'AlcoholOver190'
          && state.production.Part1List[i].SpiritCatName !== 'AlcoholUnder190'
          && state.production.Part1List[i].SpiritCatName !== 'Vodka'
          && state.production.Part1List[i].SpiritCatName !== 'Gin'
          && state.production.Part1List[i].SpiritCatName !== 'Rum'
          && state.production.Part1List[i].SpiritCatName !== 'BrandyOver170'
          && state.production.Part1List[i].SpiritCatName !== 'BrandyUnder170'
          && state.production.Part1List[i].SpiritCatName !== 'WhiskyOver160'
          && state.production.Part1List[i].SpiritCatName !== 'WhiskyUnder160'
        ) {
          return state.production.Part1List[i];
        }
      }
      return null;
    },
    productionPart1Total(state) {
      for (let i = 0; i < state.production.Part1List.length; i++) {
        if (state.production.Part1List[i].SpiritCatName === 'Total') {
          const totalsColumn = {
            ProccessingAcct: 0,
            ProducedTotal: 0,
            Recd4RedistilL17: 0,
            Recd4RedistilaltionL15: 0,
            StorageAcct: 0,
            UnfinishedSpiritsEndOfQuarterL17: 0,
          };
          for (let iter = 0; iter < state.production.Part1List.length; iter++) {
            if (state.production.Part1List[iter].SpiritCatName !== 'Total') {
              totalsColumn.ProccessingAcct
              += state.production.Part1List[iter].ProccessingAcct;
              totalsColumn.ProducedTotal
              += state.production.Part1List[iter].ProducedTotal;
              totalsColumn.Recd4RedistilL17
              += state.production.Part1List[iter].Recd4RedistilL17;
              totalsColumn.Recd4RedistilaltionL15
              += state.production.Part1List[iter].Recd4RedistilaltionL15;
              totalsColumn.StorageAcct
              += state.production.Part1List[iter].StorageAcct;
              totalsColumn.UnfinishedSpiritsEndOfQuarterL17
              += state.production.Part1List[iter].UnfinishedSpiritsEndOfQuarterL17;
            }
          }
          return totalsColumn;
        }
      }
      return null;
    },
    productionReportPart2(state) {
      const part2 = {
        grain: '',
        fruit: '',
        molasses: '',
        ethylSulfate: '',
        ethyleneGas: '',
        sulphiteLiquors: '',
        fromRedistillation: '',
      };
      const part2List = state.production.Part2Through4List;
      for (let i = 0; i < part2List.length; i++) {
        if (part2List[i].KindOfMaterial === 'Grain') {
          part2.grain = part2List[i].ProofGallons;
        } else if (part2List[i].KindOfMaterial === 'Fruit') {
          part2.fruit = part2List[i].ProofGallons;
        } else if (part2List[i].KindOfMaterial === 'Molasses') {
          part2.molasses = part2List[i].ProofGallons;
        } else if (part2List[i].KindOfMaterial === 'EthylSulfate') {
          part2.ethylSulfate = part2List[i].ProofGallons;
        } else if (part2List[i].KindOfMaterial === 'EthyleneGas') {
          part2.ethyleneGas = part2List[i].ProofGallons;
        } else if (part2List[i].KindOfMaterial === 'SulphiteLiquor') {
          part2.sulphiteLiquors = part2List[i].ProofGallons;
        } else if (part2List[i].KindOfMaterial === 'FromRedistillation') {
          part2.fromRedistillation = part2List[i].ProofGallons;
        }
      }
      return part2;
    },
    productionReportPart3(state) {
      const part3 = {
        bourbonNewCooperage: '',
        bourbonUsedCooperage: '',
        bourbonDepositedInTanks: '',
        cornNewCooperage: '',
        cornUsedCooperage: '',
        cornDepositedInTanks: '',
        ryeNewCooperage: '',
        ryeUsedCooperage: '',
        ryeDepositedInTanks: '',
        lightNewCooperage: '',
        lightUsedCooperage: '',
        lightDepositedInTanks: '',
      };
      const part3List = state.production.Part2Through4List;
      for (let i = 0; i < part3List.length; i++) {
        if (part3List[i].KindOfMaterial === 'Bourbon-New-Cooperage') {
          part3.bourbonNewCooperage = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Bourbon-Used-Cooperage') {
          part3.bourbonUsedCooperage = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Bourbon-Deposited-in-Tanks') {
          part3.bourbonDepositedInTanks = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Corn-New-Cooperage') {
          part3.cornNewCooperage = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Corn-Used-Cooperage') {
          part3.cornUsedCooperage = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Corn-Deposited-in-Tanks') {
          part3.cornDepositedInTanks = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Rye-New-Cooperage') {
          part3.ryeNewCooperage = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Rye-Used-Cooperage') {
          part3.ryeUsedCooperage = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Rye-Deposited-in-Tanks') {
          part3.ryeDepositedInTanks = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Light-New-Cooperage') {
          part3.lightNewCooperage = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Light-Used-Cooperage') {
          part3.lightUsedCooperage = part3List[i].ProofGallons;
        } else if (part3List[i].KindOfMaterial === 'Light-Deposited-in-Tanks') {
          part3.lightDepositedInTanks = part3List[i].ProofGallons;
        }
      }
      return part3;
    },
    productionReportPart4(state) {
      const part4 = {
        grapeBrandy: '',
        allOtherBrandy: '',
        neutralGrapeBrandy: '',
        allOtherNeutralBrandy: '',
      };
      const part4List = state.production.Part2Through4List;
      for (let i = 0; i < part4List.length; i++) {
        if (part4List[i].KindOfMaterial === 'GrapeBrandy') {
          part4.grapeBrandy = part4List[i].ProofGallons;
        } else if (part4List[i].KindOfMaterial === 'AllOtherBrandy') {
          part4.allOtherBrandy = part4List[i].ProofGallons;
        } else if (part4List[i].KindOfMaterial === 'NeutralGrapeBrandy') {
          part4.neutralGrapeBrandy = part4List[i].ProofGallons;
        } else if (part4List[i].KindOfMaterial === 'AllOtherNeutralBrandy') {
          part4.allOtherNeutralBrandy = part4List[i].ProofGallons;
        }
      }
      return part4;
    },
    productionReportPart5(state) {
      const part5 = {
        line_1:
          {
            spirit: '',
            value: '',
          },
        line_2:
          {
            spirit: '',
            value: '',
          },
        line_3:
          {
            spirit: '',
            value: '',
          },
        line_4:
          {
            spirit: '',
            value: '',
          },
        line_5:
          {
            spirit: '',
            value: '',
          },
        line_6:
          {
            spirit: '',
            value: '',
          },
        line_7:
          {
            spirit: '',
            value: '',
          },
        line_8:
          {
            spirit: '',
            value: '',
          },
      };
      const [part5List] = state.production.part5List;
      for (let i = 0; i < part5List.length; i++) {
        if (i === 0) {
          part5.line_1.spirit = part5List[i].KindofSpirits;
          part5.line_1.value = part5List[i].Proof;
        }
        if (i === 1) {
          part5.line_2.spirit = part5List[i].KindofSpirits;
          part5.line_2.value = part5List[i].Proof;
        }
        if (i === 2) {
          part5.line_3.spirit = part5List[i].KindofSpirits;
          part5.line_3.value = part5List[i].Proof;
        }
        if (i === 3) {
          part5.line_4.spirit = part5List[i].KindofSpirits;
          part5.line_4.value = part5List[i].Proof;
        }
        if (i === 4) {
          part5.line_5.spirit = part5List[i].KindofSpirits;
          part5.line_5.value = part5List[i].Proof;
        }
        if (i === 5) {
          part5.line_6.spirit = part5List[i].KindofSpirits;
          part5.line_6.value = part5List[i].Proof;
        }
        if (i === 6) {
          part5.line_7.spirit = part5List[i].KindofSpirits;
          part5.line_7.value = part5List[i].Proof;
        }
        if (i === 7) {
          part5.line_8.spirit = part5List[i].KindofSpirits;
          part5.line_8.value = part5List[i].Proof;
        }
      }
      return part5;
    },
    productionReportPart6(state) {
      const part6 = {
        grain:
          {
            corn:
            {
              wight: '',
              volume: '',
            },
            rye:
            {
              wight: '',
              volume: '',
            },
            malt:
            {
              wight: '',
              volume: '',
            },
            wheat:
            {
              wight: '',
              volume: '',
            },
            sorghum:
            {
              wight: '',
              volume: '',
            },
            barley:
            {
              wight: '',
              volume: '',
            },
            line_7:
            {
              material: '',
              wight: '',
              volume: '',
            },
            line_8:
            {
              material: '',
              wight: '',
              volume: '',
            },
          },
        fruit:
          {
            grape:
            {
              wight: '',
              volume: '',
            },
            line_10:
            {
              material: '',
              wight: '',
              volume: '',
            },
            line_11:
            {
              material: '',
              wight: '',
              volume: '',
            },
            line_12:
            {
              material: '',
              wight: '',
              volume: '',
            },
            line_13:
            {
              material: '',
              wight: '',
              volume: '',
            },
            line_14:
            {
              material: '',
              wight: '',
              volume: '',
            },
          },
        cane:
          {
            molasses:
            {
              wight: '',
              volume: '',
            },
            line_16:
            {
              material: '',
              wight: '',
              volume: '',
            },
            line_17:
            {
              material: '',
              wight: '',
              volume: '',
            },
            line_18:
            {
              material: '',
              wight: '',
              volume: '',
            },
          },
        other:
          {
            ethyl_sulfate:
            {
              wight: '',
              volume: '',
            },
            ethylene_gas:
            {
              wight: '',
              volume: '',
            },
            sulphite_liquors:
            {
              wight: '',
              volume: '',
            },
            butane_gas:
            {
              wight: '',
              volume: '',
            },
            line_23:
            {
              material: '',
              wight: '',
              volume: '',
            },
          },
      };
      const part6List = state.production.ProdReportPart6List;
      for (let i = 0; i < part6List.length; i++) {
        if (part6List[i].ProdReportMaterialCategoryID === 1) {
          if (part6List[i].KindOfMaterial.toUpperCase() === 'CORN') {
            part6.grain.corn.weight = part6List[i].Weight;
            part6.grain.corn.volume = part6List[i].Volume;
          } else if (part6List[i].KindOfMaterial.toUpperCase() === 'RYE') {
            part6.grain.rye.weight = part6List[i].Weight;
            part6.grain.rye.volume = part6List[i].Volume;
          } else if (part6List[i].KindOfMaterial.toUpperCase() === 'MALT') {
            part6.grain.malt.weight = part6List[i].Weight;
            part6.grain.malt.volume = part6List[i].Volume;
          } else if (part6List[i].KindOfMaterial.toUpperCase() === 'WHEAT') {
            part6.grain.wheat.weight = part6List[i].Weight;
            part6.grain.wheat.volume = part6List[i].Volume;
          } else if (part6List[i].KindOfMaterial.toUpperCase() === 'SORGHUM GRAIN') {
            part6.grain.sorghum.weight = part6List[i].Weight;
            part6.grain.sorghum.volume = part6List[i].Volume;
          } else if (part6List[i].KindOfMaterial.toUpperCase() === 'BARLEY') {
            part6.grain.barley.weight = part6List[i].Weight;
            part6.grain.barley.volume = part6List[i].Volume;
          } else if (part6.grain.line_7.material === '') {
            part6.grain.line_7.material = part6List[i].KindOfMaterial;
            part6.grain.line_7.weight = part6List[i].Weight;
            part6.grain.line_7.volume = part6List[i].Volume;
          } else if (part6.grain.line_8.material === '') {
            part6.grain.line_8.material = part6List[i].KindOfMaterial;
            part6.grain.line_8.weight = part6List[i].Weight;
            part6.grain.line_8.volume = part6List[i].Volume;
          }
        } else if (part6List[i].ProdReportMaterialCategoryID === 2) {
          if (part6List[i].KindOfMaterial.toUpperCase() === 'GRAPE') {
            part6.fruit.grape.weight = part6List[i].Weight;
            part6.fruit.grape.volume = part6List[i].Volume;
          } else if (part6.fruit.line_10.material === '') {
            part6.fruit.line_10.material = part6List[i].KindOfMaterial;
            part6.fruit.line_10.weight = part6List[i].Weight;
            part6.fruit.line_10.volume = part6List[i].Volume;
          } else if (part6.fruit.line_11.material === '') {
            part6.fruit.line_11.material = part6List[i].KindOfMaterial;
            part6.fruit.line_11.weight = part6List[i].Weight;
            part6.fruit.line_11.volume = part6List[i].Volume;
          } else if (part6.fruit.line_12.material === '') {
            part6.fruit.line_12.material = part6List[i].KindOfMaterial;
            part6.fruit.line_12.weight = part6List[i].Weight;
            part6.fruit.line_12.volume = part6List[i].Volume;
          } else if (part6.fruit.line_13.material === '') {
            part6.fruit.line_13.material = part6List[i].KindOfMaterial;
            part6.fruit.line_13.weight = part6List[i].Weight;
            part6.fruit.line_13.volume = part6List[i].Volume;
          } else if (part6.fruit.line_14.material === '') {
            part6.fruit.line_14.material = part6List[i].KindOfMaterial;
            part6.fruit.line_14.weight = part6List[i].Weight;
            part6.fruit.line_14.volume = part6List[i].Volume;
          }
        }
        if (part6List[i].ProdReportMaterialCategoryID === 3) {
          if (part6List[i].KindOfMaterial.toUpperCase() === 'MOLASSES') {
            part6.cane.molasses.weight = part6List[i].Weight;
            part6.cane.molasses.volume = part6List[i].Volume;
          } else if (part6.cane.line_16.material === '') {
            part6.cane.line_16.material = part6List[i].KindOfMaterial;
            part6.cane.line_16.weight = part6List[i].Weight;
            part6.cane.line_16.volume = part6List[i].Volume;
          } else if (part6.cane.line_17.material === '') {
            part6.cane.line_17.material = part6List[i].KindOfMaterial;
            part6.cane.line_17.weight = part6List[i].Weight;
            part6.cane.line_17.volume = part6List[i].Volume;
          } else if (part6.cane.line_18.material === '') {
            part6.cane.line_18.material = part6List[i].KindOfMaterial;
            part6.cane.line_18.weight = part6List[i].Weight;
            part6.cane.line_18.volume = part6List[i].Volume;
          }
        }
        if (part6List[i].ProdReportMaterialCategoryID === 4) {
          if (part6List[i].KindOfMaterial.toUpperCase() === 'ETHYL SULFATE') {
            part6.other.ethyl_sulfate.weight = part6List[i].Weight;
            part6.other.ethyl_sulfate.volume = part6List[i].Volume;
          } else if (part6List[i].KindOfMaterial.toUpperCase() === 'ETHYLENE GAS') {
            part6.other.ethylene_gas.weight = part6List[i].Weight;
            part6.other.ethylene_gas.volume = part6List[i].Volume;
          } else if (part6List[i].KindOfMaterial.toUpperCase() === 'SULPHITE LIQUORS') {
            part6.other.sulphite_liquors.weight = part6List[i].Weight;
            part6.other.sulphite_liquors.volume = part6List[i].Volume;
          } else if (part6List[i].KindOfMaterial.toUpperCase() === 'BUTANE GAS') {
            part6.other.butane_gas.weight = part6List[i].Weight;
            part6.other.butane_gas.volume = part6List[i].Volume;
          } else
          if (part6.other.line_23.material === '') {
            part6.other.line_23.material = part6List[i].KindOfMaterial;
            part6.other.line_23.weight = part6List[i].Weight;
            part6.other.line_23.volume = part6List[i].Volume;
          }
        }
      }
      return part6;
    },

    reportHeader(state) {
      if (state.processing) {
        return state.processing.Header;
      } else if (state.storage) {
        return state.storage.Header;
      } else if (state.production) {
        return state.production.Header;
      }
      return null;
    },
  },
};

