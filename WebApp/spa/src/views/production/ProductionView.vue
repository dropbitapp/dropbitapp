<template>
  <div>
    <h1>Production</h1>
    <p>View for displaying a list of existing production batches</p>
    <!-- TODO: v-if in ProductionCreateView or separate views for each production type? -->
    <router-link to="/production/add" exact>Add Production Batch</router-link>
    <ul>
      <!-- Convert to data table -->
      <template v-for="batch in productionBatches">
        <li :key="batch.id" @click="showDetail(batch)">
          {{ batch.id }}({{ batch.type }})
        </li>
      </template>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'ProductionView',
  data() {
    return {
      // Dummy data, will get replaced by vuex state management,
      // so a call to getProductions() or similar
      productionBatches: [
        { id: 1, type: 'fermentation', name: 'Fermenting Pomace' },
        { id: 2, type: 'distillation', name: 'Distilling Vodka' },
        { id: 3, type: 'blending', name: 'Blending Gin' },
        { id: 4, type: 'bottling', name: 'Bottling Brandy' },
      ],
    };
  },
  methods: {
    showDetail(batch) {
      this.$router.push({
        name: 'ProductionDetail',
        params: {
          id: batch.id,
          productionType: batch.type,
          batch,
        },
      });
    },
  },
};
</script>

<style scoped>
</style>
