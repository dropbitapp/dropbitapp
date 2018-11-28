<template>
  <div>
    <router-link class="brand" :to="{name: 'Home'}" exact>DROPBIT.IO</router-link>
    <router-link active-class="link-active" class="link" to="/purchase">Purchase</router-link>
    <router-link active-class="link-active" class="link" to="/production">Production</router-link>
    <router-link active-class="link-active" class="link" to="/dictionary">Dictionary</router-link>
    <!-- eslint-disable-next-line max-len -->
    <router-link active-class="link-active" class="link" to="/reporting/storage">Storage Report</router-link>
    <!-- eslint-disable-next-line max-len -->
    <router-link active-class="link-active" class="link" to="/reporting/production">Production Report</router-link>
    <!-- eslint-disable-next-line max-len -->
    <router-link active-class="link-active" class="link" to="/reporting/processing">Processing Report</router-link>
  </div>
</template>

<script>
export default {
  name: 'NavBar',
};
</script>

<style scoped>
  .brand {
    text-decoration: none;
    color: inherited;
  }

  .link-active {
    color: red;
  }

  div,
  .brand,
  .link {
    padding-right: 5px;
    padding-left: 5px;
  }

  .link-active {
    width: 50px;
  }
</style>
