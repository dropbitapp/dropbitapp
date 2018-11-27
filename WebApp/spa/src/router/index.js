import Vue from 'vue';
import Router from 'vue-router';

import HomeView from '../views/HomeView.vue';
import PurchaseView from '../views/purchase/PurchaseView.vue';
import ProductionView from '../views/production/ProductionView.vue';
import ProductionCreateView from '../views/production/ProductionCreateView.vue';
import ProductionDetailView from '../views/production/ProductionDetailView.vue';

Vue.use(Router);

export default new Router({
  routes: [{
    path: '/',
    name: 'Home',
    component: HomeView,
  },
  {
    path: '/purchase',
    name: 'Purchase',
    component: PurchaseView,
  },
  {
    path: '/production',
    name: 'Production',
    component: ProductionView,
  },
  {
    path: '/production/add',
    name: 'ProductionCreate',
    component: ProductionCreateView,
  },
  {
    path: '/production/:productionType/:id',
    name: 'ProductionDetail',
    component: ProductionDetailView,
    props: true,
  }],
});
