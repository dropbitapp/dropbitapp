<template>
  <div>
    <router-link :to="{name: 'Home'}" exact>Home</router-link>
    <router-link to="/purchase" exact>Purchase</router-link>
    <router-link to="/production" exact>Production</router-link>
  </div>
</template>

<script>
export default {
  name: 'NavBar',
};
</script>

<style scoped>
.router-link-active {
  color: red;
}
</style>
