import axios from 'axios';
import dateHelper from '../../helpers/date-helper';

export default {
  namespaced: true,
  state: {
    fermentables: null,
    fermented: null,
    distilled: null,
    supplies: null,
    additives: null,
    rawMaterials: null,
    purchaseVendors: null,
    purchaseStorages: null,
    reportingSpiritTypes: null,
  },
  // modify state only through mutations
  mutations: {
    updateFermentables(state, purchases) {
      // eslint-disable-next-line no-param-reassign
      state.fermentables = purchases;
    },
    updateFermented(state, purchases) {
      // eslint-disable-next-line no-param-reassign
      state.fermented = purchases;
    },
    updateDistilled(state, purchases) {
      // eslint-disable-next-line no-param-reassign
      state.distilled = purchases;
    },
    updateSupplies(state, purchases) {
      // eslint-disable-next-line no-param-reassign
      state.supplies = purchases;
    },
    updateAdditives(state, purchases) {
      // eslint-disable-next-line no-param-reassign
      state.additives = purchases;
    },
    rawMaterials(state, rawMaterials) {
      // eslint-disable-next-line no-param-reassign
      state.rawMaterials = rawMaterials;
    },
    updatePurchaseVendors(state, vendors) {
      // eslint-disable-next-line no-param-reassign
      state.purchaseVendors = vendors;
    },
    updatePurchaseStorages(state, storages) {
      // eslint-disable-next-line no-param-reassign
      state.purchaseStorages = storages;
    },
    updateReportingSpiritTypes(state, types) {
      // eslint-disable-next-line no-param-reassign
      state.reportingSpiritTypes = types;
    },
  },
  // actions are for async calls, such as calling an api
  actions: {
    createPurchase({
      // eslint-disable-next-line no-unused-vars
      dispatch,
    }, purchase) {
      if (!purchase) {
        throw new Error('createPurchase: invalid parameters');
      }
      return axios.post('/Purchase/CreatePurchaseRecord', purchase)
        .catch((error) => {
          throw error;
        });
    },
    getPurchases({
      commit,
    }, purchaseType) {
      if (!purchaseType) {
        throw new Error('getPurchases: invalid parameters');
      }
      return axios.get('/Purchase/GetListOfPurchases', {
        params: {
          purchaseType,
        },
      })
        .then((result) => {
          result.data.map((purchase) => {
            // eslint-disable-next-line no-param-reassign
            purchase.PurchaseDate = dateHelper.convertFromUTC(purchase.PurchaseDate);
            return purchase;
          });
          switch (purchaseType) {
            case 'Fermentable':
              commit('updateFermentables', result.data);
              break;
            case 'Fermented':
              commit('updateFermented', result.data);
              break;
            case 'Distilled':
              commit('updateDistilled', result.data);
              break;
            case 'Supply':
              commit('updateSupplies', result.data);
              break;
            case 'Additive':
              commit('updateAdditives', result.data);
              break;
            default:
              throw new Error('getPurchases: invalid parameters');
          }
        })
        .catch((error) => {
          throw error;
        });
    },
    deletePurchase({
      dispatch,
    }, {
      workflow,
      id,
    }) {
      if (!workflow || !id) {
        throw new Error('deletePurchase: invalid parameters');
      }
      const purchase = {
        DeleteRecordID: id,
        DeleteRecordType: workflow,
      };
      return axios.post('/Purchase/DeleteRecord', purchase)
        .then(() => {
          switch (workflow) {
            case 'Fermentable':
            case 'Fermented':
            case 'Distilled':
            case 'Supply':
            case 'Additive':
              dispatch('getPurchases', workflow);
              break;
            default:
              throw new Error('deletePurchase: invalid parameters');
          }
        })
        .catch((error) => {
          throw error;
        });
    },
    // RAW MATERIALS
    getRawMaterialsForPurchase({
      commit,
    }, purchaseType) {
      if (!purchaseType) {
        throw new Error('getRawMaterialsForPurchase: invalid parameters');
      }
      return axios.get('/Purchase/GetRawMaterialList', {
        params: {
          purchaseType,
        },
      })
        .then(result => commit('rawMaterials', result.data))
        .catch((error) => {
          throw error;
        });
    },
    getPurchaseVendors({
      commit,
    }) {
      return axios.get('/Purchase/GetVendorData')
        .then(result => commit('updatePurchaseVendors', result.data))
        .catch((error) => {
          throw error;
        });
    },
    getPurchaseStorages({
      commit,
    }) {
      return axios.get('/Purchase/GetStorageData')
        .then(result => commit('updatePurchaseStorages', result.data))
        .catch((error) => {
          throw error;
        });
    },
    getReportingSpiritTypes({
      commit,
    }) {
      return axios.get('/Purchase/GetReportingSpiritTypes')
        .then(result => commit('updateReportingSpiritTypes', result.data))
        .catch((error) => {
          throw error;
        });
    },
  },
};
