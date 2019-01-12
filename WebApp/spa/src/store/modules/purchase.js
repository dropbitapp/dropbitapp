import axios from 'axios';

export default {
  namespaced: true,
  state: {
  /*
  {
    AlcoholContent: 0
    Gauged: false
    Note: null
    Price: ''
    ProofGallon: 0
    PurBatchName: ''
    PurchaseDate: "/Date(1518451200000)/"
    PurchaseId: 0
    PurchaseType: ''
    Quantity: 0
    RawMaterialId: 0
    RecordId: 0
    RecordName: ''
    SpiritTypeReportingID: 0
    State: ''
    StateID: 0
    Status: ''
    StatusID: 0
    Storage: [{…}]
      Capacity: 0
      Note: null
      SerialNumber: null
      StorageId: 0
      StorageName: ''
    StorageId: 0
    StorageName: null
    UnitOfMeasurementId: 0
    UnitOfMeasurementName: null
    VendorId: 0
    VendorName: ''
    VolumeByWeight: 0
    }
    */
    fermentable: null,
    fermented: null,
    distilled: null,
    additive: null,
    supply: null,
  },
  mutations: {
    updateFermentable(state, fermentable) {
      state.fermentable = fermentable; // eslint-disable-line no-param-reassign
    },
    updateFermented(state, fermented) {
      state.fermented = fermented; // eslint-disable-line no-param-reassign
    },
    updateDistilled(state, distilled) {
      state.distilled = distilled; // eslint-disable-line no-param-reassign
    },
    updateAdditive(state, additive) {
      state.additive = additive; // eslint-disable-line no-param-reassign
    },
    updateSupply(state, supply) {
      state.supply = supply; // eslint-disable-line no-param-reassign
    },
  },
  actions: {
    getFermentable({ commit }) {
      const pType = 'Fermentable';
      return axios.get('/Purchase/GetListOfPurchases', {
        params: {
          purchaseType: pType,
        },
      }).then(result => commit('updateFermentable', result.data))
        .catch(console.error);
    },
    getFermented({ commit }) {
      const pType = 'Fermented';
      return axios.get('/Purchase/GetListOfPurchases', {
        params: {
          purchaseType: pType,
        },
      }).then(result => commit('updateFermented', result.data))
        .catch(console.error);
    },
    getDistilled({ commit }) {
      const pType = 'Distilled';
      return axios.get('/Purchase/GetListOfPurchases', {
        params: {
          purchaseType: pType,
        },
      }).then(result => commit('updateDistilled', result.data))
        .catch(console.error);
    },
    getSupplies({ commit }) {
      const pType = 'Supply';
      return axios.get('/Purchase/GetListOfPurchases', {
        params: {
          purchaseType: pType,
        },
      }).then(result => commit('updateAdditive', result.data))
        .catch(console.error);
    },
    getAdditives({ commit }) {
      const pType = 'Additive';
      return axios.get('/Purchase/GetListOfPurchases', {
        params: {
          purchaseType: pType,
        },
      }).then(result => commit('updateSupply', result.data))
        .catch(console.error);
    },
  },
};
