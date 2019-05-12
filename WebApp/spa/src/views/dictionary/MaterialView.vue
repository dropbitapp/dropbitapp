<template>
  <div class="container">
    <div class="box">
      <dbit-table :data="materials" rowId="RawMaterialId">
        <template slot-scope="props" slot="header">
          <template v-if="props.column.meta">
            <b-tooltip
              :active="!!props.column.meta"
              :label="props.column.meta"
              dashed
            >{{ props.column.label }}</b-tooltip>
          </template>
          <template v-else>{{ props.column.label }}</template>
        </template>
        <template slot="columns" slot-scope="props">
          <!-- Id -->
          <b-table-column
            field="RawMaterialId"
            label="ID"
            width="50"
            sortable
          >{{ props.row.RawMaterialId }}</b-table-column>
    
          <!-- Name -->
          <!-- eslint-disable-next-line max-len -->
          <b-table-column field="RawMaterialName" label="Name" sortable><span class="row-content">{{ props.row.RawMaterialName }}</span></b-table-column>
    
          <!-- Unit Type -->
          <!-- eslint-disable-next-line max-len -->
          <b-table-column field="UnitType" label="Unit" sortable><span class="row-content">{{ props.row.UnitType }}</span></b-table-column>
    
          <!-- Fermentable -->
          <b-table-column label="Fe" meta="Fermentable">
            <!-- Workaround for a bug where
            {{ props.row.PurchaseMaterialTypes.Prop }}
            is undefined in this scope -->
            <template v-for="(val, prop) in props.row.PurchaseMaterialTypes">
              <template v-if="prop==='Fermentable'">
                <!-- eslint-disable-next-line max-len -->
                <b-icon v-bind:key="prop" :type="val === true ? 'is-primary' : ''" :icon="val === true ? 'check-outline' : 'close-outline'"></b-icon>
              </template>
            </template>
          </b-table-column>
    
          <!-- Fermented -->
          <b-table-column label="Fd" meta="Fermented">
            <!-- Workaround for a bug where
            {{ props.row.PurchaseMaterialTypes.Prop }}
            is undefined in this scope -->
            <template v-for="(val, prop) in props.row.PurchaseMaterialTypes">
              <template v-if="prop==='Fermented'">
                <!-- eslint-disable-next-line max-len -->
                <b-icon v-bind:key="prop" :type="val === true ? 'is-primary' : ''" :icon="val === true ? 'check-outline' : 'close-outline'"></b-icon>
              </template>
            </template>
          </b-table-column>
    
          <!-- Distilled -->
          <b-table-column label="Dd" meta="Distilled">
            <!-- Workaround for a bug where
            {{ props.row.PurchaseMaterialTypes.Prop }}
            is undefined in this scope -->
            <template v-for="(val, prop) in props.row.PurchaseMaterialTypes">
              <template v-if="prop==='Distilled'">
                <!-- eslint-disable-next-line max-len -->
                <b-icon v-bind:key="prop" :type="val === true ? 'is-primary' : ''" :icon="val === true ? 'check-outline' : 'close-outline'"></b-icon>
              </template>
            </template>
          </b-table-column>
    
          <!-- Supply -->
          <b-table-column label="Sy" meta="Supply">
            <!-- Workaround for a bug where
            {{ props.row.PurchaseMaterialTypes.Prop }}
            is undefined in this scope -->
            <template v-for="(val, prop) in props.row.PurchaseMaterialTypes">
              <template v-if="prop==='Supply'">
                <!-- eslint-disable-next-line max-len -->
                <b-icon v-bind:key="prop" :type="val === true ? 'is-primary' : ''" :icon="val === true ? 'check-outline' : 'close-outline'"></b-icon>
              </template>
            </template>
          </b-table-column>
    
          <!-- Supply -->
          <b-table-column label="Ae" meta="Additive">
            <!-- Workaround for a bug where
            {{ props.row.PurchaseMaterialTypes.Prop }}
            is undefined in this scope -->
            <template v-for="(val, prop) in props.row.PurchaseMaterialTypes">
              <template v-if="prop==='Additive'">
                <!-- eslint-disable-next-line max-len -->
                <b-icon v-bind:key="prop" :type="val === true ? 'is-primary' : ''" :icon="val === true ? 'check-outline' : 'close-outline'"></b-icon>
              </template>
            </template>
          </b-table-column>
    
          <!-- Delete -->
          <b-table-column>
            <button class="button is-small is-primary is-pulled-right" @click="deleteItem(props.row)">
              <b-icon icon="delete-outline"></b-icon>
            </button>
          </b-table-column>
        </template>
        <template slot="detail" slot-scope="props">
          <div class="has-text-weight-bold">Notes</div>
          <div v-if="props.row.Note">
            <p>{{ props.row.Note }}</p>
          </div>
          <div v-else>
            <p>-</p>
          </div>
        </template>
      </dbit-table>
    </div>
  </div>
</template>

<script>
import DbitTable from '../../components/DbitTable.vue';

export default {
  name: 'MaterialView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('dictionary/getRawMaterials');
  },
  computed: {
    materials() {
      if (this.$store.state.dictionary.rawMaterials) {
        return this.$store.state.dictionary.rawMaterials;
      }
      return [{}];
    },
  },
  methods: {
    deleteItem(rawMaterial) {
      this.$store
        .dispatch('dictionary/deleteRawMaterial', rawMaterial.RawMaterialId)
        .then(() => {
          this.$toast.open({
            duration: 5000,
            message: `Successfully deleted raw material: ${
              rawMaterial.RawMaterialName
            }`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to delete raw material: ${
              rawMaterial.RawMaterialName
            }`,
            type: 'is-danger',
          });
        });
    },
  },
};
</script>

<style>
  span.row-content {
    word-break: break-all;
  }
</style>
