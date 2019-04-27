<template>
  <form @submit.prevent="validateBeforeSubmit">
    <b-field
      horizontal
      label="Material Name"
      :message="errors.first('Material Name')"
      :type="errors.first('Material Name') ? 'is-danger' : ''"
    >
      <!-- eslint-disable-next-line max-len -->
      <b-input
        v-model="materialName"
        v-validate="'required'"
        name="Material Name"
        autocomplete="off"
      ></b-input>
    </b-field>
    <b-field horizontal class="level" label="Material Type">
      <b-checkbox v-model="materialTypes" native-value="Fermentable">Fermentable</b-checkbox>
    </b-field>
    <b-field horizontal>
      <b-checkbox v-model="materialTypes" native-value="Fermented">Fermented</b-checkbox>
    </b-field>
    <b-field horizontal>
      <b-checkbox v-model="materialTypes" native-value="Distilled">Distilled</b-checkbox>
    </b-field>
    <b-field horizontal>
      <b-checkbox v-model="materialTypes" native-value="Supplies">Supplies</b-checkbox>
    </b-field>
    <b-field horizontal>
      <b-checkbox v-model="materialTypes" native-value="Additives">Additives</b-checkbox>
    </b-field>
    <b-field horizontal v-show="materialTypeMessage">
      <span id="materialTypeMessage" class="help is-danger">
        <!-- eslint-disable-next-line vue/no-parsing-error -->
        {{ materialTypes.length < 1 && materialTypeMessage ? materialTypeMessage : '' }}
      </span>
    </b-field>
    <b-field
      v-show="materialTypes && (materialTypes.includes('Fermentable') || materialTypes.includes('Fermented'))"
      horizontal
      label="Material Category"
      :message="materialTypes && (materialTypes.includes('Fermentable') || materialTypes.includes('Fermented')) && errors.first('Material Category') ? errors.first('Material Category') : ''"
      :type="materialTypes && (materialTypes.includes('Fermentable') || materialTypes.includes('Fermented')) && errors.first('Material Category') ? 'is-danger' : ''"
    >
      <!-- eslint-disable-next-line max-len -->
      <b-select
        v-model="materialCategoryId"
        placeholder="Select a Material Category"
        v-validate="'required'"
        name="Material Category"
      >
        <!-- eslint-disable-next-line max-len -->
        <option
          v-for="category in materialCategories"
          :value="category.MaterialCategoryID"
          :key="category.MaterialCategoryID"
        >{{ category.MaterialCategoryName }}</option>
      </b-select>
    </b-field>
    <b-field
      horizontal
      label="Material Unit"
      :message="errors.first('Material Unit')"
      :type="errors.first('Material Unit') ? 'is-danger' : ''"
    >
      <!-- eslint-disable-next-line max-len -->
      <b-select
        v-model="materialUnit"
        placeholder="Select a Unit for Material"
        v-validate="'required'"
        name="Material Unit"
      >
        <!-- eslint-disable-next-line max-len -->
        <option
          v-for="unit in materialUnits"
          :value="unit.UnitOfMeasurementId"
          :key="unit.UnitOfMeasurementId"
        >{{ unit.UnitName }}</option>
      </b-select>
    </b-field>
    <b-field horizontal label="Note">
      <!-- eslint-disable-next-line max-len -->
      <textarea v-model="materialNote" class="textarea"></textarea>
    </b-field>
    <b-field grouped position="is-centered">
      <p class="control">
        <button class="button level is-primary" type="reset" @click="clear()">Clear</button>
      </p>
      <p class="control">
        <button class="button level is-primary" type="submit">Submit</button>
      </p>
    </b-field>
  </form>
</template>

<script>
export default {
  name: 'AddMaterialView',
  created() {
    this.$store.dispatch('dictionary/getMaterialCategories');
    this.$store.dispatch('dictionary/getUnits');
  },
  computed: {
    materialCategories() {
      return this.$store.state.dictionary.materialCategories || [{}];
    },
    materialUnits() {
      return this.$store.state.dictionary.units || [{}];
    },
  },
  data() {
    return {
      materialName: '',
      materialTypes: [],
      materialCategoryId: undefined,
      materialUnit: undefined,
      materialNote: '',
      materialTypeMessage: '',
    };
  },
  methods: {
    validateBeforeSubmit() {
      let materialTypesAreValid;
      if (this.materialTypes.length < 1) {
        this.materialTypeMessage = 'The Material Type field is required.';
      } else {
        this.materialTypeMessage = '';
        materialTypesAreValid = true;
      }
      this.$validator.validateAll().then((result) => {
        if (result && materialTypesAreValid) {
          this.submit();
        } else {
          this.$toast.open({
            message: 'Form is not valid! Please check the fields.',
            type: 'is-danger',
            position: 'is-bottom',
          });
        }
      });
    },
    submit() {
      const material = {
        RawMaterialName: this.materialName,
        MaterialCategoryID: this.materialCategoryId,
        PurchaseMaterialTypes: {
          Fermentable: this.materialTypes.includes('Fermentable'),
          Fermented: this.materialTypes.includes('Fermented'),
          Distilled: this.materialTypes.includes('Distilled'),
          Additive: this.materialTypes.includes('Additive'),
          Supply: this.materialTypes.includes('Supply'),
        },
        UnitTypeId: this.materialUnit,
        Note: this.materialNote,
      };
      this.$store
        .dispatch('dictionary/createRawMaterial', material)
        .then(() => {
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created material: ${
              material.RawMaterialName
            }`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create material: ${material.RawMaterialName}`,
            type: 'is-danger',
          });
        });
    },
    clear() {
      this.materialName = undefined;
      this.materialTypes = [];
      this.materialCategoryId = undefined;
      this.materialUnit = undefined;
      this.materialNote = undefined;
      this.materialTypeMessage = '';
      this.$validator.reset();
    },
  },
};
</script>

<style>
#materialTypeMessage {
  margin-top: -8px;
}
</style>
