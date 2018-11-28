<template>
  <div>
    <router-link class="brand" :to="{name: 'Home'}" exact>DROPBIT.IO</router-link>
    <router-link active-class="link-active" class="link" to="/purchase">Purchase</router-link>
    <router-link active-class="link-active" class="link" to="/production">Production</router-link>
    <router-link active-class="link-active" class="link" to="/dictionary">Dictionary</router-link>
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

.foo {
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
