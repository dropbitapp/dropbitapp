import Vue from 'vue';
import Buefy from 'buefy';
import VueScrollTo from 'vue-scrollto';
import VeeValidate from 'vee-validate';
import 'buefy/dist/buefy.css';
import 'bulma-o-steps/bulma-steps.min.css';

import App from './App.vue';
import router from './router';
import store from './store';

Vue.config.productionTip = false;

Vue.use(Buefy);
Vue.use(VeeValidate);
Vue.use(VueScrollTo, {
  container: 'body',
  duration: 500,
  easing: 'ease',
  offset: -60,
});

new Vue({
  render: h => h(App),
  router,
  store,
}).$mount('#app');
