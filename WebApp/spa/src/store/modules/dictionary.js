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
  },
  // modify state only through mutations
  mutations: {
    updateProcessingReportTypes(state, types) {
      state.processingReportTypes = types;
    },
    updateSpirits(state, spirits) {
      state.spirits = spirits;
    },
    updateVendors(state, vendors) {
      state.vendors = vendors;
    },
    updateStorages(state, storages) {
      state.storages = storages;
    },
    updateUnits(state, units) {
      state.units = units;
    },
    updateMaterialCategories(state, categories) {
      state.materialCategories = categories;
    },
    updateRawMaterials(state, materials) {
      state.rawMaterials = materials;
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
    getMaterials({ commit }) {
      return axios.get('/Dictionary/GetRawMaterialList')
        .then(result => commit('updateMaterials', result.data))
        .catch(console.error);
    },
  },
  getters: {
    // getters are useful if you want to filter/sort/etc data before it is accessed
    spiritCount(state) {
      return state.spirits ? state.spirits.length : 0;
    },
  },
};
