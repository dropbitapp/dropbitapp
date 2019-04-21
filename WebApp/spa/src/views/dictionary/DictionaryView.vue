<template>
  <div id="dictionaryView">
    <section class="hero is-primary">
      <div class="hero-body">
        <div class="container is-bold">
          <h1 class="title">Dictionary Management</h1>
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
                  <b-radio v-model="workflow" native-value="spirit">Spirit</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="vendor">Vendor</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="storage">Storage</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="material">Material</b-radio>
                </div>
              </b-field>
            </div>
            <div class="column level-right is-clearfix">
              <b-field class="is-pulled-right">
                <b-dropdown class="level-item control" hoverable>
                  <button class="button is-primary" slot="trigger">
                    <b-icon icon="plus"></b-icon>
                    <span>New Dictionary Item</span>
                    <b-icon icon="menu-down"></b-icon>
                  </button>
                  <b-dropdown-item>Spirit</b-dropdown-item>
                  <b-dropdown-item>Vendor</b-dropdown-item>
                  <b-dropdown-item>Storage</b-dropdown-item>
                  <b-dropdown-item>Material</b-dropdown-item>
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
  name: 'DictionaryView',
  data() {
    return {
      workflow: 'spirit', // default workflow
    };
  },
  watch: {
    workflow(newWorkflow, oldWorkflow) {
      if (newWorkflow !== oldWorkflow) {
        this.$router.push(`/dictionary/${newWorkflow}`);
      }
    },
  },
};
</script>

