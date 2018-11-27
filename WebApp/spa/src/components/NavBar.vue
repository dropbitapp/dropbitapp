<template>
  <div>
    <router-link class="brand" :to="{name: 'Home'}" exact>DROPBIT.IO</router-link>
    <router-link class="link" to="/purchase" exact>Purchase</router-link>
    <router-link class="link" to="/production" exact>Production</router-link>
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

.router-link-active {
  color: red;
}

div,
.brand,
.link {
  padding-right: 5px;
  padding-left: 5px;
}

.link {
  width: 50px;
}
</style>
