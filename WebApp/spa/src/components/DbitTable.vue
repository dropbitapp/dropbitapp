<template>
  <div id="dbitTable">
    <div class="is-clearfix">
      <div
        id="pagingInfo"
        class="is-size-6 is-pulled-right"
      >Showing {{ pageRange }} of {{ data.length }}</div>
    </div>
    <b-table
      :data="data"
      ref="table"
      :bordered="isBordered"
      :striped="isStriped"
      :narrowed="isNarrowed"
      :hoverable="isHoverable"
      :loading="isLoading"
      :focusable="isFocusable"
      :mobile-cards="hasMobileCards"
      :paginated="isPaginated"
      :per-page="pageSize"
      :default-sort-direction="defaultSortDirection"
      :default-sort="id"
      :opened-detailed="defaultOpenedDetails"
      detailed
      :detail-key="id"
      :show-detail-icon="true"
      :current-page.sync="currentPage"
      :row-class="(row, index) => 'fix-arrow-icon-detail-width'">
    >
      >
      <template slot-scope="props">
        <slot name="columns" v-bind:row="props.row"></slot>
      </template>
      <template slot="header" slot-scope="props">
        <slot name="header" v-bind:column="props.column">{{ props.column.label }}</slot>
      </template>
      <template slot="detail" slot-scope="props">
        <slot name="detail" v-bind:row="props.row"></slot>
      </template>
      <template slot="bottom-left">
        <b-field>
          <b-select v-model="pageSizeString" :disabled="!isPaginated">
            <option value="5">5 per page</option>
            <option value="10">10 per page</option>
            <option value="25">25 per page</option>
            <option value="50">50 per page</option>
            <option value="100">100 per page</option>
          </b-select>
        </b-field>
      </template>
    </b-table>
  </div>
</template>

<script>
export default {
  name: 'DbitTable',
  computed: {
    pageSize() {
      return parseInt(this.pageSizeString, 10);
    },
    pageRange() {
      const rangeStart = (this.currentPage * this.pageSize) - (this.pageSize - 1);
      const tempRange = rangeStart + (this.pageSize - 1);
      return `${rangeStart}-${
        tempRange > this.data.length ? this.data.length : tempRange
      }`;
    },
  },
  data() {
    return {
      isEmpty: false,
      isBordered: false,
      isStriped: false,
      isNarrowed: false,
      isHoverable: true,
      isFocusable: false,
      isLoading: false,
      hasMobileCards: true,
      isPaginated: true,
      defaultSortDirection: 'desc',
      defaultOpenedDetails: [1],
      pageSizeString: 5,
      currentPage: 1,
    };
  },
  props: {
    data: {
      type: Array,
      required: true,
    },
    id: {
      type: String,
      required: true,
    },
  },
  methods: {
    toggle(row) {
      this.$refs.table.toggleDetails(row);
    },
  },
};
</script>

<style>
/* On screens that are 1023px or less */
@media screen and (max-width: 1023px) {
  #pagingInfo {
    padding-bottom: 1rem;
  }
}

.fix-arrow-icon-detail-width td[class='chevron-cell'] {
  width: 50px;
}
</style>
