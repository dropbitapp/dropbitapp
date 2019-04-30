import axios from 'axios';

export default {
  namespaced: true,
  state: {
    processing: null,
    storage: null,
  },
  mutations: {
    updateProcessing(state, processing) {
      state.processing = processing;
    },
    updateStorage(state, storage) {
      state.storage = storage;
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
        .then((response) => commit('updateProcessing', response.data))
        .catch(console.error);
    },
    getStorage({ commit }, payload) {
      return axios.get('/Reporting/GetStorageReport', {
        params: {
          startOfReporting: payload.start.toJSON(),
          endOfReporting: payload.end.toJSON(),
        },
      })
        .then((response) => commit('updateStorage', response.data))
        .catch(console.error);
    },
  },
  getters: {
    processingPart1Spirit(state) {
      if(state.processing.Part1.BulkIngredients === 'spirit')
      {
        return state.processing.Part1;
      } else {
        return null;
      }
    },
    processingPart1Wine(state) {
      if(state.processing.Part1.BulkIngredients === 'wine')
      {
        return state.processing.Part1;
      } else {
        return null;
      }
    },
    processingPart2Bottled(state) {
      if(state.processing.Part2.FinishedProduct === 'bottled')
      {
        return state.processing.Part2;
      } else {
        return null;
      }
    },
    processingPart2Packaged(state) {
      if(state.processing.Part2.FinishedProduct === 'packaged')
      {
        return state.processing.Part2;
      } else {
        return null;
      }
    },
    processingPart4BulkSpiritDumped(state) {
      for (let i = 0; i < state.processing.Part4List.length; i++) {
        if(state.processing.Part4[i].ProcessingSpirits === 'bulkSpiritDumped')
        {
          return state.processing.Part4[i];
        } else {
          return null;
        }
      }
    },
    processingPart4Bottled(state) {
      for (let i = 0; i < state.processing.Part4List.length; i++) {
        if(state.processing.Part4[i].ProcessingSpirits === 'bottled')
        {
          return state.processing.Part4[i];
        } else {
          return null;
        }
      }
    },
    storageWhiskyUnder160(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'WhiskyUnder160')
        {
          return state.storage.ReportBody[i];
        } else {
          return null;
        }
      }
    },
    storageWhiskyOver160(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'WhiskyOver160')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageBrandyUnder170(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'BrandyUnder170')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageBrandyOver170(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'BrandyOver170')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageRum(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'Rum')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageGin(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'Gin')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageVodka(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'Vodka')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storage190AndOver(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'AlcoholUnder190')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageUnder190(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'AlcoholOver190')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageOther(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'Other')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageWine(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'Wine')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    storageTotal(state) {
      for (let i = 0; i < state.storage.ReportBody.length; i++) {
        if(state.storage.ReportBody[i].CategoryName === 'Total')
        {
          return state.storage.ReportBody[i];
        }
      }
    },
    reportHeader(state) {
      if(state.processing) {
        return state.processing.Header;
      } else if (state.storage) {
        return state.storage.Header;
      }
      // todo: add handling for Production when working on them
    },
  },
};
