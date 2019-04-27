<template>
  <form @submit.prevent="validateBeforeSubmit">
    <b-field
      horizontal
      label="Storage Name"
      :message="errors.first('Storage Name')"
      :type="errors.first('Storage Name') ? 'is-danger' : ''"
    >
      <!-- eslint-disable-next-line max-len -->
      <b-input v-model="storageName" v-validate="'required'" name="Storage Name" autocomplete="off"></b-input>
    </b-field>
    <b-field
      horizontal
      label="Serial Number"
    >
      <!-- eslint-disable-next-line max-len -->
      <b-input v-model="storageSerial" name="Serial Number" autocomplete="off" placeholder="Serial number only applicable to bottle cases"></b-input>
    </b-field>
    <b-field horizontal label="Note">
      <!-- eslint-disable-next-line max-len -->
      <textarea v-model="storageNote" class="textarea"></textarea>
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
  name: 'AddStorageView',
  data() {
    return {
      storageName: '',
      storageSerial: '',
      storageNote: '',
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
      const storage = {
        StorageName: this.storageName,
        SerialNumber: this.storageSerial,
        Note: this.storageNote,
      };
      this.$store
        .dispatch('dictionary/createStorage', storage)
        .then(() => {
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created storage: ${storage.StorageName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create storage: ${storage.StorageName}`,
            type: 'is-danger',
          });
        });
    },
    clear() {
      this.storageName = undefined;
      this.storageSerial = undefined;
      this.storageNote = undefined;
      this.$validator.reset();
    },
  },
};
</script>
