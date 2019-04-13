import Vue from 'vue';
import Router from 'vue-router';

import HomeView from '../views/HomeView.vue';

// lazy load generic views
const NotFound = () => import('../views/NotFound.vue');

// lazy load mockup views
const Mockup = () => import('../mockup/Mockup.vue');
const AddRecord = () => import('../mockup/AddRecord.vue');
const EditRecord = () => import('../mockup/EditRecord.vue');
const RecordDetail = () => import('../mockup/RecordDetail.vue');
const ViewRecords = () => import('../mockup/ViewRecords.vue');

// lazy load purchase views
const Purchase = () => import('../views/purchase/Purchase.vue');
const PurchaseView = () => import('../views/purchase/PurchaseView.vue');
const AddPurchaseView = () => import('../views/purchase/AddPurchaseView.vue');
const FermentableDetail = () => import('../views/purchase/FermentableDetail.vue');
const FermentedDetail = () => import('../views/purchase/FermentedDetail.vue');
const DistilledDetail = () => import('../views/purchase/DistilledDetail.vue');
const SupplyDetail = () => import('../views/purchase/SupplyDetail.vue');
const AdditiveDetail = () => import('../views/purchase/AdditiveDetail.vue');
const FermentableEdit = () => import('../views/purchase/FermentableEdit.vue');
const FermentedEdit = () => import('../views/purchase/FermentedEdit.vue');
const DistilledEdit = () => import('../views/purchase/DistilledEdit.vue');
const SupplyEdit = () => import('../views/purchase/SupplyEdit.vue');
const AdditiveEdit = () => import('../views/purchase/AdditiveEdit.vue');

// lazy load production views
const Production = () => import('../views/production/Production.vue');
const ProductionView = () => import('../views/production/ProductionView.vue');
const AddProductionView = () => import('../views/production/AddProductionView.vue');
const FermentationDetail = () => import('../views/production/FermentationDetail.vue');
const DistillationDetail = () => import('../views/production/DistillationDetail.vue');
const BlendingDetail = () => import('../views/production/BlendingDetail.vue');
const BottlingDetail = () => import('../views/production/BottlingDetail.vue');
const FermentationEdit = () => import('../views/production/FermentationEdit.vue');
const DistillationEdit = () => import('../views/production/DistillationEdit.vue');
const BlendingEdit = () => import('../views/production/BlendingEdit.vue');
const BottlingEdit = () => import('../views/production/BottlingEdit.vue');

// lazy load dictionary views
const Dictionary = () => import('../views/dictionary/Dictionary.vue');
const DictionaryView = () => import('../views/dictionary/DictionaryView.vue');
const AddDictionaryView = () => import('../views/dictionary/AddDictionaryView.vue');
const SpiritDetail = () => import('../views/dictionary/SpiritDetail.vue');
const VendorDetail = () => import('../views/dictionary/VendorDetail.vue');
const StorageDetail = () => import('../views/dictionary/StorageDetail.vue');
const MaterialDetail = () => import('../views/dictionary/MaterialDetail.vue');
const SpiritEdit = () => import('../views/dictionary/SpiritEdit.vue');
const VendorEdit = () => import('../views/dictionary/VendorEdit.vue');
const StorageEdit = () => import('../views/dictionary/StorageEdit.vue');
const MaterialEdit = () => import('../views/dictionary/MaterialEdit.vue');

// lazy load reporting views
const StorageReport = () => import('../views/reporting/StorageReport.vue');
const ProductionReport = () => import('../views/reporting/ProductionReport.vue');
const ProcessingReport = () => import('../views/reporting/ProcessingReport.vue');

Vue.use(Router);

export default new Router({
  routes: [{
    path: '/',
    name: 'Home',
    component: HomeView,
  },
  {
    path: '/mockup',
    component: Mockup,
    children: [{
      path: 'add',
      component: AddRecord,
    }, {
      path: 'edit',
      component: EditRecord,
    }, {
      path: 'detail',
      component: RecordDetail,
    }, {
      path: 'records',
      component: ViewRecords,
    }],
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
