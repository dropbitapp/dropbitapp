<template>
  <div id="productionView">
    <section class="hero is-primary">
      <div class="hero-body">
        <div class="container is-bold">
          <h1 class="title">Production Management</h1>
          <h2 class="subtitle">Lorem Ipsum</h2>
        </div>
      </div>
    </section>
    <section class="section has-background-light">
      <div class="container">
        <div class="box">
          <!-- LINKS -->
          <div class="columns level">
            <div class="column level-left">
              <b-field grouped group-multiline>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="fermentation">Fermentation</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="distillation">Distillation</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="blending">Blending</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="bottling">Bottling</b-radio>
                </div>
              </b-field>
            </div>
            <div class="column level-right is-clearfix">
              <b-field class="is-pulled-right">
                <b-dropdown class="level-item control" hoverable>
                  <button class="button is-primary" slot="trigger">
                    <b-icon icon="plus"></b-icon>
                    <span>New Production</span>
                    <b-icon icon="menu-down"></b-icon>
                  </button>
                  <b-dropdown-item>Fermentation</b-dropdown-item>
                  <b-dropdown-item>Distillation</b-dropdown-item>
                  <b-dropdown-item>Blending</b-dropdown-item>
                  <b-dropdown-item>Bottling</b-dropdown-item>
                </b-dropdown>
              </b-field>
            </div>
          </div>
          <!-- TABLE -->
          <router-view></router-view>
        </div>
      </div>
    </section>
  </div>
</template>

<script>
export default {
  name: 'ProductionView',
  data() {
    return {
      workflow: 'fermentation', // default workflow
    };
  },
  watch: {
    workflow(newWorkflow, oldWorkflow) {
      if (newWorkflow !== oldWorkflow) {
        this.$router.push(`/production/${newWorkflow}`);
      }
    },
  },
};
</script>
