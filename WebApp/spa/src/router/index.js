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
const AddFermentableView = () => import('../views/purchase/AddFermentableView.vue');
const AddFermentedView = () => import('../views/purchase/AddFermentedView.vue');
const AddDistilledView = () => import('../views/purchase/AddDistilledView.vue');
const AddSupplyView = () => import('../views/purchase/AddSupplyView.vue');
const AddAdditiveView = () => import('../views/purchase/AddAdditiveView.vue');
const FermentableView = () => import('../views/purchase/FermentableView.vue');
const FermentedView = () => import('../views/purchase/FermentedView.vue');
const DistilledView = () => import('../views/purchase/DistilledView.vue');
const SupplyView = () => import('../views/purchase/SupplyView.vue');
const AdditiveView = () => import('../views/purchase/AdditiveView.vue');

// lazy load production views
const Production = () => import('../views/production/Production.vue');
const ProductionView = () => import('../views/production/ProductionView.vue');
const AddProductionView = () => import('../views/production/AddProductionView.vue');
const FermentationView = () => import('../views/production/FermentationView.vue');
const DistillationView = () => import('../views/production/DistillationView.vue');
const BlendingView = () => import('../views/production/BlendingView.vue');
const BottlingView = () => import('../views/production/BottlingView.vue');

// lazy load dictionary views
const Dictionary = () => import('../views/dictionary/Dictionary.vue');
const DictionaryView = () => import('../views/dictionary/DictionaryView.vue');
const AddDictionaryView = () => import('../views/dictionary/AddDictionaryView.vue');
const AddSpiritView = () => import('../views/dictionary/AddSpiritView.vue');
const AddVendorView = () => import('../views/dictionary/AddVendorView.vue');
const AddStorageView = () => import('../views/dictionary/AddStorageView.vue');
const AddMaterialView = () => import('../views/dictionary/AddMaterialView.vue');
const SpiritView = () => import('../views/dictionary/SpiritView.vue');
const VendorView = () => import('../views/dictionary/VendorView.vue');
const StorageView = () => import('../views/dictionary/StorageView.vue');
const MaterialView = () => import('../views/dictionary/MaterialView.vue');

// lazy load reporting views
const Reporting = () => import('../views/reporting/Reporting.vue');
const StorageReportView = () => import('../views/reporting/StorageReportView.vue');
const ProductionReport = () => import('../views/reporting/ProductionReport.vue');
const ProcessingReportView = () => import('../views/reporting/ProcessingReportView.vue');

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
      redirect: 'fermentable',
      component: PurchaseView,
      children: [{
        path: 'fermentable',
        component: FermentableView,
      },
      {
        path: 'fermented',
        component: FermentedView,
      },
      {
        path: 'distilled',
        component: DistilledView,
      },
      {
        path: 'supply',
        component: SupplyView,
      },
      {
        path: 'additive',
        component: AdditiveView,
      }],
    },
    ],
  },
  {
    path: '/purchase/add',
    component: Purchase,
    children: [{
      path: '',
      redirect: 'spirit',
      component: AddPurchaseView,
      children: [{
        path: 'fermentable',
        component: AddFermentableView,
      },
      {
        path: 'fermented',
        component: AddFermentedView,
      },
      {
        path: 'distilled',
        component: AddDistilledView,
      },
      {
        path: 'supply',
        component: AddSupplyView,
      },
      {
        path: 'additive',
        component: AddAdditiveView,
      },
      ],
    },
    ],
  },
  {
    path: '/production',
    component: Production,
    children: [{
      path: '',
      redirect: 'fermentation',
      component: ProductionView,
      children: [{
        path: 'fermentation',
        component: FermentationView,
      },
      {
        path: 'distillation',
        component: DistillationView,
      },
      {
        path: 'blending',
        component: BlendingView,
      },
      {
        path: 'bottling',
        component: BottlingView,
      }],
    },
    {
      path: 'add',
      component: AddProductionView,
    },
    ],
  },
  {
    path: '/dictionary',
    component: Dictionary,
    children: [{
      path: '',
      redirect: 'spirit',
      component: DictionaryView,
      children: [{
        path: 'spirit',
        component: SpiritView,
      },
      {
        path: 'vendor',
        component: VendorView,
      },
      {
        path: 'storage',
        component: StorageView,
      },
      {
        path: 'material',
        component: MaterialView,
      }],
    },
    ],
  },
  {
    path: '/dictionary/add',
    component: Dictionary,
    children: [{
      path: '',
      redirect: 'spirit',
      component: AddDictionaryView,
      children: [{
        path: 'spirit',
        component: AddSpiritView,
      },
      {
        path: 'vendor',
        component: AddVendorView,
      },
      {
        path: 'storage',
        component: AddStorageView,
      },
      {
        path: 'material',
        component: AddMaterialView,
      },
      ],
    },
    ],
  },
  {
    path: '/reporting',
    component: Reporting,
    children: [{
      path: 'storage',
      props: true,
      component: StorageReportView,
    },
    {
      path: 'production',
      component: ProductionReport,
    },
    {
      path: 'processing',
      props: true,
      component: ProcessingReportView,
    },
    ],
  },
  {
    path: '*',
    component: NotFound,
  },
  ],
});
