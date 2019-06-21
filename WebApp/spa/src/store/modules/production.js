import axios from 'axios';
import dateHelper from '../../helpers/date-helper';

export default {
  namespaced: true,
  state: {
    fermentations: null,
    distillations: null,
    blendings: null,
    bottlings: null,
    fillList: [],
    blendsForBottling: null,
    rawMaterialsForFermentation: null,
    materialsForProduction: null,
    storages: null,
    spiritTypes: null,
    reportingSpiritTypes: null,
    additives: null,
    spiritCuts: null,
  },
  // modify state only through mutations
  mutations: {
    updateFermentations(state, productions) {
      /* eslint-disable no-param-reassign */
      state.fermentations = productions;
    },
    updateDistillations(state, productions) {
      /* eslint-disable no-param-reassign */
      state.distillations = productions;
    },
    updateBlendings(state, productions) {
      /* eslint-disable no-param-reassign */
      state.blendings = productions;
    },
    updateBottlings(state, productions) {
      /* eslint-disable no-param-reassign */
      state.bottlings = productions;
    },
    updateBlendsForBottling(state, blends) {
      /* eslint-disable no-param-reassign */
      state.blendsForBottling = blends;
    },
    updateFillTest(state, fillTest) {
      state.fillList.push(fillTest);
    },
    updateRawMaterialsForFermentation(state, rawMaterials) {
      // eslint-disable-next-line no-param-reassign
      state.rawMaterialsForFermentation = rawMaterials;
    },
    updateMaterialsForProduction(state, materials) {
      // eslint-disable-next-line no-param-reassign
      state.materialsForProduction = materials;
    },
    updateStorages(state, storages) {
      // eslint-disable-next-line no-param-reassign
      state.storages = storages;
    },
    updateSpiritTypes(state, types) {
      // eslint-disable-next-line no-param-reassign
      state.spiritTypes = types;
    },
    updateReportingSpiritTypes(state, types) {
      // eslint-disable-next-line no-param-reassign
      state.reportingSpiritTypes = types;
    },
    updateAdditives(state, additives) {
      // eslint-disable-next-line no-param-reassign
      state.additives = additives;
    },
    updateSpiritCuts(state, cuts) {
      // eslint-disable-next-line no-param-reassign
      state.spiritCuts = cuts;
    },
  },
  // actions are for async calls, such as calling an api
  actions: {
    createProduction({
      // eslint-disable-next-line no-unused-vars
      dispatch,
    }, production) {
      /* eslint-disable no-param-reassign */
      if (!production) {
        throw new Error('createProduction: invalid parameters');
      }

      // convert production dates to UTC
      production.ProductionDate =
      dateHelper.convertToUTC(production.ProductionDate);
      production.ProductionStart =
      dateHelper.convertToUTC(production.ProductionStart);
      production.ProductionEnd =
      dateHelper.convertToUTC(production.ProductionEnd);

      return axios.post('/Production/CreateProductionRecord', production)
        .then((result) => {
          dispatch('updateBottlings', production);
        })
        .catch((error) => {
          throw error;
        });
    },
    getProductions({
      commit,
    }, productionType) {
      /* eslint-disable no-param-reassign */
      if (!productionType) {
        throw new Error('getProductions: invalid parameters');
      }
      return axios.get('/Production/GetProductionData', {
        params: {
          prodType: productionType,
        },
      })
        .then((result) => {
          result.data.map((production) => {
            production.ProductionDate =
            dateHelper.convertFromUTC(production.ProductionDate).toLocaleDateString();
            production.ProductionStart =
            dateHelper.convertFromUTC(production.ProductionStart).toLocaleDateString();
            production.ProductionEnd =
            dateHelper.convertFromUTC(production.ProductionEnd).toLocaleDateString();
            return production;
          });
          switch (productionType) {
            case 'Fermentation':
              commit('updateFermentations', result.data);
              break;
            case 'Distillation':
              commit('updateDistillations', result.data);
              break;
            case 'Blending':
              commit('updateBlendings', result.data);
              break;
            case 'Bottling':
              commit('updateBottlings', result.data);
              break;
            default:
              throw new Error('getProductions: invalid parameters');
          }
        })
        .catch((error) => {
          throw error;
        });
    },
    deleteProduction({
      dispatch,
    }, {
      workflow,
      id,
    }) {
      if (!workflow || !id) {
        throw new Error('deleteProduction: invalid parameters');
      }
      const production = {
        DeleteRecordID: id,
        DeleteRecordType: workflow,
      };
      return axios.post('/Production/DeleteRecord', production)
        .then(() => {
          switch (workflow) {
            case 'Fermentation':
            case 'Distillation':
            case 'Blending':
            case 'Bottling':
              dispatch('getProductions', workflow);
              break;
            default:
              throw new Error('deleteProductions: invalid parameters');
          }
        })
        .catch((error) => {
          throw error;
        });
    },
    addFillTest({ commit }, fillTest) {
      commit('updateFillTest', fillTest);
    },
    getRecordsAvaiableForBottling({ commit }, productionType) {
      /* eslint-disable no-param-reassign */
      if (!productionType) {
        throw new Error('getRecordsAvaiableForBottling: productionType is null or undefined');
      }
      return axios.get('/Production/GetBlendingData', {
        params: {
          prodType: productionType,
        },
      })
        .then((result) => {
          result.data.map((production) => {
            production.ProductionEndDate =
            dateHelper.convertFromUTC(production.ProductionEndDate).toLocaleDateString();
            return production;
          });
          commit('updateBlendsForBottling', result.data);
        });
    },
    getRawMaterialsForFermentation({
      commit,
    }) {
      return axios.get('/Production/GetRawMaterialList')
        .then((result) => {
          result.data.map((purchase) => {
            // eslint-disable-next-line no-param-reassign
            purchase.PurchaseDate = dateHelper.convertFromUTC(purchase.PurchaseDate);
            return purchase;
          });
          commit('updateRawMaterialsForFermentation', result.data);
        })
        .catch((error) => {
          throw error;
        });
    },
    getMaterialsForProduction({
      commit,
    }, productionType) {
      if (!productionType) {
        throw new Error('getMaterialsForProduction: invalid parameters');
      }
      return axios.get('/Production/GetMaterialListForProduction', {
        params: {
          productionType,
        },
      })
        .then((result) => {
          result.data.map((purchase) => {
            try {
            // eslint-disable-next-line no-param-reassign
              purchase.PurchaseDate = dateHelper.convertFromUTC(purchase.PurchaseDate);
            } catch (error) {
              // eslint-disable-next-line no-param-reassign
              purchase.PurchaseDate = new Date(0, 0, 0, 0, 0, 0, 0);
            }
            return purchase;
          });
          commit('updateMaterialsForProduction', result.data);
        })
        .catch((error) => {
          throw error;
        });
    },
    getAdditives({
      commit,
    }, materialType) {
      if (!materialType) {
        throw new Error('getAdditives: invalid parameters');
      }
      return axios.get('/Production/GetAdditivesList', {
        params: {
          matType: materialType,
        },
      })
        .then((result) => {
          commit('updateAdditives', result.data);
        })
        .catch((error) => {
          throw error;
        });
    },
    getStorages({
      commit,
    }) {
      return axios.get('/Production/GetStorageData')
        .then((result) => {
          commit('updateStorages', result.data);
        })
        .catch((error) => {
          throw error;
        });
    },
    getSpiritTypes({
      commit,
    }) {
      return axios.get('/Production/GetSpiritTypes')
        .then((result) => {
          commit('updateSpiritTypes', result.data);
        })
        .catch((error) => {
          throw error;
        });
    },
    getReportingSpiritTypes({
      commit,
    }) {
      return axios.get('/Production/GetSpiritToKindListData')
        .then((result) => {
          commit('updateReportingSpiritTypes', result.data);
        })
        .catch((error) => {
          throw error;
        });
    },
    getSpiritCuts({
      commit,
    }) {
      return axios.get('/Production/GetSpiritCutData')
        .then((result) => {
          commit('updateSpiritCuts', result.data);
        })
        .catch((error) => {
          throw error;
        });
    },
  },
};
