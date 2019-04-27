<template>
  <form @submit.prevent="validateBeforeSubmit">
    <b-field
      horizontal
      label="Spirit Name"
      :message="errors.first('Spirit Name')"
      :type="errors.first('Spirit Name') ? 'is-danger' : ''"
    >
      <!-- eslint-disable-next-line max-len -->
      <b-input v-model="spiritName" v-validate="'required'" name="Spirit Name" autocomplete="off"></b-input>
    </b-field>
    <b-field
      horizontal
      label="Spirit Type"
      :message="errors.first('Spirit Type')"
      :type="errors.first('Spirit Type') ? 'is-danger' : ''"
    >
      <!-- eslint-disable-next-line max-len -->
      <b-select
        v-model="spiritTypeId"
        placeholder="Select a Spirit Type"
        v-validate="'required'"
        name="Spirit Type"
      >
        <!-- eslint-disable-next-line max-len -->
        <option v-for="type in spiritTypes" :value="type.Id" :key="type.Id">{{ type.Name }}</option>
      </b-select>
    </b-field>
    <b-field horizontal label="Note">
      <!-- eslint-disable-next-line max-len -->
      <textarea v-model="spiritNote" class="textarea"></textarea>
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
  name: 'AddSpiritView',
  created() {
    this.$store.dispatch('dictionary/getProcessingReportTypes');
  },
  computed: {
    spiritTypes() {
      return this.$store.state.dictionary.processingReportTypes || [{}];
    },
  },
  data() {
    return {
      spiritName: '',
      spiritTypeId: undefined,
      spiritNote: '',
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
      const spirit = {
        SpiritName: this.spiritName,
        ProcessingReportTypeID: this.spiritTypeId,
        Note: this.spiritNote,
      };
      this.$store
        .dispatch('dictionary/createSpirit', spirit)
        .then(() => {
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created spirit: ${spirit.SpiritName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create spirit: ${spirit.SpiritName}`,
            type: 'is-danger',
          });
        });
    },
    clear() {
      this.spiritName = undefined;
      this.spiritTypeId = undefined;
      this.spiritNote = undefined;
      this.$validator.reset();
    },
  },
};
</script>
