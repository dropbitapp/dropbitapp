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
      // eslint-disable-next-line no-param-reassign
      state.processingReportTypes = types;
    },
    updateSpirits(state, spirits) {
      // eslint-disable-next-line no-param-reassign
      state.spirits = spirits;
    },
    updateVendors(state, vendors) {
      // eslint-disable-next-line no-param-reassign
      state.vendors = vendors;
    },
    updateStorages(state, storages) {
      // eslint-disable-next-line no-param-reassign
      state.storages = storages;
    },
    updateUnits(state, units) {
      // eslint-disable-next-line no-param-reassign
      state.units = units;
    },
    updateMaterialCategories(state, categories) {
      // eslint-disable-next-line no-param-reassign
      state.materialCategories = categories;
    },
    updateRawMaterials(state, materials) {
      // eslint-disable-next-line no-param-reassign
      state.rawMaterials = materials;
    },
  },
  // actions are for async calls, such as calling an api
  actions: {
    getProcessingReportTypes({
      commit,
    }) {
      return axios.get('/Dictionary/GetProcessingReportTypes')
        .then(result => commit('updateProcessingReportTypes', result.data))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    // SPIRITS
    getSpirits({
      commit,
    }) {
      return axios.get('/Dictionary/GetSpiritList')
        .then(result => commit('updateSpirits', result.data))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    createSpirit({
      dispatch,
    }, spirit) {
      if (!spirit) {
        throw new Error('createSpirit: invalid parameters');
      }
      return axios.post('/Dictionary/CreateSpirit', spirit)
        .then(() => dispatch('getSpirits'))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    deleteSpirit({
      dispatch,
    }, spiritId) {
      const spirit = {
        DeleteRecordID: spiritId,
        DeleteRecordType: 'Spirit',
      };
      return axios.post('/Dictionary/DeleteRecord', spirit)
        .then(() => dispatch('getSpirits'))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    // VENDORS
    getVendors({
      commit,
    }) {
      return axios.get('/Dictionary/GetVendorList')
        .then(result => commit('updateVendors', result.data))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    createVendor({
      dispatch,
    }, vendor) {
      if (!vendor) {
        throw new Error('createVendor: invalid parameters');
      }
      return axios.post('/Dictionary/CreateVendor', vendor)
        .then(() => dispatch('getVendors'))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    deleteVendor({
      dispatch,
    }, vendorId) {
      const vendor = {
        DeleteRecordID: vendorId,
        DeleteRecordType: 'Vendor',
      };
      return axios.post('/Dictionary/DeleteRecord', vendor)
        .then(() => dispatch('getVendors'))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    // STORAGES
    getStorages({
      commit,
    }) {
      return axios.get('/Dictionary/GetStorageList')
        .then(result => commit('updateStorages', result.data))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    createStorage({
      dispatch,
    }, storage) {
      if (!storage) {
        throw new Error('createStorage: invalid parameters');
      }
      return axios.post('/Dictionary/CreateStorage', storage)
        .then(() => dispatch('getStorages'))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    deleteStorage({
      dispatch,
    }, storageId) {
      const storage = {
        DeleteRecordID: storageId,
        DeleteRecordType: 'Storage',
      };
      return axios.post('/Dictionary/DeleteRecord', storage)
        .then(() => dispatch('getStorages'))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    // RAW MATERIALS
    getRawMaterials({
      commit,
    }) {
      return axios.get('/Dictionary/GetRawMaterialList')
        .then(result => commit('updateRawMaterials', result.data))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    createRawMaterial({
      dispatch,
    }, rawMaterial) {
      if (!rawMaterial) {
        throw new Error('createRawMaterial: invalid parameters');
      }
      return axios.post('/Dictionary/CreateRawMaterial', rawMaterial)
        .then(() => dispatch('getRawMaterials'))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    deleteRawMaterial({
      dispatch,
    }, rawMaterialId) {
      const spirit = {
        DeleteRecordID: rawMaterialId,
        DeleteRecordType: 'RawMaterial',
      };
      return axios.post('/Dictionary/DeleteRecord', spirit)
        .then(() => dispatch('getRawMaterials'))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    getUnits({
      commit,
    }) {
      return axios.get('/Dictionary/GetUnitList')
        .then(result => commit('updateUnits', result.data))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
    getMaterialCategories({
      commit,
    }) {
      return axios.get('/Dictionary/GetMaterialCategory')
        .then(result => commit('updateMaterialCategories', result.data))
        .catch((error) => {
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
  },
  getters: {
    // getters are useful if you want to filter/sort/etc data before it is accessed
    spiritCount(state) {
      return state.spirits ? state.spirits.length : 0;
    },
  },
};
