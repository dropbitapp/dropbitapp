<template>
  <form @submit.prevent="validateBeforeSubmit">
    <b-field
      horizontal
      label="Vendor Name"
      :message="errors.first('Vendor Name')"
      :type="errors.first('Vendor Name') ? 'is-danger' : ''"
    >
      <!-- eslint-disable-next-line max-len -->
      <b-input v-model="vendorName" v-validate="'required'" name="Vendor Name" autocomplete="off"></b-input>
    </b-field>
    <b-field horizontal label="Note">
      <!-- eslint-disable-next-line max-len -->
      <textarea v-model="vendorNote" class="textarea"></textarea>
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
  name: 'AddVendorView',
  data() {
    return {
      vendorName: '',
      vendorNote: '',
    };
  },
  methods: {
    validateBeforeSubmit() {
      this.$validator.validateAll().then((result) => {
        if (result) {
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
      const vendor = {
        VendorName: this.vendorName,
        Note: this.vendorNote,
      };
      this.$store
        .dispatch('dictionary/createVendor', vendor)
        .then(() => {
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created vendor: ${vendor.VendorName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create vendor: ${vendor.VendorName}`,
            type: 'is-danger',
          });
        });
    },
    clear() {
      this.vendorName = undefined;
      this.vendorNote = undefined;
      this.$validator.reset();
    },
  },
};
</script>
