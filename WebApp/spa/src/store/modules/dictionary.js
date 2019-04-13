import axios from 'axios';

export default {
  namespaced: true,
  state: {
    processingReportTypes: null,
    spirits: null,
    vendors: null,
    storages: null,
    units: null,
    materialCategories: null,
    rawMaterials: null,
    /*
    {
      MaterialCategoryID: 0
      Note: null
      PurchaseMaterialTypes: null
      RawMaterialId: 0
      RawMaterialName: ''
      UnitType: 'lb'
      UnitTypeId: 0
    }
    */
  },
  // modify state only through mutations
  mutations: {
    updateProcessingReportTypes(state, types) {
      state.processingReportTypes = types; // eslint-disable-line no-param-reassign
    },
    updateSpirits(state, spirits) {
      state.spirits = spirits; // eslint-disable-line no-param-reassign
    },
    updateVendors(state, vendors) {
      state.vendors = vendors; // eslint-disable-line no-param-reassign
    },
    updateStorages(state, storages) {
      state.storages = storages; // eslint-disable-line no-param-reassign
    },
    updateUnits(state, units) {
      state.units = units; // eslint-disable-line no-param-reassign
    },
    updateMaterialCategories(state, categories) {
      state.materialCategories = categories; // eslint-disable-line no-param-reassign
    },
    updateRawMaterials(state, materials) {
      state.rawMaterials = materials; // eslint-disable-line no-param-reassign
    },
  },
  // actions are for async calls, such as calling an api
  actions: {
    getProcessingReportTypes({ commit }) {
      return axios.get('/Dictionary/GetProcessingReportTypes')
        .then(result => commit('updateProcessingReportTypes', result.data))
        .catch(console.error);
    },
    getSpirits({ commit }) {
      return axios.get('/Dictionary/GetSpiritList')
        .then(result => commit('updateSpirits', result.data))
        .catch(console.error);
    },
    addSpirit({ dispatch }, spirit) {
      return axios.post('/Dictionary/CreateSpirit', spirit)
        .then(() => dispatch('getSpirits'))
        .catch(console.error);
    },
    deleteSpirit({ dispatch }, spirit) {
      return axios.post('/Dictionary/DeleteRecord', spirit)
        .then(() => dispatch('getSpirits'))
        .catch(console.error);
    },
    getVendors({ commit }) {
      return axios.get('/Dictionary/GetVendorList')
        .then(result => commit('updateVendors', result.data))
        .catch(console.error);
    },
    getStorages({ commit }) {
      return axios.get('/Dictionary/GetStorageList')
        .then(result => commit('updateStorages', result.data))
        .catch(console.error);
    },
    getUnits({ commit }) {
      return axios.get('Dictionary/GetUnitList')
        .then(result => commit('updateUnits', result.data))
        .catch(console.error);
    },
    getMaterialCategories({ commit }) {
      return axios.get('/Dictionary/GetMaterialCategory')
        .then(result => commit('updateMaterialCategories', result.data))
        .catch(console.error);
    },
    getRawMaterials({ commit }) {
      return axios.get('/Dictionary/GetRawMaterialList')
        .then(result => commit('updateRawMaterials', result.data))
        .catch(console.error);
    },
  },
  getters: {
    // getters are useful if you want to filter/sort/etc data before it is accessed
    spiritCount(state) {
      return 0;
      // state.spirits ? state.spirits.length : 0;
    },
  },
};
