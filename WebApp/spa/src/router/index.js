import Vue from 'vue';
import Router from 'vue-router';

import HomeView from '../views/HomeView.vue';
import NotFound from '../views/NotFound.vue';

// purchase imports
import Purchase from '../views/purchase/Purchase.vue';
import PurchaseView from '../views/purchase/PurchaseView.vue';
import AddPurchaseView from '../views/purchase/AddPurchaseView.vue';
import FermentableDetail from '../views/purchase/FermentableDetail.vue';
import FermentedDetail from '../views/purchase/FermentedDetail.vue';
import DistilledDetail from '../views/purchase/DistilledDetail.vue';
import SupplyDetail from '../views/purchase/SupplyDetail.vue';
import AdditiveDetail from '../views/purchase/AdditiveDetail.vue';
import FermentableEdit from '../views/purchase/FermentableEdit.vue';
import FermentedEdit from '../views/purchase/FermentedEdit.vue';
import DistilledEdit from '../views/purchase/DistilledEdit.vue';
import SupplyEdit from '../views/purchase/SupplyEdit.vue';
import AdditiveEdit from '../views/purchase/AdditiveEdit.vue';

// production imports
import Production from '../views/production/Production.vue';
import ProductionView from '../views/production/ProductionView.vue';
import AddProductionView from '../views/production/AddProductionView.vue';
import FermentationDetail from '../views/production/FermentationDetail.vue';
import DistillationDetail from '../views/production/DistillationDetail.vue';
import BlendingDetail from '../views/production/BlendingDetail.vue';
import BottlingDetail from '../views/production/BottlingDetail.vue';
import FermentationEdit from '../views/production/FermentationEdit.vue';
import DistillationEdit from '../views/production/DistillationEdit.vue';
import BlendingEdit from '../views/production/BlendingEdit.vue';
import BottlingEdit from '../views/production/BottlingEdit.vue';

// dictionary imports
import Dictionary from '../views/dictionary/Dictionary.vue';
import DictionaryView from '../views/dictionary/DictionaryView.vue';
import AddDictionaryView from '../views/dictionary/AddDictionaryView.vue';
import SpiritDetail from '../views/dictionary/SpiritDetail.vue';
import VendorDetail from '../views/dictionary/VendorDetail.vue';
import StorageDetail from '../views/dictionary/StorageDetail.vue';
import MaterialDetail from '../views/dictionary/MaterialDetail.vue';
import SpiritEdit from '../views/dictionary/SpiritEdit.vue';
import VendorEdit from '../views/dictionary/VendorEdit.vue';
import StorageEdit from '../views/dictionary/StorageEdit.vue';
import MaterialEdit from '../views/dictionary/MaterialEdit.vue';

// reporting imports
import StorageReport from '../views/reporting/StorageReport.vue';
import ProductionReport from '../views/reporting/ProductionReport.vue';
import ProcessingReport from '../views/reporting/ProcessingReport.vue';

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
  {
    path: '/reporting',
    component: Dictionary,
    children: [{
      path: 'storage',
      component: StorageReport,
    },
    {
      path: 'production',
      component: ProductionReport,
    },
    {
      path: 'processing',
      component: ProcessingReport,
    },
    ],
  },
  {
    path: '*',
    component: NotFound,
  },
  ],
});
