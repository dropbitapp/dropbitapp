<template>
  <div>
    <h1>Dictionary</h1>
    <p>View for displaying a list of existing dictionary items</p>
    <p>Spirit Count: {{spiritCount}}</p>
    <router-link to="/dictionary/add" exact>
      <button>Add Dictionary Item</button>
    </router-link>
    <div class="table-wrapper">
      <table>
        <tr>
          <th>id</th>
          <th>name</th>
          <th>note</th>
          <th>actions</th>
        </tr>
        <template v-for="item in spirits">
          <tr :key="item.SpiritId">
            <td @click="showDetail(item)">{{item.SpiritId}}</td>
            <td @click="showDetail(item)">{{item.SpiritName}}</td>
            <td @click="showDetail(item)">{{item.Note}}</td>
            <td>
              <button @click="deleteItem(item)">delete</button>
            </td>
          </tr>
        </template>
      </table>
    </div>
  </div>
</template>

<script>
export default {
  name: 'DictionaryView',
  created() {
    this.$store.dispatch('dictionary/getSpirits');
  },
  computed: {
    spirits() {
      return this.$store.state.dictionary.spirits;
    },
    spiritCount() {
      return this.$store.getters['dictionary/spiritCount'];
    },
  },
  methods: {
    showDetail(item) {
      // this.$router.push(`/dictionary/${item.type}/detail/${item.id}`);
      this.$router.push(`/dictionary/spirit/detail/${item.SpiritId}`);
    },
    deleteItem(item) {
      const spirit = { DeleteRecordID: item.SpiritId, DeleteRecordType: 'Spirit' };
      this.$store.dispatch('dictionary/deleteSpirit', spirit);
      this.$store.dispatch('dictionary/getSpirits');
    },
  },
};
</script>

<style scoped>
table {
  margin-top: 15px;
}

table, th, td {
    border: 1px solid black;
    border-collapse: collapse;
}

th, td {
    padding: 15px;
}

.table-wrapper {
  display: flex;
  justify-content: center;
}
</style>
