import axios from 'axios';
import dateHelper from '../../helpers/date-helper';

export default {
  namespaced: true,
  state: {
    fermentations: null,
    distillations: null,
    blendings: null,
    bottlings: null,
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
  },
  // actions are for async calls, such as calling an api
  actions: {
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
          // TODO: Implement front-end logging
          // TODO: Remove all console.log()
          console.log(error);
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
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
  },
};
