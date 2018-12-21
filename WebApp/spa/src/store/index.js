import Vue from 'vue';
import Vuex from 'vuex';

import dictionaryModule from './modules/dictionary';
import purchaseModule from './modules/purchase';
import productionModule from './modules/production';
import reportModule from './modules/report';

Vue.use(Vuex);

export default new Vuex.Store({
  modules: {
    dictionary: dictionaryModule,
    purchase: purchaseModule,
    production: productionModule,
    report: reportModule,
  },
});
