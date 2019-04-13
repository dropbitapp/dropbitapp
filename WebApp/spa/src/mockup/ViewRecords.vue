<template>
    <!-- eslint-disable max-len -->
    <div id="viewRecords">
        <section class="hero is-primary">
            <div class="hero-body">
                <div class="container is-bold">
                    <h1 class="title">
                        Production
                    </h1>
                    <h2 class="subtitle">
                        Manage Distillery Production
                    </h2>
                </div>
            </div>
        </section>
        <section class="section has-background-light">
            <div class="container">
                <div class="box">
                    <b-field grouped
                        group-multiline>
                        <div class="control">
                            <b-switch v-model="isBordered">Bordered</b-switch>
                        </div>
                        <div class="control">
                            <b-switch v-model="isStriped">Striped</b-switch>
                        </div>
                        <div class="control">
                            <b-switch v-model="isNarrowed">Narrowed</b-switch>
                        </div>
                        <div class="control">
                            <b-switch v-model="isHoverable">Hoverable</b-switch>
                        </div>
                        <div class="control">
                            <b-switch v-model="isFocusable">Focusable</b-switch>
                        </div>
                        <div class="control">
                            <b-switch v-model="isLoading">Loading state</b-switch>
                        </div>
                        <div class="control">
                            <b-switch v-model="isEmpty">Empty</b-switch>
                        </div>
                        <div class="control">
                            <b-switch v-model="hasMobileCards">Mobile cards <small>(collapsed rows)</small></b-switch>
                        </div>
                        <div class="control is-flex">
                            <b-switch v-model="isPaginated">Paginated</b-switch>
                        </div>
                    </b-field>
                </div>
                <div class="box">
                    <div class="columns">
                        <div class="column">
                            <h5 class="subtitle">Last</h5>
                            <div class="tags">
                                <span class="tag is-link">Day</span>
                                <span class="tag is-link">Week</span>
                                <span class="tag is-link">Month</span>
                                <span class="tag is-link">Quarter</span>
                                <span class="tag is-link">Year</span>
                            </div>
                        </div>
                        <div class="column">
                            <h5 class="subtitle">Month</h5>
                            <div class="tags">
                                <span class="tag is-link">Jan</span>
                                <span class="tag is-link">Feb</span>
                                <span class="tag is-link">Mar</span>
                                <span class="tag is-link">Apr</span>
                                <span class="tag is-link">May</span>
                                <span class="tag is-link">Jun</span>
                                <span class="tag is-link">Jul</span>
                                <span class="tag is-link">Aug</span>
                                <span class="tag is-link">Sep</span>
                                <span class="tag is-link">Oct</span>
                                <span class="tag is-link">Nov</span>
                                <span class="tag is-link">Dec</span>
                            </div>
                        </div>
                        <div class="column">
                            <h5 class="subtitle">Spirit Cut</h5>
                            <div class="tags">
                                <span class="tag is-link">Head</span>
                                <span class="tag is-link">Heart</span>
                                <span class="tag is-link">Tail</span>
                                <span class="tag is-link">Mixed</span>
                            </div>
                        </div>
                        <div class="column search">
                            <b-field>
                                <b-input placeholder="Search..."
                                    type="search"
                                    icon="magnify">
                                </b-input>
                            </b-field>
                        </div>
                    </div>
                </div>
                <div class="box">
                    <div class="columns level">
                        <div class="column level-left">
                            <b-field grouped
                                group-multiline
                                class="level-item">
                                <div class="control">
                                    <b-radio v-model="selectedProductionWorkflow"
                                        native-value="fermentation">
                                        Fermentation
                                    </b-radio>
                                </div>
                                <div class="control">
                                    <b-radio v-model="selectedProductionWorkflow"
                                        native-value="distillation">
                                        Distillation
                                    </b-radio>
                                </div>
                                <div class="control">
                                    <b-radio v-model="selectedProductionWorkflow"
                                        native-value="blending">
                                        Blending
                                    </b-radio>
                                </div>
                                <div class="control">
                                    <b-radio v-model="selectedProductionWorkflow"
                                        native-value="bottling"
                                        disabled>
                                        Bottling
                                    </b-radio>
                                </div>
                            </b-field>
                        </div>
                        <div class="column level-right">
                            <b-field class="level-item"
                                grouped
                                position="is-right">
                                <b-select v-model="perPage"
                                    :disabled="!isPaginated">
                                    <option value="5">5 per page</option>
                                    <option value="10">10 per page</option>
                                    <option value="15">15 per page</option>
                                    <option value="20">20 per page</option>
                                </b-select>
                                <b-dropdown hoverable>
                                    <button class="button is-primary"
                                        slot="trigger">
                                        <span>New Production</span>
                                        <b-icon icon="menu-down"></b-icon>
                                    </button>
                                    <b-dropdown-item>Ferment</b-dropdown-item>
                                    <b-dropdown-item>Distill</b-dropdown-item>
                                    <b-dropdown-item>Blend</b-dropdown-item>
                                    <b-dropdown-item>Bottle</b-dropdown-item>
                                </b-dropdown>
                            </b-field>
                        </div>
                    </div>
                    <div class="is-clearfix">
                        <span class="is-size-6 is-pulled-right">Showing 1-5 of 100</span>
                    </div>
                    <b-table :data="data"
                        :bordered="isBordered"
                        :striped="isStriped"
                        :narrowed="isNarrowed"
                        :hoverable="isHoverable"
                        :loading="isLoading"
                        :focusable="isFocusable"
                        :mobile-cards="hasMobileCards"
                        :paginated="isPaginated"
                        :per-page="perPage"
                        :default-sort-direction="defaultSortDirection"
                        default-sort="id"
                        :opened-detailed="defaultOpenedDetails"
                        detailed
                        detail-key="id"
                        @details-open="(row, index) => $toast.open(`Expanded ${row.name}`)">
                        <template slot-scope="props">
                            <b-table-column field="id"
                                label="ID"
                                width="40"
                                numeric
                                sortable>
                                {{ props.row.id }}
                            </b-table-column>

                            <b-table-column field="name"
                                label="Batch Name"
                                sortable>
                                {{ props.row.name }}
                            </b-table-column>

                            <b-table-column field="spiritCut"
                                label="Spirit Cut"
                                sortable>
                                {{ props.row.spiritCut }}
                            </b-table-column>


                            <b-table-column field="quantity"
                                label="Qty"
                                sortable>
                                {{ props.row.quantity }}
                            </b-table-column>


                            <b-table-column field="weight"
                                label="Weight"
                                sortable>
                                {{ props.row.weight }}
                            </b-table-column>


                            <b-table-column field="alcohol"
                                label="Alcohol %"
                                sortable>
                                {{ props.row.alcohol }}
                            </b-table-column>

                            <b-table-column field="proof"
                                label="Proof"
                                sortable>
                                {{ props.row.proof }}
                            </b-table-column>

                            <b-table-column field="date"
                                label="Date"
                                sortable
                                centered>
                                <span class="tag is-primary">
                                    {{ new Date(props.row.date).toLocaleDateString() }}
                                </span>
                            </b-table-column>
                        </template>
                        <template slot="detail"
                            slot-scope="props">
                            <article class="media">
                                <figure class="media-left">
                                    <p class="image is-64x64">
                                        <img src="/static/img/placeholder-128x128.png">
                                    </p>
                                </figure>
                                <div class="media-content">
                                    <div class="content">
                                        <p>
                                            <strong>{{ props.row.name }}</strong>
                                            <small>@{{ props.row.date }}</small>
                                            <br>
                                            Lorem ipsum dolor sit amet, consectetur adipiscing elit.
                                            Proin ornare magna eros, eu pellentesque tortor vestibulum ut.
                                            Maecenas non massa sem. Etiam finibus odio quis feugiat facilisis.
                                        </p>
                                    </div>
                                </div>
                            </article>
                        </template>
                    </b-table>
                </div>
            </div>
        </section>
    </div>
</template>
<style>
    .search {
        display: flex;
        justify-content: center;
        align-items: center;
    }
</style>
<script>
export default {
  name: 'ViewRecords',
  created() {
    const records = [];
    for (let i = 1; i <= 100; i += 1) {
      const record = {
        id: i,
        name: 'Golden Brandy',
        date: '2019-01-01 00:00:00',
        spiritCut: 'Heart',
        quantity: 10,
        weight: 50,
        alcohol: 50,
        proof: 100,
      };
      records.push(record);
    }
    this.data = records;
  },
  data() {
    return {
      data: [],
      selectedProductionWorkflow: 'fermentation',
      isEmpty: false,
      isBordered: false,
      isStriped: false,
      isNarrowed: false,
      isHoverable: false,
      isFocusable: false,
      isLoading: false,
      hasMobileCards: true,
      isPaginated: true,
      defaultSortDirection: 'desc',
      perPage: 5,
      defaultOpenedDetails: [1],
    };
  },
};
</script>
