import axios from 'axios';
import parseDateString from '../../helpers/parse-date-string';

export default {
  namespaced: true,
  state: {
    fermentables: null,
    fermented: null,
    distilled: null,
    supplies: null,
    additives: null,
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
  },
  // actions are for async calls, such as calling an api
  actions: {
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
            purchase.PurchaseDate = parseDateString(purchase.PurchaseDate, false);
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
          // TODO: Implement front-end logging
          // TODO: Remove all console.log()
          console.log(error);
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
          // TODO: Implement front-end logging
          console.log(error);
          throw error;
        });
    },
  },
};
