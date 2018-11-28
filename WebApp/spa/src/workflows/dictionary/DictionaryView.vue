<template>
  <div>
    <h1>Dictionary</h1>
    <p>View for displaying a list of existing dictionary items</p>
    <router-link to="/dictionary/add" exact>
      <button>Add Dictionary Item</button>
    </router-link>
    <div class="table-wrapper">
      <table>
        <tr>
          <th>id</th>
          <th>name</th>
          <th>type</th>
          <th>actions</th>
        </tr>
        <template v-for="item in dictionaryItems">
          <tr :key="item.id">
            <td @click="showDetail(item)">{{item.id}}</td>
            <td @click="showDetail(item)">{{item.name}}</td>
            <td @click="showDetail(item)">{{item.type}}</td>
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
  data() {
    return {
      dictionaryItems: [
        { id: 1, type: 'spirit', name: 'Brandy' },
        { id: 2, type: 'vendor', name: 'Spirited Productions' },
        { id: 3, type: 'storage', name: 'Barrel' },
        { id: 4, type: 'material', name: 'Anise' },
      ],
    };
  },
  methods: {
    showDetail(item) {
      this.$router.push(`/dictionary/${item.type}/detail/${item.id}`);
    },
    deleteItem(item) {
      // eslint-disable-next-line no-alert
      // eslint-disable-next-line no-restricted-globals
      confirm(`Delete ${item.name}?`);
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
