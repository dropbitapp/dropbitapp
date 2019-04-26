import axios from 'axios';

export default {
  namespaced: true,
  state: {
    processing: null,
  },
  mutations: {
    updateProcessing(state, processing) {
      state.processing = processing;
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
  },
  getters: {
    processingPart1Spirit(state) {
      if(state.processing.Part1.BulkIngredients === 'spirit')
      {
        return state.processing.Part1;
      }
      else {
        return null;
      }
    },
    processingPart1Wine(state) {
      if(state.processing.Part1.BulkIngredients === 'wine')
      {
        return state.processing.Part1;
      }
      else {
        return null;
      }
    },
    processingPart2Bottled(state) {
      if(state.processing.Part2.FinishedProduct === 'bottled')
      {
        return state.processing.Part2;
      }
      else {
        return null;
      }
    },
    processingPart2Packaged(state) {
      if(state.processing.Part2.FinishedProduct === 'packaged')
      {
        return state.processing.Part2;
      }
      else {
        return null;
      }
    },
    processingPart4BulkSpiritDumped(state) {
      for (let i = 0; i < state.processing.Part4List.length; i++) {
        if(state.processing.Part4[i].ProcessingSpirits === 'bulkSpiritDumped')
        {
          return state.processing.Part4[i];
        }
        else {
          return null;
        }
      }
    },
    processingPart4Bottled(state) {
      for (let i = 0; i < state.processing.Part4List.length; i++) {
        if(state.processing.Part4[i].ProcessingSpirits === 'bottled')
        {
          return state.processing.Part4[i];
        }
        else {
          return null;
        }
      }
    },
    reportHeader(state) {
      if(state.processing) {
        return state.processing.Header;
      }
      //todo: add handling for Storage and Production when working on them
    },
  },
};
