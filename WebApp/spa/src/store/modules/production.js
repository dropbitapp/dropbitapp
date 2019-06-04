import axios from 'axios';
import dateHelper from '../../helpers/date-helper';

export default {
  namespaced: true,
  state: {
    fermentations: null,
    distillations: null,
    blendings: null,
    bottlings: null,
    rawMaterialsForFermentation: null,
    materialsForProduction: null,
    storages: null,
    reportingSpiritTypes: null,
    spiritCuts: null,
  },
  // modify state only through mutations
  mutations: {
    updateFermentations(state, productions) {
      // eslint-disable-next-line no-param-reassign
      state.fermentations = productions;
    },
    updateDistillations(state, productions) {
      // eslint-disable-next-line no-param-reassign
      state.distillations = productions;
    },
    updateBlendings(state, productions) {
      // eslint-disable-next-line no-param-reassign
      state.blendings = productions;
    },
    updateBottlings(state, productions) {
      // eslint-disable-next-line no-param-reassign
      state.bottlings = productions;
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
    updateReportingSpiritTypes(state, types) {
      // eslint-disable-next-line no-param-reassign
      state.reportingSpiritTypes = types;
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
      if (!production) {
        throw new Error('createProduction: invalid parameters');
      }
      return axios.post('/Production/CreateProductionRecord', production)
        .catch((error) => {
          throw error;
        });
    },
    getProductions({
      commit,
    }, productionType) {
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
            // eslint-disable-next-line no-param-reassign
            production.ProductionDate = dateHelper.convertFromUTC(production.ProductionDate);
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
