import Vue from 'vue';
import Router from 'vue-router';

import HomeView from '../workflows/HomeView.vue';

// purchase imports
import Purchase from '../workflows/purchase/Purchase.vue';
import PurchaseView from '../workflows/purchase/PurchaseView.vue';
import AddPurchaseView from '../workflows/purchase/AddPurchaseView.vue';
import FermentableDetail from '../workflows/purchase/FermentableDetail.vue';
import FermentedDetail from '../workflows/purchase/FermentedDetail.vue';
import DistilledDetail from '../workflows/purchase/DistilledDetail.vue';
import SupplyDetail from '../workflows/purchase/SupplyDetail.vue';
import AdditiveDetail from '../workflows/purchase/AdditiveDetail.vue';
import FermentableEdit from '../workflows/purchase/FermentableEdit.vue';
import FermentedEdit from '../workflows/purchase/FermentedEdit.vue';
import DistilledEdit from '../workflows/purchase/DistilledEdit.vue';
import SupplyEdit from '../workflows/purchase/SupplyEdit.vue';
import AdditiveEdit from '../workflows/purchase/AdditiveEdit.vue';

// production imports
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

// dictionary imports
import Dictionary from '../workflows/dictionary/Dictionary.vue';
import DictionaryView from '../workflows/dictionary/DictionaryView.vue';
import AddDictionaryView from '../workflows/dictionary/AddDictionaryView.vue';
import SpiritDetail from '../workflows/dictionary/SpiritDetail.vue';
import VendorDetail from '../workflows/dictionary/VendorDetail.vue';
import StorageDetail from '../workflows/dictionary/StorageDetail.vue';
import MaterialDetail from '../workflows/dictionary/MaterialDetail.vue';
import SpiritEdit from '../workflows/dictionary/SpiritEdit.vue';
import VendorEdit from '../workflows/dictionary/VendorEdit.vue';
import StorageEdit from '../workflows/dictionary/StorageEdit.vue';
import MaterialEdit from '../workflows/dictionary/MaterialEdit.vue';

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
    children: [{
      path: '',
      component: PurchaseView,
    },
    {
      path: 'add',
      component: AddPurchaseView,
    },
    {
      path: 'fermentable/detail/:id',
      component: FermentableDetail,
      props: true,
    },
    {
      path: 'fermented/detail/:id',
      component: FermentedDetail,
      props: true,
    },
    {
      path: 'distilled/detail/:id',
      component: DistilledDetail,
      props: true,
    },
    {
      path: 'supply/detail/:id',
      component: SupplyDetail,
      props: true,
    },
    {
      path: 'additive/detail/:id',
      component: AdditiveDetail,
      props: true,
    },
    {
      path: 'fermentable/edit/:id',
      component: FermentableEdit,
      props: true,
    },
    {
      path: 'fermented/edit/:id',
      component: FermentedEdit,
      props: true,
    },
    {
      path: 'distilled/edit/:id',
      component: DistilledEdit,
      props: true,
    },
    {
      path: 'supply/edit/:id',
      component: SupplyEdit,
      props: true,
    },
    {
      path: 'additive/edit/:id',
      component: AdditiveEdit,
      props: true,
    },
    ],
  },
  {
    path: '/production',
    component: Production,
    children: [{
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
    },
    ],
  },
  {
    path: '/dictionary',
    component: Dictionary,
    children: [{
      path: '',
      component: DictionaryView,
    },
    {
      path: 'add',
      component: AddDictionaryView,
    },
    {
      path: 'spirit/detail/:id',
      component: SpiritDetail,
      props: true,
    },
    {
      path: 'vendor/detail/:id',
      component: VendorDetail,
      props: true,
    },
    {
      path: 'storage/detail/:id',
      component: StorageDetail,
      props: true,
    },
    {
      path: 'material/detail/:id',
      component: MaterialDetail,
      props: true,
    },
    {
      path: 'spirit/edit/:id',
      component: SpiritEdit,
      props: true,
    },
    {
      path: 'vendor/edit/:id',
      component: VendorEdit,
      props: true,
    },
    {
      path: 'storage/edit/:id',
      component: StorageEdit,
      props: true,
    },
    {
      path: 'material/edit/:id',
      component: MaterialEdit,
      props: true,
    },
    ],
  },
  ],
});
