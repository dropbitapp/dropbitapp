import Vue from 'vue';
import Router from 'vue-router';

import HomeView from '../workflows/HomeView.vue';
import Purchase from '../workflows/purchase/Purchase.vue';
import PurchaseView from '../workflows/purchase/PurchaseView.vue';
import Production from '../workflows/production/Production.vue';
import ProductionView from '../workflows/production/ProductionView.vue';
import AddProductionView from '../workflows/production/AddProductionView.vue';
import FermentationDetail from '../workflows/production/FermentationDetail.vue';
import DistillationDetail from '../workflows/production/DistillationDetail.vue';
import BlendingDetail from '../workflows/production/BlendingDetail.vue';
import BottlingDetail from '../workflows/production/BottlingDetail.vue';
import FermentationEdit from '../workflows/production/FermentationEdit.vue';
import DistillationEdit from '../workflows/production/DistillationEdit.vue';
import BlendingEdit from '../workflows/production/BlendingEdit.vue';
import BottlingEdit from '../workflows/production/BottlingEdit.vue';

Vue.use(Router);

export default new Router({
  routes: [{
    path: '/',
    name: 'Home',
    component: HomeView,
  },
  {
    path: '/purchase',
    component: Purchase,
    children: [
      {
        path: '',
        component: PurchaseView,
      },
    ],
  },
  {
    path: '/production',
    component: Production,
    children: [
      {
        path: '',
        component: ProductionView,
      },
      {
        path: 'add',
        component: AddProductionView,
      },
      {
        path: 'fermentation/detail/:id',
        component: FermentationDetail,
        props: true,
      },
      {
        path: 'distillation/detail/:id',
        component: DistillationDetail,
        props: true,
      },
      {
        path: 'blending/detail/:id',
        component: BlendingDetail,
        props: true,
      },
      {
        path: 'bottling/detail/:id',
        component: BottlingDetail,
        props: true,
      },
      {
        path: 'fermentation/edit/:id',
        component: FermentationEdit,
        props: true,
      },
      {
        path: 'distillation/edit/:id',
        component: DistillationEdit,
        props: true,
      },
      {
        path: 'blending/edit/:id',
        component: BlendingEdit,
        props: true,
      },
      {
        path: 'bottling/edit/:id',
        component: BottlingEdit,
        props: true,
      }],
  }],
});
