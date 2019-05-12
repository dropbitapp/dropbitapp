<template>
  <div id="addDictionaryView">
    <section class="hero is-primary">
      <div class="hero-body">
        <div class="container is-bold">
          <h1 class="title">Purchase Management</h1>
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
                  <b-radio v-model="workflow" native-value="fermentable">Fermentable</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="fermented">Fermented</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="distilled">Distilled</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="supply">Supply</b-radio>
                </div>
                <div class="level-item control">
                  <b-radio v-model="workflow" native-value="additive">Additive</b-radio>
                </div>
              </b-field>
            </div>
            <div class="column level-right is-clearfix">
              <b-field class="is-pulled-right">
                <b-dropdown class="level-item control" hoverable>
                  <button class="button is-primary" slot="trigger">
                    <span>View Purchases</span>
                    <b-icon icon="menu-down"></b-icon>
                  </button>
                  <b-dropdown-item>
                    <router-link to="/purchase/fermentable">
                      <div>Fermentable</div>
                    </router-link>
                  </b-dropdown-item>
                  <b-dropdown-item>
                    <router-link to="/purchase/fermented">
                      <div>Fermented</div>
                    </router-link>
                  </b-dropdown-item>
                  <b-dropdown-item>
                    <router-link to="/purchase/distilled">
                      <div>Distilled</div>
                    </router-link>
                  </b-dropdown-item>
                  <b-dropdown-item>
                    <router-link to="/purchase/supply">
                      <div>Supply</div>
                    </router-link>
                  </b-dropdown-item>
                  <b-dropdown-item>
                    <router-link to="/purchase/additive">
                      <div>Additive</div>
                    </router-link>
                  </b-dropdown-item>
                </b-dropdown>
              </b-field>
            </div>
          </div>
        </div>
        <!-- TABLE -->
        <router-view></router-view>
      </div>
    </section>
  </div>
</template>

<script>
export default {
  name: 'AddDictionaryView',
  data() {
    return {
      workflow: 'fermentable', // default workflow
    };
  },
  watch: {
    workflow(newWorkflow, oldWorkflow) {
      if (newWorkflow !== oldWorkflow) {
        this.$router.push(`/purchase/add/${newWorkflow}`);
      }
    },
  },
};
</script>

