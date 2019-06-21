import axios from 'axios';
import dateHelper from '../../helpers/date-helper';

export default {
  namespaced: true,
  state: {
    sellList: [],
  },
  mutations: {
  },
  actions: {
    createTaxRecords({
      // eslint-disable-next-line no-unused-vars
      dispatch,
    }, taxRecords) {
      /* eslint-disable no-param-reassign */
      if (!taxRecords) {
        throw new Error('createTaxRecords: invalid parameters');
      }
      taxRecords.TaxedRecords.map((taxRecord) => {
        taxRecord.WithdrawalDate = dateHelper.convertToUTC(taxRecord.WithdrawalDate);
        return taxRecord;
      });
      return axios({
        method: 'post',
        url: '/production/CreateTaxRecords',
        headers: {
          'Content-Type': 'application/json charset=utf-8',
        },
        data: JSON.stringify(taxRecords),
      })
      .catch((error) => {
        throw error;
      });
    },
  },
};