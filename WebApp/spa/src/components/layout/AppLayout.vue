<template>
  <div>
    <!-- NAVBAR -->
    <nav-bar></nav-bar>
    <!-- VIEW -->
    <router-view />
    <!-- FOOTER -->
    <page-footer></page-footer>
  </div>
</template>

<script>
import NavBar from './NavBar.vue';
import PageFooter from './PageFooter.vue';

export default {
  name: 'AppLayout',
  components: {
    NavBar,
    PageFooter,
  },
};
</script>
