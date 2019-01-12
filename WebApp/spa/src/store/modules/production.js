import axios from 'axios';

export default {
  namespaced: true,
  state: {
  /*
  {
    AlcoholContent: 0
    BatchName: ''
    BlendingAdditives: null
    BottlingInfo: null
    DistilledFrom: null
    FillTestList: null
    GainLoss: 0
    Gauged: false
    MaterialKindReportingID: 0
    Note: null
    ProductionDate: "/Date(1518624000000)/"
    ProductionEnd: "/Date(1518624000000)/"
    ProductionId: 0
    ProductionStart: "/Date(1518624000000)/"
    ProductionType: ''
    ProductionTypeId: 0
    ProofGallon: 0
    PurchaseId: 0
    PurchaseIdList: null
    Quantity: 0
    RecordId: null
    RecordIds: null
    RecordName: null
    SpiritCutId: 9
    SpiritCutName: ''
    SpiritId: 0
    SpiritName: ''
    SpiritTypeReportingID: 0
    StatusName: null
    Storage: [{…}]
        Capacity: 0
        Note: null
        SerialNumber: null
        StorageId: 0
        StorageName: ''
    StorageId: 0
    StorageName: null
    TaxedProof: 0
    UsedMats: null
    VolumeByWeight: 0
    WithdrawalDate: "/Date(-62135568000000)/"
  }
  */
    production: null,
    fermented: null,
    distilled: null,
    blended: null,
    bottled: null,
  },
  mutations: {
    updateProductions(state, production) {
      state.production = production; // eslint-disable-line no-param-reassign
    },
    updateFermented(state, fermented) {
      state.fermented = fermented; // eslint-disable-line no-param-reassign
    },
    updateDistilled(state, distilled) {
      state.distilled = distilled; // eslint-disable-line no-param-reassign
    },
    updateBlended(state, blended) {
      state.blended = blended; // eslint-disable-line no-param-reassign
    },
    updateBottled(state, bottled) {
      state.bottled = bottled; // eslint-disable-line no-param-reassign
    },
  },
  actions: {
    getProductions({ commit }) {
      const pType = 'Distillation';
      return axios.get('/Production/GetProductionData', {
        params: {
          prodType: pType,
        },
      }).then(result => commit('updatedProductions', result.data))
        .catch(console.error);
    },
    getFermented({ commit }) {
      const pType = 'Fermentation';
      return axios.get('/Production/GetProductionData', {
        params: {
          prodType: pType,
        },
      }).then(result => commit('updateFermented', result.data))
        .catch(console.error);
    },
    getDistilled({ commit }) {
      const pType = 'Distillation';
      return axios.get('/Production/GetProductionData', {
        params: {
          prodType: pType,
        },
      }).then(result => commit('updateDistilled', result.data))
        .catch(console.error);
    },
    getBlended({ commit }) {
      const pType = 'Blending';
      return axios.get('/Production/GetProductionData', {
        params: {
          prodType: pType,
        },
      }).then(result => commit('updateBlended', result.data))
        .catch(console.error);
    },
    getBottled({ commit }) {
      const pType = 'Bottling';
      return axios.get('/Production/GetProductionData', {
        params: {
          prodType: pType,
        },
      }).then(result => commit('updateBottled', result.data))
        .catch(console.error);
    },
  },
};
