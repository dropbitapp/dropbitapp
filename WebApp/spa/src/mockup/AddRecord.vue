<template>
    <div id="addRecords">
        <section class="hero is-primary">
            <div class="hero-body">
                <div class="container is-bold">
                    <h1 class="title">Add Production Batches</h1>
                    <h2 class="subtitle">Description</h2>
                </div>
            </div>
        </section>
        <section class="section has-background-light">
            <div class="container">
                <div class="box">
                    <div class="columns level">
                        <div class="column level-left">
                            <b-field grouped
                                group-multiline
                                class="level-item">
                                <div class="control">
                                    <b-radio v-model="selectedProductionWorkflow"
                                        native-value="ferment">
                                        Ferment
                                    </b-radio>
                                </div>
                                <div class="control">
                                    <b-radio v-model="selectedProductionWorkflow"
                                        native-value="distill">
                                        Distill
                                    </b-radio>
                                </div>
                                <div class="control">
                                    <b-radio v-model="selectedProductionWorkflow"
                                        native-value="blend">
                                        Bottle
                                    </b-radio>
                                </div>
                                <div class="control">
                                    <b-radio v-model="selectedProductionWorkflow"
                                        native-value="bottle"
                                        disabled>
                                        Blend
                                    </b-radio>
                                </div>
                            </b-field>
                        </div>
                        <div class="column level-right">
                            <b-field class="level-item"
                                grouped
                                position="is-right">
                                <b-dropdown hoverable>
                                    <button class="button is-primary"
                                        slot="trigger">
                                        <span>View Production Batches</span>
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
                </div>
                <!-- STEP 1 -->
                <div class="box"
                    id="step1">
                    <ul class="steps my-step-style has-content-centered has-gaps">
                        <li class="steps-segment is-active">
                            <a v-scroll-to="'#step1'"
                                class="steps-marker">1</a>
                            <div class="steps-content">
                                <p class="is-size-3 is-italic">Source</p>
                            </div>
                        </li>
                        <li class="steps-segment">
                            <a v-scroll-to="'#step2'"
                                class="steps-marker">2</a>
                            <div class="steps-content">
                                <p class="is-size-5">Storage</p>
                            </div>
                        </li>
                        <li class="steps-segment">
                            <a v-scroll-to="'#step3'"
                                class="steps-marker">3</a>
                            <div class="steps-content">
                                <p class="is-size-5">Detail</p>
                            </div>
                        </li>
                    </ul>
                    <!-- CONTENT -->
                    <button class="button field is-danger"
                        @click="checkedRowsSource = []"
                        :disabled="!checkedRowsSource.length">
                        <b-icon icon="close"></b-icon>
                        <span>Clear checked</span>
                    </button>
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
                        :checked-rows.sync="checkedRowsSource"
                        :is-row-checkable="(row) => row.id !== 3"
                        checkable
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
                    <div>Burndown Functionality
                        (Put in rows/tiles with
                        <!-- eslint-disable-next-line no-trailing-spaces -->
                        dropdowns for volume/weight, all/partial, burndown/total, remove):
                        <!-- eslint-disable-next-line no-trailing-spaces -->
                        and total volume/weight burned town
                        {{ checkedRowsSource }}
                    </div>

                    <div class="columns">
                        <div class="column">
                            <div class="card">
                                <header class="card-header">
                                    <p class="card-header-title">
                                        Source Batch 1
                                    </p>
                                </header>
                                <div class="card-content">
                                    <div class="content">
                                        <div>
                                            <time date="2019-1-1">Date: 1 Jan 2019</time>
                                        </div>
                                        <div>Used: 50/100 gallons</div>
                                    </div>
                                </div>
                                <footer class="card-footer">
                                    <a href="#"
                                        class="card-footer-item">Remove</a>
                                </footer>
                            </div>
                        </div>
                        <div class="column">
                            <div class="card">
                                <header class="card-header">
                                    <p class="card-header-title">
                                        Source Batch 1
                                    </p>
                                </header>
                                <div class="card-content">
                                    <div class="content">
                                        <div>
                                            <time date="2019-1-1">Date: 1 Jan 2019</time>
                                        </div>
                                        <div>Used: 50/100 gallons</div>
                                    </div>
                                </div>
                                <footer class="card-footer">
                                    <a href="#"
                                        class="card-footer-item">Remove</a>
                                </footer>
                            </div>
                        </div>
                        <div class="column">
                            <div class="card">
                                <header class="card-header">
                                    <p class="card-header-title">
                                        Source Batch 1
                                    </p>
                                </header>
                                <div class="card-content">
                                    <div class="content">
                                        <div>
                                            <time date="2019-1-1">Date: 1 Jan 2019</time>
                                        </div>
                                        <div>Used: 50/100 gallons</div>
                                    </div>
                                </div>
                                <footer class="card-footer">
                                    <a href="#"
                                        class="card-footer-item">Remove</a>
                                </footer>
                            </div>
                        </div>
                    </div>

                </div>

                <!-- STEP 2 -->
                <div class="box"
                    id="step2">
                    <ul class="steps my-step-style has-content-centered has-gaps">
                        <li class="steps-segment">
                            <a v-scroll-to="'#step1'"
                                class="steps-marker">1</a>
                            <div class="steps-content">
                                <p class="is-size-5">Source</p>
                            </div>
                        </li>
                        <li class="steps-segment is-active">
                            <a v-scroll-to="'#step2'"
                                class="steps-marker">2</a>
                            <div class="steps-content">
                                <p class="is-size-3 is-italic">Storage</p>
                            </div>
                        </li>
                        <li class="steps-segment">
                            <a v-scroll-to="'#step3'"
                                class="steps-marker">3</a>
                            <div class="steps-content">
                                <p class="is-size-5">Detail</p>
                            </div>
                        </li>
                    </ul>
                    <!-- CONTENT -->
                    <button class="button field is-danger"
                        @click="checkedRowsStorage = []"
                        :disabled="!checkedRowsStorage.length">
                        <b-icon icon="close"></b-icon>
                        <span>Clear checked</span>
                    </button>
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
                        :checked-rows.sync="checkedRowsStorage"
                        :is-row-checkable="(row) => row.id !== 3"
                        checkable
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
                    <div>Selected Storages(Put in rows/tiles with remove button):{{ checkedRowsStorage }}</div>
                </div>
                <!-- STEP 3 -->
                <div class="box"
                    id="step3">
                    <ul class="steps my-step-style has-content-centered has-gaps">
                        <li class="steps-segment">
                            <a v-scroll-to="'#step1'"
                                class="steps-marker">1</a>
                            <div class="steps-content">
                                <p class="is-size-5">Source</p>
                            </div>
                        </li>
                        <li class="steps-segment">
                            <a v-scroll-to="'#step2'"
                                class="steps-marker">2</a>
                            <div class="steps-content">
                                <p class="is-size-5">Storage</p>
                            </div>
                        </li>
                        <li class="steps-segment is-active">
                            <a v-scroll-to="'#step3'"
                                class="steps-marker">3</a>
                            <div class="steps-content">
                                <p class="is-size-3 is-italic">Detail</p>
                            </div>
                        </li>
                    </ul>
                    <!-- CONTENT -->
                    <div>- Use Cleavejs(buefy extension) to format input values on the fly</div>
                    <div>- Add buefy tooltips</div>
                    <div>- Add form validation (html5/buefy extension)</div>
                    <br />
                    <div class="columns">
                        <div class="column is-half">
                            <b-field label="Batch Name">
                                <b-input v-model="batchName"></b-input>
                            </b-field>
                        </div>
                    </div>
                    <div class="columns">
                        <div class="column is-narrow">
                            <b-field label="Select a date">
                                <b-datepicker v-model="batchDate"
                                    placeholder="Click to select..."
                                    icon="calendar-today">
                                </b-datepicker>
                            </b-field>
                        </div>
                    </div>
                    <div class="columns">
                        <div class="column is-narrow">
                            <b-field label="Spirit Type">
                                <b-select v-model="spiritType"
                                    placeholder="Select a Spirit Type">
                                    <option v-for="type in spiritTypes"
                                        :value="type.name"
                                        :key="type.id">
                                        {{ type.name }}
                                    </option>
                                </b-select>
                            </b-field>
                        </div>
                    </div>
                    <div class="columns">
                        <div class="column is-one-quarter">
                            <b-field label="Volume(gal)">
                                <b-input v-model="batchVolume"></b-input>
                            </b-field>
                        </div>
                        <div class="column is-one-quarter">
                            <b-field label="Weight(lb)">
                                <b-input v-model="batchWeight"></b-input>
                            </b-field>
                        </div>
                    </div>
                    <div class="columns">
                        <div class="column is-one-quarter">
                            <b-field label="Alcohol Content">
                                <b-input v-model="batchAlcoholContent"></b-input>
                            </b-field>
                        </div>
                        <div class="column is-one-quarter">
                            <b-field label="Proof Gallons">
                                <b-input v-model="batchProof"></b-input>
                            </b-field>
                        </div>
                    </div>
                    <div class="columns">
                        <div class="column">
                            <b-field label="Note">
                                <b-input v-model="batchNote"
                                    type="textarea"></b-input>
                            </b-field>
                        </div>
                    </div>
                    <div class="columns level">
                        <div class="column level-left">
                            <button class="button is-primary">Reset Form</button>
                        </div>
                        <div class="column level-right">
                            <b-field class="level-item"
                                grouped
                                position="is-right">
                                <b-dropdown hoverable>
                                    <button class="button is-primary"
                                        slot="trigger">
                                        <span>Create Batch</span>
                                        <b-icon icon="menu-down"></b-icon>
                                    </button>
                                    <b-dropdown-item>Create Another</b-dropdown-item>
                                </b-dropdown>
                            </b-field>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>

<script>
export default {
  name: 'AddRecord',
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
      spiritTypes: [{
        id: 1,
        name: 'Gin',
      }, {
        id: 2,
        name: 'Vodka',
      }],
      checkedRowsSource: [],
      checkedRowsStorage: [],
      selectedProductionWorkflow: 'ferment',
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
